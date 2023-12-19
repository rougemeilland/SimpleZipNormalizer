using System;
using Utility.IO;

namespace ZipUtility
{
    internal interface IZipFileWriterParameter
    {
        FilePath ZipArchiveFile { get; }
        IZipEntryNameEncodingProvider EntryNameEncodingProvider { get; }
        Byte ThisSoftwareVersion { get; }
        ZipEntryHostSystem HostSystem { get; }

        UInt16 GetVersionNeededToExtract(
            ZipEntryCompressionMethodId compressionMethodId = ZipEntryCompressionMethodId.Unknown,
            Boolean? supportDirectory = null,
            Boolean? requiredZip64 = null);
    }
}
