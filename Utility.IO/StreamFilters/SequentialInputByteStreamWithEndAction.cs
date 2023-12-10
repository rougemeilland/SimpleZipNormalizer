using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class SequentialInputByteStreamWithEndAction
        : SequentialInputByteStreamFilter
    {
        private readonly ISequentialInputByteStream _baseStream;
        private readonly Action<UInt64> _endAction;
        private Boolean _isDisposed;
        private UInt64 _totalCount;

        public SequentialInputByteStreamWithEndAction(ISequentialInputByteStream baseStream, Action<UInt64> endAction, Boolean leaveOpen)
            : base(baseStream, leaveOpen)
        {
            _baseStream = baseStream;
            _endAction = endAction;
            _isDisposed = false;
            _totalCount = 0;
        }

        protected override Int32 ReadCore(Span<Byte> buffer)
        {
            var length = _baseStream.Read(buffer);
            ProgressCounter(length);
            return length;
        }

        protected override async Task<Int32> ReadAsyncCore(Memory<Byte> buffer, CancellationToken cancellationToken)
        {
            var length = await _baseStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            ProgressCounter(length);
            return length;
        }

        protected override void Dispose(Boolean disposing)
        {
            var needToExecuteEndAction = false;
            if (!_isDisposed)
            {
                if (disposing)
                {
                }

                _isDisposed = true;
                needToExecuteEndAction = true;
            }

            base.Dispose(disposing);

            if (needToExecuteEndAction)
            {
                try
                {
                    _endAction(_totalCount);
                }
                catch (Exception)
                {
                }
            }
        }

        protected override async Task DisposeAsyncCore()
        {
            var needToExecuteEndAction = false;
            if (!_isDisposed)
            {
                _isDisposed = true;
                needToExecuteEndAction = true;
            }

            await base.DisposeAsyncCore().ConfigureAwait(false);

            if (needToExecuteEndAction)
            {
                try
                {
                    _endAction(_totalCount);
                }
                catch (Exception)
                {
                }
            }
        }

        private void ProgressCounter(Int32 length)
        {
            if (length > 0)
            {
                checked
                {
                    _totalCount += (UInt32)length;
                }
            }
        }
    }
}
