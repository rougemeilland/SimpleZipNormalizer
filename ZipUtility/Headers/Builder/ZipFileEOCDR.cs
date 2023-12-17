using System;
using Utility;
using Utility.IO;

namespace ZipUtility.Headers.Builder
{
    internal class ZipFileEOCDR
    {
        public const Int32 MinimumHeaderSize = 22;
        public const Int32 MaximumHeaderSize = 22 + UInt16.MaxValue;

        private static readonly UInt32 _signatureOfEOCDR;
        private readonly ReadOnlyMemory<ZipStreamPosition> _centralDirectoryPositions;
        private readonly ZipStreamPosition _endOfCentralDirectories;
        private readonly ReadOnlyMemory<Byte> _commentBytes;

        static ZipFileEOCDR()
        {
            _signatureOfEOCDR = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x05, 0x06);
        }

        private ZipFileEOCDR(
            ReadOnlyMemory<ZipStreamPosition> centralDirectoryPositions,
            ZipStreamPosition endOfCentralDirectories,
            ReadOnlyMemory<Byte> commentBytes)
        {
            _centralDirectoryPositions = centralDirectoryPositions;
            _endOfCentralDirectories = endOfCentralDirectories;
            _commentBytes = commentBytes;
        }

        public void WriteTo(IZipOutputStream outputStream)
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

            var headerBuffer = new Byte[MinimumHeaderSize];
            headerBuffer.Slice(0, 4).SetValueLE(_signatureOfEOCDR);
            headerBuffer.Slice(4, 2).SetValueLE(checked((UInt16)positionOfThisHeader.DiskNumber.Minimum(UInt16.MaxValue)));
            headerBuffer.Slice(6, 2).SetValueLE(checked((UInt16)firstCentralDirectoryPosition.DiskNumber.Minimum(UInt16.MaxValue)));
            headerBuffer.Slice(8, 2).SetValueLE(checked((UInt16)numberOfCentralDirectoriesOnThisDisk.Minimum(UInt16.MaxValue)));
            headerBuffer.Slice(10, 2).SetValueLE(checked((UInt16)_centralDirectoryPositions.Length.Minimum(UInt16.MaxValue)));
            headerBuffer.Slice(12, 4).SetValueLE(checked((UInt32)(_endOfCentralDirectories - firstCentralDirectoryPosition).Minimum(UInt32.MaxValue)));
            headerBuffer.Slice(16, 4).SetValueLE(checked((UInt32)firstCentralDirectoryPosition.OffsetOnTheDisk.Minimum(UInt32.MaxValue)));
            headerBuffer.Slice(20, 2).SetValueLE(checked((UInt16)_commentBytes.Length));
            outputStream.WriteBytes(headerBuffer);
            outputStream.WriteBytes(_commentBytes);
        }

        public static UInt64 GetLength(ReadOnlyMemory<Byte> commentBytes) => checked((UInt64)(22 + commentBytes.Length));

        public static ZipFileEOCDR Build(
            ReadOnlyMemory<ZipStreamPosition> centralDirectoryPositions,
            ZipStreamPosition endOfCentralDirectories,
            ReadOnlyMemory<Byte> commentBytes)
        {
            if (commentBytes.Length > UInt16.MaxValue)
                throw new InternalLogicalErrorException();

            return
                new ZipFileEOCDR(
                    centralDirectoryPositions,
                    endOfCentralDirectories,
                    commentBytes);
        }
    }
}
