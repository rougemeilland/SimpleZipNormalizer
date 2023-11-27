using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    abstract class SequentialInputBitStreamBy
        : IInputBitStream
    {
        private readonly BitQueue _bitQueue;
        private readonly BitPackingDirection _bitPackingDirection;

        private Boolean _isDisposed;
        private Boolean _isEndOfSourceSequence;
        private Boolean _isEndOfSequence;

        protected SequentialInputBitStreamBy(BitPackingDirection bitPackingDirection)
        {
            _isDisposed = false;
            _bitQueue = new BitQueue();
            _bitPackingDirection = bitPackingDirection;
            _isEndOfSourceSequence = false;
            _isEndOfSequence = false;
        }

        public Boolean? ReadBit()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_isEndOfSequence)
                return null;

            FillBitQueue(1);
            if (_bitQueue.Count <= 0)
            {
                _isEndOfSequence = true;
                return null;
            }

            return _bitQueue.DequeueBoolean();
        }

        public async Task<Boolean?> ReadBitAsync(CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (_isEndOfSequence)
                return null;

            await FillBitQueueAsync(1, cancellationToken).ConfigureAwait(false);
            if (_bitQueue.Count <= 0)
            {
                _isEndOfSequence = true;
                return null;
            }

            return _bitQueue.DequeueBoolean();
        }

        public TinyBitArray? ReadBits(Int32 bitCount)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (bitCount < 1)
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            if (_isEndOfSequence)
                return null;
            var actualBitCount = GetReadBitCount(bitCount);
            FillBitQueue(actualBitCount);
            if (_bitQueue.Count <= 0)
            {
                _isEndOfSequence = true;
                return null;
            }

            return _bitQueue.DequeueBitArray(actualBitCount.Minimum(_bitQueue.Count));
        }

        public async Task<TinyBitArray?> ReadBitsAsync(Int32 bitCount, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (bitCount < 1)
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            if (_isEndOfSequence)
                return null;
            var actualBitCount = GetReadBitCount(bitCount);
            await FillBitQueueAsync(actualBitCount, cancellationToken).ConfigureAwait(false);
            if (_bitQueue.Count <= 0)
            {
                _isEndOfSequence = true;
                return null;
            }

            return _bitQueue.DequeueBitArray(actualBitCount.Minimum(_bitQueue.Count));
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

        protected abstract Byte? GetNextByte();
        protected abstract Task<Byte?> GetNextByteAsync(CancellationToken cancellationToken);

        protected virtual void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                }

                _isDisposed = true;
            }
        }

        protected virtual ValueTask DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
            }

            return ValueTask.CompletedTask;
        }

        private static Int32 GetReadBitCount(Int32 bitCount)
        {
            const Int32 maximumCount = BitQueue.RecommendedMaxCount - 8;
            var actualBitCount = bitCount.Minimum(maximumCount);
            return
                actualBitCount < 0
                ? throw new InternalLogicalErrorException()
                : actualBitCount;
        }

        private void FillBitQueue(Int32 bitCount)
        {
            while (!_isEndOfSourceSequence && _bitQueue.Count < bitCount)
                ReadNextByte();
        }

        private async Task FillBitQueueAsync(Int32 bitCount, CancellationToken cancellationToken)
        {
            while (!_isEndOfSourceSequence && _bitQueue.Count < bitCount)
                await ReadNextByteAsync(cancellationToken).ConfigureAwait(false);
        }

        private void ReadNextByte()
        {
            var data = GetNextByte();
            if (data is not null)
                _bitQueue.Enqueue(data.Value, _bitPackingDirection);
            else
                _isEndOfSourceSequence = true;
        }

        private async Task ReadNextByteAsync(CancellationToken cancellationToken)
        {
            var data = await GetNextByteAsync(cancellationToken).ConfigureAwait(false);
            if (data is not null)
                _bitQueue.Enqueue(data.Value, _bitPackingDirection);
            else
                _isEndOfSourceSequence = true;
        }
    }
}
