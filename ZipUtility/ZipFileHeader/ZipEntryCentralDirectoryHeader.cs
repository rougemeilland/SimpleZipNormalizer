using System;
using System.Collections.Generic;
using System.Threading;
using Utility;
using Utility.IO;
using ZipUtility.ZipExtraField;

namespace ZipUtility.ZipFileHeader
{
    internal class ZipEntryCentralDirectoryHeader
         : ZipEntryInternalHeader
    {
        private static readonly UInt32 _centralHeaderSignature;

        static ZipEntryCentralDirectoryHeader()
        {
            _centralHeaderSignature = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x01, 0x02);
        }

        private ZipEntryCentralDirectoryHeader(
            IZipEntryNameEncodingProvider zipEntryNameEncodingProvider,
            ZipStreamPosition LocalHeaderPosition,
            Int32 index,
            ZipEntryHostSystem hostSystem,
            Byte versionMadeBy,
            UInt16 versionNeededToExtract,
            ZipEntryGeneralPurposeBitFlag generalPurposeBitFlag,
            ZipEntryCompressionMethodId compressionMethodId,
            DateTime? dosDateTime,
            UInt32 rawCrc,
            UInt32 rawPackedSize,
            UInt32 rawSize,
            UInt16 rawDiskStartNumber,
            UInt32 rawRelativeHeaderOffset,
            UInt64 packedSize,
            UInt64 size,
            UInt32 externalFileAttributes,
            ReadOnlyMemory<Byte> fullNameBytes,
            ReadOnlyMemory<Byte> commentBytes,
            ExtraFieldStorage extraFields,
            Boolean requiredZip64)
            : base(
                  zipEntryNameEncodingProvider,
                  LocalHeaderPosition,
                  versionNeededToExtract,
                  generalPurposeBitFlag,
                  compressionMethodId,
                  dosDateTime,
                  rawCrc,
                  rawPackedSize,
                  rawSize,
                  packedSize,
                  size,
                  fullNameBytes,
                  commentBytes,
                  extraFields,
                  requiredZip64)
        {
            Index = index;
            HostSystem = hostSystem;
            VersionMadeBy = versionMadeBy;
            RawDiskStartNumber = rawDiskStartNumber;
            RawRelativeHeaderOffset = rawRelativeHeaderOffset;
            ExternalFileAttributes = externalFileAttributes;
            IsDirectiry = CheckIfEntryNameIsDirectoryName();
        }

        public Int32 Index { get; } // ZIPの仕様上はエントリ数の最大値は UInt64.MaxValue であるが、.NETで扱える配列のインデックスの制限から Int32 にしている
        public ZipEntryHostSystem HostSystem { get; }
        public Byte VersionMadeBy { get; }
        public override UInt32 Crc => RawCrc;
        public UInt16 RawDiskStartNumber { get; }
        public UInt32 ExternalFileAttributes { get; }
        public UInt32 RawRelativeHeaderOffset { get; }
        public Boolean IsDirectiry { get; }
        public Boolean IsFile => !IsDirectiry;

        public static IEnumerable<ZipEntryCentralDirectoryHeader> Enumerate(
            ZipArchiveFileReader.IZipReaderEnvironment zipReader,
            ZipArchiveFileReader.IZipReaderStream zipStream,
            ZipStreamPosition centralDirectoryPosition,
            UInt64 centralHeadersCount,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            zipStream.Stream.Seek(centralDirectoryPosition);
            var centralHeaders = new List<ZipEntryCentralDirectoryHeader>();
            if (centralHeadersCount > Int32.MaxValue)
                throw new NotSupportedSpecificationException($"More than {Int32.MaxValue} entries are not supported.");
            for (var index = 0; (UInt32)index < centralHeadersCount; ++index)
            {
                // ZIP (ZIP64) の仕様上は index の最大値は UInt64.MaxValue であるが、.NETで扱える配列のインデックスの制限から Int32 にしている

                cancellationToken.ThrowIfCancellationRequested();

                centralHeaders.Add(Parse(zipReader, zipStream, index));
            }

            return centralHeaders;
        }

        private static ZipEntryCentralDirectoryHeader Parse(
            ZipArchiveFileReader.IZipReaderEnvironment zipReader,
            ZipArchiveFileReader.IZipReaderStream zipStream,
            Int32 index)
        {
            var minimumLengthOfHeader = 46;
            var minimumHeaderBytes = zipStream.Stream.ReadBytes(minimumLengthOfHeader);
            var signature = minimumHeaderBytes[..4].ToUInt32LE();
            if (signature != _centralHeaderSignature)
                throw new BadZipFileFormatException("Not found central header in expected position");
            var hostSystemAndVersionMadeBy = minimumHeaderBytes.Slice(4, 2).ToUInt16LE();
            var hostSystem = (ZipEntryHostSystem)(hostSystemAndVersionMadeBy >> 8);
            var versionMadeBy = (Byte)hostSystemAndVersionMadeBy;
            var versionNeededToExtract = minimumHeaderBytes.Slice(6, 2).ToUInt16LE();
            if (!zipReader.CheckVersion(versionNeededToExtract))
                throw new NotSupportedSpecificationException($"Unsupported version of ZIP file format. : Version of this software={zipReader.ThisSoftwareVersion}, VersionNeededToExtract={versionNeededToExtract}");
            var generalPurposeBitFlag = (ZipEntryGeneralPurposeBitFlag)minimumHeaderBytes.Slice(8, 2).ToUInt16LE();
            if (generalPurposeBitFlag.HasEncryptionFlag())
                throw new EncryptedZipFileNotSupportedException((generalPurposeBitFlag & (ZipEntryGeneralPurposeBitFlag.Encrypted | ZipEntryGeneralPurposeBitFlag.EncryptedCentralDirectory | ZipEntryGeneralPurposeBitFlag.StrongEncrypted)).ToString());
            if (generalPurposeBitFlag.HasFlag(ZipEntryGeneralPurposeBitFlag.CompressedPatchedData))
                throw new NotSupportedSpecificationException("Not supported \"Compressed Patched Data\".");
            var compressionMethodId = (ZipEntryCompressionMethodId)minimumHeaderBytes.Slice(10, 2).ToUInt16LE();
            var dosTime = minimumHeaderBytes.Slice(12, 2).ToUInt16LE();
            var dosDate = minimumHeaderBytes.Slice(14, 2).ToUInt16LE();
            var rawCrc = minimumHeaderBytes.Slice(16, 4).ToUInt32LE();
            var rawPackedSize = minimumHeaderBytes.Slice(20, 4).ToUInt32LE();
            var rawSize = minimumHeaderBytes.Slice(24, 4).ToUInt32LE();
            var fileNameLength = minimumHeaderBytes.Slice(28, 2).ToUInt16LE();
            var extraFieldLength = minimumHeaderBytes.Slice(30, 2).ToUInt16LE();
            var commentLength = minimumHeaderBytes.Slice(32, 2).ToUInt16LE();
            var rawDiskStartNumber = minimumHeaderBytes.Slice(34, 2).ToUInt16LE();
            var externalFileAttribute = minimumHeaderBytes.Slice(38, 4).ToUInt32LE();
            var rawRelativeLocalFileHeaderOffset = minimumHeaderBytes.Slice(42, 4).ToUInt32LE();

            var fullNameBytes = zipStream.Stream.ReadBytes(fileNameLength);
            var extraFieldDataSource = zipStream.Stream.ReadBytes(extraFieldLength);
            var commentBytes = zipStream.Stream.ReadBytes(commentLength);

            var dosDateTime =
                (dosDate == 0 && dosTime == 0)
                    ? (DateTime?)null
                    : (dosDate, dosTime).FromDosDateTimeToDateTime(DateTimeKind.Local);
            var extraFields = new ExtraFieldStorage(ZipEntryHeaderType.CentralDirectoryHeader, extraFieldDataSource);
            var zip64ExtraFieldValue = extraFields.GetExtraField<Zip64ExtendedInformationExtraFieldForCentraHeader>();
            if (zip64ExtraFieldValue is null)
            {
                return
                    new ZipEntryCentralDirectoryHeader(
                        zipReader.ZipEntryNameEncodingProvider,
                        zipStream.Stream.GetPosition(rawDiskStartNumber, rawRelativeLocalFileHeaderOffset),
                        index,
                        hostSystem,
                        versionMadeBy,
                        versionNeededToExtract,
                        generalPurposeBitFlag,
                        compressionMethodId,
                        dosDateTime,
                        rawCrc,
                        rawPackedSize,
                        rawSize,
                        rawDiskStartNumber,
                        rawRelativeLocalFileHeaderOffset,
                        rawPackedSize,
                        rawSize,
                        externalFileAttribute,
                        fullNameBytes,
                        commentBytes,
                        extraFields,
                        false);
            }
            else
            {
                var (size, packedSize, relatiiveHeaderOffset, diskStartNumber) =
                    zip64ExtraFieldValue.GetValues(rawSize, rawPackedSize, rawRelativeLocalFileHeaderOffset, rawDiskStartNumber);
                return
                    new ZipEntryCentralDirectoryHeader(
                        zipReader.ZipEntryNameEncodingProvider,
                        zipStream.Stream.GetPosition(diskStartNumber, relatiiveHeaderOffset),
                        index,
                        hostSystem,
                        versionMadeBy,
                        versionNeededToExtract,
                        generalPurposeBitFlag,
                        compressionMethodId,
                        dosDateTime,
                        rawCrc,
                        rawPackedSize,
                        rawSize,
                        rawDiskStartNumber,
                        rawRelativeLocalFileHeaderOffset,
                        packedSize,
                        size,
                        externalFileAttribute,
                        fullNameBytes,
                        commentBytes,
                        extraFields,
                        true);
            }
        }

        private Boolean CheckIfEntryNameIsDirectoryName()
        {
            if (FullName.EndsWith('/'))
                return true;
            if (Size == 0 && PackedSize == 0 && !String.IsNullOrEmpty(FullName) && FullName.EndsWith('\\'))
            {
                switch (HostSystem)
                {
                    case ZipEntryHostSystem.FAT:
                    case ZipEntryHostSystem.Windows_NTFS:
                    case ZipEntryHostSystem.OS2_HPFS:
                    case ZipEntryHostSystem.VFAT:
                        return true;
                    default:
                        break;
                }
            }

            return
                HostSystem switch
                {
                    ZipEntryHostSystem.Amiga
                        => (ExternalFileAttributes & 0x0c000000U) == 0x08000000,
                    ZipEntryHostSystem.FAT or ZipEntryHostSystem.Windows_NTFS or ZipEntryHostSystem.OS2_HPFS or ZipEntryHostSystem.VFAT
                        => ((ExternalAttributesForDos)ExternalFileAttributes).HasFlag(ExternalAttributesForDos.DOS_DIRECTORY),
                    ZipEntryHostSystem.UNIX or ZipEntryHostSystem.Macintosh
                        => (((ExternalAttributesForUnix)ExternalFileAttributes) & ExternalAttributesForUnix.UNX_IFMT) == ExternalAttributesForUnix.UNX_IFDIR,
                    _ => false,
                };
        }
    }
}
