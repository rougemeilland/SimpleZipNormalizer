using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utility;
using Utility.IO;

namespace ZipUtility
{
    /// <summary>
    /// 一般的なスタイルのマルチボリューム ZIP アーカイブ の仮想ストリームのクラスです。
    /// </summary>
    internal class MultiVolumeZipInputStream
           : ZipInputStream
    {
        private readonly UInt64 _totalDiskSize;
        private readonly ReadOnlyMemory<(FilePath volumeFile, UInt64 volumeSize, IRandomInputByteStream<UInt64> stream)> _volumeDisks;
        private readonly ReadOnlyMemory<UInt64> _totalLengthToThatInternalDisk;
        private readonly UInt32 _lastDiskNumber;
        private readonly (FilePath volumeFile, UInt64 volumeSize, IRandomInputByteStream<UInt64> stream) _lastVolumeDisk;
        private readonly ValidationStringency _stringency;
        private Boolean _isDisposed;
        private Int32 _currentVolumeDiskNumber;
        private Boolean _isLocked;

        private MultiVolumeZipInputStream(UInt64 totalDiskSize, ReadOnlyMemory<(FilePath volumeFile, UInt64 volumeSize, IRandomInputByteStream<UInt64> stream)> volumeDisks, ReadOnlyMemory<UInt64> totalLengthToThatInternalDisk, ValidationStringency stringency)
        {
            if (volumeDisks.Length < 2)
                throw new InternalLogicalErrorException();

            _totalDiskSize = totalDiskSize;
            _volumeDisks = volumeDisks;
            _totalLengthToThatInternalDisk = totalLengthToThatInternalDisk;
            _stringency = stringency;
            var lastDiskNumberAsInt32 = volumeDisks.Length - 1;
            _lastDiskNumber = checked((UInt32)lastDiskNumberAsInt32);
            _lastVolumeDisk = _volumeDisks.Span[lastDiskNumberAsInt32];
            _isDisposed = false;
            _currentVolumeDiskNumber = 0;
            _isLocked = false;
            EndOfThisStreamCore = new ZipStreamPosition(_lastDiskNumber, _lastVolumeDisk.volumeSize, this);
        }

        public override String ToString() => $"MultiVolume:Count={_volumeDisks.Length}";

        public static ZipInputStream CreateInstance(ReadOnlyMemory<FilePath> zipArchiveFiles, ValidationStringency stringency)
        {
            if (zipArchiveFiles.Length < 2)
                throw new ArgumentException("The number of volumes in the multi-volume ZIP file is less than 2.", nameof(zipArchiveFiles));

            var volumeDiskList = new List<(FilePath volumeFile, UInt64 volumeSize, IRandomInputByteStream<UInt64> stream)>();
            var currentStream = (IRandomInputByteStream<UInt64>?)null;
            var success = false;
            try
            {
                for (var index = 0; index < zipArchiveFiles.Length; ++index)
                {
                    var currentVolmeDisk = zipArchiveFiles.Span[index];
                    currentStream = currentVolmeDisk.OpenRead();
                    var currentVolumeSize = currentStream.Length;
                    if (currentVolumeSize <= 0)
                        throw new BadZipFileFormatException($"Volume disk file size is 0.: \"{currentVolmeDisk.FullName}\"");
                    volumeDiskList.Add((currentVolmeDisk, currentVolumeSize, currentStream));
                }

                var volumeDiskArray = volumeDiskList.ToArray();
#if DEBUG
                if (volumeDiskArray.Length < 2)
                    throw new Exception();
#endif

                var totalDiskSize = 0UL;
                var totalLengthToThatDisk = new UInt64[volumeDiskArray.Length];
                for (var index = 0; index < volumeDiskArray.Length; ++index)
                {
                    totalLengthToThatDisk[index] = totalDiskSize;
                    checked
                    {
                        totalDiskSize += volumeDiskArray[index].volumeSize;
                    }
                }

                var zipStream = new MultiVolumeZipInputStream(totalDiskSize, volumeDiskArray, totalLengthToThatDisk, stringency);
                success = true;
                return zipStream;
            }
            finally
            {
                if (!success)
                {
                    currentStream?.Dispose();
                    foreach (var (_, _, stream) in volumeDiskList)
                        stream.Dispose();
                }
            }
        }

        protected override ZipStreamPosition EndOfThisStreamCore { get; }
        protected override UInt64 LengthCore => _totalDiskSize;

        protected override ZipStreamPosition PositionCore
        {
            get
            {
                var currentStream = GetCurrentStreamCore();
                return
                    currentStream is not null
                    ? new ZipStreamPosition(checked((UInt32)_currentVolumeDiskNumber), currentStream.Position, this)
                    : new ZipStreamPosition(_lastDiskNumber, _lastVolumeDisk.volumeSize, this);
            }
        }

        protected override void SeekCore(UInt32 diskNumber, UInt64 offsetOnTheDisk)
        {
            // ZIP アーカイブによっては、diskNumber と offsetOnTheDisk のペアがボリュームファイルの終端を指していることがあることに注意してください。
            // (例: PKZIP によって作成された ZIP アーカイブなど。)
            // その場合は、Seek は正常に実行し、次の GetCurrentStream() の呼び出し時に次のボリュームに遷移しなければなりません。(最後のディスクの終端を指している場合を除く)

            if (diskNumber >= _volumeDisks.Length || diskNumber > Int32.MaxValue)
                throw new ArgumentOutOfRangeException($"An attempt was made to access position outside the bounds of the volume disk in a multi-volume ZIP file.: {nameof(diskNumber)}=0x{diskNumber:8}, {nameof(offsetOnTheDisk)}=0x{offsetOnTheDisk:16}", nameof(diskNumber));

            var diskNumberAsInt32 = checked((Int32)diskNumber);
            var (_, volumeSize, stream) = _volumeDisks.Span[diskNumberAsInt32];
            if (offsetOnTheDisk > volumeSize)
                throw new ArgumentOutOfRangeException($"An attempt was made to access position outside the bounds of the volume disk in a multi-volume ZIP file.: {nameof(diskNumber)}=0x{diskNumber:8}, {nameof(offsetOnTheDisk)}=0x{offsetOnTheDisk:16}", nameof(offsetOnTheDisk));

            ThrowExceptionIfLocked();
            _currentVolumeDiskNumber = diskNumberAsInt32;
            stream.Seek(offsetOnTheDisk);
        }

        protected override Boolean IsMultiVolumeZipStreamCore => true;

        protected override Boolean ValidatePositionCore(UInt32 diskNumber, UInt64 offsetOnTheDisk)
        {
            // ZIP アーカイブによっては、diskNumber と offsetOnTheDisk のペアがボリュームファイルの終端を指していることがあることに注意してください。
            // (例: PKZIP によって作成された ZIP アーカイブなど。)
            // その場合は、ValidatePosition は正常に実行し、次の GetCurrentStream() の呼び出し時に次のボリュームに遷移しなければなりません。(最後のディスクの終端を指している場合を除く)

            if (diskNumber >= _volumeDisks.Length)
                return false;
            var volumeSize = _volumeDisks.Span[checked((Int32)diskNumber)].volumeSize;

            return
                _stringency > ValidationStringency.Normal
                ? offsetOnTheDisk < volumeSize
                : offsetOnTheDisk <= volumeSize;
        }

        protected override ZipStreamPosition LastDiskStartPositionCore => new(_lastDiskNumber, 0, this);
        protected override UInt64 LastDiskSizeCore => _lastVolumeDisk.volumeSize;

        protected override Boolean CheckIfCanAtmicReadCore(UInt64 minimumAtomicDataSize)
        {
            while (true)
            {
                var (_, volumeSize, stream) = _volumeDisks.Span[_currentVolumeDiskNumber];
                var remainOfCurrentDisk = checked(volumeSize - stream.Position);
                if (remainOfCurrentDisk > 0)
                    return remainOfCurrentDisk >= minimumAtomicDataSize;
                if (!MoveToNextDisk())
                    return false;
            }
        }

        protected override void LockVolumeDiskCore()
        {
            if (_isLocked)
                throw new InternalLogicalErrorException();

            _isLocked = true;
        }

        protected override void UnlockVolumeDiskCore()
        {
            if (!_isLocked)
                throw new InternalLogicalErrorException();

            _isLocked = false;
        }

        protected override IRandomInputByteStream<UInt64>? GetCurrentStreamCore()
        {
            while (true)
            {
                var (_, volumeSize, stream) = _volumeDisks.Span[_currentVolumeDiskNumber];
                if (stream.Position < volumeSize)
                    return stream;
                if (!MoveToNextDisk())
                    return null;
            }
        }

        protected override ZipStreamPosition AddCore(UInt32 diskNumber, UInt64 offsetOnTheDisk, UInt64 offset)
        {
            var currentDiskNumber = diskNumber;
            var currentOffset = checked(offsetOnTheDisk + offset);
            while (currentDiskNumber < _volumeDisks.Length)
            {
                var currentVolumeDiskLength = _volumeDisks.Span[checked((Int32)currentDiskNumber)].volumeSize;
                if (currentOffset < currentVolumeDiskLength)
                    return new ZipStreamPosition(currentDiskNumber, currentOffset, this);
                checked
                {
                    ++currentDiskNumber;
                    currentOffset -= currentVolumeDiskLength;
                }
            }

            throw new OverflowException();
        }

        protected override ZipStreamPosition SubtractCore(UInt32 diskNumber, UInt64 offsetOnTheDisk, UInt64 offset)
        {
            if (diskNumber > _lastDiskNumber || diskNumber == _lastDiskNumber && offsetOnTheDisk > _lastVolumeDisk.volumeSize)
                throw new InternalLogicalErrorException();

            var currentDiskNumber = diskNumber;
            var currentOffset = offsetOnTheDisk;
            while (true)
            {
                if (currentOffset >= offset)
                    return new ZipStreamPosition(currentDiskNumber, checked(currentOffset - offset), this);
                if (currentDiskNumber <= 0)
                    throw new OverflowException();

                checked
                {
                    currentOffset += _volumeDisks.Span[(Int32)currentDiskNumber].volumeSize;
                    --currentDiskNumber;
                }
            }
        }

        protected override UInt64 SubtractCore(UInt32 diskNumber1, UInt64 offsetOnTheDisk1, UInt32 diskNumber2, UInt64 offsetOnTheDisk2)
        {
            if (diskNumber1 > _lastDiskNumber
                || diskNumber1 == _lastDiskNumber && offsetOnTheDisk1 > _lastVolumeDisk.volumeSize
                || diskNumber2 > _lastDiskNumber
                || diskNumber2 == _lastDiskNumber && offsetOnTheDisk2 > _lastVolumeDisk.volumeSize)
            {
                throw new InternalLogicalErrorException();
            }

            return
                checked(
                    _totalLengthToThatInternalDisk.Span[(Int32)diskNumber1] + offsetOnTheDisk1
                    - (_totalLengthToThatInternalDisk.Span[(Int32)diskNumber2] + offsetOnTheDisk2));
        }

        protected override (UInt32 diskNumber, UInt64 offsetOnTheDisk) NormalizeCore(UInt32 diskNumber, UInt64 offsetOnTheDisk)
        {
            if (diskNumber < _lastDiskNumber)
            {
                var volumeSize = _volumeDisks.Span[checked((Int32)diskNumber)].volumeSize;
                if (offsetOnTheDisk > volumeSize)
                    throw new InternalLogicalErrorException();

                return
                    offsetOnTheDisk == volumeSize
                    ? (checked(diskNumber + 1), 0)
                    : (diskNumber, offsetOnTheDisk);
            }
            else if (diskNumber == _lastDiskNumber)
            {
                if (offsetOnTheDisk > _lastVolumeDisk.volumeSize)
                    throw new InternalLogicalErrorException();

                return (diskNumber, offsetOnTheDisk);
            }
            else
            {
                throw new InternalLogicalErrorException();
            }
        }

        protected override void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    for (var index = 0; index < _volumeDisks.Length; ++index)
                        _volumeDisks.Span[0].stream.Dispose();
                }

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        protected override async Task DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                for (var index = 0; index < _volumeDisks.Length; ++index)
                    await _volumeDisks.Span[0].stream.DisposeAsync().ConfigureAwait(false);
                _isDisposed = true;
            }

            await base.DisposeAsyncCore().ConfigureAwait(false);
        }

        private Boolean MoveToNextDisk()
        {
            ThrowExceptionIfLocked();

            if (_currentVolumeDiskNumber >= _volumeDisks.Length - 1)
                return false;

            checked
            {
                ++_currentVolumeDiskNumber;
            }

            var (_, _, stream) = _volumeDisks.Span[_currentVolumeDiskNumber];
            stream.Seek(0);
            return true;
        }

        private void ThrowExceptionIfLocked()
        {
            if (_isLocked)
                throw new InvalidOperationException();
        }
    }
}
