using System;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class PartialRandomOutputStream<POSITION_T, BASE_POSITION_T>
        : RandomOutputByteStreamFilter<POSITION_T, BASE_POSITION_T>
        where POSITION_T : struct, IComparable<POSITION_T>, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>
        where BASE_POSITION_T : struct, IComparable<BASE_POSITION_T>, IAdditionOperators<BASE_POSITION_T, UInt64, BASE_POSITION_T>, ISubtractionOperators<BASE_POSITION_T, BASE_POSITION_T, UInt64>
    {
        private readonly IRandomOutputByteStream<BASE_POSITION_T> _baseStream;
        private readonly (BASE_POSITION_T startOfRegion, UInt64? length, BASE_POSITION_T? endOfRegion) _regionOnBaseStream;

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
        public PartialRandomOutputStream(IRandomOutputByteStream<BASE_POSITION_T> baseStream, UInt64 size, POSITION_T startOfPosition, Boolean leaveOpen)
            : this(baseStream, baseStream.Position, size, startOfPosition, leaveOpen)
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
        public PartialRandomOutputStream(IRandomOutputByteStream<BASE_POSITION_T> baseStream, BASE_POSITION_T offset, UInt64? size, POSITION_T startOfPosition, Boolean leaveOpen)
            : base(baseStream, startOfPosition, leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                _baseStream = baseStream;
                _regionOnBaseStream =
                    size switch
                    {
                        not null => (offset, size.Value, offset + size.Value),
                        _ => (offset, null, null),
                    };
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
                return checked(StartOfThisStreamCore + (_baseStream.Position - _regionOnBaseStream.startOfRegion));
            }
        }

        protected override UInt64 LengthCore
        {
            get
            {
                // baseStream 上の アクセス開始位置から baseStream の終端までの長さを求める。
                var length = checked(_baseStream.Length - (_regionOnBaseStream.startOfRegion - _baseStream.StartOfThisStream));

                // アクセス可能な長さが制限されていれば、アクセス可能な長さと length の小さい方の値を返す。
                // アクセス範囲な長さが制限されていなければ、length を返す。
                return
                    _regionOnBaseStream.length is not null
                    ? length.Minimum(_regionOnBaseStream.length.Value)
                    : length;
            }

            set
            {
                if (_regionOnBaseStream.length is not null && value.CompareTo(_regionOnBaseStream.length.Value) > 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _baseStream.Length = checked(_regionOnBaseStream.startOfRegion + value - _baseStream.StartOfThisStream);
            }
        }

        protected override void SeekCore(POSITION_T position)
        {
            if (_regionOnBaseStream.length is not null && position.CompareTo(checked(StartOfThisStreamCore + _regionOnBaseStream.length.Value)) > 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            _baseStream.Seek(checked(_regionOnBaseStream.startOfRegion + (position - StartOfThisStreamCore)));
        }

        protected override Int32 WriteCore(ReadOnlySpan<Byte> buffer)
        {
            AdjustBaseStreamPosition();
            return _baseStream.Write(buffer[..GetWriteCount(buffer.Length)]);
        }

        protected override Task<Int32> WriteAsyncCore(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
        {
            AdjustBaseStreamPosition();
            return _baseStream.WriteAsync(buffer[..GetWriteCount(buffer.Length)], cancellationToken);
        }

        protected override void FlushCore()
            => _baseStream.Flush();

        protected override Task FlushAsyncCore(CancellationToken cancellationToken = default)
            => _baseStream.FlushAsync(cancellationToken);

        private void AdjustBaseStreamPosition()
        {
            if (_baseStream.Position.CompareTo(_regionOnBaseStream.startOfRegion) < 0)
                _baseStream.Seek(_regionOnBaseStream.startOfRegion);
            if (_regionOnBaseStream.endOfRegion is not null && _baseStream.Position.CompareTo(_regionOnBaseStream.endOfRegion.Value) > 0)
                _baseStream.Seek(_regionOnBaseStream.endOfRegion.Value);
        }

        private Int32 GetWriteCount(Int32 bufferLength)
        {
            if (bufferLength <= 0)
                return 0;
            else if (_regionOnBaseStream.endOfRegion is null)
                return bufferLength;
            else if (_baseStream.Position.CompareTo(_regionOnBaseStream.endOfRegion.Value) > 0)
                throw new IOException("Can not write any more.");
            else
                return (Int32)checked(_regionOnBaseStream.endOfRegion.Value - _baseStream.Position).Minimum(checked((UInt64)bufferLength));
        }
    }
}
