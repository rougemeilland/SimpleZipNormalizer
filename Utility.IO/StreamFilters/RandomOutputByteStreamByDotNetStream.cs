using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class RandomOutputByteStreamByDotNetStream
        : RandomOutputByteStream<UInt64>
    {
        private readonly Stream _baseStream;
        private readonly Boolean _leaveOpen;

        private Boolean _isDisposed;

        public RandomOutputByteStreamByDotNetStream(Stream baseStream, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (!baseStream.CanWrite)
                    throw new NotSupportedException();
                if (!baseStream.CanSeek)
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

        protected override UInt64 LengthCore
        {
            get
            {
                if (_baseStream.Length < 0)
                    throw new IOException();

                return checked((UInt64)_baseStream.Length);
            }
            set
            {
                if (value is < 0 or > Int64.MaxValue)
                    throw new IOException();

                _baseStream.SetLength((Int64)value);
            }
        }

        protected override void SeekCore(UInt64 position)
        {
            if (position > Int64.MaxValue)
                throw new IOException();

            _ = _baseStream.Seek(checked((Int64)position), SeekOrigin.Begin);
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

        protected override void FlushCore() => _baseStream.Flush();
        protected override Task FlushAsyncCore(CancellationToken cancellationToken = default) => _baseStream.FlushAsync(cancellationToken);

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
