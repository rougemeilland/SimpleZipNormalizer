using System;
using System.Numerics;

namespace Utility.IO
{
    internal abstract class BufferedRandomInputStream<POSITION_T, UNSIGNED_OFFSET_T>
        : BufferedInputStream<POSITION_T, UNSIGNED_OFFSET_T>
        where POSITION_T : struct, IAdditionOperators<POSITION_T, UNSIGNED_OFFSET_T, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UNSIGNED_OFFSET_T>
        where UNSIGNED_OFFSET_T : struct, IUnsignedNumber<UNSIGNED_OFFSET_T>, IMinMaxValue<UNSIGNED_OFFSET_T>
    {
        private readonly IRandomInputByteStream<POSITION_T, UNSIGNED_OFFSET_T> _baseStream;

        public BufferedRandomInputStream(IRandomInputByteStream<POSITION_T, UNSIGNED_OFFSET_T> baseStream, Boolean leaveOpen)
            : base(baseStream, leaveOpen)
        {
            _baseStream = baseStream;
        }

        public BufferedRandomInputStream(IRandomInputByteStream<POSITION_T, UNSIGNED_OFFSET_T> baseStream, Int32 bufferSize, Boolean leaveOpen)
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

                return _baseStream.Length;
            }

            set => throw new NotSupportedException();
        }

        public void Seek(POSITION_T offset)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            ClearCache();
            _baseStream.Seek(offset);
            CurrentPosition = checked(offset - ZeroPositionValue);
        }
    }
}
