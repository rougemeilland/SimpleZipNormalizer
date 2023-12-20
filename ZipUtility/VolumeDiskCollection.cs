using System;
using System.Collections.Generic;
using Utility;

namespace ZipUtility
{
    /// <summary>
    /// シングルボリューム/マルチボリューム ZIP アーカイブのボリュームディスク情報を保持するクラスです。
    /// </summary>
    internal class VolumeDiskCollection
    {
        private abstract class BinaryTreeItem
        {
            public static BinaryTreeItem BuildTree(
                BigArray<(UInt64 totalOffset, UInt64 volumeDiskSize)> volumeDisks,
                UInt32 startDiskNumber,
                UInt32 endDiskNumber)
            {
                if (startDiskNumber > endDiskNumber)
                    throw new InternalLogicalErrorException();

                if (startDiskNumber == endDiskNumber)
                {
                    return
                        new BinaryTreeTerminal(
                            startDiskNumber,
                            volumeDisks[startDiskNumber].volumeDiskSize,
                            volumeDisks[startDiskNumber].totalOffset);
                }
                else
                {
                    var middleDiskNumber = startDiskNumber + ((endDiskNumber - startDiskNumber) >> 1) + 1;
                    return
                        new BinaryTreeBranch(
                            volumeDisks[middleDiskNumber].totalOffset,
                            BuildTree(volumeDisks, startDiskNumber, middleDiskNumber - 1),
                            BuildTree(volumeDisks, middleDiskNumber, endDiskNumber));
                }
            }
        }

        private class BinaryTreeTerminal
            : BinaryTreeItem
        {
            public BinaryTreeTerminal(UInt32 diskNumber, UInt64 volumeDiskSize, UInt64 totalOffset)
            {
                DiskNumber = diskNumber;
                VolumeDiskSize = volumeDiskSize;
                TotalOffset = totalOffset;
            }
            public UInt32 DiskNumber { get; }
            public UInt64 VolumeDiskSize { get; }
            public UInt64 TotalOffset { get; }
        }

        private class BinaryTreeBranch
            : BinaryTreeItem
        {
            public BinaryTreeBranch(UInt64 threshold, BinaryTreeItem lesser, BinaryTreeItem equalToOrGreaterThan)
            {
                Threshold = threshold;
                Lesser = lesser;
                EqualToOrGreaterThan = equalToOrGreaterThan;
            }

            public UInt64 Threshold { get; }
            public BinaryTreeItem Lesser { get; }
            public BinaryTreeItem EqualToOrGreaterThan { get; }
        }

        private const UInt32 _ARRAY_SIZE_STEP = 1024;

        private readonly BigArray<(UInt64 totalOffset, UInt64 volumeDiskSize)> _volumeDisks;
        private readonly BinaryTreeItem _rootNode;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="volumeDiskSizes">
        /// ボリュームディスクのサイズ (バイト数) を示す <see cref="UInt64"/> 値 の列挙子です。
        /// </param>
        public VolumeDiskCollection(IEnumerable<UInt64> volumeDiskSizes)
        {
            _volumeDisks = new BigArray<(UInt64 totalOffset, UInt64 volumeDiskSize)>(0);
            try
            {
                var totalOffset = 0UL;
                var lastVolumeDiskSize = 0UL;
                var index = 0U;
                foreach (var volumeDiskSize in volumeDiskSizes)
                {
                    if (index >= _volumeDisks.Length)
                        _volumeDisks.Resize((_volumeDisks.Length + _ARRAY_SIZE_STEP).Minimum(UInt32.MaxValue));
                    if (index >= _volumeDisks.Length)
                        throw new OutOfMemoryException();
                    _volumeDisks[index] = (totalOffset, volumeDiskSize);
                    lastVolumeDiskSize = volumeDiskSize;
                    checked
                    {
                        ++index;
                        totalOffset += volumeDiskSize;
                    }
                }

                if (index <= 0)
                    throw new ArgumentException($"{nameof(volumeDiskSizes)} is an empty sequence.");

                if (_volumeDisks.Length > index)
                    _volumeDisks.Resize(index);

                TotalVolumeDiskSize = totalOffset;
                LastVolumeDiskNumber = index - 1;
                LastVolumeDiskSize = lastVolumeDiskSize;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Duplicate disk number.", nameof(volumeDiskSizes), ex);
            }

            _rootNode = BinaryTreeItem.BuildTree(_volumeDisks, 0, LastVolumeDiskNumber);
        }

        /// <summary>
        /// 最後のボリュームディスクのディスク番号を取得します。
        /// </summary>
        /// <value>
        /// 最後のボリュームディスクのディスク番号を示す <see cref="UInt32"/> 値です。
        /// </value>
        public UInt32 LastVolumeDiskNumber { get; }

        /// <summary>
        /// 最後のボリュームディスクのサイズを取得します。
        /// </summary>
        /// <value>
        /// 最後のボリュームディスクのサイズ (バイト数) を示す <see cref="UInt64"/> 値です。
        /// </value>
        public UInt64 LastVolumeDiskSize { get; }

        /// <summary>
        /// 全ボリュームディスクの合計サイズを取得します。
        /// </summary>
        /// <value>
        /// 全ボリュームディスクの合計サイズ (バイト数) を示す <see cref="UInt64"/> 値です。
        /// </value>
        public UInt64 TotalVolumeDiskSize { get; }

        /// <summary>
        /// 最初のボリュームの先頭からのオフセットから、それと同じ位置を示すディスク番号とそのディスクの先頭からのオフセットを求めます。
        /// </summary>
        /// <param name="offsetFromStart">
        /// 最初のボリュームの先頭からのオフセット (バイト数) を示す <see cref="UInt64"/> 値です。
        /// </param>
        /// <param name="diskNumber">
        /// <paramref name="offsetFromStart"/> で示された位置のディスク番号を示す <see cref="UInt32"/> 値です。
        /// </param>
        /// <param name="offsetOnTheDisk">
        /// <paramref name="diskNumber"/> で示された位置の、ボリュームディスク <paramref name="diskNumber"/> の先頭からのオフセットです。
        /// </param>
        /// <returns>
        /// <paramref name="diskNumber"/> および <paramref name="offsetOnTheDisk"/> を求めることが出来た場合は true、そうではない場合は false が返ります。
        /// </returns>
        public Boolean TryGetVolumeDiskPosition(UInt64 offsetFromStart, out UInt32 diskNumber, out UInt64 offsetOnTheDisk)
        {
            if (offsetFromStart > TotalVolumeDiskSize)
            {
                diskNumber = 0;
                offsetOnTheDisk = 0;
                return false;
            }

            var node = _rootNode;
            while (node is BinaryTreeBranch branch)
                node = offsetFromStart >= branch.Threshold ? branch.EqualToOrGreaterThan : branch.Lesser;

            if (node is not BinaryTreeTerminal terminal)
                throw new InternalLogicalErrorException();

            diskNumber = terminal.DiskNumber;
            offsetOnTheDisk = checked(offsetFromStart - terminal.TotalOffset);
            return true;
        }

        /// <summary>
        /// ボリュームディスクの詳細情報を取得します。
        /// </summary>
        /// <param name="diskNumber">
        /// ボリュームディスクの番号を示す <see cref="UInt32"/> です。
        /// </param>
        /// <param name="volumeDiskSize">
        /// <paramref name="diskNumber"/> で示されるボリュームディスクのサイズ (バイト数) を示す <see cref="UInt64"/> オブジェクトです。
        /// </param>
        /// 詳細情報の取得に成功した場合は true、そうではない場合は false が返ります。
        /// <returns>
        /// </returns>
        public Boolean TryGetVolumeDiskSize(UInt32 diskNumber, out UInt64 volumeDiskSize)
        {
            if (diskNumber >= _volumeDisks.Length)
            {
                volumeDiskSize = 0;
                return false;
            }

            volumeDiskSize = _volumeDisks[diskNumber].volumeDiskSize;
            return true;
        }

        /// <summary>
        /// ボリュームディスクの番号とそのディスクの先頭からのオフセットから、それと同じ位置を示す最初のボリュームの先頭からのオフセットを求めます。
        /// </summary>
        /// <param name="diskNumber">
        /// ボリュームディスクの番号を示す <see cref="UInt32"/> 値です。
        /// </param>
        /// <param name="offsetOnTheDisk">
        /// <paramref name="diskNumber"/> で示されるボリュームディスクの先頭からのオフセット (バイト数) を示す <see cref="UInt64"/> 値です。
        /// </param>
        /// <param name="offsetFromStart">
        /// <paramref name="diskNumber"/> および <paramref name="offsetOnTheDisk"/> で示されるボリュームディスク上の位置と同じ位置を示す、最初のボリュームの先頭からのオフセット (バイト数) です。
        /// </param>
        /// <returns>
        /// <paramref name="offsetFromStart"/> を求めるのに成功した場合は true、そうではない場合は false が返ります。
        /// </returns>
        public Boolean TryGetOffsetFromStart(UInt32 diskNumber, UInt64 offsetOnTheDisk, out UInt64 offsetFromStart)
        {
            if (diskNumber >= _volumeDisks.Length)
            {
                offsetFromStart = 0;
                return false;
            }

            var (totalOffset, volumeDiskSize) = _volumeDisks[diskNumber];
            if (offsetOnTheDisk > volumeDiskSize)
            {
                offsetFromStart = 0;
                return false;
            }

            offsetFromStart = checked(totalOffset + offsetOnTheDisk);
            return true;
        }

        /// <summary>
        /// 全ボリュームディスクの情報を列挙します。
        /// </summary>
        /// <returns>
        /// 以下のタプルの列挙子を返します。
        /// <list type="bullet">
        /// <item>ボリュームディスクのディスク番号を示す <see cref="UInt32"/> 値</item>
        /// <item>ボリュームディスクのサイズ (バイト数) を示す <see cref="UInt64"/> 値</item>
        /// </list>
        /// </returns>
        public IEnumerable<(UInt32 diskNumber, UInt64 volumeDiskSize)> EnumerateVolumeDisks()
        {
            for (var diskNumber = 0U; diskNumber <= LastVolumeDiskNumber; ++diskNumber)
            {
                var (_, volumeDiskSize) = _volumeDisks[diskNumber];
                yield return (diskNumber, volumeDiskSize);
            }
        }
    }
}
