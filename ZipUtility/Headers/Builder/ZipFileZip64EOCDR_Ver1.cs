using System;
using Utility;
using Utility.IO;

namespace ZipUtility.Headers.Builder
{
    internal class ZipFileZip64EOCDR_Ver1
    {
        public const Int32 FixedHeaderSize = 56;

        private static readonly UInt32 _signatureOfZip64EOCDR;

        private readonly IZipFileWriterParameter _zipWriterParameter;
        private readonly ZipStreamPosition _startOfCentralDirectoryHeaders;
        private readonly ZipStreamPosition _endOfCentralDirectoryHeaders;
        private readonly UInt64 _totalNumberOfCentralDirectoryHeaders;
        private readonly UInt32 _diskNumberOfDiskWithLastCentralDirectoryHeader;
        private readonly UInt32 _numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader;

        static ZipFileZip64EOCDR_Ver1()
        {
            _signatureOfZip64EOCDR = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x06, 0x06);
        }

        private ZipFileZip64EOCDR_Ver1(
            IZipFileWriterParameter zipWriterParameter,
            ZipStreamPosition startOfCentralDirectoryHeaders,
            ZipStreamPosition endOfCentralDirectoryHeaders,
            UInt64 totalNumberOfCentralDirectoryHeaders,
            UInt32 diskNumberOfDiskWithLastCentralDirectoryHeader,
            UInt32 numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader)
        {
            _zipWriterParameter = zipWriterParameter;
            _startOfCentralDirectoryHeaders = startOfCentralDirectoryHeaders;
            _endOfCentralDirectoryHeaders = endOfCentralDirectoryHeaders;
            _totalNumberOfCentralDirectoryHeaders = totalNumberOfCentralDirectoryHeaders;
            _diskNumberOfDiskWithLastCentralDirectoryHeader = diskNumberOfDiskWithLastCentralDirectoryHeader;
            _numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader = numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader;
        }

        public static UInt64 GetLength() => FixedHeaderSize;

        public ZipStreamPosition WriteTo(IZipOutputStream outputStream)
        {
            var positionOfThisHeader = outputStream.Position;

            // ZIP 64 EOCDR を書き込む。
            var headerBuffer = new Byte[FixedHeaderSize];
            headerBuffer.Slice(0, 4).SetValueLE(_signatureOfZip64EOCDR);
            headerBuffer.Slice(4, 8).SetValueLE(FixedHeaderSize - 12LU);
            headerBuffer.Slice(12, 2).SetValueLE(_zipWriterParameter.ThisSoftwareVersion);
            headerBuffer.Slice(14, 2).SetValueLE(_zipWriterParameter.GetVersionNeededToExtract(ZipEntryCompressionMethodId.Unknown, false, true));
            headerBuffer.Slice(16, 4).SetValueLE(positionOfThisHeader.DiskNumber);
            headerBuffer.Slice(20, 4).SetValueLE(_startOfCentralDirectoryHeaders.DiskNumber);
            headerBuffer.Slice(24, 8).SetValueLE(positionOfThisHeader.DiskNumber == _diskNumberOfDiskWithLastCentralDirectoryHeader ? _numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader : 0);
            headerBuffer.Slice(32, 8).SetValueLE(_totalNumberOfCentralDirectoryHeaders);
            headerBuffer.Slice(40, 8).SetValueLE(_endOfCentralDirectoryHeaders - _startOfCentralDirectoryHeaders);
            headerBuffer.Slice(48, 8).SetValueLE(_startOfCentralDirectoryHeaders.OffsetOnTheDisk);
            outputStream.WriteBytes(headerBuffer);

            return positionOfThisHeader;
        }

        public static ZipFileZip64EOCDR_Ver1 Build(
            IZipFileWriterParameter zipWriterParameter,
            ZipStreamPosition startOfCentralDirectoryHeaders,
            ZipStreamPosition endOfCentralDirectoryHeaders,
            UInt64 totalNumberOfCentralDirectoryHeaders,
            UInt32 diskNumberOfDiskWithLastCentralDirectoryHeader,
            UInt32 numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader)
            => new(
                zipWriterParameter,
                startOfCentralDirectoryHeaders,
                endOfCentralDirectoryHeaders,
                totalNumberOfCentralDirectoryHeaders,
                diskNumberOfDiskWithLastCentralDirectoryHeader,
                numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader);
    }
}
