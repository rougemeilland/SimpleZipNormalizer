using System;
using Utility;
using Utility.IO;

namespace ZipUtility.Headers.Builder
{
    internal class ZipFileZip64EOCDR_Ver1
    {
        public const Int32 FixedHeaderSize = 56;

        private static readonly UInt32 _signatureOfZip64EOCDR;

        private readonly ZipArchiveFileWriter.IZipFileWriterEnvironment _zipWriter;
        private readonly ReadOnlyMemory<ZipStreamPosition> _centralDirectoryPositions;
        private readonly ZipStreamPosition _endOfCentralDirectories;

        static ZipFileZip64EOCDR_Ver1()
        {
            _signatureOfZip64EOCDR = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x06, 0x06);
        }

        private ZipFileZip64EOCDR_Ver1(
            ZipArchiveFileWriter.IZipFileWriterEnvironment zipWriter,
            ReadOnlyMemory<ZipStreamPosition> centralDirectoryPositions,
            ZipStreamPosition endOfCentralDirectories)
        {
            _zipWriter = zipWriter;
            _centralDirectoryPositions = centralDirectoryPositions;
            _endOfCentralDirectories = endOfCentralDirectories;
        }

        public static UInt64 GetLength() => FixedHeaderSize;

        public ZipStreamPosition WriteTo(IZipOutputStream outputStream)
        {
            var positionOfThisHeader = outputStream.Position;

            var firstCentralDirectoryPosition =
                _centralDirectoryPositions.Length > 0
                ? _centralDirectoryPositions.Span[0]
                : _endOfCentralDirectories;

            var numberOfCentralDirectoriesOnThisDisk = 0UL;
            for (var index = _centralDirectoryPositions.Length - 1; index >= 0; --index)
            {
                if (_centralDirectoryPositions.Span[index].DiskNumber != positionOfThisHeader.DiskNumber)
                    break;
                checked
                {
                    ++numberOfCentralDirectoriesOnThisDisk;
                }
            }

            // ZIP 64 EOCDR を書き込む。
            var headerBuffer = new Byte[FixedHeaderSize];
            headerBuffer.Slice(0, 4).SetValueLE(_signatureOfZip64EOCDR);
            headerBuffer.Slice(4, 8).SetValueLE(FixedHeaderSize - 12LU);
            headerBuffer.Slice(12, 2).SetValueLE(_zipWriter.ThisSoftwareVersion);
            headerBuffer.Slice(14, 2).SetValueLE(_zipWriter.GetVersionNeededToExtract(ZipEntryCompressionMethodId.Unknown, false, true));
            headerBuffer.Slice(16, 4).SetValueLE(positionOfThisHeader.DiskNumber);
            headerBuffer.Slice(20, 4).SetValueLE(firstCentralDirectoryPosition.DiskNumber);
            headerBuffer.Slice(24, 8).SetValueLE(numberOfCentralDirectoriesOnThisDisk);
            headerBuffer.Slice(32, 8).SetValueLE(checked((UInt64)_centralDirectoryPositions.Length));
            headerBuffer.Slice(40, 8).SetValueLE(_endOfCentralDirectories - firstCentralDirectoryPosition);
            headerBuffer.Slice(48, 8).SetValueLE(firstCentralDirectoryPosition.OffsetOnTheDisk);
            outputStream.WriteBytes(headerBuffer);

            return positionOfThisHeader;
        }

        public static ZipFileZip64EOCDR_Ver1 Build(
            ZipArchiveFileWriter.IZipFileWriterEnvironment zipWriter,
            ReadOnlyMemory<ZipStreamPosition> centralDirectoryPositions,
            ZipStreamPosition endOfCentralDirectories)
            => new(
                zipWriter,
                centralDirectoryPositions,
                endOfCentralDirectories);
    }
}
