using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Utility;
using Utility.IO;

namespace ZipUtility.ZipFileHeader
{
    class ZipFileEOCDR
    {
        private static readonly UInt32 _eocdSignature;

        // PKZIP の実装が怪しいため、プロパティの方を Obsolete にしているが、自動プロパティにすると
        // 内部コードから _numberOfCentralDirectoryRecordsOnThisDisk にアクセスできなくなってしまう。
        [SuppressMessage("Style", "IDE0032:自動プロパティを使用する", Justification = "<保留中>")]
        private readonly UInt16 _numberOfCentralDirectoryRecordsOnThisDisk;

        static ZipFileEOCDR()
        {
            _eocdSignature = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x05, 0x06);
        }

        private ZipFileEOCDR(UInt64 offsetOfThisHeader, UInt16 numberOfThisDisk, UInt16 diskWhereCentralDirectoryStarts, UInt16 numberOfCentralDirectoryRecordsOnThisDisk, UInt16 totalNumberOfCentralDirectoryRecords, UInt32 sizeOfCentralDirectory, UInt32 offsetOfStartOfCentralDirectory, ReadOnlyMemory<Byte> commentBytes)
        {
            OffsetOfThisHeader = offsetOfThisHeader;
            NumberOfThisDisk = numberOfThisDisk;
            DiskWhereCentralDirectoryStarts = diskWhereCentralDirectoryStarts;
            _numberOfCentralDirectoryRecordsOnThisDisk = numberOfCentralDirectoryRecordsOnThisDisk;
            TotalNumberOfCentralDirectoryRecords = totalNumberOfCentralDirectoryRecords;
            SizeOfCentralDirectory = sizeOfCentralDirectory;
            OffsetOfStartOfCentralDirectory = offsetOfStartOfCentralDirectory;
            CommentBytes = commentBytes;
            IsRequiresZip64 =
                NumberOfThisDisk == UInt16.MaxValue ||
                DiskWhereCentralDirectoryStarts == UInt16.MaxValue ||
                _numberOfCentralDirectoryRecordsOnThisDisk == UInt16.MaxValue ||
                TotalNumberOfCentralDirectoryRecords == UInt16.MaxValue ||
                SizeOfCentralDirectory == UInt32.MaxValue ||
                OffsetOfStartOfCentralDirectory == UInt32.MaxValue;
        }

        public UInt64 OffsetOfThisHeader { get; }
        public UInt16 NumberOfThisDisk { get; }
        public UInt16 DiskWhereCentralDirectoryStarts { get; }

        /// <summary>
        /// EOCDRのあるディスクに含まれるセントラルディレクトリレコードの数。
        /// </summary>
        /// <remarks>
        /// PKZIPの実装では、最後のディスクがEOCDRから始まっている場合 (つまり最後のディスクにセントラルディレクトリヘッダが存在しない) でも、このプロパティが1になってしまう。
        /// このプロパティの値をもとにセントラルディレクトリヘッダを探してはならない。
        /// </remarks>
        [Obsolete]
        public UInt16 NumberOfCentralDirectoryRecordsOnThisDisk => _numberOfCentralDirectoryRecordsOnThisDisk;

        public UInt16 TotalNumberOfCentralDirectoryRecords { get; }
        public UInt32 SizeOfCentralDirectory { get; }
        public UInt32 OffsetOfStartOfCentralDirectory { get; }
        public ReadOnlyMemory<Byte> CommentBytes { get; }
        public Boolean IsRequiresZip64 { get; }

        public static ZipFileEOCDR Find(IRandomInputByteStream<UInt64> zipInputStream)
        {
            var zipFileLength = zipInputStream.Length;
            var minimumLengthOfHeader = 22U;
            var maximumLengthOfHeader = 22U + UInt16.MaxValue;
            var offsetLowerLimit =
                zipFileLength > maximumLengthOfHeader
                ? zipFileLength - maximumLengthOfHeader
                : 0;
            var offsetUpperLimit =
                zipFileLength > minimumLengthOfHeader - sizeof(UInt32)
                ? zipFileLength - minimumLengthOfHeader + sizeof(UInt32)
                : 0;
            if (zipFileLength < offsetLowerLimit + minimumLengthOfHeader)
                throw new BadZipFileFormatException("Too short Zip file");
            while (offsetUpperLimit >= offsetLowerLimit + sizeof(UInt32))
            {
                var offsetOfThisHeader =
                    zipInputStream.FindLastSigunature(_eocdSignature, offsetLowerLimit, offsetUpperLimit - offsetLowerLimit)
                    ?? throw new BadZipFileFormatException("EOCD Not found in Zip file");
                zipInputStream.Seek(offsetOfThisHeader);
                try
                {
                    var minimumHeader = zipInputStream.ReadBytes(minimumLengthOfHeader);
                    var signature = minimumHeader[..4].ToUInt32LE();
                    if (signature != _eocdSignature)
                        throw new BadZipFileFormatException();
                    var numberOfThisDisk = minimumHeader.Slice(4, 2).ToUInt16LE();
                    var diskWhereCentralDirectoryStarts = minimumHeader.Slice(6, 2).ToUInt16LE();
                    var numberOfCentralDirectoryRecordsOnThisDisk = minimumHeader.Slice(8, 2).ToUInt16LE();
                    var totalNumberOfCentralDirectoryRecords = minimumHeader.Slice(10, 2).ToUInt16LE();
                    var sizeOfCentralDirectory = minimumHeader.Slice(12, 4).ToUInt32LE();
                    var offsetOfStartOfCentralDirectory = minimumHeader.Slice(16, 4).ToUInt32LE();
                    var commentLength = minimumHeader.Slice(20, 2).ToUInt16LE();
                    var commentBytes = zipInputStream.ReadBytes(commentLength);
                    return
                        new ZipFileEOCDR(
                            offsetOfThisHeader,
                            numberOfThisDisk,
                            diskWhereCentralDirectoryStarts,
                            numberOfCentralDirectoryRecordsOnThisDisk,
                            totalNumberOfCentralDirectoryRecords,
                            sizeOfCentralDirectory,
                            offsetOfStartOfCentralDirectory,
                            commentBytes);
                }
                catch (UnexpectedEndOfStreamException)
                {
                }

                offsetUpperLimit = (offsetOfThisHeader + sizeof(UInt32) - 1U).Maximum(0U);
            }

            throw new BadZipFileFormatException("EOCD Not found in Zip file");
        }
    }
}
