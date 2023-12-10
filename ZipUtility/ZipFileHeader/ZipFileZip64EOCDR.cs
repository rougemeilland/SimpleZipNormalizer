using System;
using System.Collections.Generic;
using System.Linq;
using Utility;
using Utility.IO;

namespace ZipUtility.ZipFileHeader
{
    internal class ZipFileZip64EOCDR
    {
        public const UInt32 MinimumHeaderSize = 56U;

        private static readonly UInt32 _zip64EndOfCentralDirectoryRecordSignature;

        static ZipFileZip64EOCDR()
        {
            _zip64EndOfCentralDirectoryRecordSignature = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x06, 0x06);
        }

        private ZipFileZip64EOCDR(UInt64 offsetOfThisHeader, UInt16 versionMadeBy, UInt16 versionNeededToExtract, UInt32 numberOfThisDisk, UInt32 numberOfTheDiskWithTheStartOfTheCentralDirectory, UInt64 totalNumberOfEntriesInTheCentralDirectoryOnThisDisk, UInt64 totalNumberOfEntriesInTheCentralDirectory, UInt64 sizeOfTheCentralDirectory, UInt64 offsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber, IEnumerable<Byte> zip64ExtensibleDataSector)
        {
            OffsetOfThisHeader = offsetOfThisHeader;
            VersionMadeBy = versionMadeBy;
            VersionNeededToExtract = versionNeededToExtract;
            NumberOfThisDisk = numberOfThisDisk;
            NumberOfTheDiskWithTheStartOfTheCentralDirectory = numberOfTheDiskWithTheStartOfTheCentralDirectory;
            TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk = totalNumberOfEntriesInTheCentralDirectoryOnThisDisk;
            TotalNumberOfEntriesInTheCentralDirectory = totalNumberOfEntriesInTheCentralDirectory;
            SizeOfTheCentralDirectory = sizeOfTheCentralDirectory;
            OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber = offsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber;
            Zip64ExtensibleDataSector = zip64ExtensibleDataSector.ToArray();
        }

        public UInt64 OffsetOfThisHeader { get; }
        public UInt16 VersionMadeBy { get; }
        public UInt16 VersionNeededToExtract { get; }
        public UInt32 NumberOfThisDisk { get; }
        public UInt32 NumberOfTheDiskWithTheStartOfTheCentralDirectory { get; }
        public UInt64 TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk { get; }
        public UInt64 TotalNumberOfEntriesInTheCentralDirectory { get; }
        public UInt64 SizeOfTheCentralDirectory { get; }
        public UInt64 OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber { get; }
        public IEnumerable<Byte> Zip64ExtensibleDataSector { get; }

        public static ZipFileZip64EOCDR Parse(
            ZipArchiveFileReader.IZipReaderEnvironment zipReader,
            IZipInputStream zipInputStream,
            ZipFileZip64EOCDL zip64EOCDL)
        {
            var zip64EOCDRPositionGivenByZip64EOCDL =
                zipInputStream.GetPosition(
                    zip64EOCDL.NumberOfTheDiskWithTheStartOfTheZip64EndOfCentralDirectory,
                    zip64EOCDL.OffsetOfTheZip64EndOfCentralDirectoryRecord)
                ?? throw new InternalLogicalErrorException();

            try
            {
                zipInputStream.Seek(zip64EOCDRPositionGivenByZip64EOCDL);
            }
            catch (ArgumentException ex)
            {
                throw new BadZipFileFormatException($"Unable to read ZIP64 EOCDR on ZIP archive.: position=\"{zip64EOCDRPositionGivenByZip64EOCDL}\"", ex);
            }

            // ZIP ストリームに対し、最低読み込みバイト数とともに、これからボリュームをロックした状態でのヘッダの読み込みを行うことを宣言する。
            // これが必要な理由は、もしこの時点でアクセス位置がボリュームの終端にある場合は、次のボリュームへの移動を促すため。
            // もし、現在のボリュームの残りバイト数がヘッダの最小サイズ未満である場合は、ヘッダがボリューム間で分割されていると判断し、ZIP アーカイブの破損とみなす。
            // ※ ZIP の仕様上、すべてのヘッダは ボリューム境界をまたいではならない。
            if (!zipInputStream.CheckIfCanAtomicRead(MinimumHeaderSize))
                throw new BadZipFileFormatException($"ZIP64 EOCDR is not in the expected position or is fragmented.: position=\"{zipInputStream.Position}\"");

            var zip64EOCDRPosition = zipInputStream.Position;

            // ボリュームをロックする。これ以降、ボリュームをまたいだ読み込みが禁止される。
            zipInputStream.LockVolumeDisk();
            try
            {
                var minimumHeaderBytes = zipInputStream.ReadBytes(MinimumHeaderSize);
                if (minimumHeaderBytes.Length != checked((Int32)MinimumHeaderSize))
                    throw new BadZipFileFormatException($"Unable to read ZIP64 EOCDR to the end.: position=\"{zip64EOCDRPosition}\"");
                var signature = minimumHeaderBytes[..4].ToUInt32LE();
                if (signature != _zip64EndOfCentralDirectoryRecordSignature)
                    throw new BadZipFileFormatException("Not found 'zip64 end of central directory record' for ZIP-64");
                var sizeOfZip64EndOfCentralDirectoryRecord = minimumHeaderBytes.Slice(4, 8).ToUInt64LE();
                var versionMadeBy = minimumHeaderBytes.Slice(12, 2).ToUInt16LE();
                var versionNeededToExtract = minimumHeaderBytes.Slice(14, 2).ToUInt16LE();
                if (!zipReader.CheckVersion(versionNeededToExtract))
                    throw new NotSupportedSpecificationException($"Unsupported version of ZIP file format. : Version of this software={zipReader.ThisSoftwareVersion}, VersionNeededToExtract={versionNeededToExtract}");
                var numberOfThisDisk = minimumHeaderBytes.Slice(16, 4).ToUInt32LE();
                var numberOfTheDiskWithTheStartOfTheCentralDirectory = minimumHeaderBytes.Slice(20, 4).ToUInt32LE();
                var totalNumberOfEntriesInTheCentralDirectoryOnThisDisk = minimumHeaderBytes.Slice(24, 8).ToUInt64LE();
                var totalNumberOfEntriesInTheCentralDirectory = minimumHeaderBytes.Slice(32, 8).ToUInt64LE();
                var sizeOfTheCentralDirectory = minimumHeaderBytes.Slice(40, 8).ToUInt64LE();
                var offsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber = minimumHeaderBytes.Slice(48, 8).ToUInt64LE();
                var zip64ExtensibleDataSector = zipInputStream.ReadByteSequence(sizeOfZip64EndOfCentralDirectoryRecord - MinimumHeaderSize + 12);
                var centralDirectoryEncryptionHeader = ZipFileCentralDirectoryEncryptionHeader.Parse(zip64ExtensibleDataSector);
                if (centralDirectoryEncryptionHeader is not null)
                    throw new EncryptedZipFileNotSupportedException(ZipEntryGeneralPurposeBitFlag.EncryptedCentralDirectory.ToString());

                if (numberOfThisDisk >= zip64EOCDL.TotalNumberOfDisks)
                    throw new BadZipFileFormatException($"The value of field \"{nameof(numberOfThisDisk)}\" in ZIP64 EOCDR is greater than the last disk number.: {nameof(numberOfThisDisk)}=0x{numberOfThisDisk:8}");

                if (totalNumberOfEntriesInTheCentralDirectoryOnThisDisk > totalNumberOfEntriesInTheCentralDirectory)
                    throw new BadZipFileFormatException($"TThe value of field \"{nameof(totalNumberOfEntriesInTheCentralDirectoryOnThisDisk)}\" in ZIP64 EOCDR exceeds the value of field \"{nameof(totalNumberOfEntriesInTheCentralDirectory)}\".: {nameof(totalNumberOfEntriesInTheCentralDirectoryOnThisDisk)}=0x{totalNumberOfEntriesInTheCentralDirectoryOnThisDisk:8}, {nameof(totalNumberOfEntriesInTheCentralDirectory)}=0x{totalNumberOfEntriesInTheCentralDirectory:8}");

                if (sizeOfTheCentralDirectory > zip64EOCDRPosition - zipInputStream.StartOfThisStream)
                    throw new BadZipFileFormatException($"The value of field \"{sizeOfTheCentralDirectory}\" in ZIP64 EOCDR is too large. : {nameof(sizeOfTheCentralDirectory)}=0x{sizeOfTheCentralDirectory:x16}, totalLengthUpToZip64EOCDR=0x{zip64EOCDRPosition - zipInputStream.StartOfThisStream:16}");

                return
                    new ZipFileZip64EOCDR(
                        zip64EOCDL.OffsetOfTheZip64EndOfCentralDirectoryRecord,
                        versionMadeBy,
                        versionNeededToExtract,
                        numberOfThisDisk,
                        numberOfTheDiskWithTheStartOfTheCentralDirectory,
                        totalNumberOfEntriesInTheCentralDirectoryOnThisDisk,
                        totalNumberOfEntriesInTheCentralDirectory,
                        sizeOfTheCentralDirectory,
                        offsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber,
                        zip64ExtensibleDataSector);
            }
            catch (InvalidOperationException ex)
            {
                // ボリュームがロックされている最中に、ボリュームをまたいだ読み込みが行われた場合

                // ヘッダがボリュームをまたいでいると判断し、ZIP アーカイブの破損とみなす。
                throw new BadZipFileFormatException($"It is possible that the ZIP64 EOCDR is split across multiple disks.: position=\"{zip64EOCDRPosition}\"", ex);
            }
            catch (UnexpectedEndOfStreamException ex)
            {
                // ヘッダの読み込み中に ZIP アーカイブの終端に達した場合

                // ZIP アーカイブの破損とみなす。
                throw new BadZipFileFormatException($"Unable to read ZIP64 EOCDR.: position=\"{zip64EOCDRPosition}\"", ex);
            }
            finally
            {
                // ボリュームのロックを解除する。
                zipInputStream.UnlockVolumeDisk();
            }
        }
    }
}
