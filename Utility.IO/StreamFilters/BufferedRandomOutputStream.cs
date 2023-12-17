using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class BufferedRandomOutputStream<POSITION_T>
        : RandomOutputByteStreamFilter<POSITION_T, POSITION_T>
        where POSITION_T : struct, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>
    {
        private readonly IRandomOutputByteStream<POSITION_T> _baseStream;
        private readonly WriteOnlyBytesCache _cache;
        private Boolean _isDisposed;

        public BufferedRandomOutputStream(IRandomOutputByteStream<POSITION_T> baseStream, Boolean leaveOpen)
            : this(baseStream, WriteOnlyBytesCache.DEFAULT_BUFFER_SIZE, leaveOpen)
        {
        }

        public BufferedRandomOutputStream(IRandomOutputByteStream<POSITION_T> baseStream, Int32 bufferSize, Boolean leaveOpen)
            : base(baseStream, baseStream.StartOfThisStream, leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (bufferSize <= 0)
                    throw new ArgumentOutOfRangeException(nameof(bufferSize));

                _baseStream = baseStream;
                _cache = new WriteOnlyBytesCache(bufferSize);
                _isDisposed = false;
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        protected override POSITION_T PositionCore
            => _baseStream.Position + _cache.CachedDataLength;

        protected override UInt64 LengthCore
        {
            get => _baseStream.Length;
            set
            {
                FlushCore();
                _baseStream.Length = value;
            }
        }

        protected override void SeekCore(POSITION_T position)
        {
            FlushCore();
            _baseStream.Seek(position);
        }

        protected override Int32 WriteCore(ReadOnlySpan<Byte> buffer)
            => _cache.Write(buffer, b => _baseStream.Write(b.Span));

        protected override Task<Int32> WriteAsyncCore(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
            => _cache.WriteAsync(buffer, b => _baseStream.WriteAsync(b, cancellationToken));

        protected override void FlushCore()
        {
            _cache.Flush(b => _baseStream.WriteBytes(b.Span));
            _baseStream.Flush();
        }

        protected override async Task FlushAsyncCore(CancellationToken cancellationToken = default)
        {
            await _cache.FlushAsync(b => _baseStream.WriteBytesAsync(b, cancellationToken)).ConfigureAwait(false);
            await _baseStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        protected override void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    try
                    {
                        FlushCore();
                    }
                    catch (Exception)
                    {
                    }
                }

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        protected override async Task DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                try
                {
                    await FlushAsyncCore(default).ConfigureAwait(false);
                }
                catch (Exception)
                {
                }

                _isDisposed = true;
            }

            await base.DisposeAsyncCore().ConfigureAwait(false);
        }
    }
}
