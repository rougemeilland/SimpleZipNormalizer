using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    class SequentialInputByteStreamBySequence
        : IInputByteStream<UInt64>
    {
        private readonly IEnumerator<Byte> _sourceSequenceEnumerator;

        private Boolean _isDisposed;
        private UInt64 _position;
        private Boolean _isEndOfBaseStream;
        private Boolean _isEndOfStream;

        public SequentialInputByteStreamBySequence(IEnumerable<Byte> sourceSequence)
        {
            _isDisposed = false;
            _sourceSequenceEnumerator = sourceSequence.GetEnumerator();
            _position = 0;
            _isEndOfBaseStream = false;
            _isEndOfStream = false;
        }

        public UInt64 Position => !_isDisposed ? _position : throw new ObjectDisposedException(GetType().FullName);

        public Int32 Read(Span<Byte> buffer)
            => _isDisposed
                ? throw new ObjectDisposedException(GetType().FullName)
                : InternalRead(buffer, default);

        public Task<Int32> ReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken = default)
            => _isDisposed
                ? throw new ObjectDisposedException(GetType().FullName)
                : Task.FromResult(InternalRead(buffer.Span, cancellationToken));

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

        protected virtual void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                    _sourceSequenceEnumerator.Dispose();
                _isDisposed = true;
            }
        }

        protected virtual ValueTask DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                _sourceSequenceEnumerator.Dispose();
                _isDisposed = true;
            }

            return ValueTask.CompletedTask;
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

            _position += (UInt32)bufferIndex;
            return bufferIndex;
        }
    }
}
