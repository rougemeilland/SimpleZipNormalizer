using System;
using System.Numerics;

namespace Utility.IO
{
    internal abstract class BufferedRandomOutputStream<POSITION_T, UNSIGNED_OFFSET_T>
        : BufferedOutputStream<POSITION_T, UNSIGNED_OFFSET_T>
        where POSITION_T : struct, IAdditionOperators<POSITION_T, UNSIGNED_OFFSET_T, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UNSIGNED_OFFSET_T>
        where UNSIGNED_OFFSET_T : struct, IComparable<UNSIGNED_OFFSET_T>, IUnsignedNumber<UNSIGNED_OFFSET_T>, IMinMaxValue<UNSIGNED_OFFSET_T>
    {
        private readonly IRandomOutputByteStream<POSITION_T, UNSIGNED_OFFSET_T> _baseStream;

        public BufferedRandomOutputStream(IRandomOutputByteStream<POSITION_T, UNSIGNED_OFFSET_T> baseStream, Boolean leaveOpen)
            : base(baseStream, leaveOpen)
        {
            _baseStream = baseStream;
        }

        public BufferedRandomOutputStream(IRandomOutputByteStream<POSITION_T, UNSIGNED_OFFSET_T> baseStream, Int32 bufferSize, Boolean leaveOpen)
            : base(baseStream, bufferSize, leaveOpen)
        {
            _baseStream = baseStream;
        }

        public UNSIGNED_OFFSET_T Length
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return
                    CachedDataLength > 0
                    ? _baseStream.Length.Maximum(checked(_baseStream.Position - ZeroPositionValue + FromInt32ToOffset(CachedDataLength)))
                    : _baseStream.Length;
            }

            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                InternalFlush();
                _baseStream.Length = value;
            }
        }

        public void Seek(POSITION_T offset)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            InternalFlush();
            _baseStream.Seek(offset);
            CurrentPosition = checked(offset - ZeroPositionValue);
        }
    }
}
