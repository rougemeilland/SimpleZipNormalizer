using System;
using System.IO;
using Utility;

namespace ZipUtility
{
    class ZipDiskFile
    {
        public ZipDiskFile(UInt32 diskNumber, FileInfo diskFile, UInt64 offset)
        {
            if (!diskFile.Exists || diskFile.Length < 0)
                throw new InternalLogicalErrorException();

            DiskNumber = diskNumber;
            DiskFile = diskFile;
            Offset = offset;
            Length = (UInt64)diskFile.Length;
        }

        public UInt32 DiskNumber { get; }
        public FileInfo DiskFile { get; }
        public UInt64 Offset { get; }
        public UInt64 Length { get; }
    }
}
