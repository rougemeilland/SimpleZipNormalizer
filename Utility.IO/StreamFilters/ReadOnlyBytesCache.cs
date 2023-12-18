using System;
using System.Numerics;
using System.Threading.Tasks;
using Utility;

namespace Utility.IO.StreamFilters
{
    internal class ReadOnlyBytesCache<POSITION_T>
        where POSITION_T : struct, IComparable<POSITION_T>, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>
    {
        public const Int32 MAXIMUM_BUFFER_SIZE = 1024 * 1024;
        public const Int32 DEFAULT_BUFFER_SIZE = 80 * 1024;
        public const Int32 MINIMUM_BUFFER_SIZE = 4 * 1024;

        private readonly Byte[] _internalBuffer;

        private POSITION_T? _baseStreamPosition;
        private Int32 _cachedDataLength;
        private Int32 _internalBufferIndex;
        private Boolean _isEndOfBaseStream;

        public ReadOnlyBytesCache(Int32 bufferSize, POSITION_T? baseStreamPosition = null)
        {
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            _baseStreamPosition = baseStreamPosition;
            _internalBuffer = new Byte[(bufferSize.Minimum(MAXIMUM_BUFFER_SIZE).Maximum(MINIMUM_BUFFER_SIZE))];
            _cachedDataLength = 0;
            _internalBufferIndex = 0;
            _isEndOfBaseStream = false;
        }

        public POSITION_T Position
        {
            get
            {
                if (_baseStreamPosition is null)
                    throw new InvalidOperationException();

                return _baseStreamPosition.Value + checked((UInt64)_internalBufferIndex);
            }
        }

        public void Seek(POSITION_T position, Action<POSITION_T> baseStreamSeeker)
        {
            UInt64 offset;
            if (_baseStreamPosition is not null
                && position.CompareTo(_baseStreamPosition.Value) >= 0
                && (offset = position - _baseStreamPosition.Value) <= checked((UInt64)_internalBufferIndex))
            {
                _internalBufferIndex = checked((Int32)offset);
            }
            else
            {
                _cachedDataLength = 0;
                _internalBufferIndex = 0;
                _isEndOfBaseStream = false;
                baseStreamSeeker(position);
                _baseStreamPosition = position;
            }
        }

        public Int32 Read(Span<Byte> destination, Func<Memory<Byte>, (POSITION_T? basePosition, Int32 length)> baseStreamReader)
        {
            if (_internalBufferIndex >= _cachedDataLength)
            {
                if (_isEndOfBaseStream)
                    return 0;

                (_baseStreamPosition, _cachedDataLength) = baseStreamReader(_internalBuffer);
                _internalBufferIndex = 0;
                if (_cachedDataLength <= 0)
                {
                    _isEndOfBaseStream = true;
                    return 0;
                }
            }

            return ReadFromBuffer(destination);
        }

        public async Task<Int32> ReadAsync(Memory<Byte> destination, Func<Memory<Byte>, Task<(POSITION_T? basePosition, Int32 length)>> baseReader)
        {
            if (_internalBufferIndex >= _cachedDataLength)
            {
                if (_isEndOfBaseStream)
                    return 0;

                (_baseStreamPosition, _cachedDataLength) = await baseReader(_internalBuffer).ConfigureAwait(false);
                _internalBufferIndex = 0;
                if (_cachedDataLength <= 0)
                {
                    _isEndOfBaseStream = true;
                    return 0;
                }
            }

            return ReadFromBuffer(destination.Span);
        }

        private Int32 ReadFromBuffer(Span<Byte> destination)
        {
            var copyCount = checked(_cachedDataLength - _internalBufferIndex).Minimum(destination.Length);
            _internalBuffer.AsSpan(_internalBufferIndex, copyCount).CopyTo(destination[..copyCount]);
            checked
            {
                _internalBufferIndex += copyCount;
            }

            return copyCount;
        }
    }
}
