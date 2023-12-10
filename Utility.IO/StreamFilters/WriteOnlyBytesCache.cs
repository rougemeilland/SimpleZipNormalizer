using System;
using System.Threading.Tasks;
using Utility;

namespace Utility.IO.StreamFilters
{
    internal class WriteOnlyBytesCache
    {
        public const Int32 MAXIMUM_BUFFER_SIZE = 1024 * 1024;
        public const Int32 DEFAULT_BUFFER_SIZE = 80 * 1024;
        public const Int32 MINIMUM_BUFFER_SIZE = 4 * 1024;

        private readonly Int32 _bufferSize;
        private readonly Byte[] _internalBuffer;
        private Int32 _cachedDataLength;

        public WriteOnlyBytesCache(Int32 bufferSize)
        {
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            _bufferSize = bufferSize.Minimum(MAXIMUM_BUFFER_SIZE).Maximum(MINIMUM_BUFFER_SIZE);
            _internalBuffer = new Byte[_bufferSize];
            _cachedDataLength = 0;
        }

        public UInt64 CachedDataLength => checked((UInt64)_cachedDataLength);

        public Int32 Write(ReadOnlySpan<Byte> buffer, Action<ReadOnlyMemory<Byte>> baseWriter)
        {
            if (_cachedDataLength >= _internalBuffer.Length)
            {
                baseWriter(_internalBuffer);
                _cachedDataLength = 0;
            }

            return WriteToCache(buffer);
        }

        public async Task<Int32> WriteAsync(ReadOnlyMemory<Byte> buffer, Func<ReadOnlyMemory<Byte>, Task> baseWriter)
        {
            if (_cachedDataLength >= _internalBuffer.Length)
            {
                await baseWriter(_internalBuffer).ConfigureAwait(false);
                _cachedDataLength = 0;
            }

            return WriteToCache(buffer.Span);
        }

        public void Flush(Action<ReadOnlyMemory<Byte>> baseWriter)
        {
            if (_cachedDataLength > 0)
            {
                baseWriter(_internalBuffer.Slice(0, _cachedDataLength));
                _cachedDataLength = 0;
            }
        }

        public async Task FlushAsync(Func<ReadOnlyMemory<Byte>, Task> baseWriter)
        {
            if (_cachedDataLength > 0)
            {
                await baseWriter(_internalBuffer.Slice(0, _cachedDataLength)).ConfigureAwait(false);
                _cachedDataLength = 0;
            }
        }

        private Int32 WriteToCache(ReadOnlySpan<Byte> buffer)
        {
            var actualCount = (_internalBuffer.Length - _cachedDataLength).Minimum(buffer.Length);
            buffer[..actualCount].CopyTo(_internalBuffer.AsSpan(_cachedDataLength, actualCount));
            _cachedDataLength += actualCount;
            return actualCount;
        }
    }
}
