using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    class SequentialOutputByteStreamByStream<BASE_STREAM_T>
        : IOutputByteStream<UInt64>
        where BASE_STREAM_T : Stream
    {
        private readonly BASE_STREAM_T _baseStream;
        private readonly Action<BASE_STREAM_T> _finishAction;
        private readonly Boolean _leaveOpne;

        private Boolean _isDisposed;
        private UInt64 _position;

        public SequentialOutputByteStreamByStream(BASE_STREAM_T baseStream, Action<BASE_STREAM_T> finishAction, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (finishAction is null)
                    throw new ArgumentNullException(nameof(finishAction));
                if (!baseStream.CanWrite)
                    throw new NotSupportedException();

                _baseStream = baseStream;
                _finishAction = finishAction;
                _leaveOpne = leaveOpen;
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

        public Int32 Write(ReadOnlySpan<Byte> buffer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            _baseStream.Write(buffer);
            _position += (UInt64)buffer.Length;
            return buffer.Length;
        }

        public async Task<Int32> WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            await _baseStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            _position += (UInt64)buffer.Length;
            return buffer.Length;
        }

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

                    if (!_leaveOpne)
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

                if (!_leaveOpne)
                    await _baseStream.DisposeAsync().ConfigureAwait(false);
                _isDisposed = true;
            }
        }
    }
}
