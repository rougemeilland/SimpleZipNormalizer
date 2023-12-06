using System;
using System.Collections.Generic;
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
        private readonly UInt64 _totalDiskSize;
        private readonly ReadOnlyMemory<(FilePath volumeFile, UInt64 volumeSize, IRandomInputByteStream<UInt64, UInt64> stream)> _internalVolumeDisks;
        private readonly UInt64 _internalVolumeDiskSizePerDisk;
        private readonly ReadOnlyMemory<UInt64> _totalLengthToThatInternalDisk;
        private Boolean _isDisposed;
        private Int32 _currentInternalVolmeDiskNumber;

        private SevenZipStyleMultiVolumeZipInputStream(UInt64 totalDiskSize, ReadOnlyMemory<(FilePath volumeFile, UInt64 volumeSize, IRandomInputByteStream<UInt64, UInt64> stream)> internalVolumeDisks, UInt64 internalVolumeDiskSizePerDisk, ReadOnlyMemory<UInt64> totalLengthToThatInternalDisk)
            : base(totalDiskSize)
        {
            _totalDiskSize = totalDiskSize;
            _internalVolumeDisks = internalVolumeDisks;
            _internalVolumeDiskSizePerDisk = internalVolumeDiskSizePerDisk;
            _totalLengthToThatInternalDisk = totalLengthToThatInternalDisk;
            _isDisposed = false;
            _currentInternalVolmeDiskNumber = 0;
        }

        public override String ToString() => $"MultiVolume:Count={_internalVolumeDisks.Length}";

        public static ZipInputStream CreateInstance(ReadOnlyMemory<FilePath> zipArchiveFiles)
        {
            if (zipArchiveFiles.Length < 1)
                throw new ArgumentException("The number of volumes in the multi-volume ZIP file is less than 2.", nameof(zipArchiveFiles));

            var internalVolumeDiskList = new List<(FilePath volumeFile, UInt64 volumeSize, IRandomInputByteStream<UInt64, UInt64> stream)>();
            var currentStream = (IInputByteStream<UInt64>?)null;
            var success = false;
            try
            {
                for (var index = 0; index < zipArchiveFiles.Length; ++index)
                {
                    var currentInternalVolmeDisk = zipArchiveFiles.Span[index];
                    currentStream = currentInternalVolmeDisk.OpenRead();
                    if (currentStream is not IRandomInputByteStream<UInt64, UInt64> randomAccessStream)
                        throw new NotSupportedException();
                    var currentVolumeSize = randomAccessStream.Length;
                    if (currentVolumeSize <= 0)
                        throw new BadZipFileFormatException($"Volume disk file size is 0.: \"{currentInternalVolmeDisk.FullName}\"");
                    internalVolumeDiskList.Add((currentInternalVolmeDisk, currentVolumeSize, randomAccessStream));
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

                var zipStream = new SevenZipStyleMultiVolumeZipInputStream(totalDiskSize, internalVolumeDiskArray, internalVolumeDiskArray[0].volumeSize, totalLengthToThatDisk);
                success = true;
                return zipStream;
            }
            finally
            {
                if (!success)
                {
                    currentStream?.Dispose();
                    foreach (var (_, _, stream) in internalVolumeDiskList)
                        stream.Dispose();
                }
            }
        }

        protected override ZipStreamPosition Position
            => new(
                0,
                checked(_totalLengthToThatInternalDisk.Span[_currentInternalVolmeDiskNumber] + _internalVolumeDisks.Span[_currentInternalVolmeDiskNumber].stream.Position),
                this);

        protected override void Seek(UInt32 diskNumber, UInt64 offsetOnTheDisk)
        {
            if (diskNumber != 0)
                throw new InternalLogicalErrorException();
            if (offsetOnTheDisk > Length || offsetOnTheDisk > Int64.MaxValue)
                throw new ArgumentOutOfRangeException($"An attempt was made to access a position outside the bounds of the volume disk in a 7-zip style multi-volume ZIP file.: {nameof(offsetOnTheDisk)}=0x{offsetOnTheDisk:x16}", nameof(offsetOnTheDisk));

            var (internalDiskNumberUInt64, offsetOnTheInternalDisk) = UInt64.DivRem(offsetOnTheDisk, _internalVolumeDiskSizePerDisk);
            var internalDiskNumber = checked((Int32)internalDiskNumberUInt64);
            if (internalDiskNumber >= _internalVolumeDisks.Length)
                throw new ArgumentOutOfRangeException($"An attempt was made to access a position outside the bounds of the volume disk in a 7-zip style multi-volume ZIP file.: {nameof(offsetOnTheDisk)}=0x{offsetOnTheDisk:16}", nameof(offsetOnTheDisk));

            var (_, volumeSize, stream) = _internalVolumeDisks.Span[internalDiskNumber];
            if (offsetOnTheInternalDisk > volumeSize)
                throw new ArgumentOutOfRangeException($"An attempt was made to access a position outside the bounds of the volume disk in a 7-zip style multi-volume ZIP file.: {nameof(offsetOnTheDisk)}=0x{offsetOnTheDisk:16}", nameof(offsetOnTheDisk));

            _currentInternalVolmeDiskNumber = internalDiskNumber;
            stream.Seek(offsetOnTheInternalDisk);
        }

        protected override Boolean ValidatePosition(UInt32 diskNumber, UInt64 offsetOnTheDisk)
            => diskNumber == 0 && offsetOnTheDisk <= _totalDiskSize;

        protected override ZipStreamPosition Add(UInt32 diskNumber, UInt64 offsetOnTheDisk, UInt64 offset)
        {
            if (diskNumber != 0)
                throw new InternalLogicalErrorException();

            var newOffset = checked(offsetOnTheDisk + offset);
            if (newOffset > _totalDiskSize)
                throw new OverflowException();

            return new ZipStreamPosition(0, newOffset, this);
        }

        protected override ZipStreamPosition Subtract(UInt32 diskNumber, UInt64 offsetOnTheDisk, UInt64 offset)
        {
            if (diskNumber != 0)
                throw new InternalLogicalErrorException();

            return new ZipStreamPosition(0, checked(offsetOnTheDisk - offset), this);
        }

        protected override UInt64 Subtract(UInt32 diskNumber1, UInt64 offsetOnTheDisk1, UInt32 diskNumber2, UInt64 offsetOnTheDisk2)
        {
            if (diskNumber1 != 0)
                throw new InternalLogicalErrorException();
            if (diskNumber2 != 0)
                throw new InternalLogicalErrorException();

            return checked(offsetOnTheDisk1 - offsetOnTheDisk2);
        }

        protected override IRandomInputByteStream<UInt64, UInt64>? GetCurrentStream()
        {
            while (true)
            {
                var (_, volumeSize, stream) = _internalVolumeDisks.Span[_currentInternalVolmeDiskNumber];
                if (stream.Position < volumeSize)
                    return stream;
                if (!MoveToNextDisk())
                    return null;
            }
        }

        protected override void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    for (var index = 0; index < _internalVolumeDisks.Length; ++index)
                        _internalVolumeDisks.Span[index].stream.Dispose();
                }

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }

        protected override async ValueTask DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                for (var index = 0; index < _internalVolumeDisks.Length; ++index)
                    await _internalVolumeDisks.Span[index].stream.DisposeAsync().ConfigureAwait(false);
                _isDisposed = true;
            }

            await base.DisposeAsyncCore().ConfigureAwait(false);
        }

        private Boolean MoveToNextDisk()
        {
            if (_currentInternalVolmeDiskNumber >= _internalVolumeDisks.Length - 1)
                return false;

            ++_currentInternalVolmeDiskNumber;
            _internalVolumeDisks.Span[_currentInternalVolmeDiskNumber].stream.Seek(0);
            return true;
        }
    }
}
