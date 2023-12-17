using System;
using System.Threading.Tasks;
using Utility;
using Utility.IO;

namespace ZipUtility
{
    internal class SingleVolumeZipInputStream
        : ZipInputStream
    {
        private readonly UInt64 _totalDiskSize;
        private readonly FilePath _zipArchiveFile;
        private readonly IRandomInputByteStream<UInt64> _baseStream;
        private Boolean _isDisposed;

        private SingleVolumeZipInputStream(UInt64 totalDiskSize, FilePath zipArchiveFile, IRandomInputByteStream<UInt64> baseStream)
        {
            _totalDiskSize = totalDiskSize;
            _zipArchiveFile = zipArchiveFile;
            _baseStream = baseStream;
            _isDisposed = false;
        }

        public override String ToString() => $"SingleVolume:Path=\"{_zipArchiveFile.FullName}\"";

        public static ZipInputStream CreateInstance(FilePath zipArchiveFile)
        {
            var success = false;
            var stream = zipArchiveFile.OpenRead();
            try
            {
                var zipStream =
                    new SingleVolumeZipInputStream(
                        stream.Length,
                        zipArchiveFile,
                        stream);
                success = true;
                return zipStream;
            }
            finally
            {
                if (!success)
                    stream.Dispose();
            }
        }

        protected override ZipStreamPosition EndOfThisStreamCore => new(0, _totalDiskSize, this);
        protected override UInt64 LengthCore => _totalDiskSize;
        protected override ZipStreamPosition PositionCore => new(0, _baseStream.Position, this);

        protected override void SeekCore(UInt32 diskNumber, UInt64 offsetOnTheDisk)
        {
            if (diskNumber != 0)
                throw new InternalLogicalErrorException();
            if (offsetOnTheDisk > LengthCore || offsetOnTheDisk > Int64.MaxValue)
                throw new ArgumentOutOfRangeException($"An attempt was made to access position outside the bounds of a single-volume ZIP file.: {nameof(offsetOnTheDisk)}=0x{offsetOnTheDisk:x16}", nameof(offsetOnTheDisk));

            _baseStream.Seek(offsetOnTheDisk);
        }

        protected override Boolean ValidatePositionCore(UInt32 diskNumber, UInt64 offsetOnTheDisk)
            => diskNumber == 0 && offsetOnTheDisk <= _totalDiskSize;

        protected override IRandomInputByteStream<UInt64>? GetCurrentStreamCore() => _baseStream;

        protected override ZipStreamPosition AddCore(UInt32 diskNumber, UInt64 offsetOnTheDisk, UInt64 offset)
        {
            if (diskNumber != 0)
                throw new InternalLogicalErrorException();

            var newOffset = checked(offsetOnTheDisk + offset);
            if (newOffset > _totalDiskSize)
                throw new OverflowException();

            return new ZipStreamPosition(0, newOffset, this);
        }

        protected override ZipStreamPosition SubtractCore(UInt32 diskNumber, UInt64 offsetOnTheDisk, UInt64 offset)
        {
            if (diskNumber != 0)
                throw new InternalLogicalErrorException();

            return new ZipStreamPosition(0, checked(offsetOnTheDisk - offset), this);
        }

        protected override UInt64 SubtractCore(UInt32 diskNumber1, UInt64 offsetOnTheDisk1, UInt32 diskNumber2, UInt64 offsetOnTheDisk2)
        {
            if (diskNumber1 != 0)
                throw new InternalLogicalErrorException();
            if (diskNumber2 != 0)
                throw new InternalLogicalErrorException();

            return checked(offsetOnTheDisk1 - offsetOnTheDisk2);
        }

        protected override void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                    _baseStream.Dispose();
                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        protected override async Task DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                await _baseStream.DisposeAsync().ConfigureAwait(false);
                _isDisposed = true;
            }

            await base.DisposeAsyncCore().ConfigureAwait(false);
        }
    }
}
