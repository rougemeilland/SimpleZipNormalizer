using System;

namespace Utility
{
    public class ByteQueue
    {
        private const Int32 _DEFAULT_BUFFER_SIZE = 80 * 1024;

        private readonly Byte[] _internalBuffer;
        private Int32 _startOfDataInInternalBuffer;

        public ByteQueue(Int32 bufferSize = _DEFAULT_BUFFER_SIZE)
        {
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            _internalBuffer = new Byte[bufferSize];
            _startOfDataInInternalBuffer = 0;
            AvailableDataCount = 0;
            IsCompleted = false;
        }

        public Boolean IsCompleted { get; private set; }
        public Boolean IsEmpty => AvailableDataCount <= 0;
        public Boolean IsFull => AvailableDataCount >= _internalBuffer.Length;
        public Int32 AvailableDataCount { get; private set; }
        public Int32 FreeAreaCount => _internalBuffer.Length - AvailableDataCount;
        public Int32 BufferSize => _internalBuffer.Length;

        public Int32 Read(Span<Byte> buffer)
        {
            lock (this)
            {
                var actualCount =
                    buffer.Length
                    .Minimum(AvailableDataCount)
                    .Minimum(_internalBuffer.Length - _startOfDataInInternalBuffer);
                if (actualCount <= 0)
                {
                    if (buffer.Length > 0 && !IsCompleted)
                        throw new InvalidOperationException("Tried to read even though the buffer is empty.");

                    return 0;
                }

                _internalBuffer.AsSpan(_startOfDataInInternalBuffer, actualCount).CopyTo(buffer[..actualCount]);
                _startOfDataInInternalBuffer += actualCount;
                AvailableDataCount -= actualCount;
                if (_startOfDataInInternalBuffer >= _internalBuffer.Length)
                    _startOfDataInInternalBuffer = 0;
#if DEBUG
                if (!_startOfDataInInternalBuffer.InRange(0, _internalBuffer.Length))
                    throw new Exception();
                if (!AvailableDataCount.InRange(0, _internalBuffer.Length))
                    throw new Exception();
#endif
                return actualCount;
            }
        }

        public Int32 Write(ReadOnlySpan<Byte> buffer)
        {
            lock (this)
            {
                if (IsCompleted)
                    throw new InvalidOperationException("Can not write any more.");

                var actualCount =
                    buffer.Length
                    .Minimum(
                        AvailableDataCount >= _internalBuffer.Length - _startOfDataInInternalBuffer
                        ? _internalBuffer.Length - AvailableDataCount
                        : _internalBuffer.Length - AvailableDataCount - _startOfDataInInternalBuffer);
                var offsetInInputBuffer =
                    AvailableDataCount >= _internalBuffer.Length - _startOfDataInInternalBuffer
                    ? _startOfDataInInternalBuffer - (_internalBuffer.Length - AvailableDataCount)
                    : _startOfDataInInternalBuffer + AvailableDataCount;

                buffer[..actualCount].CopyTo(_internalBuffer.AsSpan(offsetInInputBuffer, actualCount));
                AvailableDataCount += actualCount;
                if (AvailableDataCount == _internalBuffer.Length)
                    AvailableDataCount = 0;
#if DEBUG
                if (!_startOfDataInInternalBuffer.InRange(0, _internalBuffer.Length))
                    throw new Exception();
                if (!AvailableDataCount.InRange(0, _internalBuffer.Length))
                    throw new Exception();
#endif
                return actualCount;
            }
        }

        public void Compete()
        {
            lock (this)
            {
                IsCompleted = true;
            }
        }
    }
}
