using System;
using System.Threading;
using System.Threading.Tasks;
using Utility;
using Utility.IO;

namespace ZipUtility
{
    internal abstract class ZipInputStream
        : IZipInputStream, IVirtualZipFile
    {
        private readonly Guid _instanceId;
        private Boolean _isDisposed;

        public ZipInputStream(UInt64 totalDiskSize)
        {
            Length = totalDiskSize;
            _instanceId = Guid.NewGuid();
            _isDisposed = false;
        }

        void IDisposable.Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        Int32 IBasicInputByteStream.Read(Span<Byte> buffer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            var currentStream = GetCurrentStream();
            if (currentStream is null)
                return 0;
            var remainOfCurrentDisk = currentStream.Length - currentStream.Position;
            var lengthToRead = checked((Int32)checked((UInt64)buffer.Length).Minimum(remainOfCurrentDisk));
            return currentStream.Read(buffer[..lengthToRead]);
        }

        Task<Int32> IBasicInputByteStream.ReadAsync(Memory<Byte> buffer, CancellationToken cancellationToken)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            var currentStream = GetCurrentStream();
            if (currentStream is null)
                return Task.FromResult(0);
            var remainOfCurrentDisk = currentStream.Length - currentStream.Position;
            var lengthToRead = checked((Int32)checked((UInt64)buffer.Length).Minimum(remainOfCurrentDisk));
            return currentStream.ReadAsync(buffer[..lengthToRead], cancellationToken);
        }

        ZipStreamPosition IInputByteStream<ZipStreamPosition>.Position
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return Position;
            }
        }

        UInt64 IRandomInputByteStream<ZipStreamPosition, UInt64>.Length
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return Length;
            }
        }

        void IRandomInputByteStream<ZipStreamPosition, UInt64>.Seek(ZipStreamPosition position)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (!position.Owner.Equals(this))
                throw new InternalLogicalErrorException();

            Seek(position.DiskNumber, position.OffsetOnTheDisk);
        }

        Boolean IZipInputStream.IsMultiVolumeZipStream
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return IsMultiVolumeZipStream;
            }
        }

        public ZipStreamPosition? GetPosition(UInt32 diskNumber, UInt64 offsetOnTheDisk)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return
                ValidatePosition(diskNumber, offsetOnTheDisk)
                ? new ZipStreamPosition(diskNumber, offsetOnTheDisk, this)
                : null;
        }

        ZipStreamPosition IZipInputStream.FirstDiskStartPosition
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return new ZipStreamPosition(0, 0, this);
            }
        }

        ZipStreamPosition IZipInputStream.LastDiskStartPosition
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return LastDiskStartPosition;
            }
        }

        UInt64 IZipInputStream.LastDiskSize
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return LastDiskSize;
            }
        }

        Boolean IZipInputStream.CheckIfCanAtomicRead(UInt64 minimumAtomicDataSize)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return CheckIfCanAtmicRead(minimumAtomicDataSize);
        }

        void IZipInputStream.LockVolumeDisk()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            LockVolumeDisk();
        }

        void IZipInputStream.UnlockVolumeDisk()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            UnlockVolumeDisk();
        }

        ZipStreamPosition IVirtualZipFile.Add(ZipStreamPosition position, UInt64 offset)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (!position.Owner.Equals(this))
                throw new InternalLogicalErrorException();

            try
            {
                return Add(position.DiskNumber, position.OffsetOnTheDisk, offset);
            }
            catch (OverflowException ex)
            {
                throw new OverflowException($"Overflow occurred while calculating \"{position}\" + 0x{offset:x16}.", ex);
            }
        }

        ZipStreamPosition IVirtualZipFile.Subtract(ZipStreamPosition position, UInt64 offset)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (!position.Owner.Equals(this))
                throw new InternalLogicalErrorException();

            try
            {
                return Subtract(position.DiskNumber, position.OffsetOnTheDisk, offset);
            }
            catch (OverflowException ex)
            {
                throw new OverflowException($"Overflow occurred while calculating \"{position}\" - 0x{offset:x16}.", ex);
            }
        }

        UInt64 IVirtualZipFile.Subtract(ZipStreamPosition position1, ZipStreamPosition position2)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (!position1.Owner.Equals(this))
                throw new InternalLogicalErrorException();
            if (!position2.Owner.Equals(this))
                throw new InternalLogicalErrorException();

            try
            {
                return Subtract(position1.DiskNumber, position1.OffsetOnTheDisk, position2.DiskNumber, position2.OffsetOnTheDisk);
            }
            catch (OverflowException ex)
            {
                throw new OverflowException($"Overflow occurred while calculating \"{position1}\" - \"{position2}\".", ex);
            }
        }

        Boolean IEquatable<IVirtualZipFile>.Equals(IVirtualZipFile? other)
           => other is not null
               && GetType() == other.GetType()
               && _instanceId == ((ZipInputStream)other)._instanceId;

        protected abstract ZipStreamPosition Position { get; }
        protected UInt64 Length { get; }
        protected abstract void Seek(UInt32 diskNumber, UInt64 offsetOnTheDisk);
        protected virtual Boolean IsMultiVolumeZipStream => false;
        protected abstract Boolean ValidatePosition(UInt32 diskNumber, UInt64 offsetOnTheDisk);
        protected virtual ZipStreamPosition LastDiskStartPosition => new(0, 0, this);
        protected virtual UInt64 LastDiskSize => Length;
        protected virtual Boolean CheckIfCanAtmicRead(UInt64 minimumAtomicDataSize) => true;
        protected virtual void LockVolumeDisk() { }
        protected virtual void UnlockVolumeDisk() { }
        protected abstract ZipStreamPosition Add(UInt32 diskNumber, UInt64 offsetOnTheDisk, UInt64 offset);
        protected abstract ZipStreamPosition Subtract(UInt32 diskNumber, UInt64 offsetOnTheDisk, UInt64 offset);
        protected abstract UInt64 Subtract(UInt32 diskNumber1, UInt64 offsetOnTheDisk1, UInt32 diskNumber2, UInt64 offsetOnTheDisk2);
        protected abstract IRandomInputByteStream<UInt64, UInt64>? GetCurrentStream();

        protected virtual void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                }

                _isDisposed = true;
            }
        }

        protected virtual ValueTask DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
            }

            return ValueTask.CompletedTask;
        }
    }
}
