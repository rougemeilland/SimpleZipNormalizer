using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class BranchOutputStream
        : SequentialOutputByteStream
    {
        private readonly ISequentialOutputByteStream _baseStream1;
        private readonly ISequentialOutputByteStream _baseStream2;
        private readonly Boolean _leaveOpen;
        private Boolean _isDisposed;

        public BranchOutputStream(ISequentialOutputByteStream baseStream1, ISequentialOutputByteStream baseStream2, Boolean leaveOpen)
        {
            _baseStream1 = baseStream1;
            _baseStream2 = baseStream2;
            _leaveOpen = leaveOpen;
            _isDisposed = false;
        }

        protected override Int32 WriteCore(ReadOnlySpan<Byte> buffer)
        {
            _baseStream1.WriteBytes(buffer);
            _baseStream2.WriteBytes(buffer);
            return buffer.Length;
        }

        protected override async Task<Int32> WriteAsyncCore(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
        {
            await _baseStream1.WriteBytesAsync(buffer, cancellationToken).ConfigureAwait(false);
            await _baseStream2.WriteBytesAsync(buffer, cancellationToken).ConfigureAwait(false);
            return buffer.Length;
        }

        protected override void FlushCore()
        {
            _baseStream1.Flush();
            _baseStream2.Flush();
        }

        protected override async Task FlushAsyncCore(CancellationToken cancellationToken = default)
        {
            await _baseStream1.FlushAsync(cancellationToken).ConfigureAwait(false);
            await _baseStream2.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        protected override void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (!_leaveOpen)
                    {
                        _baseStream1.Dispose();
                        _baseStream2.Dispose();
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
                if (!_leaveOpen)
                {
                    await _baseStream1.DisposeAsync().ConfigureAwait(false);
                    await _baseStream2.DisposeAsync().ConfigureAwait(false);
                }

                _isDisposed = true;
            }

            await base.DisposeAsyncCore().ConfigureAwait(false);
        }
    }
}
