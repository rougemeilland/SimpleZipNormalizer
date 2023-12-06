using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace Utility.IO
{
    public abstract class ReverseByteSequenceByByteStreamEnumerable<POSITION_T, UNSIGNED_OFFSET_T>
        : IEnumerable<Byte>
        where POSITION_T : struct, IComparable<POSITION_T>, IAdditionOperators<POSITION_T, UNSIGNED_OFFSET_T, POSITION_T>, ISubtractionOperators<POSITION_T, UNSIGNED_OFFSET_T, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UNSIGNED_OFFSET_T>
        where UNSIGNED_OFFSET_T : struct, IComparable<UNSIGNED_OFFSET_T>, IUnsignedNumber<UNSIGNED_OFFSET_T>, IMinMaxValue<UNSIGNED_OFFSET_T>
    {
        private class Enumerator
            : IEnumerator<Byte>
        {
            private const Int32 _bufferSize = 64 * 1024;

            private readonly ReverseByteSequenceByByteStreamEnumerable<POSITION_T, UNSIGNED_OFFSET_T> _parent;
            private readonly IRandomInputByteStream<POSITION_T, UNSIGNED_OFFSET_T> _inputStream;
            private readonly POSITION_T _offset;
            private readonly UNSIGNED_OFFSET_T _count;
            private readonly Boolean _leaveOpen;
            private readonly Byte[] _buffer;
            private readonly ProgressCounter<UNSIGNED_OFFSET_T> _processedCounter;

            private Boolean _isDisposed;
            private Int32 _bufferCount;
            private Int32 _bufferIndex;
            private POSITION_T _fileIndex;

            public Enumerator(ReverseByteSequenceByByteStreamEnumerable<POSITION_T, UNSIGNED_OFFSET_T> parent, IRandomInputByteStream<POSITION_T, UNSIGNED_OFFSET_T> randomAccessStream, POSITION_T offset, UNSIGNED_OFFSET_T count, IProgress<UNSIGNED_OFFSET_T>? progress, Boolean leaveOpen)
            {
                _parent = parent;
                _inputStream = randomAccessStream;
                _offset = offset;
                _count = count;
                _leaveOpen = leaveOpen;
                _buffer = new Byte[_bufferSize];
                _processedCounter = new ProgressCounter<UNSIGNED_OFFSET_T>(progress, _parent.MinimumProgressStep);
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

                    return _buffer[_bufferIndex];
                }
            }

            Object IEnumerator.Current => Current;

            public Boolean MoveNext()
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                if (_processedCounter.Value.CompareTo(UNSIGNED_OFFSET_T.MinValue) <= 0)
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
                    _ = _inputStream.ReadBytes(_buffer, 0, _bufferCount);
                    _bufferIndex = _bufferCount;
                    _processedCounter.AddValue(_parent.FromInt32ToOffset(_bufferCount));
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
                _fileIndex = checked(_offset - _count);
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

        private readonly IRandomInputByteStream<POSITION_T, UNSIGNED_OFFSET_T> _baseStream;
        private readonly POSITION_T _offset;
        private readonly UNSIGNED_OFFSET_T _count;
        private readonly Boolean _leaveOpen;
        private readonly IProgress<UNSIGNED_OFFSET_T>? _progress;

        public ReverseByteSequenceByByteStreamEnumerable(IRandomInputByteStream<POSITION_T, UNSIGNED_OFFSET_T> baseStream, POSITION_T offset, UNSIGNED_OFFSET_T count, IProgress<UNSIGNED_OFFSET_T>? progress, Boolean leaveOpen)
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

        public IEnumerator<Byte> GetEnumerator() => new Enumerator(this, _baseStream, _offset, _count, _progress, _leaveOpen);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected abstract UNSIGNED_OFFSET_T MinimumProgressStep { get; }
        protected abstract UNSIGNED_OFFSET_T FromInt32ToOffset(Int32 offset);
        protected abstract Int32 FromOffsetToInt32(UNSIGNED_OFFSET_T offset);
    }
}
