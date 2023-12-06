using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    /// <summary>
    /// バイトストリームの部分的な範囲のアクセスのみを可能にするクラスです。
    /// </summary>
    public abstract class PartialRandomInputStream<POSITION_T, BASE_POSITION_T, UNSIGNED_OFFSET_T>
        : IRandomInputByteStream<POSITION_T, UNSIGNED_OFFSET_T>
        where POSITION_T : struct, IAdditionOperators<POSITION_T, UNSIGNED_OFFSET_T, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UNSIGNED_OFFSET_T>
        where BASE_POSITION_T : struct, IComparable<BASE_POSITION_T>, IEquatable<BASE_POSITION_T>, IAdditionOperators<BASE_POSITION_T, UNSIGNED_OFFSET_T, BASE_POSITION_T>, ISubtractionOperators<BASE_POSITION_T, BASE_POSITION_T, UNSIGNED_OFFSET_T>
        where UNSIGNED_OFFSET_T : struct, IUnsignedNumber<UNSIGNED_OFFSET_T>
    {
        private readonly BASE_POSITION_T _startOfStream;
        private readonly BASE_POSITION_T _endOfStream;
        private readonly Boolean _leaveOpen;

        private Boolean _isDisposed;

        /// <summary>
        /// 元になるバイトストリームを使用して初期化するコンストラクタです。
        /// </summary>
        /// <param name="baseStream">
        /// 元になるバイトストリームを示す <see cref="IInputByteStream{BASE_POSITION_T}">IInputByteStream&lt;<typeparamref name="BASE_POSITION_T"/>&gt;</see> オブジェクトです。
        /// </param>
        /// <param name="leaveOpen">
        /// コンストラクタによって初期化されたオブジェクトが破棄されるときに元になるバイトストリームもともに破棄するかどうかを示す <see cref="Boolean"/> 値です。
        /// true の場合は元のバイトストリームを破棄しません。
        /// false の場合は元のバイトストリームを破棄します。
        /// </param>
        /// <remarks>
        /// このコンストラクタを使用した場合、元になったバイトストリーム上でアクセス可能な開始位置は、コンストラクタ呼び出し時点での <code>baseStream.Position</code> となり、
        /// 元になったバイトストリームの終端までアクセス可能になります。
        /// </remarks>
        public PartialRandomInputStream(IRandomInputByteStream<BASE_POSITION_T, UNSIGNED_OFFSET_T> baseStream, Boolean leaveOpen)
            : this(baseStream, null, null, leaveOpen)
        {
        }

        /// <summary>
        /// 元になるバイトストリームとアクセス可能な範囲(長さ)を使用して初期化するコンストラクタです。
        /// </summary>
        /// <param name="baseStream">
        /// 元になるバイトストリームを示す <see cref="IInputByteStream{BASE_POSITION_T}">IInputByteStream&lt;<typeparamref name="BASE_POSITION_T"/>&gt;</see> オブジェクトです。
        /// </param>
        /// <param name="size">
        /// 元になるバイトストリームで、現在位置からアクセス可能なバイト数を示す <see cref="UNSIGNED_OFFSET_T"/> 値です。
        /// </param>
        /// <param name="leaveOpen">
        /// コンストラクタによって初期化されたオブジェクトが破棄されるときに元になるバイトストリームもともに破棄するかどうかを示す <see cref="Boolean"/> 値です。
        /// true の場合は元のバイトストリームを破棄しません。
        /// false の場合は元のバイトストリームを破棄します。
        /// </param>
        /// <remarks>
        /// このコンストラクタを使用した場合、元になったバイトストリーム上でアクセス可能な開始位置は、コンストラクタ呼び出し時点での <code>baseStream.Position</code> となります。
        /// </remarks>
        public PartialRandomInputStream(IRandomInputByteStream<BASE_POSITION_T, UNSIGNED_OFFSET_T> baseStream, UNSIGNED_OFFSET_T size, Boolean leaveOpen)
            : this(baseStream, null, size, leaveOpen)
        {
        }

        /// <summary>
        /// 元になるバイトストリームとアクセス可能な範囲(開始位置と長さ)を使用して初期化するコンストラクタです。
        /// </summary>
        /// <param name="baseStream">
        /// 元になるバイトストリームを示す <see cref="IInputByteStream{BASE_POSITION_T}">IInputByteStream&lt;<typeparamref name="BASE_POSITION_T"/>&gt;</see> オブジェクトです。
        /// </param>
        /// <param name="offset">
        /// 元になるバイトストリームで、アクセスが許可される最初の位置を示す <typeparamref name="BASE_POSITION_T"/> 値です。
        /// </param>
        /// <param name="size">
        /// 元になるバイトストリームで、<paramref name="offset"/> で与えられた位置からアクセス可能なバイト数を示す <see cref="UNSIGNED_OFFSET_T"/> 値です。
        /// </param>
        /// <param name="leaveOpen">
        /// コンストラクタによって初期化されたオブジェクトが破棄されるときに元になるバイトストリームもともに破棄するかどうかを示す <see cref="Boolean"/> 値です。
        /// true の場合は元のバイトストリームを破棄しません。
        /// false の場合は元のバイトストリームを破棄します。
        /// </param>
        public PartialRandomInputStream(IRandomInputByteStream<BASE_POSITION_T, UNSIGNED_OFFSET_T> baseStream, BASE_POSITION_T offset, UNSIGNED_OFFSET_T size, Boolean leaveOpen)
            : this(baseStream, (BASE_POSITION_T?)offset, size, leaveOpen)
        {
        }

        /// <summary>
        /// 元になるバイトストリームとアクセス可能な範囲(開始位置と長さ)を使用して初期化するコンストラクタです。
        /// </summary>
        /// <param name="baseStream">
        /// 元になるバイトストリームを示す <see cref="IInputByteStream{BASE_POSITION_T}">IInputByteStream&lt;<typeparamref name="BASE_POSITION_T"/>&gt;</see> オブジェクトです。
        /// </param>
        /// <param name="offset">
        /// 元になるバイトストリームで、アクセスが許可される最初の位置を示す <see cref="BASE_POSITION_T?"/> 値です。
        /// nullの場合は、元になるバイトストリームの現在位置 <code>baseStream.Position</code> が最初の位置とみなされます。
        /// </param>
        /// <param name="size">
        /// 元になるバイトストリームで、<paramref name="offset"/> で与えられた位置からアクセス可能なバイト数を示す <see cref="UNSIGNED_OFFSET_T?"/> 値です。
        /// nullの場合は、元になるバイトストリームの終端までアクセスが可能になります。
        /// </param>
        /// <param name="leaveOpen">
        /// コンストラクタによって初期化されたオブジェクトが破棄されるときに元になるバイトストリームもともに破棄するかどうかを示す <see cref="Boolean"/> 値です。
        /// true の場合は元のバイトストリームを破棄しません。
        /// false の場合は元のバイトストリームを破棄します。
        /// </param>
        public PartialRandomInputStream(IRandomInputByteStream<BASE_POSITION_T, UNSIGNED_OFFSET_T> baseStream, BASE_POSITION_T? offset, UNSIGNED_OFFSET_T? size, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                BaseStream = baseStream;
                _startOfStream = offset ?? BaseStream.Position;
                _endOfStream =
                    size is not null
                    ? _startOfStream + size.Value
                    : EndBasePositionValue;

                _isDisposed = false;

                _leaveOpen = leaveOpen;

                if (_startOfStream.CompareTo(EndBasePositionValue) > 0)
                    throw new ArgumentOutOfRangeException(nameof(offset));

                if (!BaseStream.Position.Equals(_startOfStream))
                    BaseStream.Seek(_startOfStream);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public UNSIGNED_OFFSET_T Length
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return checked(_endOfStream - _startOfStream);
            }

            set => throw new NotSupportedException();
        }

        public POSITION_T Position
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);
                if (BaseStream.Position.CompareTo(_startOfStream) < 0)
                    throw new IOException();

                return GetCurrentPosition();
            }
        }

        public void Seek(POSITION_T position)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            BaseStream.Seek(checked(_startOfStream + (position - ZeroPositionValue)));
        }

        public Int32 Read(Span<Byte> buffer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (BaseStream.Position.CompareTo(_startOfStream) < 0)
                throw new IOException();

            var actualCount = GetReadCount(buffer.Length);
            if (actualCount <= 0)
                return 0;
            var length = BaseStream.Read(buffer[..actualCount]);
            if (length <= 0 && actualCount > 0)
                throw new IOException("Stream length is not match");
            return length;
        }

        public async Task<Int32> ReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (BaseStream.Position.CompareTo(_startOfStream) < 0)
                throw new IOException();

            var actualCount = GetReadCount(buffer.Length);
            if (actualCount <= 0)
                return 0;
            var length = await BaseStream.ReadAsync(buffer[..actualCount], cancellationToken).ConfigureAwait(false);
            if (length <= 0 && actualCount > 0)
                throw new IOException("Stream length is not match");

            return length;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        protected IRandomInputByteStream<BASE_POSITION_T, UNSIGNED_OFFSET_T> BaseStream { get; private set; }
        protected abstract POSITION_T ZeroPositionValue { get; }
        protected abstract BASE_POSITION_T EndBasePositionValue { get; }
        protected abstract Int32 FromOffsetToInt32(UNSIGNED_OFFSET_T offset);

        protected void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (!_leaveOpen)
                        BaseStream.Dispose();
                }

                _isDisposed = true;
            }
        }

        protected async Task DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                if (!_leaveOpen)
                    await BaseStream.DisposeAsync().ConfigureAwait(false);

                _isDisposed = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private POSITION_T GetCurrentPosition()
            => checked(ZeroPositionValue + (BaseStream.Position - _startOfStream));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Int32 GetReadCount(Int32 bufferLength)
            => bufferLength
                .Maximum(0)
                .Minimum(FromOffsetToInt32(checked(_endOfStream - BaseStream.Position)));
    }
}
