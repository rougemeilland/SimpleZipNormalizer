using System;

namespace Utility.IO
{
    internal abstract class BufferedOutputStream<POSITION_T>
        : BufferedBasicOutputStream, IOutputByteStream<POSITION_T>
    {
        public BufferedOutputStream(IOutputByteStream<POSITION_T> baseStream, Boolean leaveOpen)
            : base(baseStream, leaveOpen)
        {
        }

        public BufferedOutputStream(IOutputByteStream<POSITION_T> baseStream, Int32 bufferSize, Boolean leaveOpen)
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
