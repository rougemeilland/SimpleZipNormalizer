using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class BufferedRandomInputStream<POSITION_T>
        : RandomInputByteStreamFilter<POSITION_T, POSITION_T>
        where POSITION_T : struct, IComparable<POSITION_T>, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>, ISubtractionOperators<POSITION_T, UInt64, POSITION_T>
    {
        private readonly IRandomInputByteStream<POSITION_T> _baseStream;
        private readonly ReadOnlyBytesCache<POSITION_T> _cache;

        public BufferedRandomInputStream(IRandomInputByteStream<POSITION_T> baseStream, Boolean leaveOpen)
            : this(baseStream, ReadOnlyBytesCache<POSITION_T>.DEFAULT_BUFFER_SIZE, leaveOpen)
        {
        }

        public BufferedRandomInputStream(IRandomInputByteStream<POSITION_T> baseStream, Int32 bufferSize, Boolean leaveOpen)
            : base(baseStream, baseStream.StartOfThisStream, leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (bufferSize <= 0)
                    throw new ArgumentOutOfRangeException(nameof(bufferSize));

                _baseStream = baseStream;
                _cache = new ReadOnlyBytesCache<POSITION_T>(bufferSize, baseStream.Position);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        protected override POSITION_T PositionCore
            => _cache.Position;

        protected override void SeekCore(POSITION_T position)
            => _cache.Seek(position, p => _baseStream.Seek(p));

        protected override Int32 ReadCore(Span<Byte> buffer)
            => _cache.Read(
                buffer,
                b =>
                {
                    var position = _baseStream.Position;
                    var length = _baseStream.Read(b.Span);
                    return (position, length);
                });

        protected override Task<Int32> ReadAsyncCore(Memory<Byte> buffer, CancellationToken cancellationToken)
            => _cache.ReadAsync(
                buffer,
                async b =>
                {
                    var position = _baseStream.Position;
                    var length = await _baseStream.ReadAsync(b, cancellationToken).ConfigureAwait(false);
                    return (position, length);
                });
    }
}
