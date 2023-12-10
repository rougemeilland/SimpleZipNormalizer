using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class RandomInputByteStreamByDotNetStream
        : RandomInputByteStream<UInt64>
    {
        private readonly Stream _baseStream;
        private readonly Boolean _leaveOpen;

        private Boolean _isDisposed;

        public RandomInputByteStreamByDotNetStream(Stream baseStream, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (!baseStream.CanRead)
                    throw new NotSupportedException();
                if (!baseStream.CanSeek)
                    throw new NotSupportedException();

                _baseStream = baseStream;
                _leaveOpen = leaveOpen;
                EndOfThisStreamCore = checked((UInt64)_baseStream.Length);
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        protected override UInt64 PositionCore
        {
            get
            {
                if (_baseStream.Position < 0)
                    throw new IOException();

                return checked((UInt64)_baseStream.Position);
            }
        }

        protected override UInt64 StartOfThisStreamCore => 0;

        protected override UInt64 EndOfThisStreamCore { get; }

        protected override UInt64 LengthCore
        {
            get
            {
                if (_baseStream.Length < 0)
                    throw new IOException();

                return checked((UInt64)_baseStream.Length);
            }
        }

        protected override void SeekCore(UInt64 position)
        {
            if (position > Int64.MaxValue)
                throw new IOException();

            _ = _baseStream.Seek((Int64)position, SeekOrigin.Begin);
        }

        protected override Int32 ReadCore(Span<Byte> buffer)
            => _baseStream.Read(buffer);

        protected override Task<Int32> ReadAsyncCore(Memory<Byte> buffer, CancellationToken cancellationToken)
            => _baseStream.ReadAsync(buffer, cancellationToken).AsTask();

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
