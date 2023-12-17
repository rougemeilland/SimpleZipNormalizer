using System;
using System.Threading;
using System.Threading.Tasks;
using Utility;
using Utility.IO;

namespace ZipUtility
{
    internal abstract class ZipOutputStream
        : SequentialOutputByteStream, IZipOutputStream, IVirtualZipFile
    {
        private readonly Guid _instanceId;
        private Boolean _isDisposed;
        private Boolean _isCompletedSuccessfully;

        protected ZipOutputStream()
        {
            _instanceId = Guid.NewGuid();
            _isDisposed = false;
            _isCompletedSuccessfully = false;
        }

        public ZipStreamPosition Position
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return PositionCore;
            }
        }

        public void ReserveAtomicSpace(UInt64 atomicSpaceSize)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            ReserveAtomicSpaceCore(atomicSpaceSize);
        }

        public void LockVolumeDisk()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            LockVolumeDiskCore();
        }

        public void UnlockVolumeDisk()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            UnlockVolumeDiskCore();
        }

        public void CompletedSuccessfully()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            _isCompletedSuccessfully = true;
        }

        public ZipStreamPosition Add(ZipStreamPosition position, UInt64 offset)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (!Equals(position.Host))
                throw new InternalLogicalErrorException();

            try
            {
                return AddCore(position.DiskNumber, position.OffsetOnTheDisk, offset);
            }
            catch (OverflowException ex)
            {
                throw new OverflowException($"Overflow occurred while calculating \"{position}\" + 0x{offset:x16}.", ex);
            }
        }

        public ZipStreamPosition Subtract(ZipStreamPosition position, UInt64 offset)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (!Equals(position.Host))
                throw new InternalLogicalErrorException();

            try
            {
                return SubtractCore(position.DiskNumber, position.OffsetOnTheDisk, offset);
            }
            catch (OverflowException ex)
            {
                throw new OverflowException($"Overflow occurred while calculating \"{position}\" - 0x{offset:x16}.", ex);
            }
        }

        public UInt64 Subtract(ZipStreamPosition position1, ZipStreamPosition position2)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (!Equals(position1.Host))
                throw new InternalLogicalErrorException();
            if (!Equals(position2.Host))
                throw new InternalLogicalErrorException();

            try
            {
                return SubtractCore(position1.DiskNumber, position1.OffsetOnTheDisk, position2.DiskNumber, position2.OffsetOnTheDisk);
            }
            catch (OverflowException ex)
            {
                throw new OverflowException($"Overflow occurred while calculating \"{position1}\" - \"{position2}\".", ex);
            }
        }

        public Int32 Compare(ZipStreamPosition position1, ZipStreamPosition position2)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (!Equals(position1.Host))
                throw new InternalLogicalErrorException();
            if (!Equals(position2.Host))
                throw new InternalLogicalErrorException();

            var (diskNumber1, offsetOnTheDisk1) = NormalizeCore(position1.DiskNumber, position1.OffsetOnTheDisk);
            var (diskNumber2, offsetOnTheDisk2) = NormalizeCore(position2.DiskNumber, position2.OffsetOnTheDisk);
            var c = diskNumber1.CompareTo(diskNumber2);
            if (c != 0)
                return c;
            return offsetOnTheDisk1.CompareTo(offsetOnTheDisk2);
        }

        public Boolean Equal(ZipStreamPosition position1, ZipStreamPosition position2)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (!Equals(position1.Host))
                throw new InternalLogicalErrorException();
            if (!Equals(position2.Host))
                throw new InternalLogicalErrorException();

            var (diskNumber1, offsetOnTheDisk1) = NormalizeCore(position1.DiskNumber, position1.OffsetOnTheDisk);
            var (diskNumber2, offsetOnTheDisk2) = NormalizeCore(position2.DiskNumber, position2.OffsetOnTheDisk);

            return
                diskNumber1 == diskNumber2
                && offsetOnTheDisk1 == offsetOnTheDisk2;
        }

        public Int32 GetHashCode(ZipStreamPosition position)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (!Equals(position.Host))
                throw new InternalLogicalErrorException();

            var (diskNumber, offsetOnTheDisk) = NormalizeCore(position.DiskNumber, position.OffsetOnTheDisk);
            return HashCode.Combine(diskNumber, offsetOnTheDisk);
        }

        public Boolean Equals(IVirtualZipFile? other)
           => other is not null
               && GetType() == other.GetType()
               && _instanceId == ((ZipOutputStream)other)._instanceId;

        protected abstract ZipStreamPosition PositionCore { get; }

        protected override Int32 WriteCore(ReadOnlySpan<Byte> buffer)
        {
            var currentStream = GetCurrentStreamCore();
            return currentStream.Write(buffer[..GetSizeToWrite(currentStream, buffer)]);
        }

        protected override Task<Int32> WriteAsyncCore(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
        {
            var currentStream = GetCurrentStreamCore();
            return currentStream.WriteAsync(buffer[..GetSizeToWrite(currentStream, buffer.Span)], cancellationToken);
        }

        protected override void FlushCore() { }
        protected override Task FlushAsyncCore(CancellationToken cancellationToken = default) => Task.CompletedTask;
        protected virtual UInt64 MaximumDiskSizeCore => UInt64.MaxValue;
        protected virtual void ReserveAtomicSpaceCore(UInt64 atomicSpaceSize) { }
        protected virtual void LockVolumeDiskCore() { }
        protected virtual void UnlockVolumeDiskCore() { }
        protected abstract void CleanUpCore();
        protected abstract IRandomOutputByteStream<UInt64> GetCurrentStreamCore();
        protected abstract ZipStreamPosition AddCore(UInt32 diskNumber, UInt64 offsetOnTheDisk, UInt64 offset);
        protected abstract ZipStreamPosition SubtractCore(UInt32 diskNumber, UInt64 offsetOnTheDisk, UInt64 offset);
        protected abstract UInt64 SubtractCore(UInt32 diskNumber1, UInt64 offsetOnTheDisk1, UInt32 diskNumber2, UInt64 offsetOnTheDisk2);
        protected virtual (UInt32 diskNumber, UInt64 offsetOnTheDisk) NormalizeCore(UInt32 diskNumber, UInt64 offsetOnTheDisk) => (diskNumber, offsetOnTheDisk);

        protected override void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                }

                if (!_isCompletedSuccessfully)
                    CleanUpCore();

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        protected override async Task DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                if (!_isCompletedSuccessfully)
                    CleanUpCore();
                _isDisposed = true;
            }

            await base.DisposeAsyncCore().ConfigureAwait(false);
        }

        private Int32 GetSizeToWrite(IRandomOutputByteStream<UInt64> stream, ReadOnlySpan<Byte> buffer)
        {
            var sizeToWrite = checked((Int32)(MaximumDiskSizeCore - stream.Length).Minimum((UInt64)buffer.Length));
            if (sizeToWrite <= 0)
                throw new InternalLogicalErrorException();
            return sizeToWrite;
        }
    }
}
