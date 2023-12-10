using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Utility.IO
{
    public abstract class RandomOutputByteStream<POSITION_T>
        : SequentialOutputByteStream, IRandomOutputByteStream<POSITION_T>
        where POSITION_T : struct, IAdditionOperators<POSITION_T, UInt64, POSITION_T>
    {
        private Boolean _isDisposed;

        protected RandomOutputByteStream()
        {
            _isDisposed = false;
        }

        public POSITION_T Position
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return PositionCore;
            }
        }

        public POSITION_T StartOfThisStream
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return StartOfThisStreamCore;
            }
        }

        public UInt64 Length
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return LengthCore;
            }

            set
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                LengthCore = value;
            }
        }

        public void Seek(POSITION_T position)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            SeekCore(position);
        }

        protected abstract POSITION_T PositionCore { get; }

        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <term>[実装する際の注意]</term>
        /// <description>
        /// <para>
        /// <see cref="StartOfThisStreamCore"/> が示す値は、いわゆる「ゼロ値」でなければならないことに注意してください。
        /// </para>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        protected abstract POSITION_T StartOfThisStreamCore { get; }

        protected abstract UInt64 LengthCore { get; set; }
        protected abstract void SeekCore(POSITION_T position);

        protected override void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                }

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        protected override Task DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
            }

            return base.DisposeAsyncCore();
        }
    }
}
