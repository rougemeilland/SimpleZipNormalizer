﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    public abstract class AsyncReverseByteSequenceByByteStreamEnumerable<POSITION_T, UNSIGNED_OFFSET_T>
        : IAsyncEnumerable<Byte>
        where POSITION_T : IComparable<POSITION_T>, IAdditionOperators<POSITION_T, UNSIGNED_OFFSET_T, POSITION_T>, ISubtractionOperators<POSITION_T, UNSIGNED_OFFSET_T, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UNSIGNED_OFFSET_T>
        where UNSIGNED_OFFSET_T : IUnsignedNumber<UNSIGNED_OFFSET_T>
    {
        private class Enumerator
            : IAsyncEnumerator<Byte>
        {
            private const Int32 _bufferSize = 64 * 1024;

            private readonly AsyncReverseByteSequenceByByteStreamEnumerable<POSITION_T, UNSIGNED_OFFSET_T> _parent;
            private readonly IRandomInputByteStream<POSITION_T, UNSIGNED_OFFSET_T> _inputStream;
            private readonly POSITION_T _offset;
            private readonly UNSIGNED_OFFSET_T _count;
            private readonly Boolean _leaveOpen;
            private readonly CancellationToken _cancellationToken;
            private readonly Byte[] _buffer;
            private readonly ProgressCounterUInt64 _processedCounter;

            private Boolean _isDisposed;
            private Int32 _bufferCount;
            private Int32 _bufferIndex;
            private POSITION_T _fileIndex;

            public Enumerator(AsyncReverseByteSequenceByByteStreamEnumerable<POSITION_T, UNSIGNED_OFFSET_T> parent, IRandomInputByteStream<POSITION_T, UNSIGNED_OFFSET_T> randomAccessStream, POSITION_T offset, UNSIGNED_OFFSET_T count, IProgress<UInt64>? progress, Boolean leaveOpen, CancellationToken cancellationToken)
            {
                _parent = parent;
                _inputStream = randomAccessStream;
                _offset = offset;
                _count = count;
                _leaveOpen = leaveOpen;
                _cancellationToken = cancellationToken;
                _buffer = new Byte[_bufferSize];
                _processedCounter = new ProgressCounterUInt64(progress);
                _isDisposed = false;
                _bufferCount = 0;
                _bufferIndex = 0;
                _fileIndex = checked(_offset + _count);
            }

            public Byte Current
            {
                get
                {
                    if (_isDisposed)
                        throw new ObjectDisposedException(GetType().FullName);
                    if (!_bufferIndex.IsBetween(0, _bufferCount - 1))
                        throw new InvalidOperationException();
                    _cancellationToken.ThrowIfCancellationRequested();

                    return _buffer[_bufferIndex];
                }
            }

            public async ValueTask<Boolean> MoveNextAsync()
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                _cancellationToken.ThrowIfCancellationRequested();

                if (_processedCounter.Value <= 0)
                    _processedCounter.Report();

                if (_bufferIndex <= 0)
                {
                    var newFileIndex =
                        _fileIndex.CompareTo(checked(_offset + _parent.FromInt32ToOffset(_bufferSize))) < 0
                        ? _offset
                        : checked(_fileIndex - _parent.FromInt32ToOffset(_bufferSize));
                    _bufferCount = _parent.FromOffsetToInt32(checked(_fileIndex - newFileIndex));
                    if (_bufferCount <= 0)
                    {
                        _processedCounter.Report();
                        return false;
                    }

                    _fileIndex = newFileIndex;
                    _inputStream.Seek(_fileIndex);
                    _ = await _inputStream.ReadBytesAsync(_buffer, 0, _bufferCount, _cancellationToken).ConfigureAwait(false);
                    _bufferIndex = _bufferCount;
                    _processedCounter.AddValue((UInt32)_bufferCount);
                }

                --_bufferIndex;
                return true;
            }

            public void Reset()
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                _bufferCount = 0;
                _bufferIndex = 0;
                _fileIndex = checked(_offset + _count);
            }

            public async ValueTask DisposeAsync()
            {
                if (!_isDisposed)
                {
                    if (!_leaveOpen)
                        await _inputStream.DisposeAsync().ConfigureAwait(false);
                    _isDisposed = true;
                }

                GC.SuppressFinalize(this);
            }
        }

        private readonly IRandomInputByteStream<POSITION_T, UNSIGNED_OFFSET_T> _baseStream;
        private readonly POSITION_T _offset;
        private readonly UNSIGNED_OFFSET_T _count;
        private readonly Boolean _leaveOpen;
        private readonly IProgress<UInt64>? _progress;

        public AsyncReverseByteSequenceByByteStreamEnumerable(IRandomInputByteStream<POSITION_T, UNSIGNED_OFFSET_T> baseStream, POSITION_T offset, UNSIGNED_OFFSET_T count, IProgress<UInt64>? progress, Boolean leaveOpen)
        {
            try
            {
                if (baseStream is null)
                    throw new ArgumentNullException(nameof(baseStream));

                _baseStream = baseStream;
                _offset = offset;
                _count = count;
                _leaveOpen = leaveOpen;
                _progress = progress;

                if (_baseStream is null)
                    throw new NotSupportedException();
            }
            catch (Exception)
            {
                if (!leaveOpen)
                    baseStream?.Dispose();
                throw;
            }
        }

        public IAsyncEnumerator<Byte> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new Enumerator(this, _baseStream, _offset, _count, _progress, _leaveOpen, cancellationToken);

        protected abstract UNSIGNED_OFFSET_T FromInt32ToOffset(Int32 value);
        protected abstract Int32 FromOffsetToInt32(UNSIGNED_OFFSET_T value);
    }
}
