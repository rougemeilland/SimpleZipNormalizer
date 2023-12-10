using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class BufferedSequentialInputStream
        : SequentialInputByteStreamFilter
    {
        private readonly ISequentialInputByteStream _baseStream;
        private readonly ReadOnlyBytesCache _cache;

        public BufferedSequentialInputStream(ISequentialInputByteStream baseStream, Boolean leaveOpen)
            : this(baseStream, ReadOnlyBytesCache.DEFAULT_BUFFER_SIZE, leaveOpen)
        {
        }

        public BufferedSequentialInputStream(ISequentialInputByteStream baseStream, Int32 bufferSize, Boolean leaveOpen)
            : base(baseStream, leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (bufferSize <= 0)
                    throw new ArgumentOutOfRangeException(nameof(bufferSize));

                _baseStream = baseStream;
                _cache = new ReadOnlyBytesCache(bufferSize);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        protected override Int32 ReadCore(Span<Byte> buffer)
            => _cache.Read(buffer, b => _baseStream.Read(b.Span));

        protected override Task<Int32> ReadAsyncCore(Memory<Byte> buffer, CancellationToken cancellationToken)
            => _cache.ReadAsync(buffer, b => _baseStream.ReadAsync(b, cancellationToken));
    }
}
