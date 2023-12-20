using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
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

        private readonly VolumeDiskCollection _internalVolumeDisks;
        private readonly Func<UInt32, FilePath> _internalVolumeDiskFileGetter;
        private readonly VolumeDiskStreamCache<IRandomInputByteStream<UInt64>> _streamCache;

        private Boolean _isDisposed;
        private UInt32 _currentInternalVolmeDiskNumber;

        private SevenZipStyleMultiVolumeZipInputStream(VolumeDiskCollection internalVolumeDisks, Func<UInt32, FilePath> internalVolumeDiskFileGetter)
        {
            _internalVolumeDisks = internalVolumeDisks;
            _internalVolumeDiskFileGetter = internalVolumeDiskFileGetter;
            _streamCache = new VolumeDiskStreamCache<IRandomInputByteStream<UInt64>>(_STREAM_CACHE_CAPACITY, OpenInternalVolumeDisk);
            _isDisposed = false;
            _currentInternalVolmeDiskNumber = 0;
        }

        public override String ToString() => $"{GetType().Name}:Count={checked((UInt64)_internalVolumeDisks.LastVolumeDiskNumber + 1):N0}";

        public static IZipInputStream CreateInstance(IEnumerable<UInt64> volumeDiskSizes, Func<UInt32, FilePath> internalVolumeDiskFileGetter)
        {
            var volumeDisks = new VolumeDiskCollection(volumeDiskSizes);

            // 最後以外のディスクのサイズは同じであることの検査をする。
            var volumeDiskSizeExceptLastDisk = (UInt64?)null;
            foreach (var (diskNumber, volumeDiskSize) in volumeDisks.EnumerateVolumeDisks())
            {
                if (diskNumber < volumeDisks.LastVolumeDiskNumber)
                {
                    if (volumeDiskSizeExceptLastDisk is null)
                    {
                        volumeDiskSizeExceptLastDisk = volumeDiskSize;
                    }
                    else
                    {
                        if (volumeDiskSize != volumeDiskSizeExceptLastDisk.Value)
                            throw new BadZipFileFormatException("Volume disks with different sizes were found among volume disks other than the last.");
                    }
                }
            }

            return new SevenZipStyleMultiVolumeZipInputStream(volumeDisks, internalVolumeDiskFileGetter);
        }

        protected override ZipStreamPosition EndOfThisStreamCore => new(0, _internalVolumeDisks.TotalVolumeDiskSize, this);
        protected override UInt64 LengthCore => _internalVolumeDisks.TotalVolumeDiskSize;
        protected override ZipStreamPosition PositionCore
        {
            get
            {

                if (!_internalVolumeDisks.TryGetOffsetFromStart(_currentInternalVolmeDiskNumber, GetCurrentVolumeDiskStream().Position, out var offset))
                    throw new InternalLogicalErrorException();

                return new(0, offset, this);
            }
        }

        protected override void SeekCore(UInt32 diskNumber, UInt64 offsetOnTheDisk)
        {
            if (diskNumber != 0)
                throw new InternalLogicalErrorException();
            if (offsetOnTheDisk > _internalVolumeDisks.TotalVolumeDiskSize)
                throw new ArgumentOutOfRangeException($"An attempt was made to access a position outside the bounds of the volume disk in a 7-zip style multi-volume ZIP file.: {nameof(offsetOnTheDisk)}=0x{offsetOnTheDisk:x16}", nameof(offsetOnTheDisk));

            if (!_internalVolumeDisks.TryGetVolumeDiskPosition(offsetOnTheDisk, out var internalDiskNumber, out var internalOffsetOnTheDisk))
                throw new InternalLogicalErrorException();

            _currentInternalVolmeDiskNumber = internalDiskNumber;
            GetCurrentVolumeDiskStream().Seek(internalOffsetOnTheDisk);
        }

        protected override Boolean ValidatePositionCore(UInt32 diskNumber, UInt64 offsetOnTheDisk)
            => diskNumber == 0 && offsetOnTheDisk <= _internalVolumeDisks.TotalVolumeDiskSize;

        protected override IRandomInputByteStream<UInt64>? GetCurrentStreamCore()
        {
            while (true)
            {
                var currentInternalVolmeStream = GetCurrentVolumeDiskStream();
                if (currentInternalVolmeStream.Position < GetCurrentVolumeDiskSize())
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
            if (newOffset > _internalVolumeDisks.TotalVolumeDiskSize)
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
            if (_currentInternalVolmeDiskNumber >= _internalVolumeDisks.LastVolumeDiskNumber)
                return false;

            checked
            {
                ++_currentInternalVolmeDiskNumber;
            }

            _streamCache.GetStream(_currentInternalVolmeDiskNumber).Seek(0);
            return true;
        }

        private IRandomInputByteStream<UInt64> OpenInternalVolumeDisk(UInt32 internalVolumeDiskNumber)
        {
            var stream = (IRandomInputByteStream<UInt64>?)null;
            var success = false;
            try
            {
                if (!_internalVolumeDisks.TryGetVolumeDiskSize(internalVolumeDiskNumber, out var volumeDiskSize))
                    throw new InternalLogicalErrorException();

                var volumeDiskFile = _internalVolumeDiskFileGetter(internalVolumeDiskNumber);
                try
                {
                    stream = volumeDiskFile.OpenRead().WithCache();
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to open the ZIP archive volume file.: path=\"{volumeDiskFile.FullName}\"", ex);
                }

                if (stream.Length != volumeDiskSize)
                    throw new IOException($"Detected that the size of a ZIP archive's volume file has changed.: path=\"{volumeDiskFile.FullName}\"");

                success = true;
                return stream;
            }
            finally
            {
                if (!success)
                    stream?.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private UInt64 GetVolumeDiskSize(UInt32 diskNumber)
        {
            if (!_internalVolumeDisks.TryGetVolumeDiskSize(diskNumber, out var volumeDiskSize))
                throw new InternalLogicalErrorException();

            return volumeDiskSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private UInt64 GetCurrentVolumeDiskSize() => GetVolumeDiskSize(_currentInternalVolmeDiskNumber);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private IRandomInputByteStream<UInt64> GetCurrentVolumeDiskStream() => _streamCache.GetStream(_currentInternalVolmeDiskNumber);
    }
}
