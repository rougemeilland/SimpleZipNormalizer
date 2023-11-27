using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Utility.IO;

namespace ZipUtility.IO
{
    public abstract class HierarchicalDecoder
        : IBasicInputByteStream
    {
        private readonly IBasicInputByteStream _baseStream;
        private readonly UInt64 _size;
        private readonly IProgress<UInt64>? _unpackedCountProgress;

        private Boolean _isDisposed;
        private UInt64 _position;
        private Boolean _isEndOfStream;

        public HierarchicalDecoder(IBasicInputByteStream baseStream, UInt64 size, IProgress<UInt64>? unpackedCountProgress)
        {
            if (baseStream is null)
                throw new ArgumentNullException(nameof(baseStream));

            _isDisposed = false;
            _baseStream = baseStream;
            _size = size;
            _unpackedCountProgress = unpackedCountProgress;
            _position = 0;
        }

        public Int32 Read(Span<Byte> buffer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_position <= 0)
                ReportProgress(0);
            if (_isEndOfStream || buffer.Length <= 0)
                return 0;
            var length = ReadFromSourceStream(_baseStream, buffer);
            UpdatePosition(length);
            return length;
        }
        public async Task<Int32> ReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_position <= 0)
                ReportProgress(0);
            if (_isEndOfStream || buffer.Length <= 0)
                return 0;
            var length = await ReadFromSourceStreamAsync(_baseStream, buffer, cancellationToken).ConfigureAwait(false);
            UpdatePosition(length);
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

        protected virtual Int32 ReadFromSourceStream(IBasicInputByteStream sourceStream, Span<Byte> buffer)
            => sourceStream.Read(buffer);

        protected virtual Task<Int32> ReadFromSourceStreamAsync(IBasicInputByteStream sourceStream, Memory<Byte> buffer, CancellationToken cancellationToken = default)
            => sourceStream.ReadAsync(buffer, cancellationToken);

        protected virtual void OnEndOfStream()
        {
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                    _baseStream.Dispose();
                _isDisposed = true;
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                await _baseStream.DisposeAsync().ConfigureAwait(false);
                _isDisposed = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdatePosition(Int32 length)
        {
            if (length > 0)
            {
                checked
                {
                    _position += (UInt64)length;
                }
            }
            else
            {
                _isEndOfStream = true;
                if (_position != _size)
                    throw new IOException("Size not match");
                else
                    OnEndOfStream();
            }

            ReportProgress(_position);
        }

        private void ReportProgress(UInt64 unpackedCount)
        {
            try
            {
                _unpackedCountProgress?.Report(unpackedCount);
            }
            catch (Exception)
            {
            }
        }
    }
}
