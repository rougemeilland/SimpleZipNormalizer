using System;
using System.Threading;
using System.Threading.Tasks;
using Utility;
using Utility.IO;
using ZipUtility.ExtraFields;

namespace ZipUtility.Headers.Parser
{
    internal class ZipEntryCentralDirectoryHeader
         : ZipEntryInternalHeader
    {
        public const UInt32 MinimumHeaderSize = 46U;
        public const UInt32 MaximumHeaderSize = MinimumHeaderSize + UInt16.MaxValue + UInt16.MaxValue + UInt16.MaxValue;

        private static readonly UInt32 _centralHeaderSignature;

        static ZipEntryCentralDirectoryHeader()
        {
            _centralHeaderSignature = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x01, 0x02);
        }

        private ZipEntryCentralDirectoryHeader(
            ZipStreamPosition centralDirectoryHeaderPosition,
            IZipEntryNameEncodingProvider zipEntryNameEncodingProvider,
            ZipStreamPosition LocalHeaderPosition,
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
            ExtraFieldCollection extraFields,
            UInt64 HeaderSize,
            Boolean requiredZip64,
            ValidationStringency stringency)
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
                  HeaderSize,
                  requiredZip64,
                  stringency)
        {
            CentralDirectoryHeaderPosition = centralDirectoryHeaderPosition;
            HostSystem = hostSystem;
            VersionMadeBy = versionMadeBy;
            RawDiskStartNumber = rawDiskStartNumber;
            RawRelativeHeaderOffset = rawRelativeHeaderOffset;
            ExternalFileAttributes = externalFileAttributes;
            IsDirectory = CheckIfEntryNameIsDirectoryName();
        }

        public ZipEntryHostSystem HostSystem { get; }
        public Byte VersionMadeBy { get; }
        public override UInt32 Crc => RawCrc;
        public UInt16 RawDiskStartNumber { get; }
        public UInt32 ExternalFileAttributes { get; }
        public UInt32 RawRelativeHeaderOffset { get; }
        public Boolean IsDirectory { get; }
        public Boolean IsFile => !IsDirectory;

        public static ZipEntryCentralDirectoryHeader Parse(
            ZipArchiveFileReader.IZipReaderEnvironment zipReader,
            ZipArchiveFileReader.IZipReaderStream zipStream,
            ValidationStringency stringency)
        {
            // ZIP ストリームに対し、最低読み込みバイト数とともに、これからボリュームをロックした状態でのヘッダの読み込みを行うことを宣言する。
            // これが必要な理由は、もしこの時点でアクセス位置がボリュームの終端にある場合は、次のボリュームへの移動を促すため。
            // もし、現在のボリュームの残りバイト数がヘッダの最小サイズ未満である場合は、ヘッダがボリューム間で分割されていると判断し、ZIP アーカイブの破損とみなす。
            // ※ ZIP の仕様上、すべてのヘッダは ボリューム境界をまたいではならない。
            if (!zipStream.Stream.CheckIfCanAtomicRead(MinimumHeaderSize))
                throw new BadZipFileFormatException($"The central directory header is not in the expected position or is fragmented.: position=\"{zipStream.Stream.Position}\"");

            var centralDirectoryHeaderPosition = zipStream.Stream.Position;

            // ボリュームをロックする。これ以降、ボリュームをまたいだ読み込みが禁止される。
            zipStream.Stream.LockVolumeDisk();
            try
            {
                var headerBytes = zipStream.Stream.ReadBytes(MinimumHeaderSize);
                var (fullNameLength, extraFieldsLength, commentLength) = ParseHeaderEasily(headerBytes, centralDirectoryHeaderPosition);
                var fullNameBytes = zipStream.Stream.ReadBytes(fullNameLength);
                if (fullNameBytes.Length != fullNameLength)
                    throw new BadZipFileFormatException($"Unable to read central directory header to the end.: position=\"{centralDirectoryHeaderPosition}\"");
                var extraFieldsBytes = zipStream.Stream.ReadBytes(extraFieldsLength);
                if (extraFieldsBytes.Length != extraFieldsLength)
                    throw new BadZipFileFormatException($"Unable to read central directory header to the end.: position=\"{centralDirectoryHeaderPosition}\"");
                var commentBytes = zipStream.Stream.ReadBytes(commentLength);
                if (commentBytes.Length != commentLength)
                    throw new BadZipFileFormatException($"Unable to read central directory header to the end.: position=\"{centralDirectoryHeaderPosition}\"");

                return
                    CreateIntance(
                        zipReader,
                        zipStream,
                        centralDirectoryHeaderPosition,
                        headerBytes,
                        fullNameBytes,
                        extraFieldsBytes,
                        commentBytes,
                        stringency);
            }
            catch (InvalidOperationException ex)
            {
                // ボリュームがロックされている最中に、ボリュームをまたいだ読み込みが行われた場合

                // ヘッダがボリュームをまたいでいると判断し、ZIP アーカイブの破損とみなす。
                throw new BadZipFileFormatException($"It is possible that the central directory header is split across multiple disks.: position=\"{centralDirectoryHeaderPosition}\"", ex);
            }
            catch (UnexpectedEndOfStreamException ex)
            {
                // ヘッダの読み込み中に ZIP アーカイブの終端に達した場合

                // ZIP アーカイブの破損とみなす。
                throw new BadZipFileFormatException($"Unable to read central directory header.: position=\"{centralDirectoryHeaderPosition}\"", ex);
            }
            finally
            {
                // ボリュームのロックを解除する。
                zipStream.Stream.UnlockVolumeDisk();
            }
        }

        public static async Task<ZipEntryCentralDirectoryHeader> ParseAsync(
            ZipArchiveFileReader.IZipReaderEnvironment zipReader,
            ZipArchiveFileReader.IZipReaderStream zipStream,
            ValidationStringency stringency,
            CancellationToken cancellationToken = default)
        {
            // ZIP ストリームに対し、最低読み込みバイト数とともに、これからボリュームをロックした状態でのヘッダの読み込みを行うことを宣言する。
            // これが必要な理由は、もしこの時点でアクセス位置がボリュームの終端にある場合は、次のボリュームへの移動を促すため。
            // もし、現在のボリュームの残りバイト数がヘッダの最小サイズ未満である場合は、ヘッダがボリューム間で分割されていると判断し、ZIP アーカイブの破損とみなす。
            // ※ ZIP の仕様上、すべてのヘッダは ボリューム境界をまたいではならない。
            if (!zipStream.Stream.CheckIfCanAtomicRead(MinimumHeaderSize))
                throw new BadZipFileFormatException($"The central directory header is not in the expected position or is fragmented.: position=\"{zipStream.Stream.Position}\"");

            var centralDirectoryHeaderPosition = zipStream.Stream.Position;

            // ボリュームをロックする。これ以降、ボリュームをまたいだ読み込みが禁止される。
            zipStream.Stream.LockVolumeDisk();
            try
            {
                var headerBytes = await zipStream.Stream.ReadBytesAsync(MinimumHeaderSize, cancellationToken).ConfigureAwait(false);
                var (fullNameLength, extraFieldsLength, commentLength) = ParseHeaderEasily(headerBytes, centralDirectoryHeaderPosition);
                var fullNameBytes = await zipStream.Stream.ReadBytesAsync(fullNameLength, cancellationToken).ConfigureAwait(false);
                if (fullNameBytes.Length != fullNameLength)
                    throw new BadZipFileFormatException($"Unable to read central directory header to the end.: position=\"{centralDirectoryHeaderPosition}\"");
                var extraFieldsBytes = await zipStream.Stream.ReadBytesAsync(extraFieldsLength, cancellationToken).ConfigureAwait(false);
                if (extraFieldsBytes.Length != extraFieldsLength)
                    throw new BadZipFileFormatException($"Unable to read central directory header to the end.: position=\"{centralDirectoryHeaderPosition}\"");
                var commentBytes = await zipStream.Stream.ReadBytesAsync(commentLength, cancellationToken).ConfigureAwait(false);
                if (commentBytes.Length != commentLength)
                    throw new BadZipFileFormatException($"Unable to read central directory header to the end.: position=\"{centralDirectoryHeaderPosition}\"");

                return
                    CreateIntance(
                        zipReader,
                        zipStream,
                        centralDirectoryHeaderPosition,
                        headerBytes,
                        fullNameBytes,
                        extraFieldsBytes,
                        commentBytes,
                        stringency);
            }
            catch (InvalidOperationException ex)
            {
                // ボリュームがロックされている最中に、ボリュームをまたいだ読み込みが行われた場合

                // ヘッダがボリュームをまたいでいると判断し、ZIP アーカイブの破損とみなす。
                throw new BadZipFileFormatException($"It is possible that the central directory header is split across multiple disks.: position=\"{centralDirectoryHeaderPosition}\"", ex);
            }
            catch (UnexpectedEndOfStreamException ex)
            {
                // ヘッダの読み込み中に ZIP アーカイブの終端に達した場合

                // ZIP アーカイブの破損とみなす。
                throw new BadZipFileFormatException($"Unable to read central directory header.: position=\"{centralDirectoryHeaderPosition}\"", ex);
            }
            finally
            {
                // ボリュームのロックを解除する。
                zipStream.Stream.UnlockVolumeDisk();
            }
        }

        private static (UInt16 fullNameLength, UInt16 extraFieldsLength, UInt16 commentLength) ParseHeaderEasily(
            ReadOnlyMemory<Byte> headerBytes,
            ZipStreamPosition centralDirectoryHeaderPosition)
        {
            if (headerBytes.Length != checked((Int32)MinimumHeaderSize))
                throw new BadZipFileFormatException($"Unable to read central directory header to the end.: position=\"{centralDirectoryHeaderPosition}\"");
            var signature = headerBytes[..4].ToUInt32LE();
            if (signature != _centralHeaderSignature)
                throw new BadZipFileFormatException($"Not found central header in expected position.: position=\"{centralDirectoryHeaderPosition}\"");
            var fullNameLength = headerBytes.Slice(28, 2).ToUInt16LE();
            var extraFieldsLength = headerBytes.Slice(30, 2).ToUInt16LE();
            var commentLength = headerBytes.Slice(32, 2).ToUInt16LE();
            return (fullNameLength, extraFieldsLength, commentLength);
        }

        private static ZipEntryCentralDirectoryHeader CreateIntance(
            ZipArchiveFileReader.IZipReaderEnvironment zipReader,
            ZipArchiveFileReader.IZipReaderStream zipStream,
            ZipStreamPosition centralDirectoryHeaderPosition,
            ReadOnlyMemory<Byte> headerBytes,
            ReadOnlyMemory<Byte> fullNameBytes,
            ReadOnlyMemory<Byte> extraFieldsBytes,
            ReadOnlyMemory<Byte> commentBytes,
            ValidationStringency stringency)
        {
            var hostSystemAndVersionMadeBy = headerBytes.Slice(4, 2).ToUInt16LE();
            var hostSystem = (ZipEntryHostSystem)(hostSystemAndVersionMadeBy >> 8);
            var versionMadeBy = (Byte)hostSystemAndVersionMadeBy;
            var versionNeededToExtract = headerBytes.Slice(6, 2).ToUInt16LE();
            if (!zipReader.CheckVersion(versionNeededToExtract))
                throw new NotSupportedSpecificationException($"Unsupported version of ZIP file format. : Version of this software={zipReader.ThisSoftwareVersion}, VersionNeededToExtract={versionNeededToExtract}");
            var generalPurposeBitFlag = (ZipEntryGeneralPurposeBitFlag)headerBytes.Slice(8, 2).ToUInt16LE();
            if (generalPurposeBitFlag.HasEncryptionFlag())
                throw new EncryptedZipFileNotSupportedException((generalPurposeBitFlag & (ZipEntryGeneralPurposeBitFlag.Encrypted | ZipEntryGeneralPurposeBitFlag.EncryptedCentralDirectory | ZipEntryGeneralPurposeBitFlag.StrongEncrypted)).ToString());
            if (generalPurposeBitFlag.HasFlag(ZipEntryGeneralPurposeBitFlag.CompressedPatchedData))
                throw new NotSupportedSpecificationException("Not supported \"Compressed Patched Data\".");
            var compressionMethodId = (ZipEntryCompressionMethodId)headerBytes.Slice(10, 2).ToUInt16LE();
            var dosTime = headerBytes.Slice(12, 2).ToUInt16LE();
            var dosDate = headerBytes.Slice(14, 2).ToUInt16LE();
            var rawCrc = headerBytes.Slice(16, 4).ToUInt32LE();
            var rawPackedSize = headerBytes.Slice(20, 4).ToUInt32LE();
            var rawSize = headerBytes.Slice(24, 4).ToUInt32LE();
            //var fullNameLength = headerBytes.Slice(28, 2).ToUInt16LE();
            //var extraFieldsLength = headerBytes.Slice(30, 2).ToUInt16LE();
            //var commentLength = headerBytes.Slice(32, 2).ToUInt16LE();
            var rawDiskStartNumber = headerBytes.Slice(34, 2).ToUInt16LE();
            var externalFileAttribute = headerBytes.Slice(38, 4).ToUInt32LE();
            var rawRelativeLocalHeaderOffset = headerBytes.Slice(42, 4).ToUInt32LE();

            var dosDateTime =
                dosDate == 0 && dosTime == 0
                    ? (DateTime?)null
                    : (dosDate, dosTime).FromDosDateTimeToDateTime(DateTimeKind.Local);
            var extraFields = new ExtraFieldCollection(ZipEntryHeaderType.CentralDirectoryHeader, extraFieldsBytes);
            var zip64ExtraFieldValue = extraFields.GetExtraField<Zip64ExtendedInformationExtraFieldForCentraHeader>(stringency);
            if (zip64ExtraFieldValue is null)
            {
                return
                    new ZipEntryCentralDirectoryHeader(
                        centralDirectoryHeaderPosition,
                        zipReader.ZipEntryNameEncodingProvider,
                        zipStream.Stream.GetPosition(rawDiskStartNumber, rawRelativeLocalHeaderOffset)
                            ?? throw new BadZipFileFormatException($"The local header position read from the central directory does not point to the correct disk position.: {nameof(rawDiskStartNumber)}=0x{rawDiskStartNumber:x8}, {nameof(rawRelativeLocalHeaderOffset)}=0x{rawRelativeLocalHeaderOffset:x16}"),
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
                        rawRelativeLocalHeaderOffset,
                        rawPackedSize,
                        rawSize,
                        externalFileAttribute,
                        fullNameBytes,
                        commentBytes,
                        extraFields,
                        checked(MinimumHeaderSize + (UInt32)fullNameBytes.Length + (UInt32)extraFieldsBytes.Length + (UInt32)commentBytes.Length),
                        false,
                        stringency);
            }
            else
            {
                var (size, packedSize, relatiiveHeaderOffset, diskStartNumber) =
                    zip64ExtraFieldValue.GetValues(rawSize, rawPackedSize, rawRelativeLocalHeaderOffset, rawDiskStartNumber);
                return
                    new ZipEntryCentralDirectoryHeader(
                        centralDirectoryHeaderPosition,
                        zipReader.ZipEntryNameEncodingProvider,
                        zipStream.Stream.GetPosition(diskStartNumber, relatiiveHeaderOffset)
                            ?? throw new BadZipFileFormatException($"The local header position read from the ZIP64 extra field in the central directory does not point to the correct disk position.: {nameof(diskStartNumber)}=0x{diskStartNumber:x8}, {nameof(relatiiveHeaderOffset)}=0x{relatiiveHeaderOffset:x16}"),
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
                        rawRelativeLocalHeaderOffset,
                        packedSize,
                        size,
                        externalFileAttribute,
                        fullNameBytes,
                        commentBytes,
                        extraFields,
                        checked(MinimumHeaderSize + (UInt32)fullNameBytes.Length + (UInt32)extraFieldsBytes.Length + (UInt32)commentBytes.Length),
                        true,
                        stringency);
            }
        }

        internal ZipStreamPosition CentralDirectoryHeaderPosition { get; }

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
                        => ((ExternalAttributesForUnix)ExternalFileAttributes & ExternalAttributesForUnix.UNX_IFMT) == ExternalAttributesForUnix.UNX_IFDIR,
                    _ => false,
                };
        }
    }
}
