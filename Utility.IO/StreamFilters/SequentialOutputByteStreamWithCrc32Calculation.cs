using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class SequentialOutputByteStreamWithCrc32Calculation
        : SequentialOutputByteStreamFilter
    {
        private readonly ISequentialOutputByteStream _baseStream;
        private readonly ICrcCalculationState<UInt32> _session;
        private readonly Action<(UInt32 Crc, UInt64 Length)> _onCompleted;

        private Boolean _isDisposed;

        public SequentialOutputByteStreamWithCrc32Calculation(ISequentialOutputByteStream baseStream, ICrcCalculationState<UInt32> session, Action<(UInt32 Crc, UInt64 Length)> onCompleted, Boolean leaveOpen)
            : base(baseStream, leaveOpen)
        {
            _baseStream = baseStream;
            _session = session;
            _onCompleted = onCompleted;
        }

        protected override Int32 WriteCore(ReadOnlySpan<Byte> buffer)
        {
            var length = _baseStream.Write(buffer);
            CalculateCrc(buffer, length);
            return length;
        }

        protected override async Task<Int32> WriteAsyncCore(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
        {
            var length = await _baseStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            CalculateCrc(buffer.Span, length);
            return length;
        }

        protected override void FlushCore()
            => _baseStream.Flush();

        protected override Task FlushAsyncCore(CancellationToken cancellationToken = default)
            => _baseStream.FlushAsync(cancellationToken);

        protected override void Dispose(Boolean disposing)
        {
            var needToReportCrcValue = false;
            if (!_isDisposed)
            {
                if (disposing)
                {
                }

                needToReportCrcValue = true;

                _isDisposed = true;
            }

            base.Dispose(disposing);

            if (needToReportCrcValue)
                ReportCrcValue();
        }

        protected override async Task DisposeAsyncCore()
        {
            var needToReportCrcValue = false;
            if (!_isDisposed)
            {
                needToReportCrcValue = true;
                _isDisposed = true;
            }

            await base.DisposeAsyncCore().ConfigureAwait(false);

            if (needToReportCrcValue)
                ReportCrcValue();
        }

        private void CalculateCrc(ReadOnlySpan<Byte> buffer, Int32 length)
        {
            if (length > 0)
                _session.Put(buffer[..length]);
        }

        private void ReportCrcValue()
        {
            var resultValue = _session.GetResultValue();
            try
            {
                _onCompleted(resultValue);
            }
            catch (Exception)
            {
            }
        }
    }
}
