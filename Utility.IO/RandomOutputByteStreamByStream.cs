using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    class RandomOutputByteStreamByStream<BASE_STREAM_T>
        : IRandomOutputByteStream<UInt64>
        where BASE_STREAM_T : Stream
    {
        private readonly BASE_STREAM_T _baseStream;
        private readonly Action<BASE_STREAM_T> _finishAction;
        private readonly Boolean _leaveOpen;

        private Boolean _isDisposed;

        public RandomOutputByteStreamByStream(BASE_STREAM_T baseStream, Action<BASE_STREAM_T> finishAction, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (finishAction is null)
                    throw new ArgumentNullException(nameof(finishAction));
                if (!baseStream.CanWrite)
                    throw new NotSupportedException();
                if (!baseStream.CanSeek)
                    throw new NotSupportedException();

                _baseStream = baseStream;
                _finishAction = finishAction;
                _leaveOpen = leaveOpen;
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public UInt64 Position
            => _isDisposed
                ? throw new ObjectDisposedException(GetType().FullName)
                : _baseStream.Position < 0 ? throw new IOException() : (UInt64)_baseStream.Position;

        public UInt64 Length
        {
            get =>
                _isDisposed
                ? throw new ObjectDisposedException(GetType().FullName)
                : _baseStream.Length < 0
                ? throw new IOException()
                : (UInt64)_baseStream.Length;

            set
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);
                if (value > Int64.MaxValue)
                    throw new IOException();

                _baseStream.SetLength((Int64)value);
            }
        }

        public void Seek(UInt64 offset)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (offset > Int64.MaxValue)
                throw new IOException();

            _ = _baseStream.Seek((Int64)offset, SeekOrigin.Begin);
        }

        public Int32 Write(ReadOnlySpan<Byte> buffer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            _baseStream.Write(buffer);
            return buffer.Length;
        }

        public Task<Int32> WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public void Flush()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            _baseStream.Flush();
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            await _baseStream.FlushAsync(cancellationToken).ConfigureAwait(false);
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
                    try
                    {
                        _finishAction(_baseStream);
                    }
                    catch (Exception)
                    {
                    }

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
                try
                {
                    _finishAction(_baseStream);
                }
                catch (Exception)
                {
                }

                if (!_leaveOpen)
                    await _baseStream.DisposeAsync().ConfigureAwait(false);
                _isDisposed = true;
            }
        }
    }
}
