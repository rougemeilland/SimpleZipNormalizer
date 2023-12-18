using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class BufferedRandomOutputStream<POSITION_T>
        : RandomOutputByteStreamFilter<POSITION_T, POSITION_T>
        where POSITION_T : struct, IComparable<POSITION_T>, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>
    {
        private readonly IRandomOutputByteStream<POSITION_T> _baseStream;
        private readonly WriteOnlyBytesCache<POSITION_T> _cache;
        private Boolean _isDisposed;

        public BufferedRandomOutputStream(IRandomOutputByteStream<POSITION_T> baseStream, Boolean leaveOpen)
            : this(baseStream, WriteOnlyBytesCache<POSITION_T>.DEFAULT_BUFFER_SIZE, leaveOpen)
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
                _cache = new WriteOnlyBytesCache<POSITION_T>(bufferSize, baseStream.Position);
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
            => _cache.Position;

        protected override UInt64 LengthCore
        {
            get => _baseStream.Length.Maximum(_cache.EndOfCache - _baseStream.StartOfThisStream);
            set
            {
                FlushCore();
                _baseStream.Length = value;
            }
        }

        protected override void SeekCore(POSITION_T position)
            => _cache.Seek(
                position,
                b =>
                {
                    _baseStream.WriteBytes(b.Span);
                    return _baseStream.Position;
                },
                p => _baseStream.Seek(p));

        protected override Int32 WriteCore(ReadOnlySpan<Byte> buffer)
            => _cache.Write(
                buffer,
                b =>
                {
                    _baseStream.WriteBytes(b.Span);
                    return _baseStream.Position;
                });

        protected override Task<Int32> WriteAsyncCore(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
            => _cache.WriteAsync(
                buffer,
                async b =>
                {
                    await _baseStream.WriteBytesAsync(b, cancellationToken).ConfigureAwait(false);
                    return _baseStream.Position;
                });

        protected override void FlushCore()
        {
            _cache.Flush(b =>
            {
                _baseStream.WriteBytes(b.Span);
                return _baseStream.Position;
            });
            _baseStream.Flush();
        }

        protected override async Task FlushAsyncCore(CancellationToken cancellationToken = default)
        {
            await _cache.FlushAsync(async b =>
            {
                await _baseStream.WriteBytesAsync(b, cancellationToken).ConfigureAwait(false);
                return _baseStream.Position;
            }).ConfigureAwait(false);
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
