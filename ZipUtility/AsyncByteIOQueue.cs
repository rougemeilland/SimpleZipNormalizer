using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Utility.IO;
using Utility.Threading;

namespace ZipUtility
{
    internal class AsyncByteIOQueue
    {
        private class Reader
            : SequentialInputByteStream
        {
            private readonly AsyncByteIOQueue _parent;
            private Boolean _isDisposed;

            public Reader(AsyncByteIOQueue parent)
            {
                _parent = parent;
                _isDisposed = false;
            }

            protected override Int32 ReadCore(Span<Byte> buffer)
                => _parent._queue.Read(buffer);

            protected override Task<Int32> ReadAsyncCore(Memory<Byte> buffer, CancellationToken cancellationToken)
                => _parent._queue.ReadAsync(buffer, cancellationToken);

            protected override void Dispose(Boolean disposing)
            {
                if (!_isDisposed)
                {
                    if (disposing)
                        _parent.DisposeByReader();
                    _isDisposed = true;
                }

                base.Dispose(disposing);
            }

            protected override Task DisposeAsyncCore()
            {
                if (!_isDisposed)
                {
                    _parent.DisposeByReader();
                    _isDisposed = true;
                }

                return base.DisposeAsyncCore();
            }
        }

        private class Writer
            : SequentialOutputByteStream
        {
            private readonly AsyncByteIOQueue _parent;
            private Boolean _isDisposed;

            public Writer(AsyncByteIOQueue parent)
            {
                _parent = parent;
                _isDisposed = false;
            }

            protected override Int32 WriteCore(ReadOnlySpan<Byte> buffer)
            {
                try
                {
                    return _parent._queue.Write(buffer);
                }
                catch (InvalidOperationException ex)
                {
                    throw new IOException("Stream is already closed.", ex);
                }
            }

            protected override Task<Int32> WriteAsyncCore(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
            {
                try
                {
                    return _parent._queue.WriteAsync(buffer, cancellationToken);
                }
                catch (InvalidOperationException ex)
                {
                    throw new IOException("Stream is already closed.", ex);
                }
            }

            protected override void FlushCore()
            {
                try
                {
                    _parent._queue.Flush();
                }
                catch (InvalidOperationException ex)
                {
                    throw new IOException("Stream is already closed.", ex);
                }
            }

            protected override async Task FlushAsyncCore(CancellationToken cancellationToken = default)
            {
                try
                {
                    await _parent._queue.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (InvalidOperationException ex)
                {
                    throw new IOException("Stream is already closed.", ex);
                }
            }

            protected override void Dispose(Boolean disposing)
            {
                if (!_isDisposed)
                {
                    if (disposing)
                        _parent.DisposeByWriter();
                    _isDisposed = true;
                }

                base.Dispose(disposing);
            }

            protected override Task DisposeAsyncCore()
            {
                if (!_isDisposed)
                {
                    _parent.DisposeByWriter();
                    _isDisposed = true;
                }

                return base.DisposeAsyncCore();
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

        public ISequentialInputByteStream GetReader()
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

        public ISequentialOutputByteStream GetWriter()
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
