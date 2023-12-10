using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class SequentialInputByteStreamWithCrc32Calculation
        : SequentialInputByteStreamFilter
    {
        private readonly ISequentialInputByteStream _baseStream;
        private readonly ICrcCalculationState<UInt32> _session;
        private readonly Action<(UInt32 Crc, UInt64 Length)> _onCompleted;

        private Boolean _isDisposed;

        public SequentialInputByteStreamWithCrc32Calculation(ISequentialInputByteStream baseStream, ICrcCalculationState<UInt32> session, Action<(UInt32 Crc, UInt64 Length)> onCompleted, Boolean leaveOpen)
            : base(baseStream, leaveOpen)
        {
            _baseStream = baseStream;
            _session = session;
            _onCompleted = onCompleted;
        }

        protected override Int32 ReadCore(Span<Byte> buffer)
        {
            var length = _baseStream.Read(buffer);
            ProgressCalculation(buffer, length);
            return length;
        }

        protected override async Task<Int32> ReadAsyncCore(Memory<Byte> buffer, CancellationToken cancellationToken)
        {
            var length = await _baseStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            ProgressCalculation(buffer.Span, length);
            return length;
        }

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

        private void ProgressCalculation(Span<Byte> buffer, Int32 length)
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
