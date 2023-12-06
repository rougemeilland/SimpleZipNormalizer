using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    class SequentialOutputBitStreamByByteStream
        : IOutputBitStream
    {
        private readonly IOutputByteStream<UInt64> _baseStream;
        private readonly BitPackingDirection _bitPackingDirection;
        private readonly Boolean _leaveOpen;
        private readonly BitQueue _bitQueue;

        private Boolean _isDisposed;

        public SequentialOutputBitStreamByByteStream(IOutputByteStream<UInt64> baseStream, BitPackingDirection bitPackingDirection, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                _isDisposed = false;
                _baseStream = baseStream;
                _bitPackingDirection = bitPackingDirection;
                _leaveOpen = leaveOpen;
                _bitQueue = new BitQueue();
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public void Write(Boolean bit)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            _bitQueue.Enqueue(bit);
            FlushBytes();
        }

        public Task WriteAsync(Boolean bit, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            _bitQueue.Enqueue(bit);
            return FlushBytesAsync(cancellationToken);
        }

        public void Write(TinyBitArray bitArray)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            while (bitArray.Length > 0)
            {
                bitArray = QueueBitArray(bitArray);
                FlushBytes();
            }
        }

        public async Task WriteAsync(TinyBitArray bitArray, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            while (bitArray.Length > 0)
            {
                bitArray = QueueBitArray(bitArray);
                await FlushBytesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public void Flush()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            _baseStream.Flush();
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return _baseStream.FlushAsync(cancellationToken);
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
                    try
                    {
                        FlushAllBytes();
                    }
                    catch (Exception)
                    {
                    }

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
                try
                {
                    await FlushAllBytesAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                }

                if (!_leaveOpen)
                    await _baseStream.DisposeAsync().ConfigureAwait(false);
                _isDisposed = true;
            }
        }

        private TinyBitArray QueueBitArray(TinyBitArray bitArray)
        {
            var bitCount = (BitQueue.RecommendedMaxCount - _bitQueue.Count).Minimum(bitArray.Length);
            var (firstHalf, secondHalf) = bitArray.Divide(bitCount);
            _bitQueue.Enqueue(firstHalf);
            bitArray = secondHalf;
            return bitArray;
        }

        private void FlushAllBytes()
        {
            FlushBytes();
#if DEBUG
            if (!_bitQueue.Count.IsAnyOf(0, 7))
                throw new Exception();
#endif
            if (_bitQueue.Count > 0)
            {
                _bitQueue.Enqueue(0, 8 - _bitQueue.Count);
#if DEBUG
                if (_bitQueue.Count != 8)
                    throw new Exception();
#endif
            }
#if DEBUG
            if (_bitQueue.Count % 8 != 0)
                throw new Exception();
#endif
            FlushBytes();
            _baseStream.Flush();
        }

        private async Task FlushAllBytesAsync()
        {
            await FlushBytesAsync(default).ConfigureAwait(false);
#if DEBUG
            if (!_bitQueue.Count.InRange(0, 8))
                throw new Exception();
#endif
            if (_bitQueue.Count > 0)
            {
                _bitQueue.Enqueue(0, 8 - _bitQueue.Count);
#if DEBUG
                if (_bitQueue.Count != 8)
                    throw new Exception();
#endif
            }
#if DEBUG
            if (_bitQueue.Count % 8 != 0)
                throw new Exception();
#endif
            await FlushBytesAsync(default).ConfigureAwait(false);
            await _baseStream.FlushAsync(default).ConfigureAwait(false);
        }

        private void FlushBytes()
        {
            while (_bitQueue.Count >= 8)
                _baseStream.WriteByte(_bitQueue.DequeueByte(_bitPackingDirection));
        }

        private async Task FlushBytesAsync(CancellationToken cancellationToken)
        {
            while (_bitQueue.Count >= 8)
                await _baseStream.WriteByteAsync(_bitQueue.DequeueByte(_bitPackingDirection), cancellationToken).ConfigureAwait(false);
        }
    }
}
