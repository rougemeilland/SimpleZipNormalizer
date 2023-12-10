using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    public abstract class SequentialOutputByteStreamFilter
        : SequentialOutputByteStream
    {
        private readonly ISequentialOutputByteStream _baseStream;
        private readonly Boolean _leaveOpen;
        private Boolean _isDisposed;

        protected SequentialOutputByteStreamFilter(ISequentialOutputByteStream baseStream, Boolean leaveOpen)
        {
            _baseStream = baseStream;
            _leaveOpen = leaveOpen;
            _isDisposed = false;
        }

        protected override Int32 WriteCore(ReadOnlySpan<Byte> buffer)
            => _baseStream.Write(buffer);

        protected override Task<Int32> WriteAsyncCore(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
            => _baseStream.WriteAsync(buffer, cancellationToken);

        protected override void FlushCore()
            => _baseStream.Flush();

        protected override Task FlushAsyncCore(CancellationToken cancellationToken = default)
            => _baseStream.FlushAsync(cancellationToken);

        protected override void Dispose(Boolean disposing)
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

            base.Dispose(disposing);
        }

        protected override async Task DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                if (!_leaveOpen)
                    await _baseStream.DisposeAsync().ConfigureAwait(false);
                _isDisposed = true;
            }

            await base.DisposeAsyncCore().ConfigureAwait(false);
        }
    }
}
