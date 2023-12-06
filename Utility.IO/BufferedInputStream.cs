using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    internal abstract class BufferedInputStream<POSITION_T, UNSIGNED_OFFSET_T>
        : IInputByteStream<POSITION_T>
        where POSITION_T : struct, IAdditionOperators<POSITION_T, UNSIGNED_OFFSET_T, POSITION_T>
        where UNSIGNED_OFFSET_T : struct, IUnsignedNumber<UNSIGNED_OFFSET_T>, IMinMaxValue<UNSIGNED_OFFSET_T>, IAdditionOperators<UNSIGNED_OFFSET_T, UNSIGNED_OFFSET_T, UNSIGNED_OFFSET_T>
    {
        private const Int32 _MAXIMUM_BUFFER_SIZE = 1024 * 1024;
        private const Int32 _DEFAULT_BUFFER_SIZE = 80 * 1024;
        private const Int32 _MINIMUM_BUFFER_SIZE = 4 * 1024;

        private readonly IBasicInputByteStream _baseStream;
        private readonly Boolean _leaveOpen;
        private readonly Int32 _bufferSize;
        private readonly Byte[] _internalBuffer;
        private Int32 _internalBufferCount;
        private Int32 _internalBufferIndex;
        private Boolean _isEndOfStream;
        private Boolean _isEndOfBaseStream;

        public BufferedInputStream(IBasicInputByteStream baseStream, Boolean leaveOpen)
            : this(baseStream, _DEFAULT_BUFFER_SIZE, leaveOpen)
        {
        }

        public BufferedInputStream(IBasicInputByteStream baseStream, Int32 bufferSize, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (bufferSize < 0)
                    throw new ArgumentOutOfRangeException(nameof(bufferSize));

                _baseStream = baseStream;
                _bufferSize = bufferSize.Minimum(_MAXIMUM_BUFFER_SIZE).Maximum(_MINIMUM_BUFFER_SIZE);
                _leaveOpen = leaveOpen;
                IsDisposed = false;
                CurrentPosition = UNSIGNED_OFFSET_T.MinValue;
                _internalBuffer = new Byte[_bufferSize];
                _internalBufferCount = 0;
                _internalBufferIndex = 0;
                _isEndOfStream = false;
                _isEndOfBaseStream = false;

            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public POSITION_T Position
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return checked(ZeroPositionValue + CurrentPosition);
            }
        }

        public Int32 Read(Span<Byte> buffer)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_isEndOfStream)
                return 0;

            var length = InternalRead(buffer);
            UpdateCurrentPosition(length);
            return length;
        }

        public async Task<Int32> ReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_isEndOfStream)
                return 0;

            var length = await InternalReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            UpdateCurrentPosition(length);
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

        protected Boolean IsDisposed { get; private set; }
        protected UNSIGNED_OFFSET_T CurrentPosition { get; set; }
        protected abstract POSITION_T ZeroPositionValue { get; }
        protected abstract UNSIGNED_OFFSET_T FromInt32ToOffset(Int32 offset);

        protected void ClearCache()
        {
            _internalBufferCount = 0;
            _internalBufferIndex = 0;
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (!_leaveOpen)
                        _baseStream.Dispose();
                }

                IsDisposed = true;
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!IsDisposed)
            {
                if (!_leaveOpen)
                    await _baseStream.DisposeAsync().ConfigureAwait(false);
                IsDisposed = true;
            }
        }

        private Int32 InternalRead(Span<Byte> buffer)
        {
            if (_isEndOfBaseStream)
                return 0;

            if (IsBufferEmpty)
            {
                if (!SetReadLength(_baseStream.Read(_internalBuffer.AsSpan())))
                    return 0;
            }

            return ReadFromBuffer(buffer);
        }

        private async Task<Int32> InternalReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken)
        {
            if (_isEndOfBaseStream)
                return 0;

            if (IsBufferEmpty)
            {
                if (!SetReadLength(await _baseStream.ReadAsync(_internalBuffer.AsMemory(), cancellationToken).ConfigureAwait(false)))
                    return 0;
            }

            return ReadFromBuffer(buffer.Span);
        }

        private void UpdateCurrentPosition(Int32 length)
        {
            if (length <= 0)
                _isEndOfStream = true;
            else
                CurrentPosition = checked(CurrentPosition + FromInt32ToOffset(length));
        }

        private Boolean IsBufferEmpty
            => _internalBufferIndex >= _internalBufferCount;

        private Boolean SetReadLength(Int32 readLength)
        {
            _internalBufferCount = readLength;
            _internalBufferIndex = 0;
            if (readLength <= 0)
            {
                _isEndOfBaseStream = true;
                return false;
            }

            return true;
        }

        private Int32 ReadFromBuffer(Span<Byte> destination)
        {
            var copyCount = (_internalBufferCount - _internalBufferIndex).Minimum(destination.Length);
            _internalBuffer.AsSpan(_internalBufferIndex, copyCount).CopyTo(destination[..copyCount]);
            _internalBufferIndex += copyCount;
            return copyCount;
        }
    }
}
