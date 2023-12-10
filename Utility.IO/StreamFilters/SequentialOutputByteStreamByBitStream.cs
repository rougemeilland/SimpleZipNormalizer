using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class SequentialOutputByteStreamByBitStream
        : SequentialOutputByteStream
    {
        private readonly IOutputBitStream _baseStream;
        private readonly BitPackingDirection _bitPackingDirection;
        private readonly Boolean _leaveOpen;

        private Boolean _isDisposed;

        public SequentialOutputByteStreamByBitStream(IOutputBitStream baseStream, BitPackingDirection bitPackingDirection, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                _baseStream = baseStream;
                _bitPackingDirection = bitPackingDirection;
                _leaveOpen = leaveOpen;
                _isDisposed = false;
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
            for (var index = 0; index < buffer.Length; ++index)
                _baseStream.Write(TinyBitArray.FromByte(buffer[index], _bitPackingDirection));
            return buffer.Length;
        }

        protected override async Task<Int32> WriteAsyncCore(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
        {
            for (var index = 0; index < buffer.Length; ++index)
                await _baseStream.WriteAsync(TinyBitArray.FromByte(buffer.Span[index], _bitPackingDirection), cancellationToken).ConfigureAwait(false);
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
