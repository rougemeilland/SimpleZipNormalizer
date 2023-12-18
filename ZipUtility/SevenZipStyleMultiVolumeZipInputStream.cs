using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Utility;
using Utility.IO;

namespace ZipUtility
{
    /// <summary>
    /// 7-Zip スタイルのマルチボリューム ZIP アーカイブ の仮想ストリームのクラスです。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// <para>
    /// 7-Zip でのマルチボリューム ZIP は
    /// 「ZIPフォーマット上はシングルボリュームであるが、実際のファイルは物理的に一定サイズで分割されている」
    /// という特殊なスタイルで実現されています。
    /// </para>
    /// <pra>
    /// そのため、このクラスでは内部的には複数のボリュームファイルを管理していますが、公開インターフェースではシングルボリュームであると見せかけなければなりません。
    /// </pra>
    /// </item>
    /// </list>
    /// </remarks>
    internal class SevenZipStyleMultiVolumeZipInputStream
        : ZipInputStream
    {
        private const Int32 _STREAM_CACHE_CAPACITY = 8;

        private readonly UInt64 _totalDiskSize;
        private readonly ReadOnlyMemory<(FilePath volumeFile, UInt64 volumeSize)> _internalVolumeDisks;
        private readonly UInt64 _internalVolumeDiskSizePerDisk;
        private readonly ReadOnlyMemory<UInt64> _totalLengthToThatInternalDisk;
        private readonly VolumeDiskStreamCache<IRandomInputByteStream<UInt64>> _streamCache;

        private Boolean _isDisposed;
        private Int32 _currentInternalVolmeDiskNumber;

        private SevenZipStyleMultiVolumeZipInputStream(UInt64 totalDiskSize, ReadOnlyMemory<(FilePath volumeFile, UInt64 volumeSize)> internalVolumeDisks, UInt64 internalVolumeDiskSizePerDisk, ReadOnlyMemory<UInt64> totalLengthToThatInternalDisk)
        {
            _totalDiskSize = totalDiskSize;
            _internalVolumeDisks = internalVolumeDisks;
            _internalVolumeDiskSizePerDisk = internalVolumeDiskSizePerDisk;
            _totalLengthToThatInternalDisk = totalLengthToThatInternalDisk;
            _streamCache = new VolumeDiskStreamCache<IRandomInputByteStream<UInt64>>(_STREAM_CACHE_CAPACITY, OpenInternalVolumeDisk);
            _isDisposed = false;
            _currentInternalVolmeDiskNumber = 0;
        }

        public override String ToString() => $"MultiVolume:Count={_internalVolumeDisks.Length}";

        public static ZipInputStream CreateInstance(ReadOnlyMemory<FilePath> zipArchiveFiles)
        {
            if (zipArchiveFiles.Length < 1)
                throw new ArgumentException("The number of volumes in the multi-volume ZIP file is less than 2.", nameof(zipArchiveFiles));

            var internalVolumeDiskList = new List<(FilePath volumeFile, UInt64 volumeSize)>();
            for (var index = 0; index < zipArchiveFiles.Length; ++index)
            {
                var currentInternalVolmeDisk = zipArchiveFiles.Span[index];
                var currentVolumeSize = checked((UInt64)currentInternalVolmeDisk.Length);
                if (currentVolumeSize <= 0)
                    throw new BadZipFileFormatException($"Volume disk file size is 0.: \"{currentInternalVolmeDisk.FullName}\"");
                internalVolumeDiskList.Add((currentInternalVolmeDisk, currentVolumeSize));
            }

            var internalVolumeDiskArray = internalVolumeDiskList.ToArray();
#if DEBUG
            if (internalVolumeDiskArray.Length < 2)
                throw new Exception();
#endif

            var totalDiskSize = 0UL;
            var totalLengthToThatDisk = new UInt64[internalVolumeDiskArray.Length];
            for (var index = 0; index < internalVolumeDiskArray.Length; ++index)
            {
                var currentInternalVolumeDiskSize = internalVolumeDiskArray[index].volumeSize;
                if (index < internalVolumeDiskArray.Length - 2)
                {
                    // 最後のディスクを除いて長さが同じであることのチェック
                    var nextInternalVolumeDiskSize = internalVolumeDiskArray[index + 1].volumeSize;
                    if (nextInternalVolumeDiskSize != currentInternalVolumeDiskSize)
                        throw new BadZipFileFormatException("Volume disks with different sizes were found among volume disks other than the last.");
                }

                totalLengthToThatDisk[index] = totalDiskSize;
                checked
                {
                    totalDiskSize += currentInternalVolumeDiskSize;
                }
            }

            return
                new SevenZipStyleMultiVolumeZipInputStream(
                    totalDiskSize,
                    internalVolumeDiskArray,
                    internalVolumeDiskArray[0].volumeSize,
                    totalLengthToThatDisk);
        }

        protected override ZipStreamPosition EndOfThisStreamCore => new(0, _totalDiskSize, this);
        protected override UInt64 LengthCore => _totalDiskSize;
        protected override ZipStreamPosition PositionCore
            => new(
                0,
                checked(_totalLengthToThatInternalDisk.Span[_currentInternalVolmeDiskNumber] + _streamCache.GetStream(_currentInternalVolmeDiskNumber).Position),
                this);

        protected override void SeekCore(UInt32 diskNumber, UInt64 offsetOnTheDisk)
        {
            if (diskNumber != 0)
                throw new InternalLogicalErrorException();
            if (offsetOnTheDisk > LengthCore || offsetOnTheDisk > Int64.MaxValue)
                throw new ArgumentOutOfRangeException($"An attempt was made to access a position outside the bounds of the volume disk in a 7-zip style multi-volume ZIP file.: {nameof(offsetOnTheDisk)}=0x{offsetOnTheDisk:x16}", nameof(offsetOnTheDisk));

            var (internalDiskNumberUInt64, offsetOnTheInternalDisk) = UInt64.DivRem(offsetOnTheDisk, _internalVolumeDiskSizePerDisk);
            var internalDiskNumber = checked((Int32)internalDiskNumberUInt64);
            if (internalDiskNumber >= _internalVolumeDisks.Length)
                throw new ArgumentOutOfRangeException($"An attempt was made to access a position outside the bounds of the volume disk in a 7-zip style multi-volume ZIP file.: {nameof(offsetOnTheDisk)}=0x{offsetOnTheDisk:16}", nameof(offsetOnTheDisk));

            if (offsetOnTheInternalDisk > _internalVolumeDisks.Span[internalDiskNumber].volumeSize)
                throw new ArgumentOutOfRangeException($"An attempt was made to access a position outside the bounds of the volume disk in a 7-zip style multi-volume ZIP file.: {nameof(offsetOnTheDisk)}=0x{offsetOnTheDisk:16}", nameof(offsetOnTheDisk));

            _currentInternalVolmeDiskNumber = internalDiskNumber;
            _streamCache.GetStream(_currentInternalVolmeDiskNumber).Seek(offsetOnTheInternalDisk);
        }

        protected override Boolean ValidatePositionCore(UInt32 diskNumber, UInt64 offsetOnTheDisk)
            => diskNumber == 0 && offsetOnTheDisk <= _totalDiskSize;

        protected override IRandomInputByteStream<UInt64>? GetCurrentStreamCore()
        {
            while (true)
            {
                var currentInternalVolmeStream = _streamCache.GetStream(_currentInternalVolmeDiskNumber);
                if (currentInternalVolmeStream.Position < _internalVolumeDisks.Span[_currentInternalVolmeDiskNumber].volumeSize)
                    return currentInternalVolmeStream;
                if (!MoveToNextDisk())
                    return null;
            }
        }

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
                    _streamCache.Dispose();

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        protected override async Task DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                await _streamCache.DisposeAsync().ConfigureAwait(false);
                _isDisposed = true;
            }

            await base.DisposeAsyncCore().ConfigureAwait(false);
        }

        private Boolean MoveToNextDisk()
        {
            if (_currentInternalVolmeDiskNumber >= _internalVolumeDisks.Length - 1)
                return false;

            checked
            {
                ++_currentInternalVolmeDiskNumber;
            }

            _streamCache.GetStream(_currentInternalVolmeDiskNumber).Seek(0);
            return true;
        }

        private IRandomInputByteStream<UInt64> OpenInternalVolumeDisk(Int32 internalVolumeDiskNumber)
        {
            var stream = (IRandomInputByteStream<UInt64>?)null;
            var success = false;
            try
            {
                var (volumeFile, volumeSize) = _internalVolumeDisks.Span[checked((Int32)internalVolumeDiskNumber)];
                try
                {
                    stream = volumeFile.OpenRead().WithCache();
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to open the ZIP archive volume file.: path=\"{volumeFile.FullName}\"", ex);
                }

                if (stream.Length != volumeSize)
                    throw new IOException($"Detected that the size of a ZIP archive's volume file has changed.: path=\"{volumeFile.FullName}\"");

                success = true;
                return stream;
            }
            finally
            {
                if (!success)
                    stream?.Dispose();
            }
        }
    }
}
