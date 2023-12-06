using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    public abstract class PartialRandomOutputStream<POSITION_T, BASE_POSITION_T, UNSIGNED_OFFSET_T>
        : IRandomOutputByteStream<POSITION_T, UNSIGNED_OFFSET_T>
        where POSITION_T : struct, IAdditionOperators<POSITION_T, UNSIGNED_OFFSET_T, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UNSIGNED_OFFSET_T>
        where BASE_POSITION_T : struct, IComparable<BASE_POSITION_T>, IEquatable<BASE_POSITION_T>, IAdditionOperators<BASE_POSITION_T, UNSIGNED_OFFSET_T, BASE_POSITION_T>, ISubtractionOperators<BASE_POSITION_T, BASE_POSITION_T, UNSIGNED_OFFSET_T>
        where UNSIGNED_OFFSET_T : struct, IComparable<UNSIGNED_OFFSET_T>, IUnsignedNumber<UNSIGNED_OFFSET_T>, IMinMaxValue<UNSIGNED_OFFSET_T>, IAdditionOperators<UNSIGNED_OFFSET_T, UNSIGNED_OFFSET_T, UNSIGNED_OFFSET_T>
    {
        private readonly IRandomOutputByteStream<BASE_POSITION_T, UNSIGNED_OFFSET_T> _baseStream;
        private readonly BASE_POSITION_T _startOfStream;
        private readonly BASE_POSITION_T? _limitOfStream;
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
        /// アクセス可能な長さの制限はありません。
        /// </remarks>
        public PartialRandomOutputStream(IRandomOutputByteStream<BASE_POSITION_T, UNSIGNED_OFFSET_T> baseStream, Boolean leaveOpen)
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
        public PartialRandomOutputStream(IRandomOutputByteStream<BASE_POSITION_T, UNSIGNED_OFFSET_T> baseStream, UNSIGNED_OFFSET_T size, Boolean leaveOpen)
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
        public PartialRandomOutputStream(IRandomOutputByteStream<BASE_POSITION_T, UNSIGNED_OFFSET_T> baseStream, BASE_POSITION_T offset, UNSIGNED_OFFSET_T size, Boolean leaveOpen)
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
        /// 元になるバイトストリームで、<paramref name="offset"/> で与えられた位置からアクセス可能な長さのバイト数を示す <see cref="UNSIGNED_OFFSET_T?"/> 値です。
        /// nullの場合は、アクセス可能な長さの制限はありません。
        /// </param>
        /// <param name="leaveOpen">
        /// コンストラクタによって初期化されたオブジェクトが破棄されるときに元になるバイトストリームもともに破棄するかどうかを示す <see cref="Boolean"/> 値です。
        /// true の場合は元のバイトストリームを破棄しません。
        /// false の場合は元のバイトストリームを破棄します。
        /// </param>
        public PartialRandomOutputStream(IRandomOutputByteStream<BASE_POSITION_T, UNSIGNED_OFFSET_T> baseStream, BASE_POSITION_T? offset, UNSIGNED_OFFSET_T? size, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                _isDisposed = false;
                _baseStream = baseStream;
                _startOfStream = offset ?? _baseStream.Position;
                _limitOfStream =
                    size is not null
                    ? checked(_startOfStream + size.Value)
                    : null;

                _leaveOpen = leaveOpen;
                if (!_baseStream.Position.Equals(_startOfStream))
                    _baseStream.Seek(_startOfStream);
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

                var endOfStream = checked(ZeroBasePositionValue + _baseStream.Length);
                if (_limitOfStream is not null)
                    endOfStream = endOfStream.Minimum(_limitOfStream.Value);
                return checked(endOfStream - _startOfStream);
            }

            set
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                _baseStream.Length = checked(_startOfStream - ZeroBasePositionValue + value);
            }
        }

        public POSITION_T Position
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);
                if (_baseStream.Position.CompareTo(_startOfStream) < 0)
                    throw new IOException();

                return GetCurrentPosition();
            }
        }

        public void Seek(POSITION_T offset)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            _baseStream.Seek(checked(_startOfStream + (offset - ZeroPositionValue)));
        }

        public Int32 Write(ReadOnlySpan<Byte> buffer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (_baseStream.Position.CompareTo(_startOfStream) < 0)
                throw new IOException();

            return _baseStream.Write(buffer[..GetWriteCount(buffer.Length)]);
        }

        public Task<Int32> WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (_baseStream.Position.CompareTo(_startOfStream) < 0)
                throw new IOException();

            return _baseStream.WriteAsync(buffer[..GetWriteCount(buffer.Length)], cancellationToken);
        }

        public void Flush()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            _baseStream.Flush();
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return _baseStream.FlushAsync(cancellationToken);
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

        protected abstract POSITION_T ZeroPositionValue { get; }
        protected abstract BASE_POSITION_T ZeroBasePositionValue { get; }
        protected abstract Int32 FromOffsetToInt32(UNSIGNED_OFFSET_T offset);

        protected void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    try
                    {
                        _baseStream.Flush();
                    }
                    catch (Exception)
                    {
                    }

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
                try
                {
                    await _baseStream.FlushAsync(default).ConfigureAwait(false);
                }
                catch (Exception)
                {
                }

                if (!_leaveOpen)
                    await _baseStream.DisposeAsync().ConfigureAwait(false);

                _isDisposed = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Int32 GetWriteCount(Int32 bufferLength)
        {
            if (bufferLength <= 0)
            {
                return 0;
            }
            else if (_limitOfStream is null)
            {
                return bufferLength;
            }
            else
            {
                var distance = checked(_limitOfStream.Value - _baseStream.Position);
                if (distance.CompareTo(UNSIGNED_OFFSET_T.MinValue) <= 0)
                    throw new IOException("Can not write any more.");

                return FromOffsetToInt32(distance).Minimum(bufferLength);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private POSITION_T GetCurrentPosition()
            => checked(ZeroPositionValue + (_baseStream.Position - _startOfStream));
    }
}
