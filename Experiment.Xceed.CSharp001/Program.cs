// Note: PlatformTarget must be x86.

using System;
using System.IO;
using XceedZipLib;

namespace Experiment.Xceed.CSharp001
{
    internal partial class Program
    {
        static void Main(string[] args)
        {
            var destinationDirectoryPath = args[0];
            var sourceDirectoryPath = args[1];

            // Specify all extra fields.
            CreateZipFile(
                Path.Combine(destinationDirectoryPath, "sample_all.zip"),
                sourceDirectoryPath,
                xcdExtraHeader.xehUnicode
                    | xcdExtraHeader.xehUTF8Filename
                    | xcdExtraHeader.xehUTF8Comment,
                false);

            CreateZipFile(
                Path.Combine(destinationDirectoryPath, "sample_except_0x554e.zip"),
                sourceDirectoryPath,
                xcdExtraHeader.xehUTF8Filename
                    | xcdExtraHeader.xehUTF8Comment,
                false);

            CreateZipFile(
                Path.Combine(destinationDirectoryPath, "sample_except_0x6375_0x7075.zip"),
                sourceDirectoryPath,
                xcdExtraHeader.xehUnicode,
                false);

            CreateZipFile(
                Path.Combine(destinationDirectoryPath, "sample_only_0x000a.zip"),
                sourceDirectoryPath,
                xcdExtraHeader.xehFileTimes,
                false);

            CreateZipFile(
                Path.Combine(destinationDirectoryPath, "sample_use_utf8.zip"),
                sourceDirectoryPath,
                xcdExtraHeader.xehNone,
                true);

            Console.WriteLine($"Completed.");
            _ = Console.ReadLine();
        }

        private static void CreateZipFile(string targetZipFilePath, string sourceDirectoryPath, xcdExtraHeader extraHeaders, bool useUtf8)
        {
            var xZip = new XceedZip
            {
                ZipFilename = targetZipFilePath,
                BasePath = sourceDirectoryPath,
                FilesToProcess = "*.*",
                ProcessSubfolders = true,
                ExtraHeaders = extraHeaders,
                PreservePaths = true,
                CompressionMethod = xcdCompressionMethod.xcmDeflated,
                CompressionLevel = xcdCompressionLevel.xclHigh,
                Use64BitEvents = true,
                TextEncoding = useUtf8 ? xcdTextEncoding.xteUnicode : xcdTextEncoding.xteStandard,
            };

            // Declare the required license by calling the License(string license) method on the xZip object.
            SetLicense(xZip);

            xZip.ZipComment += XZip_ZipComment;
            xZip.ZipPreprocessingFile64 += XZip_ZipPreprocessingFile64;
            var err = xZip.Zip();

            Console.WriteLine($"{err}: \"{targetZipFilePath}\"");
        }

        private static void XZip_ZipComment(ref string sComment)
            => sComment = "This is comment for zip file";

        private static void XZip_ZipPreprocessingFile64(
            ref string sFilename,
            ref string sComment,
            string sSourceFilename,
            int lSizeLow,
            int lSizeHigh,
            ref xcdFileAttributes xAttributes,
            ref DateTime dtLastModified,
            ref DateTime dtLastAccessed,
            ref DateTime dtCreated,
            ref xcdCompressionMethod xMethod,
            ref bool bEncrypted,
            ref string sPassword,
            ref bool bExcluded,
            xcdSkippingReason xReason,
            bool bExisting)
            => sComment = $"This is comment for \"{sFilename}\".";
    }
}
