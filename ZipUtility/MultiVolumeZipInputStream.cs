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
    /// 一般的なスタイルのマルチボリューム ZIP アーカイブ の仮想ストリームのクラスです。
    /// </summary>
    internal class MultiVolumeZipInputStream
           : ZipInputStream
    {
        private const Int32 _STREAM_CACHE_CAPACITY = 8;

        private readonly VolumeDiskCollection _volumeDisks;
        private readonly Func<UInt32, FilePath> _volumeDiskFileGetter;
        private readonly ValidationStringency _stringency;
        private readonly VolumeDiskStreamCache<IRandomInputByteStream<UInt64>> _streamCache;

        private Boolean _isDisposed;
        private UInt32 _currentVolumeDiskNumber;
        private Boolean _isLocked;

        private MultiVolumeZipInputStream(VolumeDiskCollection volumeDisks, Func<UInt32, FilePath> volumeDiskFileGetter, ValidationStringency stringency)
        {
            if (volumeDisks.LastVolumeDiskNumber <= 0)
                throw new InternalLogicalErrorException();

            _volumeDisks = volumeDisks;
            _volumeDiskFileGetter = volumeDiskFileGetter;
            _stringency = stringency;
            _streamCache = new VolumeDiskStreamCache<IRandomInputByteStream<UInt64>>(_STREAM_CACHE_CAPACITY, OpenVolumeDisk);
            _isDisposed = false;
            _currentVolumeDiskNumber = 0;
            _isLocked = false;
            EndOfThisStreamCore = new ZipStreamPosition(_volumeDisks.LastVolumeDiskNumber, _volumeDisks.LastVolumeDiskSize, this);
        }

        public override String ToString() => $"{GetType().Name}:Count={checked((UInt64)_volumeDisks.LastVolumeDiskNumber + 1):N0}";

        public static IZipInputStream CreateInstance(IEnumerable<UInt64> volumeDiskSizes, Func<UInt32, FilePath> volumeDiskFileGetter, ValidationStringency stringency)
            => new MultiVolumeZipInputStream(new VolumeDiskCollection(volumeDiskSizes), volumeDiskFileGetter, stringency);

        protected override ZipStreamPosition EndOfThisStreamCore { get; }
        protected override UInt64 LengthCore => _volumeDisks.TotalVolumeDiskSize;

        protected override ZipStreamPosition PositionCore
        {
            get
            {
                var currentStream = GetCurrentStreamCore();
                return
                    currentStream is not null
                    ? new ZipStreamPosition(_currentVolumeDiskNumber, currentStream.Position, this)
                    : new ZipStreamPosition(_volumeDisks.LastVolumeDiskNumber, _volumeDisks.LastVolumeDiskSize, this);
            }
        }

        protected override void SeekCore(UInt32 diskNumber, UInt64 offsetOnTheDisk)
        {
            // ZIP アーカイブによっては、diskNumber と offsetOnTheDisk のペアがボリュームファイルの終端を指していることがあることに注意してください。
            // (例: PKZIP によって作成された ZIP アーカイブなど。)
            // その場合は、Seek は正常に実行し、次の GetCurrentStream() の呼び出し時に次のボリュームに遷移しなければなりません。(最後のディスクの終端を指している場合を除く)

            if (offsetOnTheDisk > GetVolumeDiskSize(diskNumber))
                throw new InternalLogicalErrorException();

            ThrowExceptionIfLocked();

            _currentVolumeDiskNumber = diskNumber;
            GetCurrentVolumeDiskStream().Seek(offsetOnTheDisk);
        }

        protected override Boolean IsMultiVolumeZipStreamCore => true;

        /// <remarks>
        /// <para>
        /// ZIP アーカイブによっては、diskNumber と offsetOnTheDisk のペアがボリュームファイルの終端を指していることがあることに注意してください。
        /// (例: PKZIP によって作成された ZIP アーカイブなど。)
        /// </para>
        /// <para>
        /// その場合は、ValidatePosition は正常に実行し、次の GetCurrentStream() の呼び出し時に次のボリュームに遷移しなければなりません。(最後のディスクの終端を指している場合を除く)
        /// </para>
        /// </remarks>
        protected override Boolean ValidatePositionCore(UInt32 diskNumber, UInt64 offsetOnTheDisk)
            => _volumeDisks.TryGetVolumeDiskSize(diskNumber, out var volumeDiskSize)
                && (_stringency > ValidationStringency.Normal
                    ? offsetOnTheDisk < volumeDiskSize
                    : offsetOnTheDisk <= volumeDiskSize);

        protected override ZipStreamPosition LastDiskStartPositionCore => new(_volumeDisks.LastVolumeDiskNumber, 0, this);
        protected override UInt64 LastDiskSizeCore => _volumeDisks.LastVolumeDiskSize;

        protected override Boolean CheckIfCanAtmicReadCore(UInt64 minimumAtomicDataSize)
        {
            while (true)
            {
                var remainOfCurrentDisk = checked(GetCurrentVolumeDiskSize() - GetCurrentVolumeDiskStream().Position);
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
                var currentVolumeStream = GetCurrentVolumeDiskStream();
                if (currentVolumeStream.Position < GetCurrentVolumeDiskSize())
                    return currentVolumeStream;
                if (!MoveToNextDisk())
                    return null;
            }
        }

        protected override ZipStreamPosition AddCore(UInt32 diskNumber, UInt64 offsetOnTheDisk, UInt64 offset)
        {
            if (!_volumeDisks.TryGetOffsetFromStart(diskNumber, offsetOnTheDisk, out var offsetFromStart))
                throw new OverflowException();
            if (!_volumeDisks.TryGetVolumeDiskPosition(checked(offsetFromStart + offset), out var newDiskNumber, out var newOffsetOnTheDisk))
                throw new OverflowException();

            return new ZipStreamPosition(newDiskNumber, newOffsetOnTheDisk, this);
        }

        protected override ZipStreamPosition SubtractCore(UInt32 diskNumber, UInt64 offsetOnTheDisk, UInt64 offset)
        {
            if (!_volumeDisks.TryGetOffsetFromStart(diskNumber, offsetOnTheDisk, out var offsetFromStart))
                throw new OverflowException();
            if (!_volumeDisks.TryGetVolumeDiskPosition(checked(offsetFromStart - offset), out var newDiskNumber, out var newOffsetOnTheDisk))
                throw new OverflowException();

            return new ZipStreamPosition(newDiskNumber, newOffsetOnTheDisk, this);
        }

        protected override UInt64 SubtractCore(UInt32 diskNumber1, UInt64 offsetOnTheDisk1, UInt32 diskNumber2, UInt64 offsetOnTheDisk2)
        {
            if (!_volumeDisks.TryGetOffsetFromStart(diskNumber1, offsetOnTheDisk1, out var offsetFromStart1))
                throw new OverflowException();
            if (!_volumeDisks.TryGetOffsetFromStart(diskNumber2, offsetOnTheDisk2, out var offsetFromStart2))
                throw new OverflowException();

            return checked(offsetFromStart1 - offsetFromStart2);
        }

        protected override (UInt32 diskNumber, UInt64 offsetOnTheDisk) NormalizeCore(UInt32 diskNumber, UInt64 offsetOnTheDisk)
            => diskNumber < _volumeDisks.LastVolumeDiskNumber && offsetOnTheDisk == GetVolumeDiskSize(diskNumber)
                ? (checked(diskNumber + 1), 0)
                : (diskNumber, offsetOnTheDisk);

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
            ThrowExceptionIfLocked();

            if (_currentVolumeDiskNumber >= _volumeDisks.LastVolumeDiskNumber)
                return false;

            checked
            {
                ++_currentVolumeDiskNumber;
            }

            GetCurrentVolumeDiskStream().Seek(0);
            return true;
        }

        private IRandomInputByteStream<UInt64> OpenVolumeDisk(UInt32 volumeDiskNumber)
        {
            var stream = (IRandomInputByteStream<UInt64>?)null;
            var success = false;
            try
            {
                if (!_volumeDisks.TryGetVolumeDiskSize(volumeDiskNumber, out var volumeDiskSize))
                    throw new InternalLogicalErrorException();

                var volumeDiskFile = _volumeDiskFileGetter(volumeDiskNumber);
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

        private void ThrowExceptionIfLocked()
        {
            if (_isLocked)
                throw new InvalidOperationException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private UInt64 GetVolumeDiskSize(UInt32 diskNumber)
        {
            if (!_volumeDisks.TryGetVolumeDiskSize(diskNumber, out var volumeDiskSize))
                throw new InternalLogicalErrorException();

            return volumeDiskSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private UInt64 GetCurrentVolumeDiskSize() => GetVolumeDiskSize(_currentVolumeDiskNumber);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private IRandomInputByteStream<UInt64> GetCurrentVolumeDiskStream() => _streamCache.GetStream(_currentVolumeDiskNumber);
    }
}
