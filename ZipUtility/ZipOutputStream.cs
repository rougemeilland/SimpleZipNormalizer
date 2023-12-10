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

        protected ZipOutputStream()
        {
            _instanceId = Guid.NewGuid();
            _isDisposed = false;
        }

        public ZipStreamPosition Position
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return CurrentPositionCore;
            }
        }

        public Boolean IsMultiVolumeZipStream
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return IsMultiVolumeZipStreamCore;
            }
        }

        public UInt64 MaximumDiskSize
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return MaximumDiskSizeCore;
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

        public ZipStreamPosition Add(ZipStreamPosition position, UInt64 offset)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (!position.Host.Equals(this))
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
            if (!position.Host.Equals(this))
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
            if (!position1.Host.Equals(this))
                throw new InternalLogicalErrorException();
            if (!position2.Host.Equals(this))
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
            if (!position1.Host.Equals(this))
                throw new InternalLogicalErrorException();
            if (!position2.Host.Equals(this))
                throw new InternalLogicalErrorException();

            return CompareCore(position1.DiskNumber, position1.OffsetOnTheDisk, position2.DiskNumber, position2.OffsetOnTheDisk);
        }

        public Boolean Equal(ZipStreamPosition position1, ZipStreamPosition position2)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (!position1.Host.Equals(this))
                throw new InternalLogicalErrorException();
            if (!position2.Host.Equals(this))
                throw new InternalLogicalErrorException();

            return EqualCore(position1.DiskNumber, position1.OffsetOnTheDisk, position2.DiskNumber, position2.OffsetOnTheDisk);
        }

        public Int32 GetHashCode(ZipStreamPosition position)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (!position.Host.Equals(this))
                throw new InternalLogicalErrorException();

            return GetHashCodeCore(position.DiskNumber, position.OffsetOnTheDisk);
        }

        public Boolean Equals(IVirtualZipFile? other)
           => other is not null
               && GetType() == other.GetType()
               && _instanceId == ((ZipOutputStream)other)._instanceId;

        protected abstract ZipStreamPosition CurrentPositionCore { get; }

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
        protected virtual Boolean IsMultiVolumeZipStreamCore => false;
        protected virtual UInt64 MaximumDiskSizeCore => UInt64.MaxValue;
        protected virtual void ReserveAtomicSpaceCore(UInt64 atomicSpaceSize) { }
        protected virtual void LockVolumeDiskCore() { }
        protected virtual void UnlockVolumeDiskCore() { }
        protected abstract IRandomOutputByteStream<UInt64> GetCurrentStreamCore();
        protected abstract ZipStreamPosition AddCore(UInt32 diskNumber, UInt64 offsetOnTheDisk, UInt64 offset);
        protected abstract ZipStreamPosition SubtractCore(UInt32 diskNumber, UInt64 offsetOnTheDisk, UInt64 offset);
        protected abstract UInt64 SubtractCore(UInt32 diskNumber1, UInt64 offsetOnTheDisk1, UInt32 diskNumber2, UInt64 offsetOnTheDisk2);
        protected abstract Int32 CompareCore(UInt32 diskNumber1, UInt64 offsetOnTheDisk1, UInt32 diskNumber2, UInt64 offsetOnTheDisk2);
        protected abstract Boolean EqualCore(UInt32 diskNumber1, UInt64 offsetOnTheDisk1, UInt32 diskNumber2, UInt64 offsetOnTheDisk2);
        protected abstract Int32 GetHashCodeCore(UInt32 diskNumber, UInt64 offsetOnTheDisk);

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

        private Int32 GetSizeToWrite(IRandomOutputByteStream<UInt64> stream, ReadOnlySpan<Byte> buffer)
        {
            var sizeToWrite = checked((Int32)(MaximumDiskSize - stream.Length).Minimum((UInt64)buffer.Length));
            if (sizeToWrite <= 0)
                throw new InternalLogicalErrorException();
            return sizeToWrite;
        }
    }
}
