using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Utility;
using Utility.IO;
using Utility.IO.StreamFilters;

namespace ZipUtility.IO
{
    public abstract class HierarchicalDecoder
        : SequentialInputByteStreamFilter
    {
        private readonly ISequentialInputByteStream _baseStream;
        private readonly UInt64 _size;
        private readonly ProgressCounterUInt64 _unpackedSizeCounter;

        private Boolean _isDisposed;
        private Boolean _isEndOfStream;

        public HierarchicalDecoder(ISequentialInputByteStream baseStream, UInt64 size, IProgress<UInt64>? unpackedCountProgress, Boolean leaveOpen)
            : base(baseStream, leaveOpen)
        {
            if (baseStream is null)
                throw new ArgumentNullException(nameof(baseStream));

            _isDisposed = false;
            _baseStream = baseStream;
            _size = size;
            _unpackedSizeCounter = new ProgressCounterUInt64(unpackedCountProgress);
        }

        protected override Int32 ReadCore(Span<Byte> buffer)
        {
            _unpackedSizeCounter.ReportIfInitial();
            if (_isEndOfStream || buffer.Length <= 0)
                return 0;
            var length = ReadFromSourceStream(_baseStream, buffer);
            ProgressCounter(length);
            return length;
        }

        protected override async Task<Int32> ReadAsyncCore(Memory<Byte> buffer, CancellationToken cancellationToken)
        {
            _unpackedSizeCounter.ReportIfInitial();
            if (_isEndOfStream || buffer.Length <= 0)
                return 0;
            var length = await ReadFromSourceStreamAsync(_baseStream, buffer, cancellationToken).ConfigureAwait(false);
            ProgressCounter(length);
            return length;
        }

        protected virtual Int32 ReadFromSourceStream(ISequentialInputByteStream sourceStream, Span<Byte> buffer)
            => sourceStream.Read(buffer);

        protected virtual Task<Int32> ReadFromSourceStreamAsync(ISequentialInputByteStream sourceStream, Memory<Byte> buffer, CancellationToken cancellationToken)
            => sourceStream.ReadAsync(buffer, cancellationToken);

        protected virtual void OnEndOfStream()
        {
        }

        protected override void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                }

                _unpackedSizeCounter.Report();
                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        protected override Task DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                _unpackedSizeCounter.Report();
                _isDisposed = true;
            }

            return Task.CompletedTask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProgressCounter(Int32 length)
        {
            if (length > 0)
            {
                _unpackedSizeCounter.AddValue(checked((UInt32)length));
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
