using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class SequentialInputByteStreamWithProgression
        : SequentialInputByteStreamFilter
    {
        private readonly ISequentialInputByteStream _baseStream;
        private readonly ProgressCounterUInt64 _progressCounter;
        private Boolean _isDisposed;

        public SequentialInputByteStreamWithProgression(ISequentialInputByteStream baseStream, IProgress<UInt64> progress, Boolean leaveOpen)
            : base(baseStream, leaveOpen)
        {
            _baseStream = baseStream;
            _progressCounter = new ProgressCounterUInt64(progress);
            _isDisposed = false;
        }

        protected override Int32 ReadCore(Span<Byte> buffer)
        {
            _progressCounter.ReportIfInitial();
            var length = _baseStream.Read(buffer);
            ProgressPosition(length);
            return length;
        }

        protected override async Task<Int32> ReadAsyncCore(Memory<Byte> buffer, CancellationToken cancellationToken)
        {
            _progressCounter.ReportIfInitial();
            var length = await _baseStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            ProgressPosition(length);
            return length;
        }

        protected override void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                }

                _isDisposed = true;
                _progressCounter.Report();
            }

            base.Dispose(disposing);
        }

        protected override Task DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                _progressCounter.Report();
            }

            return base.DisposeAsyncCore();
        }

        private void ProgressPosition(Int32 length)
        {
            if (length > 0)
                _progressCounter.AddValue(checked((UInt32)length));
        }
    }
}
