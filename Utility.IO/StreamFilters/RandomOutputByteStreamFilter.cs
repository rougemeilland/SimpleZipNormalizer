using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Utility.IO.StreamFilters
{
    public abstract class RandomOutputByteStreamFilter<POSITION_T, BASE_POSITION_T>
        : SequentialOutputByteStreamFilter, IRandomOutputByteStream<POSITION_T>
        where POSITION_T : struct, IAdditionOperators<POSITION_T, UInt64, POSITION_T>, ISubtractionOperators<POSITION_T, POSITION_T, UInt64>
        where BASE_POSITION_T : struct, IAdditionOperators<BASE_POSITION_T, UInt64, BASE_POSITION_T>, ISubtractionOperators<BASE_POSITION_T, BASE_POSITION_T, UInt64>
    {
        private readonly IRandomOutputByteStream<BASE_POSITION_T> _baseStream;
        private Boolean _isDisposed;

        protected RandomOutputByteStreamFilter(IRandomOutputByteStream<BASE_POSITION_T> baseStream, POSITION_T startOfThisStream, Boolean leaveOpen)
            : base(baseStream, leaveOpen)
        {
            _baseStream = baseStream;
            StartOfThisStreamCore = startOfThisStream;
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

        protected virtual POSITION_T PositionCore => checked(StartOfThisStreamCore + (_baseStream.Position - _baseStream.StartOfThisStream));

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
        protected POSITION_T StartOfThisStreamCore { get; }

        protected virtual UInt64 LengthCore
        {
            get => _baseStream.Length;
            set => _baseStream.Length = value;
        }

        protected virtual void SeekCore(POSITION_T position) => _baseStream.Seek(checked(_baseStream.StartOfThisStream + (position - StartOfThisStreamCore)));

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

        protected override async Task DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
            }

            await base.DisposeAsyncCore().ConfigureAwait(false);
        }
    }
}
