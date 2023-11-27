using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.Threading
{
    public static class LockableExtensions
    {
        private class LocalSemaphore
            : ILockable, IAsyncLockable
        {
            private readonly SemaphoreSlim _semaphore;
            private Boolean _isDisposed;

            public LocalSemaphore(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
                _isDisposed = false;
            }

            ~LocalSemaphore()
            {
                Dispose(disposing: false);
            }

            public void Lock() => _semaphore.Wait();

            public Task LockAsync() => _semaphore.WaitAsync();

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(Boolean disposing)
            {
                if (!_isDisposed)
                {
                    if (disposing)
                    {
                    }

                    _ = _semaphore.Release();

                    _isDisposed = true;
                }
            }
        }

        private class GlobalSemaphore
            : ILockable, IAsyncLockable
        {
            private readonly Semaphore _semaphore;
            private Boolean _isDisposed;

            public GlobalSemaphore(Semaphore semaphore)
            {
                _semaphore = semaphore;
                _isDisposed = false;
            }

            ~GlobalSemaphore()
            {
                Dispose(disposing: false);
            }

            public void Lock() => _ = _semaphore.WaitOne();

            public Task LockAsync() => Task.Run(() => _semaphore.WaitOne());

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(Boolean disposing)
            {
                if (!_isDisposed)
                {
                    if (disposing)
                    {
                    }

                    _ = _semaphore.Release();

                    _isDisposed = true;
                }
            }
        }

        private class GlobalMutex
            : ILockable
        {
            private readonly Mutex _mutex;
            private Boolean _isDisposed;

            public GlobalMutex(Mutex mutex)
            {
                _mutex = mutex;
                _isDisposed = false;
            }

            ~GlobalMutex()
            {
                Dispose(disposing: false);
            }

            public void Lock() => _ = _mutex.WaitOne();

            public Task LockAsync() => Task.Run(() => _mutex.WaitOne());

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(Boolean disposing)
            {
                if (!_isDisposed)
                {
                    if (disposing)
                    {
                    }

                    _mutex.ReleaseMutex();

                    _isDisposed = true;
                }
            }
        }

        public static IDisposable Lock(this SemaphoreSlim semaphore)
        {
            var lockObject = new LocalSemaphore(semaphore);
            lockObject.Lock();
            return lockObject;
        }

        public static async Task<IDisposable> LockAsync(this SemaphoreSlim semaphore)
        {
            var lockObject = new LocalSemaphore(semaphore);
            await lockObject.LockAsync().ConfigureAwait(false);
            return lockObject;
        }

        public static IDisposable Lock(this Semaphore semaphore)
        {
            var lockObject = new GlobalSemaphore(semaphore);
            lockObject.Lock();
            return lockObject;
        }

        public static async Task<IDisposable> LockAsync(this Semaphore semaphore)
        {
            var lockObject = new GlobalSemaphore(semaphore);
            await lockObject.LockAsync().ConfigureAwait(false);
            return lockObject;
        }

        public static IDisposable Lock(this Mutex mutex)
        {
            var lockObject = new GlobalMutex(mutex);
            lockObject.Lock();
            return lockObject;
        }
    }
}
