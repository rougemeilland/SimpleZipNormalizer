﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    class RandomInputByteStreamByStream
        : IRandomInputByteStream<UInt64>
    {
        private readonly Stream _baseStream;
        private readonly Boolean _leaveOpen;

        private Boolean _isDisposed;

        public RandomInputByteStreamByStream(Stream baseStream, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));
                if (!baseStream.CanRead)
                    throw new NotSupportedException();
                if (!baseStream.CanSeek)
                    throw new NotSupportedException();

                _baseStream = baseStream;
                _leaveOpen = leaveOpen;
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public UInt64 Position
            => _isDisposed
                ? throw new ObjectDisposedException(GetType().FullName)
                : _baseStream.Position < 0
                ? throw new IOException()
                : (UInt64)_baseStream.Position;

        public UInt64 Length
        {
            get =>
                _isDisposed
                ? throw new ObjectDisposedException(GetType().FullName)
                : _baseStream.Length < 0 ? throw new IOException() : (UInt64)_baseStream.Length;

            set => throw new NotSupportedException();
        }

        public void Seek(UInt64 offset)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (offset > Int64.MaxValue)
                throw new IOException();

            _ = _baseStream.Seek((Int64)offset, SeekOrigin.Begin);
        }

        public Int32 Read(Span<Byte> buffer)
            => _isDisposed
                ? throw new ObjectDisposedException(GetType().FullName)
                : _baseStream.Read(buffer);

        public Task<Int32> ReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken = default)
            => _isDisposed
                ? throw new ObjectDisposedException(GetType().FullName)
                : _baseStream.ReadAsync(buffer, cancellationToken).AsTask();

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
    }
}
