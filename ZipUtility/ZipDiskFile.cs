using System;
using Utility;
using Utility.IO;

namespace ZipUtility
{
    class ZipDiskFile
    {
        public ZipDiskFile(UInt32 diskNumber, FilePath diskFile, UInt64 offset)
        {
            if (!diskFile.Exists || diskFile.Length < 0)
                throw new InternalLogicalErrorException();

            DiskNumber = diskNumber;
            DiskFile = diskFile;
            Offset = offset;
            Length = (UInt64)diskFile.Length;
        }

        public UInt32 DiskNumber { get; }
        public FilePath DiskFile { get; }
        public UInt64 Offset { get; }
        public UInt64 Length { get; }
    }
}
