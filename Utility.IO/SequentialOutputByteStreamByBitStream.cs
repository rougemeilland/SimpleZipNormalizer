﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    class SequentialOutputByteStreamByBitStream
        : IOutputByteStream<UInt64>
    {
        private readonly IOutputBitStream _baseStream;
        private readonly BitPackingDirection _bitPackingDirection;
        private readonly Boolean _leaveOpen;

        private Boolean _isDisposed;
        private UInt64 _position;

        public SequentialOutputByteStreamByBitStream(IOutputBitStream baseStream, BitPackingDirection bitPackingDirection, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                _baseStream = baseStream;
                _bitPackingDirection = bitPackingDirection;
                _leaveOpen = leaveOpen;
                _isDisposed = false;
                _position = 0;
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public UInt64 Position
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return _position;
            }
        }

        public Int32 Write(ReadOnlySpan<Byte> buffer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            for (Int32 index = 0; index < buffer.Length; ++index)
                _baseStream.Write(TinyBitArray.FromByte(buffer[index], _bitPackingDirection));
            var length = buffer.Length;
            UpdatePosition(length);
            return length;
        }

        public async Task<Int32> WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            for (var index = 0; index < buffer.Length; ++index)
                await _baseStream.WriteAsync(TinyBitArray.FromByte(buffer.Span[index], _bitPackingDirection), cancellationToken).ConfigureAwait(false);
            var length = buffer.Length;
            UpdatePosition(length);
            return length;
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

        private void UpdatePosition(Int32 length)
        {
            if (length > 0)
            {
                checked
                {
                    _position += (UInt32)length;
                }
            }
        }
    }
}
