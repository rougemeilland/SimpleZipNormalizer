using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    public abstract class SequentialInputByteStream
        : ISequentialInputByteStream
    {
        private Boolean _isDisposed;

        protected SequentialInputByteStream()
        {
            _isDisposed = false;
        }

        ~SequentialInputByteStream()
        {
            Dispose(disposing: false);
        }

        public Int32 Read(Span<Byte> buffer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            var length = ReadCore(buffer);
            return length;
        }

        public Task<Int32> ReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return ReadAsyncCore(buffer, cancellationToken);
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

        protected abstract Int32 ReadCore(Span<Byte> buffer);
        protected abstract Task<Int32> ReadAsyncCore(Memory<Byte> buffer, CancellationToken cancellationToken);

        protected virtual void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                }

                _isDisposed = true;
            }
        }

        protected virtual Task DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
            }

            return Task.CompletedTask;
        }
    }
}
