using System;
using System.Threading;
using System.Threading.Tasks;
using Utility;
using Utility.IO;

namespace ZipUtility
{
    internal class SingleVolumeZipOutputStream
          : IZipOutputStream, IVirtualZipFile
    {
        private readonly Guid _instanceId;
        private readonly FilePath _zipArchiveFile;
        private readonly IRandomOutputByteStream<UInt64, UInt64> _baseStream;
        private Boolean _isDisposed;

        public SingleVolumeZipOutputStream(FilePath zipArchiveFile)
        {
            _instanceId = Guid.NewGuid();
            _zipArchiveFile = zipArchiveFile;
            _isDisposed = false;
            var success = false;
            var stream = zipArchiveFile.Create();
            try
            {
                if (stream is not IRandomOutputByteStream<UInt64, UInt64> randomAccessStream)
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

        public override String ToString() => $"SingleVolume:Path=\"{_zipArchiveFile.FullName}\"";

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

        Int32 IBasicOutputByteStream.Write(ReadOnlySpan<Byte> buffer)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return _baseStream.Write(buffer);
        }

        Task<Int32> IBasicOutputByteStream.WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return _baseStream.WriteAsync(buffer, cancellationToken);
        }

        void IBasicOutputByteStream.Flush()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            _baseStream.Flush();
        }

        Task IBasicOutputByteStream.FlushAsync(CancellationToken cancellationToken)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return _baseStream.FlushAsync(cancellationToken);
        }

        ZipStreamPosition IOutputByteStream<ZipStreamPosition>.Position
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return new ZipStreamPosition(0, _baseStream.Position, this);
            }
        }

        Boolean IZipOutputStream.IsMultiVolumeZipStream => false;
        UInt64 IZipOutputStream.MaximumDiskSize => UInt64.MaxValue;
        void IZipOutputStream.ReserveAtomicSpace(UInt64 atomicSpaceSize)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        void IZipOutputStream.LockVolumeDisk()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        void IZipOutputStream.UnlockVolumeDisk()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        ZipStreamPosition IVirtualZipFile.Add(ZipStreamPosition position, UInt64 offset)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (position.DiskNumber != 0)
                throw new InternalLogicalErrorException();

            var newOffset = checked(position.OffsetOnTheDisk + offset);
            if (newOffset > _baseStream.Length)
            {
                // ディスクの現在の終端を超える位置になってしまったら例外
                throw new OverflowException();
            }

            return new ZipStreamPosition(0, newOffset, this);
        }

        ZipStreamPosition IVirtualZipFile.Subtract(ZipStreamPosition position, UInt64 offset)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (position.DiskNumber != 0)
                throw new InternalLogicalErrorException();

            return new ZipStreamPosition(0, checked(position.OffsetOnTheDisk - offset), this);
        }

        UInt64 IVirtualZipFile.Subtract(ZipStreamPosition position1, ZipStreamPosition position2)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (position1.DiskNumber != 0)
                throw new InternalLogicalErrorException();
            if (position2.DiskNumber != 0)
                throw new InternalLogicalErrorException();

            return checked(position1.OffsetOnTheDisk - position2.OffsetOnTheDisk);
        }

        Boolean IEquatable<IVirtualZipFile>.Equals(IVirtualZipFile? other)
            => other is not null
                && GetType() == other.GetType()
                && _instanceId == ((SingleVolumeZipOutputStream)other)._instanceId;

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
