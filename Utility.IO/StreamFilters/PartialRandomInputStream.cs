using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    /// <summary>
    /// バイトストリームの部分的な範囲のアクセスのみを可能にするクラスです。
    /// </summary>
    internal class PartialRandomInputStream<POSITION_T, BASE_POSITION_T>
        : RandomInputByteStreamFilter<POSITION_T, BASE_POSITION_T>
        where POSITION_T : struct, IComparable<POSITION_T>, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>
        where BASE_POSITION_T : struct, IComparable<BASE_POSITION_T>, IAdditionOperators<BASE_POSITION_T, UInt64, BASE_POSITION_T>, ISubtractionOperators<BASE_POSITION_T, BASE_POSITION_T, UInt64>
    {
        private readonly IRandomInputByteStream<BASE_POSITION_T> _baseStream;
        private readonly POSITION_T _endOfStream;
        private readonly (BASE_POSITION_T startOfRegion, UInt64 length, BASE_POSITION_T endOfRegion) _regionOnBaseStream;

        /// <summary>
        /// 元になるバイトストリームとアクセス可能な範囲(長さ)を使用して初期化するコンストラクタです。
        /// </summary>
        /// <param name="baseStream">
        /// 元になるバイトストリームを示す <see cref="ISequentialInputByteStream"/> オブジェクトです。
        /// </param>
        /// <param name="size">
        /// 元になるバイトストリームで、現在位置からアクセス可能なバイト数を示す <see cref="UInt64"/> 値です。
        /// </param>
        /// <param name="leaveOpen">
        /// コンストラクタによって初期化されたオブジェクトが破棄されるときに元になるバイトストリームもともに破棄するかどうかを示す <see cref="Boolean"/> 値です。
        /// true の場合は元のバイトストリームを破棄しません。
        /// false の場合は元のバイトストリームを破棄します。
        /// </param>
        /// <remarks>
        /// このコンストラクタを使用した場合、元になったバイトストリーム上でアクセス可能な開始位置は、コンストラクタ呼び出し時点での <code>baseStream.Position</code> となります。
        /// </remarks>
        public PartialRandomInputStream(IRandomInputByteStream<BASE_POSITION_T> baseStream, UInt64? size, POSITION_T zeroPositionValue, Boolean leaveOpen)
            : this(baseStream, baseStream.Position, size, zeroPositionValue, leaveOpen)
        {
        }

        /// <summary>
        /// 元になるバイトストリームとアクセス可能な範囲(開始位置と長さ)を使用して初期化するコンストラクタです。
        /// </summary>
        /// <param name="baseStream">
        /// 元になるバイトストリームを示す <see cref="ISequentialInputByteStream"/> オブジェクトです。
        /// </param>
        /// <param name="offset">
        /// 元になるバイトストリームで、アクセスが許可される最初の位置を示す <typeparamref name="BASE_POSITION_T"/> 値です。
        /// </param>
        /// <param name="size">
        /// 元になるバイトストリームで、<paramref name="offset"/> で与えられた位置からアクセス可能なバイト数を示す <see cref="UInt64"/> 値です。
        /// </param>
        /// <param name="leaveOpen">
        /// コンストラクタによって初期化されたオブジェクトが破棄されるときに元になるバイトストリームもともに破棄するかどうかを示す <see cref="Boolean"/> 値です。
        /// true の場合は元のバイトストリームを破棄しません。
        /// false の場合は元のバイトストリームを破棄します。
        /// </param>
        public PartialRandomInputStream(IRandomInputByteStream<BASE_POSITION_T> baseStream, BASE_POSITION_T offset, UInt64? size, POSITION_T zeroPositionValue, Boolean leaveOpen)
            : base(baseStream, zeroPositionValue, leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                _baseStream = baseStream;

                if (offset.CompareTo(baseStream.EndOfThisStream) > 0)
                    throw new ArgumentOutOfRangeException(nameof(offset));

                if (size is not null)
                {
                    if (checked(offset + size.Value).CompareTo(baseStream.EndOfThisStream) > 0)
                        throw new ArgumentOutOfRangeException(nameof(offset));

                    _regionOnBaseStream = (offset, size.Value, offset + size.Value);
                }
                else
                {
                    _regionOnBaseStream = (offset, baseStream.EndOfThisStream - offset, baseStream.EndOfThisStream);
                }

                _endOfStream = checked(zeroPositionValue + _regionOnBaseStream.length);

                baseStream.Seek(_regionOnBaseStream.startOfRegion);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        protected override POSITION_T PositionCore
        {
            get
            {
                AdjustBaseStreamPosition();
                if (_baseStream.Position.CompareTo(_regionOnBaseStream.startOfRegion) >= 0)
                {
                    return checked(StartOfThisStreamCore + (_baseStream.Position - _regionOnBaseStream.startOfRegion));
                }
                else
                {
                    _baseStream.Seek(_regionOnBaseStream.startOfRegion);
                    return StartOfThisStreamCore;
                }
            }
        }

        protected override POSITION_T EndOfThisStreamCore => checked(StartOfThisStream + _regionOnBaseStream.length);
        protected override UInt64 LengthCore => _regionOnBaseStream.length;

        protected override void SeekCore(POSITION_T position)
        {
            if (position.CompareTo(_endOfStream) > 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            _baseStream.Seek(checked(_regionOnBaseStream.startOfRegion + (position - StartOfThisStreamCore)));
        }

        protected override Int32 ReadCore(Span<Byte> buffer)
        {
            AdjustBaseStreamPosition();
            var actualCount = GetReadCount(buffer.Length);
            if (actualCount <= 0)
                return 0;
            return _baseStream.Read(buffer[..actualCount]);
        }

        protected override Task<Int32> ReadAsyncCore(Memory<Byte> buffer, CancellationToken cancellationToken)
        {
            AdjustBaseStreamPosition();
            var actualCount = GetReadCount(buffer.Length);
            if (actualCount <= 0)
                return Task.FromResult(0);
            return _baseStream.ReadAsync(buffer[..actualCount], cancellationToken);
        }

        private void AdjustBaseStreamPosition()
        {
            if (_baseStream.Position.CompareTo(_regionOnBaseStream.startOfRegion) < 0)
                _baseStream.Seek(_regionOnBaseStream.startOfRegion);
            if (_baseStream.Position.CompareTo(_regionOnBaseStream.endOfRegion) > 0)
                _baseStream.Seek(_regionOnBaseStream.endOfRegion);
        }

        private Int32 GetReadCount(Int32 bufferLength)
            => (Int32)checked(_regionOnBaseStream.endOfRegion - _baseStream.Position).Minimum(checked((UInt64)bufferLength));
    }
}
