using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    public abstract class SequentialInputByteStreamFilter
        : SequentialInputByteStream
    {
        private readonly ISequentialInputByteStream _baseStream;
        private readonly Boolean _leaveOpen;
        private Boolean _isDisposed;

        protected SequentialInputByteStreamFilter(ISequentialInputByteStream baseStream, Boolean leaveOpen)
        {
            _baseStream = baseStream;
            _leaveOpen = leaveOpen;
            _isDisposed = false;
        }

        protected override Int32 ReadCore(Span<Byte> buffer)
            => _baseStream.Read(buffer);

        protected override Task<Int32> ReadAsyncCore(Memory<Byte> buffer, CancellationToken cancellationToken)
            => _baseStream.ReadAsync(buffer, cancellationToken);

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
