using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Utility.IO;
using Utility.Threading;

namespace ZipUtility
{
    class AsyncByteIOQueue
    {
        private class Reader
            : IBasicInputByteStream
        {
            private readonly AsyncByteIOQueue _parent;
            private Boolean _isDisposed;

            public Reader(AsyncByteIOQueue parent)
            {
                _isDisposed = false;
                _parent = parent;
            }

            public Int32 Read(Span<Byte> buffer)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return _parent._queue.Read(buffer);
            }

            public Task<Int32> ReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken = default)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return _parent._queue.ReadAsync(buffer, cancellationToken);
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
                        _parent.DisposeByReader();
                    _isDisposed = true;
                }
            }

            protected virtual ValueTask DisposeAsyncCore()
            {
                if (!_isDisposed)
                {
                    _parent.DisposeByReader();
                    _isDisposed = true;
                }

                return ValueTask.CompletedTask;
            }
        }

        private class Writer
            : IBasicOutputByteStream
        {
            private readonly AsyncByteIOQueue _parent;

            private Boolean _isDisposed;

            public Writer(AsyncByteIOQueue parent)
            {
                _isDisposed = false;
                _parent = parent;
            }

            public Int32 Write(ReadOnlySpan<Byte> buffer)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                try
                {
                    return _parent._queue.Write(buffer);
                }
                catch (InvalidOperationException ex)
                {
                    throw new IOException("Stream is already closed.", ex);
                }
            }

            public Task<Int32> WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                try
                {
                    return _parent._queue.WriteAsync(buffer, cancellationToken);
                }
                catch (InvalidOperationException ex)
                {
                    throw new IOException("Stream is already closed.", ex);
                }
            }

            public void Flush()
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                try
                {
                    _parent._queue.Flush();
                }
                catch (InvalidOperationException ex)
                {
                    throw new IOException("Stream is already closed.", ex);
                }
            }

            public async Task FlushAsync(CancellationToken cancellationToken = default)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                try
                {
                    await _parent._queue.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (InvalidOperationException ex)
                {
                    throw new IOException("Stream is already closed.", ex);
                }
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
                        _parent.DisposeByWriter();
                    _isDisposed = true;
                }
            }

            protected virtual ValueTask DisposeAsyncCore()
            {
                if (!_isDisposed)
                {
                    _parent.DisposeByWriter();
                    _isDisposed = true;
                }

                return ValueTask.CompletedTask;
            }
        }

        private enum StreamState
        {
            Initial,
            Opened,
            Disposed,
        }

        private readonly AsyncByteQueue _queue;

        private Boolean _isDisposed;
        private StreamState _readerState;
        private StreamState _writerState;

        public AsyncByteIOQueue()
        {
            _isDisposed = false;
            _queue = new AsyncByteQueue();
            _readerState = StreamState.Initial;
            _writerState = StreamState.Initial;
        }

        public IBasicInputByteStream GetReader()
        {
            try
            {
                lock (this)
                {
                    if (_readerState != StreamState.Initial)
                        throw new InvalidOperationException();

                    var reader = new Reader(this);
                    _readerState = StreamState.Opened;
                    return reader;
                }
            }
            catch (Exception)
            {
                DisposeByReader();
                throw;
            }
        }

        public IBasicOutputByteStream GetWriter()
        {
            try
            {
                lock (this)
                {
                    if (_writerState != StreamState.Initial)
                        throw new InvalidOperationException();

                    var writer = new Writer(this);
                    _writerState = StreamState.Opened;
                    return writer;
                }
            }
            catch (Exception)
            {
                DisposeByWriter();
                throw;
            }
        }

        private void DisposeByReader()
        {
            lock (this)
            {
                _readerState = StreamState.Disposed;
                Dispose();
            }
        }

        private void DisposeByWriter()
        {
            lock (this)
            {
                _queue.Complete();
                _writerState = StreamState.Disposed;
                Dispose();
            }
        }

        private void Dispose()
        {
            if (_readerState == StreamState.Disposed && _writerState == StreamState.Disposed)
            {
                if (!_isDisposed)
                {
                    _queue.Dispose();
                    _isDisposed = true;
                }
            }
        }
    }
}
