using System;
using Utility;

namespace ZipUtility.Headers.Parser
{
    internal class ZipFileZip64EOCDL
    {
        public const UInt32 FixedHeaderSize = 20U;

        private static readonly UInt32 _zip64EndOfCentralDirectoryLocatorSignature;

        static ZipFileZip64EOCDL()
        {
            _zip64EndOfCentralDirectoryLocatorSignature = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x06, 0x07);
        }

        private ZipFileZip64EOCDL(
            ZipStreamPosition zip64EOCDLPosition,
            UInt32 numberOfTheDiskWithTheStartOfTheZip64EndOfCentralDirectory,
            UInt64 offsetOfTheZip64EndOfCentralDirectoryRecord,
            UInt32 totalNumberOfDisks,
            UInt64 headerSize)
        {
            Zip64EOCDLPosition = zip64EOCDLPosition;
            NumberOfTheDiskWithTheStartOfTheZip64EndOfCentralDirectory = numberOfTheDiskWithTheStartOfTheZip64EndOfCentralDirectory;
            OffsetOfTheZip64EndOfCentralDirectoryRecord = offsetOfTheZip64EndOfCentralDirectoryRecord;
            TotalNumberOfDisks = totalNumberOfDisks;
            HeaderSize = headerSize;
        }

        public ZipStreamPosition Zip64EOCDLPosition { get; }
        public UInt32 NumberOfTheDiskWithTheStartOfTheZip64EndOfCentralDirectory { get; }
        public UInt64 OffsetOfTheZip64EndOfCentralDirectoryRecord { get; }
        public UInt32 TotalNumberOfDisks { get; }
        public UInt64 HeaderSize { get; }

        public Boolean CheckDiskNumber(UInt32 actualLastDiskNumber)
            => TotalNumberOfDisks == checked(actualLastDiskNumber + 1)
                && NumberOfTheDiskWithTheStartOfTheZip64EndOfCentralDirectory <= actualLastDiskNumber;

        public static ZipFileZip64EOCDL? Parse(ReadOnlySpan<Byte> buffer, ZipStreamPosition zip64EOCDLPosition)
        {
            var signature = buffer[..4].ToUInt32LE();
            if (signature != _zip64EndOfCentralDirectoryLocatorSignature)
                return null;
            var numberOfTheDiskWithTheStartOfTheZip64EndOfCentralDirectory = buffer.Slice(4, 4).ToUInt32LE();
            var offsetOfTheZip64EndOfCentralDirectoryRecord = buffer.Slice(8, 8).ToUInt64LE();
            var totalNumberOfDisks = buffer.Slice(16, 4).ToUInt32LE();
            return
                new ZipFileZip64EOCDL(
                    zip64EOCDLPosition,
                    numberOfTheDiskWithTheStartOfTheZip64EndOfCentralDirectory,
                    offsetOfTheZip64EndOfCentralDirectoryRecord,
                    totalNumberOfDisks,
                    FixedHeaderSize);
        }
    }
}
