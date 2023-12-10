using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class SequentialInputByteStreamByDotNetStream
        : SequentialInputByteStream
    {
        private readonly Stream _baseStream;
        private readonly Boolean _leaveOpen;

        private Boolean _isDisposed;

        public SequentialInputByteStreamByDotNetStream(Stream baseStream, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (!baseStream.CanRead)
                    throw new NotSupportedException();

                _baseStream = baseStream;
                _leaveOpen = leaveOpen;
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        protected override Int32 ReadCore(Span<Byte> buffer)
            => _baseStream.Read(buffer);

        protected override async Task<Int32> ReadAsyncCore(Memory<Byte> buffer, CancellationToken cancellationToken)
            => await _baseStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

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
