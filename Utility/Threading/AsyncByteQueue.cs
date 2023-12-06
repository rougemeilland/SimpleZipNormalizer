using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.Threading
{
    public class AsyncByteQueue
        : IDisposable, IAsyncDisposable
    {
        private const Int32 _DEFAULT_BUFFER_SIZE = 80 * 1024;

        private readonly ByteQueue _queue;
        private readonly ManualResetEventSlim _isNotEmptyOrCompletedEvent;
        private readonly AsyncManualResetEvent _isNotEmptyOrCompletedAsyncEvent;
        private readonly ManualResetEventSlim _isNotFullOrCompletedEvent;
        private readonly AsyncManualResetEvent _isNotFullOrCompletedAsyncEvent;
        private readonly ManualResetEventSlim _isEmptyOrCompletedEvent;
        private readonly AsyncManualResetEvent _isEmptyOrCompletedAsyncEvent;

        private Boolean _isDisposed;
        private Boolean _processingReader;
        private Boolean _processingWriter;

        public AsyncByteQueue(Int32 bufferSize = _DEFAULT_BUFFER_SIZE)
        {
            _queue = new ByteQueue(bufferSize);
            _processingReader = false;
            _processingWriter = false;
            _isNotEmptyOrCompletedEvent = new ManualResetEventSlim();
            _isNotEmptyOrCompletedAsyncEvent = new AsyncManualResetEvent();
            _isNotFullOrCompletedEvent = new ManualResetEventSlim();
            _isNotFullOrCompletedAsyncEvent = new AsyncManualResetEvent();
            _isEmptyOrCompletedEvent = new ManualResetEventSlim();
            _isEmptyOrCompletedAsyncEvent = new AsyncManualResetEvent();
        }

        public Int32 Read(Span<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            LockReader();
            try
            {
                _isNotEmptyOrCompletedEvent.Wait(cancellationToken);
                var length = _queue.Read(buffer);
                if (length == 0 && !(buffer.Length <= 0 || _queue.IsCompleted && _queue.IsEmpty))
                    throw new InternalLogicalErrorException();

                // 読み込めた長さが 0 になるのは、 buffer.Length が 0 の場合か、あるいは、 キューが空でかつ Complete 済である場合
                return length;
            }
            finally
            {
                UpdateEvent();
                UnlockReader();
            }
        }

        public async Task<Int32> ReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            LockReader();
            try
            {
                await _isNotEmptyOrCompletedAsyncEvent.WaitAsync(cancellationToken).ConfigureAwait(false);
                var length = _queue.Read(buffer.Span);
                if (length == 0 && !(buffer.Length <= 0 || _queue.IsCompleted && _queue.IsEmpty))
                    throw new InternalLogicalErrorException();

                // 読み込めた長さが 0 になるのは、 buffer.Length が 0 の場合か、あるいは、 キューが空でかつ Complete 済である場合
                return length;
            }
            finally
            {
                UpdateEvent();
                UnlockReader();
            }
        }

        public Int32 Write(ReadOnlySpan<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            LockWriter();
            try
            {
                _isNotFullOrCompletedEvent.Wait(cancellationToken);
                if (_queue.IsCompleted)
                    throw new InvalidOperationException();
                var length = _queue.Write(buffer);
                if (length == 0 && !(buffer.Length <= 0))
                    throw new InternalLogicalErrorException();

                // 書き込めた長さが 0 になるのは、 buffer.Length が 0 の場合
                return length;
            }
            finally
            {
                UpdateEvent();
                UnlockWriter();
            }
        }

        public async Task<Int32> WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            LockWriter();
            try
            {
                await _isNotFullOrCompletedAsyncEvent.WaitAsync(cancellationToken).ConfigureAwait(false);
                if (_queue.IsCompleted)
                    throw new InvalidOperationException();
                var length = _queue.Write(buffer.Span);
                if (length == 0 && !(buffer.Length <= 0))
                    throw new InternalLogicalErrorException();

                // 書き込めた長さが 0 になるのは、 buffer.Length が 0 の場合
                return length;
            }
            finally
            {
                UpdateEvent();
                UnlockWriter();
            }
        }

        public void Flush(CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            LockWriter();
            try
            {
                _isEmptyOrCompletedEvent.Wait(cancellationToken);
                if (_queue.IsCompleted)
                    throw new InvalidOperationException();
            }
            finally
            {
                UpdateEvent();
                UnlockWriter();
            }
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            LockWriter();
            try
            {
                await _isEmptyOrCompletedAsyncEvent.WaitAsync(cancellationToken).ConfigureAwait(false);
                if (_queue.IsCompleted)
                    throw new InvalidOperationException();
            }
            finally
            {
                UpdateEvent();
                UnlockWriter();
            }
        }

        public void Complete()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            try
            {
                _queue.Compete();
            }
            finally
            {
                UpdateEvent();
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
                {
                    _isNotEmptyOrCompletedEvent.Dispose();
                    _isNotFullOrCompletedEvent.Dispose();
                    _isEmptyOrCompletedEvent.Dispose();
                }

                _isDisposed = true;
            }
        }

        protected virtual ValueTask DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                _isNotEmptyOrCompletedEvent.Dispose();
                _isNotFullOrCompletedEvent.Dispose();
                _isEmptyOrCompletedEvent.Dispose();
                _isDisposed = true;
            }

            return ValueTask.CompletedTask;
        }

        private void UpdateEvent()
        {
            lock (this)
            {
                if (!_queue.IsEmpty || _queue.IsCompleted)
                {
                    _isNotEmptyOrCompletedEvent.Set();
                    _isNotEmptyOrCompletedAsyncEvent.Set();
                }
                else
                {
                    _isNotEmptyOrCompletedEvent.Reset();
                    _isNotEmptyOrCompletedAsyncEvent.Reset();
                }

                if (!_queue.IsFull || _queue.IsCompleted)
                {
                    _isNotFullOrCompletedEvent.Set();
                    _isNotFullOrCompletedAsyncEvent.Set();
                }
                else
                {
                    _isNotFullOrCompletedEvent.Reset();
                    _isNotFullOrCompletedAsyncEvent.Reset();
                }

                if (_queue.IsEmpty || _queue.IsCompleted)
                {
                    _isEmptyOrCompletedEvent.Set();
                    _isEmptyOrCompletedAsyncEvent.Set();
                }
                else
                {
                    _isEmptyOrCompletedEvent.Reset();
                    _isEmptyOrCompletedAsyncEvent.Reset();
                }
            }
        }

        private void LockReader()
        {
            lock (this)
            {
                if (_processingReader)
                    throw new InvalidOperationException();
                _processingReader = true;
            }
        }

        private void UnlockReader()
        {
            lock (this)
            {
                if (!_processingReader)
                    throw new InvalidOperationException();
                _processingReader = false;
            }
        }

        private void LockWriter()
        {
            lock (this)
            {
                if (_processingWriter)
                    throw new InvalidOperationException();
                _processingWriter = true;
            }
        }

        private void UnlockWriter()
        {
            lock (this)
            {
                if (!_processingWriter)
                    throw new InvalidOperationException();
                _processingWriter = false;
            }
        }
    }
}
