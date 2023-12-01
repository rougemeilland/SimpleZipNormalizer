using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    internal abstract class BufferedOutputStream<POSITION_T>
        : IOutputByteStream<POSITION_T>
    {
        private const Int32 _MAXIMUM_BUFFER_SIZE = 1024 * 1024;
        private const Int32 _DEFAULT_BUFFER_SIZE = 80 * 1024;
        private const Int32 _MINIMUM_BUFFER_SIZE = 4 * 1024;

        private readonly IBasicOutputByteStream _baseStream;
        private readonly Int32 _bufferSize;
        private readonly Boolean _leaveOpen;
        private readonly Byte[] _internalBuffer;

        public BufferedOutputStream(IBasicOutputByteStream baseStream, Boolean leaveOpen)
            : this(baseStream, _DEFAULT_BUFFER_SIZE, leaveOpen)
        {
        }

        public BufferedOutputStream(IBasicOutputByteStream baseStream, Int32 bufferSize, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (bufferSize <= 0)
                    throw new ArgumentOutOfRangeException(nameof(bufferSize));

                _baseStream = baseStream;
                _bufferSize = bufferSize.Minimum(_MAXIMUM_BUFFER_SIZE).Maximum(_MINIMUM_BUFFER_SIZE);
                _leaveOpen = leaveOpen;
                IsDisposed = false;
                CurrentPosition = 0;
                CachedDataLength = 0;
                _internalBuffer = new Byte[_bufferSize];
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public POSITION_T Position
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return AddPosition(ZeroPositionValue, CurrentPosition);
            }
        }

        public Int32 Write(ReadOnlySpan<Byte> buffer)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            var written = InternalWrite(buffer);
            UpdateCurrentPosition(written);
            return written;
        }

        public async Task<Int32> WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            var written = await InternalWriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            UpdateCurrentPosition(written);
            return written;
        }

        public void Flush()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            InternalFlush();
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return InternalFlushAsync(cancellationToken);
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

        protected Boolean IsDisposed { get; private set; }
        protected UInt64 CurrentPosition { get; set; }
        protected Int32 CachedDataLength { get; private set; }

        // 以下のメソッドは .NET 7.0 以降では IAdditionOperators / ISubtractionOperators で代替可能で、しかもわかりやすくコード量も減る。
        protected abstract POSITION_T ZeroPositionValue { get; }
        protected abstract POSITION_T AddPosition(POSITION_T x, UInt64 y);

        protected void InternalFlush()
        {
            if (!IsCacheEmpty)
            {
                _baseStream.WriteBytes(_internalBuffer.AsReadOnly(0, CachedDataLength));
                CachedDataLength = 0;
            }

            _baseStream.Flush();
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    try
                    {
                        InternalFlush();
                    }
                    catch (Exception)
                    {
                    }

                    if (!_leaveOpen)
                        _baseStream.Dispose();
                }

                IsDisposed = true;
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!IsDisposed)
            {
                try
                {
                    await InternalFlushAsync(default).ConfigureAwait(false);
                }
                catch (Exception)
                {
                }

                if (!_leaveOpen)
                    await _baseStream.DisposeAsync().ConfigureAwait(false);
                IsDisposed = true;
            }
        }

        private Boolean IsCacheFull => CachedDataLength >= _internalBuffer.Length;
        private Boolean IsCacheEmpty => CachedDataLength <= 0;

        private Int32 InternalWrite(ReadOnlySpan<Byte> buffer)
        {
            if (IsCacheFull)
            {
                _baseStream.WriteBytes(_internalBuffer.AsReadOnly());
                CachedDataLength = 0;
            }

            return WriteToCache(buffer);
        }

        private async Task<Int32> InternalWriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
        {
            if (IsCacheFull)
            {
                await _baseStream.WriteBytesAsync(_internalBuffer.AsReadOnly(), cancellationToken).ConfigureAwait(false);
                CachedDataLength = 0;
            }

            return WriteToCache(buffer.Span);
        }

        private void UpdateCurrentPosition(Int32 written)
        {
            checked
            {
                CurrentPosition += (UInt32)written;
            }
        }

        private async Task InternalFlushAsync(CancellationToken cancellationToken)
        {
            if (!IsCacheEmpty)
            {
                await _baseStream.WriteBytesAsync(_internalBuffer.AsReadOnly(0, CachedDataLength), default).ConfigureAwait(false);
                CachedDataLength = 0;
            }

            await _baseStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        private Int32 WriteToCache(ReadOnlySpan<Byte> buffer)
        {
            var actualCount = (_internalBuffer.Length - CachedDataLength).Minimum(buffer.Length);
            buffer[..actualCount].CopyTo(_internalBuffer.AsSpan(CachedDataLength, actualCount));
            CachedDataLength += actualCount;
            return actualCount;
        }
    }
}
