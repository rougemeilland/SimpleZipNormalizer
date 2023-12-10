using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class SequentialOutputByteStreamByDotNetStream
        : SequentialOutputByteStream
    {
        private readonly Stream _baseStream;
        private readonly Boolean _leaveOpne;

        private Boolean _isDisposed;

        public SequentialOutputByteStreamByDotNetStream(Stream baseStream, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (!baseStream.CanWrite)
                    throw new NotSupportedException();

                _baseStream = baseStream;
                _leaveOpne = leaveOpen;
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        protected override Int32 WriteCore(ReadOnlySpan<Byte> buffer)
        {
            _baseStream.Write(buffer);
            return buffer.Length;
        }

        protected override async Task<Int32> WriteAsyncCore(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
        {
            await _baseStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            return buffer.Length;
        }

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
                    if (!_leaveOpne)
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
                if (!_leaveOpne)
                    await _baseStream.DisposeAsync().ConfigureAwait(false);
                _isDisposed = true;
            }

            await base.DisposeAsyncCore().ConfigureAwait(false);
        }
    }
}
