using System;
using System.Threading;
using System.Threading.Tasks;
using Utility;
using Utility.IO;

namespace ZipUtility
{
    internal abstract class ZipInputStream
        : RandomInputByteStream<ZipStreamPosition>, IZipInputStream, IVirtualZipFile
    {
        private readonly Guid _instanceId;
        private Boolean _isDisposed;

        protected ZipInputStream()
        {
            _instanceId = Guid.NewGuid();
            _isDisposed = false;
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

        public ZipStreamPosition? GetPosition(UInt32 diskNumber, UInt64 offsetOnTheDisk)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return
                ValidatePositionCore(diskNumber, offsetOnTheDisk)
                ? new ZipStreamPosition(diskNumber, offsetOnTheDisk, this)
                : null;
        }

        public ZipStreamPosition LastDiskStartPosition
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return LastDiskStartPositionCore;
            }
        }

        public UInt64 LastDiskSize
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return LastDiskSizeCore;
            }
        }

        public Boolean CheckIfCanAtomicRead(UInt64 minimumAtomicDataSize)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return CheckIfCanAtmicReadCore(minimumAtomicDataSize);
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
               && _instanceId == ((ZipInputStream)other)._instanceId;

        protected override ZipStreamPosition StartOfThisStreamCore
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return new ZipStreamPosition(0, 0, this);
            }
        }

        protected override void SeekCore(ZipStreamPosition position)
        {
            if (!Equals(position.Host))
                throw new InternalLogicalErrorException();

            SeekCore(position.DiskNumber, position.OffsetOnTheDisk);
        }

        protected override Int32 ReadCore(Span<Byte> buffer)
        {
            var currentStream = GetCurrentStreamCore();
            if (currentStream is null)
                return 0;
            return currentStream.Read(buffer[..GetSizeToRead(currentStream, buffer)]);
        }

        protected override Task<Int32> ReadAsyncCore(Memory<Byte> buffer, CancellationToken cancellationToken)
        {
            var currentStream = GetCurrentStreamCore();
            if (currentStream is null)
                return Task.FromResult(0);
            return currentStream.ReadAsync(buffer[..GetSizeToRead(currentStream, buffer.Span)], cancellationToken);
        }

        protected abstract void SeekCore(UInt32 diskNumber, UInt64 offsetOnTheDisk);
        protected virtual Boolean IsMultiVolumeZipStreamCore => false;
        protected abstract Boolean ValidatePositionCore(UInt32 diskNumber, UInt64 offsetOnTheDisk);
        protected virtual ZipStreamPosition LastDiskStartPositionCore => new(0, 0, this);
        protected virtual UInt64 LastDiskSizeCore => LengthCore;
        protected virtual Boolean CheckIfCanAtmicReadCore(UInt64 minimumAtomicDataSize) => true;
        protected virtual void LockVolumeDiskCore() { }
        protected virtual void UnlockVolumeDiskCore() { }
        protected abstract IRandomInputByteStream<UInt64>? GetCurrentStreamCore();
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

        private static Int32 GetSizeToRead(IRandomInputByteStream<UInt64> currentStream, Span<Byte> buffer)
            => checked((Int32)((UInt64)buffer.Length).Minimum(currentStream.Length - currentStream.Position));
    }
}
