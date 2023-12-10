using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class SequentialOutputByteStreamWithEndAction
        : SequentialOutputByteStreamFilter
    {
        private readonly ISequentialOutputByteStream _baseStream;
        private readonly Action<UInt64> _endAction;
        private Boolean _isDisposed;
        private UInt64 _totalCount;

        public SequentialOutputByteStreamWithEndAction(ISequentialOutputByteStream baseStream, Action<UInt64> endAction, Boolean leaveOpen)
            : base(baseStream, leaveOpen)
        {
            _baseStream = baseStream;
            _endAction = endAction;
            _isDisposed = false;
            _totalCount = 0;
        }

        protected override Int32 WriteCore(ReadOnlySpan<Byte> buffer)
        {
            var length = _baseStream.Write(buffer);
            ProgressPosition(length);
            return length;
        }

        protected override async Task<Int32> WriteAsyncCore(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
        {
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

        private void ProgressPosition(Int32 length)
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
