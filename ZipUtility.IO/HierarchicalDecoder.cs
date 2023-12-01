using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Utility;
using Utility.IO;

namespace ZipUtility.IO
{
    public abstract class HierarchicalDecoder
        : IInputByteStream<UInt64>
    {
        private readonly IBasicInputByteStream _baseStream;
        private readonly UInt64 _size;
        private readonly ProgressCounterUInt64 _unpackedSizeCounter;

        private Boolean _isDisposed;
        private Boolean _isEndOfStream;

        public HierarchicalDecoder(IBasicInputByteStream baseStream, UInt64 size, IProgress<UInt64>? unpackedCountProgress)
        {
            if (baseStream is null)
                throw new ArgumentNullException(nameof(baseStream));

            _isDisposed = false;
            _baseStream = baseStream;
            _size = size;
            _unpackedSizeCounter = new ProgressCounterUInt64(unpackedCountProgress);
        }

        public UInt64 Position
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return _unpackedSizeCounter.Value;
            }
        }

        public Int32 Read(Span<Byte> buffer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_unpackedSizeCounter.Value <= 0)
                _unpackedSizeCounter.Report();
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

            if (_unpackedSizeCounter.Value <= 0)
                _unpackedSizeCounter.Report();
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
                _unpackedSizeCounter.Report();
                _isDisposed = true;
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                await _baseStream.DisposeAsync().ConfigureAwait(false);
                _unpackedSizeCounter.Report();
                _isDisposed = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdatePosition(Int32 length)
        {
            if (length > 0)
            {
                _unpackedSizeCounter.AddValue((UInt32)length);
            }
            else
            {
                _isEndOfStream = true;
                if (_unpackedSizeCounter.Value != _size)
                    throw new IOException("Size not match");
                else
                    OnEndOfStream();
            }
        }
    }
}
