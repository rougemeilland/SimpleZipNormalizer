using System;
using System.Numerics;
using System.Threading.Tasks;
using Utility;

namespace Utility.IO.StreamFilters
{
    internal class WriteOnlyBytesCache<POSITION_T>
        where POSITION_T : struct, IComparable<POSITION_T>, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>
    {
        public const Int32 MAXIMUM_BUFFER_SIZE = 1024 * 1024;
        public const Int32 DEFAULT_BUFFER_SIZE = 80 * 1024;
        public const Int32 MINIMUM_BUFFER_SIZE = 4 * 1024;

        private readonly Byte[] _internalBuffer;

        private POSITION_T? _baseStreamPosition;
        private Int32 _internalBufferIndex;
        private Int32 _cachedDataLength;

        public WriteOnlyBytesCache(Int32 bufferSize, POSITION_T? baseStreamPosition = null)
        {
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            _baseStreamPosition = baseStreamPosition;
            _internalBuffer = new Byte[(bufferSize.Minimum(MAXIMUM_BUFFER_SIZE).Maximum(MINIMUM_BUFFER_SIZE))];
            _internalBufferIndex = 0;
            _cachedDataLength = 0;
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

        public POSITION_T EndOfCache
        {
            get
            {
                if (_baseStreamPosition is null)
                    throw new InvalidOperationException();

                return _baseStreamPosition.Value + checked((UInt64)_cachedDataLength);
            }
        }

        public void Seek(POSITION_T position, Func<ReadOnlyMemory<Byte>, POSITION_T?> baseStreamWriter, Action<POSITION_T> baseStreamSeeker)
        {
            UInt64 offset;
            if (_baseStreamPosition is not null
                && position.CompareTo(_baseStreamPosition.Value) >= 0
                && (offset = position - _baseStreamPosition.Value) <= checked((UInt64)_cachedDataLength))
            {
                _internalBufferIndex = checked((Int32)offset);
            }
            else
            {
                Flush(baseStreamWriter);
                baseStreamSeeker(position);
                _baseStreamPosition = position;
            }
        }

        public Int32 Write(ReadOnlySpan<Byte> buffer, Func<ReadOnlyMemory<Byte>, POSITION_T?> baseStreamWriter)
        {
            if (_cachedDataLength >= _internalBuffer.Length)
            {
                _baseStreamPosition = baseStreamWriter(_internalBuffer);
                _internalBufferIndex = 0;
                _cachedDataLength = 0;
            }

            return WriteToCache(buffer);
        }

        public async Task<Int32> WriteAsync(ReadOnlyMemory<Byte> buffer, Func<ReadOnlyMemory<Byte>, Task<POSITION_T?>> baseStreamWriter)
        {
            if (_cachedDataLength >= _internalBuffer.Length)
            {
                _baseStreamPosition = await baseStreamWriter(_internalBuffer).ConfigureAwait(false);
                _internalBufferIndex = 0;
                _cachedDataLength = 0;
            }

            return WriteToCache(buffer.Span);
        }

        public void Flush(Func<ReadOnlyMemory<Byte>, POSITION_T?> baseStreamWriter)
        {
            if (_cachedDataLength > 0)
            {
                _baseStreamPosition = baseStreamWriter(_internalBuffer.Slice(0, _cachedDataLength));
                _internalBufferIndex = 0;
                _cachedDataLength = 0;
            }
        }

        public async Task FlushAsync(Func<ReadOnlyMemory<Byte>, Task<POSITION_T?>> baseStreamWriter)
        {
            if (_cachedDataLength > 0)
            {
                _baseStreamPosition = await baseStreamWriter(_internalBuffer.Slice(0, _cachedDataLength)).ConfigureAwait(false);
                _internalBufferIndex = 0;
                _cachedDataLength = 0;
            }
        }

        private Int32 WriteToCache(ReadOnlySpan<Byte> buffer)
        {
            var copyCount = (_internalBuffer.Length - _internalBufferIndex).Minimum(buffer.Length);
            buffer[..copyCount].CopyTo(_internalBuffer.AsSpan(_internalBufferIndex, copyCount));
            _internalBufferIndex += copyCount;
            if (_internalBufferIndex > _cachedDataLength)
                _cachedDataLength = _internalBufferIndex;
            return copyCount;
        }
    }
}
