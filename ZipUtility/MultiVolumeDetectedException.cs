using System;

namespace ZipUtility
{
    /// <summary>
    /// マルチボリュームを要求されたことを示す例外オブジェクトです。
    /// </summary>
    public class MultiVolumeDetectedException
        : Exception
    {
        internal MultiVolumeDetectedException(UInt32 lastDiskNumber)
            : base($"Detected Multi-Volume ZIP file, but not supported in the stream. : disk count = {lastDiskNumber}")
        {
            LastDiskNumber = lastDiskNumber;
        }

        internal MultiVolumeDetectedException(String message, UInt32 lastDiskNumber)
            : base(message)
        {
            LastDiskNumber = lastDiskNumber;
        }

        internal MultiVolumeDetectedException(String message, UInt32 lastDiskNumber, Exception inner)
            : base(message, inner)
        {
            LastDiskNumber = lastDiskNumber;
        }

        /// <summary>
        /// 最後のボリュームのディスク番号です。
        /// </summary>
        public UInt32 LastDiskNumber { get; }
    }
}
