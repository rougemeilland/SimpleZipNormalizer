using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    class SequentialInputBitStreamByByteStream
        : SequentialInputBitStreamBy
    {
        private readonly IInputByteStream<UInt64> _baseStream;
        private readonly Boolean _leaveOpen;

        private Boolean _isDisposed;

        public SequentialInputBitStreamByByteStream(IInputByteStream<UInt64> baseStream, BitPackingDirection bitPackingDirection, Boolean leaveOpen)
            : base(bitPackingDirection)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                _isDisposed = false;
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

        protected override Byte? GetNextByte()
            => _baseStream.ReadByteOrNull();

        protected override Task<Byte?> GetNextByteAsync(CancellationToken cancellationToken)
            => _baseStream.ReadByteOrNullAsync(cancellationToken);

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

        protected async override ValueTask DisposeAsyncCore()
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
