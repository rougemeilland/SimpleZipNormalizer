using System;
using System.Threading.Tasks;
using Utility;
using Utility.IO;

namespace ZipUtility
{
    internal class SingleVolumeZipOutputStream
          : ZipOutputStream
    {
        private readonly FilePath _zipArchiveFile;
        private readonly IRandomOutputByteStream<UInt64> _baseStream;
        private Boolean _isDisposed;

        private SingleVolumeZipOutputStream(FilePath zipArchiveFile, IRandomOutputByteStream<UInt64> baseStream)
        {
            _zipArchiveFile = zipArchiveFile;
            _baseStream = baseStream;
            _isDisposed = false;
        }

        public override String ToString() => $"SingleVolume:Path=\"{_zipArchiveFile.FullName}\"";

        public static IZipOutputStream CreateInstance(FilePath zipArchiveFile)
        {
            var success = false;
            var stream = zipArchiveFile.Create();
            try
            {
                if (stream is not IRandomOutputByteStream<UInt64> randomAccessStream)
                    throw new NotSupportedException();
                var instance = new SingleVolumeZipOutputStream(zipArchiveFile, randomAccessStream);
                success = true;
                return instance;

            }
            finally
            {
                if (!success)
                    stream.Dispose();
            }
        }

        protected override ZipStreamPosition CurrentPositionCore => new(0, _baseStream.Position, this);
        protected override IRandomOutputByteStream<UInt64> GetCurrentStreamCore() => _baseStream;

        protected override ZipStreamPosition AddCore(UInt32 diskNumber, UInt64 offsetOnTheDisk, UInt64 offset)
        {
            if (diskNumber != 0)
                throw new InternalLogicalErrorException();

            var newOffset = checked(offsetOnTheDisk + offset);
            if (newOffset > _baseStream.Length)
            {
                // ディスクの現在の終端を超える位置になってしまったら例外
                throw new OverflowException();
            }

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

        protected override Int32 CompareCore(UInt32 diskNumber1, UInt64 offsetOnTheDisk1, UInt32 diskNumber2, UInt64 offsetOnTheDisk2)
        {
            if (diskNumber1 != 0)
                throw new InternalLogicalErrorException();
            if (diskNumber2 != 0)
                throw new InternalLogicalErrorException();

            return offsetOnTheDisk1.CompareTo(offsetOnTheDisk2);
        }

        protected override Boolean EqualCore(UInt32 diskNumber1, UInt64 offsetOnTheDisk1, UInt32 diskNumber2, UInt64 offsetOnTheDisk2)
        {
            if (diskNumber1 != 0)
                throw new InternalLogicalErrorException();
            if (diskNumber2 != 0)
                throw new InternalLogicalErrorException();

            return offsetOnTheDisk1 == offsetOnTheDisk2;
        }

        protected override Int32 GetHashCodeCore(UInt32 diskNumber, UInt64 offsetOnTheDisk)
        {
            if (diskNumber != 0)
                throw new InternalLogicalErrorException();

            return offsetOnTheDisk.GetHashCode();
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
