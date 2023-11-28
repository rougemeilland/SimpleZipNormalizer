using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Utility.IO;

namespace ZipUtility
{
    internal class SingleVolumeZipOutputStream
          : IZipOutputStream, IVirtualZipFile
    {
        private readonly IRandomOutputByteStream<UInt64> _baseStream;

        private Boolean _isDisposed;

        public SingleVolumeZipOutputStream(FilePath file)
        {
            _isDisposed = false;
            var success = false;
            var stream = file.Create();
            try
            {
                if (stream is not IRandomOutputByteStream<UInt64> randomAccessStream)
                    throw new NotSupportedException();
                _baseStream = randomAccessStream;

                success = true;
            }
            finally
            {
                if (!success)
                    stream.Dispose();
            }
        }

        public UInt64 Length
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return _baseStream.Length;
            }

            set
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                _baseStream.Length = value;
            }
        }

        public ZipStreamPosition Position
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return new ZipStreamPosition(0, _baseStream.Position, this);
            }
        }

        public void Seek(ZipStreamPosition offset)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            var rawOffset = offset;
            if (rawOffset.DiskNumber > 0)
                throw new BadZipFileFormatException("Invalid disk number");
            _baseStream.Seek(rawOffset.OffsetOnTheDisk);
        }

        public Int32 Write(ReadOnlySpan<Byte> buffer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return _baseStream.Write(buffer);
        }

        public Task<Int32> WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return _baseStream.WriteAsync(buffer, cancellationToken);
        }

        public void Flush()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            _baseStream.Flush();
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return _baseStream.FlushAsync(cancellationToken);
        }

        public ZipStreamPosition GetPosition(UInt32 diskNumber, UInt64 offset)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return new ZipStreamPosition(diskNumber, offset, this);
        }

        public ZipStreamPosition LastDiskStartPosition
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return new ZipStreamPosition(0, 0, this);
            }
        }

        public UInt64 LastDiskSize
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return _baseStream.Length;
            }
        }

        Boolean IZipOutputStream.IsMultiVolumeZipStream => false;

        UInt64 IZipOutputStream.MaximumDiskSize => UInt64.MaxValue;

        void IZipOutputStream.ReserveAtomicSpace(UInt64 atomicSpaceSize)
        {
            // シングルボリューム ZIP ファイルなので何もしない
        }

        ZipStreamPosition? IVirtualZipFile.Add(ZipStreamPosition position, UInt64 offset)
        {
            var rawPosition = position;
            if (rawPosition.DiskNumber > 0)
                throw new BadZipFileFormatException("Invalid disk number");

            var newPosition = checked(rawPosition.OffsetOnTheDisk + offset);
            if (newPosition > _baseStream.Length)
                throw new IOException("Position is out of ZIP file.");

            return (ZipStreamPosition?)new ZipStreamPosition(0, newPosition, this);
        }

        ZipStreamPosition? IVirtualZipFile.Subtract(ZipStreamPosition position, UInt64 offset)
        {
            var rawPosition = position;
            if (rawPosition.DiskNumber > 0)
                throw new BadZipFileFormatException("Invalid disk number");

            return (ZipStreamPosition?)new ZipStreamPosition(0, checked(rawPosition.OffsetOnTheDisk - offset), this);
        }

        UInt64 IVirtualZipFile.Subtract(ZipStreamPosition position1, ZipStreamPosition position2)
        {
            var rawPosition1 = position1;
            if (rawPosition1.DiskNumber > 0)
                throw new BadZipFileFormatException("Invalid disk number");
            var rawPosition2 = position2;
            if (rawPosition2.DiskNumber > 0)
                throw new BadZipFileFormatException("Invalid disk number");

            return checked(rawPosition1.OffsetOnTheDisk - rawPosition2.OffsetOnTheDisk);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                    _baseStream.Dispose();
                _isDisposed = true;
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                await _baseStream.DisposeAsync().ConfigureAwait(false);
                _isDisposed = true;
            }
        }
    }
}
