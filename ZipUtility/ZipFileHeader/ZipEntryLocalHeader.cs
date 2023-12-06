using System;
using Utility;
using Utility.IO;
using ZipUtility.ZipExtraField;

namespace ZipUtility.ZipFileHeader
{
    internal class ZipEntryLocalHeader
         : ZipEntryInternalHeader
    {
        public const UInt32 MinimumHeaderSize = 30U;
        public const UInt32 MaximumHeaderSize = MinimumHeaderSize + UInt16.MinValue + UInt16.MinValue;

        private static readonly UInt32 _localHeaderSignature;

        static ZipEntryLocalHeader()
        {
            _localHeaderSignature = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x03, 0x04);
        }

        private ZipEntryLocalHeader(
            IZipEntryNameEncodingProvider zipEntryNameEncodingProvider,
            ZipStreamPosition localHeaderPosition,
            UInt16 versionNeededToExtract,
            ZipEntryGeneralPurposeBitFlag generalPurposeBitFlag,
            ZipEntryCompressionMethodId compressionMethodId,
            DateTime? dosDateTime,
            UInt32 rawCrc,
            UInt32 rawPackedSize,
            UInt32 rawSize,
            UInt64 packedSize,
            UInt64 size,
            ReadOnlyMemory<Byte> fullNameBytes,
            ReadOnlyMemory<Byte> commentBytes,
            ExtraFieldStorage extraFields,
            ZipStreamPosition dataPosition,
            ZipEntryDataDescriptor? dataDescriptor,
            Boolean requiredZip64)
            : base(
                  zipEntryNameEncodingProvider,
                  localHeaderPosition,
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
            if (generalPurposeBitFlag.HasFlag(ZipEntryGeneralPurposeBitFlag.HasDataDescriptor) && dataDescriptor is null)
                throw new InternalLogicalErrorException();

            DataDescriptor = dataDescriptor;
            DataPosition = dataPosition;
        }

        public override UInt32 Crc => DataDescriptor?.Crc ?? RawCrc;
        public override ReadOnlyMemory<Byte> CommentBytes => throw new NotSupportedException();
        public override String Comment => throw new NotSupportedException();
        public ZipEntryDataDescriptor? DataDescriptor { get; }
        public ZipStreamPosition DataPosition { get; }

        public static ZipEntryLocalHeader Parse(
            ZipArchiveFileReader.IZipReaderEnvironment zipReader,
            ZipArchiveFileReader.IZipReaderStream zipStream,
            ZipEntryCentralDirectoryHeader centralDirectoryHeader)
        {
            if (zipReader is null)
                throw new ArgumentNullException(nameof(zipReader));
            if (zipStream is null)
                throw new ArgumentNullException(nameof(zipStream));
            if (centralDirectoryHeader is null)
                throw new ArgumentNullException(nameof(centralDirectoryHeader));

            try
            {
                zipStream.Stream.Seek(centralDirectoryHeader.LocalHeaderPosition);
            }
            catch (ArgumentException ex)
            {
                throw new BadZipFileFormatException($"Unable to read local header on ZIP archive.: {nameof(centralDirectoryHeader.LocalHeaderPosition)}=\"{centralDirectoryHeader.LocalHeaderPosition}\"", ex);
            }

            // ZIP ストリームに対し、最低読み込みバイト数とともに、これからボリュームをロックした状態でのヘッダの読み込みを行うことを宣言する。
            // これが必要な理由は、もしこの時点でアクセス位置がボリュームの終端にある場合は、次のボリュームへの移動を促すため。
            // もし、現在のボリュームの残りバイト数がヘッダの最小サイズ未満である場合は、ヘッダがボリューム間で分割されていると判断し、ZIP アーカイブの破損とみなす。
            // ※ ZIP の仕様上、すべてのヘッダは ボリューム境界をまたいではならない。
            if (!zipStream.Stream.CheckIfCanAtomicRead(MinimumHeaderSize))
                throw new BadZipFileFormatException($"The local header is not in the expected position or is fragmented.: position=\"{zipStream.Stream.Position}\"");

            var localHeaderPosition = zipStream.Stream.Position;

            // ボリュームをロックする。これ以降、ボリュームをまたいだ読み込みが禁止される。
            zipStream.Stream.LockVolumeDisk();
            try
            {
                var minimumHeaderBytes = zipStream.Stream.ReadBytes(MinimumHeaderSize);
                if (minimumHeaderBytes.Length != checked((Int32)MinimumHeaderSize))
                    throw new BadZipFileFormatException($"Unable to read local header to the end.: position=\"{localHeaderPosition}\"");
                var signature = minimumHeaderBytes[..4].ToUInt32LE();
                if (signature != _localHeaderSignature)
                    throw new BadZipFileFormatException("Not found in local header in expected position");
                var versionNeededToExtract = minimumHeaderBytes.Slice(4, 2).ToUInt16LE();
                if (!zipReader.CheckVersion(versionNeededToExtract))
                    throw new NotSupportedSpecificationException($"Unsupported version of ZIP file format. : Version of this software={zipReader.ThisSoftwareVersion}, VersionNeededToExtract={versionNeededToExtract}");
                var generalPurposeBitFlag = (ZipEntryGeneralPurposeBitFlag)minimumHeaderBytes.Slice(6, 2).ToUInt16LE();
                if (generalPurposeBitFlag.HasEncryptionFlag())
                    throw new EncryptedZipFileNotSupportedException((generalPurposeBitFlag & (ZipEntryGeneralPurposeBitFlag.Encrypted | ZipEntryGeneralPurposeBitFlag.EncryptedCentralDirectory | ZipEntryGeneralPurposeBitFlag.StrongEncrypted)).ToString());
                if (generalPurposeBitFlag.HasFlag(ZipEntryGeneralPurposeBitFlag.CompressedPatchedData))
                    throw new NotSupportedSpecificationException("Not supported \"Compressed Patched Data\".");
                var compressionMethodId = (ZipEntryCompressionMethodId)minimumHeaderBytes.Slice(8, 2).ToUInt16LE();
                var dosTime = minimumHeaderBytes.Slice(10, 2).ToUInt16LE();
                var dosDate = minimumHeaderBytes.Slice(12, 2).ToUInt16LE();
                var rawCrc = minimumHeaderBytes.Slice(14, 4).ToUInt32LE();
                var rawPackedSize = minimumHeaderBytes.Slice(18, 4).ToUInt32LE();
                var rawSize = minimumHeaderBytes.Slice(22, 4).ToUInt32LE();
                var fullNameLength = minimumHeaderBytes.Slice(26, 2).ToUInt16LE();
                var extraFieldLength = minimumHeaderBytes.Slice(28, 2).ToUInt16LE();
                var fullNameBytes = zipStream.Stream.ReadBytes(fullNameLength);
                if (fullNameBytes.Length != fullNameLength)
                    throw new BadZipFileFormatException($"Unable to read local header to the end.: position=\"{localHeaderPosition}\"");
                var extraFieldDataSource = zipStream.Stream.ReadBytes(extraFieldLength);
                if (extraFieldDataSource.Length != extraFieldLength)
                    throw new BadZipFileFormatException($"Unable to read local header to the end.: position=\"{localHeaderPosition}\"");
                var dataPosition = localHeaderPosition + MinimumHeaderSize + fullNameLength + extraFieldLength;

                var dosDateTime =
                    (dosDate == 0 && dosTime == 0)
                        ? (DateTime?)null
                        : (dosDate, dosTime).FromDosDateTimeToDateTime(DateTimeKind.Local);
                var dataDescriptor =
                    generalPurposeBitFlag.HasFlag(ZipEntryGeneralPurposeBitFlag.HasDataDescriptor)
                        ? ZipEntryDataDescriptor.Parse(
                            zipStream.Stream,
                            dataPosition,
                            centralDirectoryHeader.PackedSize,
                            centralDirectoryHeader.Size,
                            centralDirectoryHeader.Crc,
                            centralDirectoryHeader.RawSize >= UInt32.MaxValue || centralDirectoryHeader.RawPackedSize >= UInt32.MaxValue)
                            ?? throw new BadZipFileFormatException($"No suitable data descriptor was found even though the general purpose flag 3bit is set.: {nameof(centralDirectoryHeader.LocalHeaderPosition)}=\"{centralDirectoryHeader.LocalHeaderPosition}\", {nameof(centralDirectoryHeader.Size)}=0x{centralDirectoryHeader.Size:x16}, {nameof(centralDirectoryHeader.PackedSize)}=0x{centralDirectoryHeader.PackedSize:x16}")
                        : null;

                var extraFields = new ExtraFieldStorage(ZipEntryHeaderType.LocalFileHeader, extraFieldDataSource);
                var zip64ExtraFieldValue = extraFields.GetExtraField<Zip64ExtendedInformationExtraFieldForLocalHeader>();
                if (zip64ExtraFieldValue is null)
                {
                    return
                        new ZipEntryLocalHeader(
                            zipReader.ZipEntryNameEncodingProvider,
                            localHeaderPosition,
                            versionNeededToExtract,
                            generalPurposeBitFlag,
                            compressionMethodId,
                            dosDateTime,
                            rawCrc,
                            rawPackedSize,
                            rawSize,
                            rawPackedSize,
                            rawSize,
                            fullNameBytes,
                            ReadOnlyMemory<Byte>.Empty,
                            extraFields,
                            dataPosition,
                            dataDescriptor,
                            false);
                }
                else
                {
                    var (size, packedSize) = zip64ExtraFieldValue.GetValues(rawSize, rawPackedSize);
                    return
                        new ZipEntryLocalHeader(
                            zipReader.ZipEntryNameEncodingProvider,
                            localHeaderPosition,
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
                            ReadOnlyMemory<Byte>.Empty,
                            extraFields,
                            dataPosition,
                            dataDescriptor,
                            true);
                }
            }
            catch (InvalidOperationException ex)
            {
                // ボリュームがロックされている最中に、ボリュームをまたいだ読み込みが行われた場合

                // ヘッダがボリュームをまたいでいると判断し、ZIP アーカイブの破損とみなす。
                throw new BadZipFileFormatException($"It is possible that the local header is split across multiple disks.: position=\"{localHeaderPosition}\"", ex);
            }
            catch (UnexpectedEndOfStreamException ex)
            {
                // ヘッダの読み込み中に ZIP アーカイブの終端に達した場合

                // ZIP アーカイブの破損とみなす。
                throw new BadZipFileFormatException($"Unable to read local header.: position=\"{localHeaderPosition}\"", ex);
            }
            finally
            {
                // ボリュームのロックを解除する。
                zipStream.Stream.UnlockVolumeDisk();
            }
        }
    }
}
