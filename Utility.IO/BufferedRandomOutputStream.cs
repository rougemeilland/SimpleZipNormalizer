using System;

namespace Utility.IO
{
    internal abstract class BufferedRandomOutputStream<POSITION_T>
        : BufferedOutputStream<POSITION_T>
    {
        private readonly IRandomOutputByteStream<POSITION_T> _baseStream;

        public BufferedRandomOutputStream(IRandomOutputByteStream<POSITION_T> baseStream, Boolean leaveOpen)
            : base(baseStream, leaveOpen)
        {
            _baseStream = baseStream;
        }

        public BufferedRandomOutputStream(IRandomOutputByteStream<POSITION_T> baseStream, Int32 bufferSize, Boolean leaveOpen)
            : base(baseStream, bufferSize, leaveOpen)
        {
            _baseStream = baseStream;
        }

        public UInt64 Length
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return
                    CachedDataLength > 0
                    ? _baseStream.Length.Maximum(GetDistanceBetweenPositions(_baseStream.Position, ZeroPositionValue) + (UInt32)CachedDataLength)
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
            CurrentPosition = GetDistanceBetweenPositions(offset, ZeroPositionValue);
        }

        // 以下のメソッドは .NET 7.0 以降では IAdditionOperators / ISubtractionOperators で代替可能で、しかもわかりやすくコード量も減る。
        protected abstract UInt64 GetDistanceBetweenPositions(POSITION_T x, POSITION_T y);
    }
}
