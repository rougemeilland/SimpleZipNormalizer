using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Utility.IO
{
    public abstract class AsyncReverseByteSequenceByByteStreamEnumerable<POSITION_T>
        : IAsyncEnumerable<Byte>
        where POSITION_T : IComparable<POSITION_T>
    {
        private class Enumerator
            : IAsyncEnumerator<Byte>
        {
            private const Int32 _bufferSize = 64 * 1024;

            private readonly AsyncReverseByteSequenceByByteStreamEnumerable<POSITION_T> _parent;
            private readonly IRandomInputByteStream<POSITION_T> _inputStream;
            private readonly POSITION_T _offset;
            private readonly UInt64 _count;
            private readonly Boolean _leaveOpen;
            private readonly IProgress<UInt64>? _progress;
            private readonly CancellationToken _cancellationToken;
            private readonly Byte[] _buffer;

            private Boolean _isDisposed;
            private Int32 _bufferCount;
            private Int32 _bufferIndex;
            private POSITION_T _fileIndex;
            private UInt64 _processedCount;

            public Enumerator(AsyncReverseByteSequenceByByteStreamEnumerable<POSITION_T> parent, IRandomInputByteStream<POSITION_T> randomAccessStream, POSITION_T offset, UInt64 count, IProgress<UInt64>? progress, Boolean leaveOpen, CancellationToken cancellationToken)
            {
                _isDisposed = false;
                _parent = parent;
                _inputStream = randomAccessStream;
                _offset = offset;
                _count = count;
                _leaveOpen = leaveOpen;
                _progress = progress;
                _cancellationToken = cancellationToken;
                _buffer = new Byte[_bufferSize];
                _bufferCount = 0;
                _bufferIndex = 0;
                _fileIndex = _parent.AddPositionAndDistance(_offset, _count);
                _processedCount = 0;
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

                if (_processedCount <= 0)
                {
                    try
                    {
                        _progress?.Report(_processedCount);
                    }
                    catch (Exception)
                    {
                    }
                }

                try
                {
                    if (_bufferIndex <= 0)
                    {
                        var newFileIndex =
                            _fileIndex.CompareTo(_parent.AddPositionAndDistance(_offset, _bufferSize)) < 0
                            ? _offset
                            : _parent.SubtractBufferSizeFromPosition(_fileIndex, _bufferSize);
                        _bufferCount = _parent.GetDistanceBetweenPositions(_fileIndex, newFileIndex);
                        if (_bufferCount <= 0)
                            return false;
                        _fileIndex = newFileIndex;
                        _inputStream.Seek(_fileIndex);
                        _ = await _inputStream.ReadBytesAsync(_buffer, 0, _bufferCount, _cancellationToken).ConfigureAwait(false);
                        _bufferIndex = _bufferCount;
                        _processedCount += (UInt32)_bufferCount;
                    }

                    --_bufferIndex;
                    return true;
                }
                finally
                {
                    try
                    {
                        _progress?.Report(_processedCount);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            public void Reset()
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                _bufferCount = 0;
                _bufferIndex = 0;
                _fileIndex = _parent.AddPositionAndDistance(_offset, _count);
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

        private readonly IRandomInputByteStream<POSITION_T> _baseStream;
        private readonly POSITION_T _offset;
        private readonly UInt64 _count;
        private readonly Boolean _leaveOpen;
        private readonly IProgress<UInt64>? _progress;

        public AsyncReverseByteSequenceByByteStreamEnumerable(IRandomInputByteStream<POSITION_T> baseStream, POSITION_T offset, UInt64 count, IProgress<UInt64>? progress, Boolean leaveOpen)
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
        protected abstract POSITION_T AddPositionAndDistance(POSITION_T position, UInt64 distance);
        protected abstract POSITION_T SubtractBufferSizeFromPosition(POSITION_T position, UInt32 distance);
        protected abstract Int32 GetDistanceBetweenPositions(POSITION_T position1, POSITION_T position2);
    }
}
