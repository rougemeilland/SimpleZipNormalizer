using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    internal class SequentialInputByteStreamBySequence
          : SequentialInputByteStream
    {
        private readonly IEnumerator<Byte> _sourceSequenceEnumerator;

        private Boolean _isDisposed;
        private Boolean _isEndOfBaseStream;
        private Boolean _isEndOfStream;

        public SequentialInputByteStreamBySequence(IEnumerable<Byte> sourceSequence)
        {
            _sourceSequenceEnumerator = sourceSequence.GetEnumerator();
            _isDisposed = false;
            _isEndOfBaseStream = false;
            _isEndOfStream = false;
        }

        protected override Int32 ReadCore(Span<Byte> buffer)
            => InternalRead(buffer, default);

        protected override Task<Int32> ReadAsyncCore(Memory<Byte> buffer, CancellationToken cancellationToken)
            => Task.FromResult(InternalRead(buffer.Span, cancellationToken));

        protected override void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                    _sourceSequenceEnumerator.Dispose();
                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        protected override Task DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                _sourceSequenceEnumerator.Dispose();
                _isDisposed = true;
            }

            return base.DisposeAsyncCore();
        }

        private Int32 InternalRead(Span<Byte> buffer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_isEndOfStream)
                return 0;
            var bufferIndex = 0;
            while (!_isEndOfBaseStream && bufferIndex < buffer.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (_sourceSequenceEnumerator.MoveNext())
                {
                    buffer[bufferIndex++] = _sourceSequenceEnumerator.Current;
                }
                else
                {
                    _isEndOfBaseStream = true;
                    break;
                }
            }

            if (bufferIndex <= 0)
            {
                _isEndOfStream = true;
                return 0;
            }

            return bufferIndex;
        }
    }
}
