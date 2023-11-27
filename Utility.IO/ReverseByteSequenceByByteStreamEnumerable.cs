﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Utility.IO
{
    public abstract class ReverseByteSequenceByByteStreamEnumerable<POSITION_T>
        : IEnumerable<Byte>
        where POSITION_T : IComparable<POSITION_T>
    {
        private class Enumerator
            : IEnumerator<Byte>
        {
            private const Int32 _bufferSize = 64 * 1024;

            private readonly ReverseByteSequenceByByteStreamEnumerable<POSITION_T> _parent;
            private readonly IRandomInputByteStream<POSITION_T> _inputStream;
            private readonly POSITION_T _offset;
            private readonly UInt64 _count;
            private readonly Boolean _leaveOpen;
            private readonly IProgress<UInt64>? _progress;
            private readonly Byte[] _buffer;

            private Boolean _isDisposed;
            private Int32 _bufferCount;
            private Int32 _bufferIndex;
            private POSITION_T _fileIndex;
            private UInt64 _processedCount;

            public Enumerator(ReverseByteSequenceByByteStreamEnumerable<POSITION_T> parent, IRandomInputByteStream<POSITION_T> randomAccessStream, POSITION_T offset, UInt64 count, IProgress<UInt64>? progress, Boolean leaveOpen)
            {
                _isDisposed = false;
                _parent = parent;
                _inputStream = randomAccessStream;
                _offset = offset;
                _count = count;
                _leaveOpen = leaveOpen;
                _progress = progress;
                _buffer = new Byte[_bufferSize];
                _bufferCount = 0;
                _bufferIndex = 0;
                _fileIndex = _parent.AddPositionAndDistance(_offset, _count);
                _processedCount = 0;
            }

            public Byte Current
                => _isDisposed
                    ? throw new ObjectDisposedException(GetType().FullName)
                    : !_bufferIndex.IsBetween(0, _bufferCount - 1)
                    ? throw new InvalidOperationException()
                    : _buffer[_bufferIndex];

            Object IEnumerator.Current => Current;

            public Boolean MoveNext()
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                if (_processedCount <= 0)
                {
                    try
                    {
                        _progress?.Report(0);
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
                        _ = _inputStream.ReadBytes(_buffer, 0, _bufferCount);
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

            protected virtual void Dispose(Boolean disposing)
            {
                if (!_isDisposed)
                {
                    if (disposing)
                    {
                        if (!_leaveOpen)
                            _inputStream.Dispose();
                    }

                    _isDisposed = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        private readonly IRandomInputByteStream<POSITION_T> _baseStream;
        private readonly POSITION_T _offset;
        private readonly UInt64 _count;
        private readonly Boolean _leaveOpen;
        private readonly IProgress<UInt64>? _progress;

        public ReverseByteSequenceByByteStreamEnumerable(IRandomInputByteStream<POSITION_T> baseStream, POSITION_T offset, UInt64 count, IProgress<UInt64>? progress, Boolean leaveOpen)
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

        protected abstract POSITION_T AddPositionAndDistance(POSITION_T position, UInt64 distance);
        protected abstract POSITION_T SubtractBufferSizeFromPosition(POSITION_T position, UInt32 distance);
        protected abstract Int32 GetDistanceBetweenPositions(POSITION_T position1, POSITION_T position2);
        public IEnumerator<Byte> GetEnumerator() => new Enumerator(this, _baseStream, _offset, _count, _progress, _leaveOpen);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
