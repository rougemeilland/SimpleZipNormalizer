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
        private readonly ZipStreamPosition _startOfCentralDirectoryHeaders;
        private readonly ZipStreamPosition _endOfCentralDirectoryHeaders;
        private readonly UInt64 _totalNumberOfCentralDirectoryHeaders;
        private readonly UInt32 _diskNumberOfDiskWithLastCentralDirectoryHeader;
        private readonly UInt32 _numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader;
        private readonly ReadOnlyMemory<Byte> _commentBytes;

        static ZipFileEOCDR()
        {
            _signatureOfEOCDR = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x05, 0x06);
        }

        private ZipFileEOCDR(
            ZipStreamPosition startOfCentralDirectoryHeaders,
            ZipStreamPosition endOfCentralDirectoryHeaders,
            UInt64 totalNumberOfCentralDirectoryHeaders,
            UInt32 diskNumberOfDiskWithLastCentralDirectoryHeader,
            UInt32 numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader,
            ReadOnlyMemory<Byte> commentBytes)
        {
            _startOfCentralDirectoryHeaders = startOfCentralDirectoryHeaders;
            _endOfCentralDirectoryHeaders = endOfCentralDirectoryHeaders;
            _totalNumberOfCentralDirectoryHeaders = totalNumberOfCentralDirectoryHeaders;
            _diskNumberOfDiskWithLastCentralDirectoryHeader = diskNumberOfDiskWithLastCentralDirectoryHeader;
            _numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader = numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader;
            _commentBytes = commentBytes;
        }

        public void WriteTo(IZipOutputStream outputStream)
        {
            var positionOfThisHeader = outputStream.Position;

            var headerBuffer = new Byte[MinimumHeaderSize];
            headerBuffer.Slice(0, 4).SetValueLE(_signatureOfEOCDR);
            headerBuffer.Slice(4, 2).SetValueLE(checked((UInt16)positionOfThisHeader.DiskNumber.Minimum(UInt16.MaxValue)));
            headerBuffer.Slice(6, 2).SetValueLE(checked((UInt16)_startOfCentralDirectoryHeaders.DiskNumber.Minimum(UInt16.MaxValue)));
            headerBuffer.Slice(8, 2).SetValueLE(checked((UInt16)(positionOfThisHeader.DiskNumber == _diskNumberOfDiskWithLastCentralDirectoryHeader ? _numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader : 0).Minimum(UInt16.MaxValue)));
            headerBuffer.Slice(10, 2).SetValueLE(checked((UInt16)_totalNumberOfCentralDirectoryHeaders.Minimum(UInt16.MaxValue)));
            headerBuffer.Slice(12, 4).SetValueLE(checked((UInt32)(_endOfCentralDirectoryHeaders - _startOfCentralDirectoryHeaders).Minimum(UInt32.MaxValue)));
            headerBuffer.Slice(16, 4).SetValueLE(checked((UInt32)_startOfCentralDirectoryHeaders.OffsetOnTheDisk.Minimum(UInt32.MaxValue)));
            headerBuffer.Slice(20, 2).SetValueLE(checked((UInt16)_commentBytes.Length));
            outputStream.WriteBytes(headerBuffer);
            outputStream.WriteBytes(_commentBytes);
        }

        public static UInt64 GetLength(ReadOnlyMemory<Byte> commentBytes) => checked((UInt64)(22 + commentBytes.Length));

        public static ZipFileEOCDR Build(
            ZipStreamPosition startOfCentralDirectoryHeaders,
            ZipStreamPosition endOfCentralDirectoryHeaders,
            UInt64 totalNumberOfCentralDirectoryHeaders,
            UInt32 diskNumberOfDiskWithLastCentralDirectoryHeader,
            UInt32 numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader,
            ReadOnlyMemory<Byte> commentBytes)
        {
            if (commentBytes.Length > UInt16.MaxValue)
                throw new InternalLogicalErrorException();

            return
                new ZipFileEOCDR(
                    startOfCentralDirectoryHeaders,
                    endOfCentralDirectoryHeaders,
                    totalNumberOfCentralDirectoryHeaders,
                    diskNumberOfDiskWithLastCentralDirectoryHeader,
                    numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader,
                    commentBytes);
        }
    }
}
