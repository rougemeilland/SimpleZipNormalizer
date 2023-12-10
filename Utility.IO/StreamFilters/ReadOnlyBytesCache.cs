using System;
using System.Threading.Tasks;
using Utility;

namespace Utility.IO.StreamFilters
{
    internal class ReadOnlyBytesCache
    {

        public const Int32 MAXIMUM_BUFFER_SIZE = 1024 * 1024;
        public const Int32 DEFAULT_BUFFER_SIZE = 80 * 1024;
        public const Int32 MINIMUM_BUFFER_SIZE = 4 * 1024;

        private readonly Byte[] _internalBuffer;

        private Int32 _internalBufferCount;
        private Int32 _internalBufferIndex;
        private Boolean _isEndOfBaseStream;

        public ReadOnlyBytesCache(Int32 bufferSize)
        {
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            _internalBuffer = new Byte[(bufferSize.Minimum(MAXIMUM_BUFFER_SIZE).Maximum(MINIMUM_BUFFER_SIZE))];
            _internalBufferCount = 0;
            _internalBufferIndex = 0;
            _isEndOfBaseStream = false;
        }

        public UInt64 CachedDataLength => checked((UInt64)(_internalBufferCount - _internalBufferIndex));

        public Int32 Read(Span<Byte> destination, Func<Memory<Byte>, Int32> baseReader)
        {
            if (_internalBufferIndex >= _internalBufferCount)
            {
                if (_isEndOfBaseStream)
                    return 0;

                _internalBufferCount = baseReader(_internalBuffer);
                _internalBufferIndex = 0;
                if (_internalBufferCount <= 0)
                {
                    _isEndOfBaseStream = true;
                    return 0;
                }
            }

            return ReadFromBuffer(destination);
        }

        public async Task<Int32> ReadAsync(Memory<Byte> destination, Func<Memory<Byte>, Task<Int32>> baseReader)
        {
            if (_internalBufferIndex >= _internalBufferCount)
            {
                if (_isEndOfBaseStream)
                    return 0;

                _internalBufferCount = await baseReader(_internalBuffer).ConfigureAwait(false);
                _internalBufferIndex = 0;
                if (_internalBufferCount <= 0)
                {
                    _isEndOfBaseStream = true;
                    return 0;
                }
            }

            return ReadFromBuffer(destination.Span);
        }

        public void Clear()
        {
            _internalBufferCount = 0;
            _internalBufferIndex = 0;
            _isEndOfBaseStream = false;
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
