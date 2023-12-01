using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    /// <summary>
    /// バイトストリームの部分的な範囲のアクセスのみを可能にするクラスです。
    /// </summary>
    public abstract class PartialInputStream<POSITION_T, BASE_POSITION_T>
        : IInputByteStream<POSITION_T>
    {
        private readonly IInputByteStream<BASE_POSITION_T> _baseStream;
        private readonly UInt64? _size;
        private readonly Boolean _leaveOpen;

        private Boolean _isDisposed;
        private UInt64 _position;

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
        public PartialInputStream(IInputByteStream<BASE_POSITION_T> baseStream, Boolean leaveOpen)
            : this(baseStream, null, leaveOpen)
        {
        }

        /// <summary>
        /// 元になるバイトストリームとアクセス可能な範囲(長さ)を使用して初期化するコンストラクタです。
        /// </summary>
        /// <param name="baseStream">
        /// 元になるバイトストリームを示す <see cref="IInputByteStream{BASE_POSITION_T}">IInputByteStream&lt;<typeparamref name="BASE_POSITION_T"/>&gt;</see> オブジェクトです。
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
        public PartialInputStream(IInputByteStream<BASE_POSITION_T> baseStream, UInt64 size, Boolean leaveOpen)
            : this(baseStream, (UInt64?)size, leaveOpen)
        {
        }

        /// <summary>
        /// 元になるバイトストリームとアクセス可能な範囲(開始位置と長さ)を使用して初期化するコンストラクタです。
        /// </summary>
        /// <param name="baseStream">
        /// 元になるバイトストリームを示す <see cref="IInputByteStream{BASE_POSITION_T}">IInputByteStream&lt;<typeparamref name="BASE_POSITION_T"/>&gt;</see> オブジェクトです。
        /// </param>
        /// <param name="size">
        /// 元になるバイトストリームで、最初の位置からアクセス可能なバイト数を示す <see cref="UInt64?"/> 値です。
        /// nullの場合は、元になるバイトストリームの終端までアクセスが可能になります。
        /// </param>
        /// <param name="leaveOpen">
        /// コンストラクタによって初期化されたオブジェクトが破棄されるときに元になるバイトストリームもともに破棄するかどうかを示す <see cref="Boolean"/> 値です。
        /// true の場合は元のバイトストリームを破棄しません。
        /// false の場合は元のバイトストリームを破棄します。
        /// </param>
        public PartialInputStream(IInputByteStream<BASE_POSITION_T> baseStream, UInt64? size, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                _isDisposed = false;
                _baseStream = baseStream;
                _size = size;
                _leaveOpen = leaveOpen;
                _position = 0;
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public virtual POSITION_T Position
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return AddPosition(ZeroPositionValue, _position);
            }
        }

        public virtual Int32 Read(Span<Byte> buffer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            var actualCount = GetReadCount(buffer.Length);
            if (actualCount <= 0)
                return 0;
            var length = _baseStream.Read(buffer[..actualCount]);
            UpdatePosition(length);
            return length;
        }

        public virtual async Task<Int32> ReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            cancellationToken.ThrowIfCancellationRequested();

            var actualCount = GetReadCount(buffer.Length);
            if (actualCount <= 0)
                return 0;
            var length = await _baseStream.ReadAsync(buffer[..actualCount], cancellationToken).ConfigureAwait(false);
            UpdatePosition(length);
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

        // 以下のメソッドは .NET 7.0 以降では IAdditionOperators / ISubtractionOperators で代替可能で、しかもわかりやすくコード量も減る。
        protected abstract POSITION_T ZeroPositionValue { get; }
        protected abstract POSITION_T AddPosition(POSITION_T x, UInt64 y);

        protected void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (!_leaveOpen)
                        _baseStream.Dispose();
                }

                _isDisposed = true;
            }
        }

        protected async Task DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                if (!_leaveOpen)
                    await _baseStream.DisposeAsync().ConfigureAwait(false);
                _isDisposed = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Int32 GetReadCount(Int32 bufferLength)
        {
            if (bufferLength <= 0)
                return 0;
            if (_size is null)
                return bufferLength;
            if (_position > _size.Value)
                throw new IOException("Size not match");
            if (_position == _size.Value)
                return 0;
            return checked((Int32)((UInt64)bufferLength).Minimum(_size.Value - _position));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdatePosition(Int32 length)
        {
            if (length > 0)
            {
                checked
                {
                    _position += (UInt32)length;
                }
            }
        }
    }
}
