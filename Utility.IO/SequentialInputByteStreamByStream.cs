using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    class SequentialInputByteStreamByStream
        : IInputByteStream<UInt64>
    {
        private readonly Stream _baseStream;
        private readonly Boolean _leaveOpen;

        private Boolean _isDisposed;
        private UInt64 _position;

        public SequentialInputByteStreamByStream(Stream baseStream, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (!baseStream.CanRead)
                    throw new NotSupportedException();

                _baseStream = baseStream;
                _leaveOpen = leaveOpen;
                _position = 0;
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public UInt64 Position => !_isDisposed ? _position : throw new ObjectDisposedException(GetType().FullName);

        public Int32 Read(Span<Byte> buffer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            var length = _baseStream.Read(buffer);
            if (length > 0)
                _position += (UInt32)length;
            return length;
        }

        public async Task<Int32> ReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            var length = await _baseStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (length > 0)
                _position += (UInt32)length;
            return length;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(Boolean disposing)
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
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                if (!_leaveOpen)
                    await _baseStream.DisposeAsync().ConfigureAwait(false);
                _isDisposed = true;
            }
        }
    }
}
