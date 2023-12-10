using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class SequentialOutputByteStreamWithProgression
        : SequentialOutputByteStreamFilter
    {
        private readonly ISequentialOutputByteStream _baseStream;
        private readonly ProgressCounterUInt64 _progressCounter;
        private Boolean _isDisposed;

        public SequentialOutputByteStreamWithProgression(ISequentialOutputByteStream baseStream, IProgress<UInt64> progress, Boolean leaveOpen)
            : base(baseStream, leaveOpen)
        {
            _baseStream = baseStream;
            _progressCounter = new ProgressCounterUInt64(progress);
            _isDisposed = false;
        }

        protected override Int32 WriteCore(ReadOnlySpan<Byte> buffer)
        {
            _progressCounter.ReportIfInitial();
            var length = _baseStream.Write(buffer);
            ProgressPosition(length);
            return length;
        }

        protected override async Task<Int32> WriteAsyncCore(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
        {
            _progressCounter.ReportIfInitial();
            var length = await _baseStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            ProgressPosition(length);
            return length;
        }

        protected override void FlushCore()
            => _baseStream.Flush();

        protected override Task FlushAsyncCore(CancellationToken cancellationToken = default)
            => _baseStream.FlushAsync(cancellationToken);

        protected override void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _progressCounter.Report();
                }

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        protected override Task DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                _progressCounter.Report();
                _isDisposed = true;
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
