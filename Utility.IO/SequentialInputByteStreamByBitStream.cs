using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    class SequentialInputByteStreamByBitStream
        : IInputByteStream<UInt64>
    {
        private readonly IInputBitStream _baseStream;
        private readonly BitPackingDirection _bitPackingDirection;
        private readonly Boolean _leaveOpen;
        private readonly BitQueue _bitQueue;

        private Boolean _isDisposed;
        private UInt64 _position;
        private Boolean _isEndOfBaseStream;
        private Boolean _isEndOfStream;

        public SequentialInputByteStreamByBitStream(IInputBitStream baseStream, BitPackingDirection bitPackingDirection, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                _isDisposed = false;
                _baseStream = baseStream;
                _bitPackingDirection = bitPackingDirection;
                _leaveOpen = leaveOpen;
                _position = 0;
                _bitQueue = new BitQueue();
                _isEndOfBaseStream = false;
                _isEndOfStream = false;
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public UInt64 Position => !_isDisposed ? _position : throw new ObjectDisposedException(GetType().FullName);

        public Int32 Read(Span<Byte> buffer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_isEndOfStream)
                return 0;

            ReadToBitQueue();
            return ReadFromBitQueue(buffer);
        }

        public async Task<Int32> ReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_isEndOfStream)
                return 0;

            await ReadToBitQueueAsync(cancellationToken).ConfigureAwait(false);
            return ReadFromBitQueue(buffer.Span);
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

        protected virtual void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (!_leaveOpen)
                        _baseStream.Dispose();
                }

                _isDisposed = true;
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                if (!_leaveOpen)
                    await _baseStream.DisposeAsync().ConfigureAwait(false);
                _isDisposed = true;
            }
        }

        private Int32 ReadFromBitQueue(Span<Byte> buffer)
        {
            if (_bitQueue.Count <= 0)
            {
                _isEndOfStream = true;
                return 0;
            }

            var bufferIndex = 0;
            while (bufferIndex < buffer.Length && _bitQueue.Count >= 8)
                buffer[bufferIndex++] = _bitQueue.DequeueByte(_bitPackingDirection);
            if (bufferIndex <= 0 && _bitQueue.Count > 0)
                buffer[bufferIndex++] = _bitQueue.DequeueByte(_bitPackingDirection);
            _position += (UInt64)bufferIndex;
            return bufferIndex;
        }

        private void ReadToBitQueue()
        {
            while (!_isEndOfBaseStream && _bitQueue.Count < BitQueue.RecommendedMaxCount)
            {
                var bitArray = _baseStream.ReadBits(BitQueue.RecommendedMaxCount - _bitQueue.Count);
                if (bitArray is null)
                {
                    _isEndOfBaseStream = true;
                    break;
                }

                _bitQueue.Enqueue(bitArray.Value);
            }
        }

        private async Task ReadToBitQueueAsync(CancellationToken cancellationToken)
        {
            while (!_isEndOfBaseStream && _bitQueue.Count < BitQueue.RecommendedMaxCount)
            {
                var bitArray = await _baseStream.ReadBitsAsync(BitQueue.RecommendedMaxCount - _bitQueue.Count, cancellationToken).ConfigureAwait(false);
                if (bitArray is null)
                {
                    _isEndOfBaseStream = true;
                    break;
                }

                _bitQueue.Enqueue(bitArray.Value);
            }
        }
    }
}
