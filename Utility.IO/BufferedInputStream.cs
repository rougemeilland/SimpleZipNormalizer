using System;

namespace Utility.IO
{
    internal abstract class BufferedInputStream<POSITION_T>
        : BufferedBasicInputStream, IInputByteStream<POSITION_T>
    {
        public BufferedInputStream(IInputByteStream<POSITION_T> baseStream, Boolean leaveOpen)
            : base(baseStream, leaveOpen)
        {
        }

        public BufferedInputStream(IInputByteStream<POSITION_T> baseStream, Int32 bufferSize, Boolean leaveOpen)
            : base(baseStream, bufferSize, leaveOpen)
        {
        }

        public POSITION_T Position
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return AddPosition(ZeroPositionValue, CurrentPosition);
            }
        }

        protected abstract POSITION_T ZeroPositionValue { get; }
        protected abstract POSITION_T AddPosition(POSITION_T x, UInt64 y);
    }
}
