using System;
using Utility;
using Utility.IO;
using ZipUtility.ExtraFields;

namespace ZipUtility.Headers.Builder
{
    internal class ZipEntryCentralDirectoryHeader
    {
        public const Int32 MaximumHeaderSize = MinimumHeaderSize + UInt16.MaxValue + UInt16.MaxValue + UInt16.MaxValue;
        public const Int32 MinimumHeaderSize = 46;

        private static readonly UInt32 _centralDirectoryHeaderSignature;
        private readonly UInt16 _versionMadeBy;
        private readonly ZipEntryGeneralPurposeBitFlag _generalPurposeBitFlag;
        private readonly ZipEntryCompressionMethodId _compressionMethodId;
        private readonly UInt16 _dosDate;
        private readonly UInt16 _dosTime;
        private readonly UInt32 _crc;
        private readonly UInt32 _rawSize;
        private readonly UInt32 _rawPackedSize;
        private readonly ReadOnlyMemory<Byte> _entryFullNameBytes;
        private readonly ReadOnlyMemory<Byte> _entryCommentBytes;
        private readonly ReadOnlyMemory<Byte> _extraFieldsBytes;
        private readonly UInt32 _externalFileAttributes;
        private readonly UInt16 _rawDiskNumberStart;
        private readonly UInt32 _rawRelativeOffsetOfLocalHeader;

        static ZipEntryCentralDirectoryHeader()
        {
            _centralDirectoryHeaderSignature = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x01, 0x02);
        }

        private ZipEntryCentralDirectoryHeader(
            UInt16 versionMadeBy,
            UInt16 versionNeededToExtract,
            ZipEntryGeneralPurposeBitFlag generalPurposeBitFlag,
            ZipEntryCompressionMethodId compressionMethodId,
            UInt16 dosDate,
            UInt16 dosTime,
            UInt32 crc,
            UInt32 rawSize,
            UInt32 rawPackedSize,
            ReadOnlyMemory<Byte> entryFullNameBytes,
            ReadOnlyMemory<Byte> entryCommentBytes,
            ReadOnlyMemory<Byte> extraFieldsBytes,
            UInt32 externalFileAttributes,
            UInt16 rawDiskNumberStart,
            UInt32 rawRelativeOffsetOfLocalHeader)
        {
            if (entryFullNameBytes.Length > UInt16.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(entryFullNameBytes));
            if (entryCommentBytes.Length > UInt16.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(entryCommentBytes));

            _versionMadeBy = versionMadeBy;
            VersionNeededToExtract = versionNeededToExtract;
            _generalPurposeBitFlag = generalPurposeBitFlag;
            _compressionMethodId = compressionMethodId;
            _dosDate = dosDate;
            _dosTime = dosTime;
            _crc = crc;
            _rawSize = rawSize;
            _rawPackedSize = rawPackedSize;
            _entryFullNameBytes = entryFullNameBytes;
            _entryCommentBytes = entryCommentBytes;
            _extraFieldsBytes = extraFieldsBytes;
            _rawDiskNumberStart = rawDiskNumberStart;
            _externalFileAttributes = externalFileAttributes;
            _rawRelativeOffsetOfLocalHeader = rawRelativeOffsetOfLocalHeader;
        }

        public UInt16 VersionNeededToExtract { get; }

        public ZipStreamPosition WriteTo(IZipOutputStream outputStream)
        {
            // セントラルディレクトリヘッダの不可分書き込みを宣言する。
            // ※このとき書き込み対象のボリュームディスクが変化する可能性があることに注意。
            outputStream.ReserveAtomicSpace(checked((UInt64)(MinimumHeaderSize + _entryFullNameBytes.Length + _extraFieldsBytes.Length + _entryCommentBytes.Length)));

            // 不可分書き込みのために出力先ボリュームをロックする。
            outputStream.LockVolumeDisk();
            try
            {
                // セントラルディレクトリヘッダの先頭位置を保存する
                var position = outputStream.Position;

                // セントラルディレクトリヘッダを書き込む。
                var headerBuffer = new Byte[MinimumHeaderSize];
                headerBuffer.Slice(0, 4).SetValueLE(_centralDirectoryHeaderSignature);
                headerBuffer.Slice(4, 2).SetValueLE(_versionMadeBy);
                headerBuffer.Slice(6, 2).SetValueLE(VersionNeededToExtract);
                headerBuffer.Slice(8, 2).SetValueLE((UInt16)_generalPurposeBitFlag);
                headerBuffer.Slice(10, 2).SetValueLE((UInt16)_compressionMethodId);
                headerBuffer.Slice(12, 2).SetValueLE(_dosTime);
                headerBuffer.Slice(14, 2).SetValueLE(_dosDate);
                headerBuffer.Slice(16, 4).SetValueLE(_crc);
                headerBuffer.Slice(20, 4).SetValueLE(_rawPackedSize);
                headerBuffer.Slice(24, 4).SetValueLE(_rawSize);
                headerBuffer.Slice(28, 2).SetValueLE((UInt16)_entryFullNameBytes.Length);
                headerBuffer.Slice(30, 2).SetValueLE((UInt16)_extraFieldsBytes.Length);
                headerBuffer.Slice(32, 2).SetValueLE((UInt16)_entryCommentBytes.Length);
                headerBuffer.Slice(34, 2).SetValueLE(_rawDiskNumberStart);
                headerBuffer.Slice(36, 2).SetValueLE((UInt16)0); // internal attributes
                headerBuffer.Slice(38, 4).SetValueLE(_externalFileAttributes);
                headerBuffer.Slice(42, 4).SetValueLE(_rawRelativeOffsetOfLocalHeader);
                outputStream.WriteBytes(headerBuffer);
                outputStream.WriteBytes(_entryFullNameBytes);
                outputStream.WriteBytes(_extraFieldsBytes);
                outputStream.WriteBytes(_entryCommentBytes);

                // セントラルディレクトリヘッダの先頭位置を返す。
                return position;
            }
            finally
            {
                // 出力先ボリュームのロックを解除する。
                outputStream.UnlockVolumeDisk();
            }
        }

        public static ZipEntryCentralDirectoryHeader Build(
            ZipArchiveFileWriter.IZipFileWriterEnvironment zipWriter,
            ZipStreamPosition localHeaderPosition,
            ZipEntryGeneralPurposeBitFlag generalPurposeBitFlag,
            ZipEntryCompressionMethodId compressionMethodId,
            UInt64 size,
            UInt64 packedSize,
            UInt32 crc,
            UInt32 externalAttributes,
            ExtraFieldCollection extraFields,
            ReadOnlyMemory<Byte> entryFullNameBytes,
            ReadOnlyMemory<Byte> entryCommentBytes,
            DateTime? lastWriteTimeUtc,
            Boolean isDirectory,
            Boolean useDataDescriptor)
        {
            if (useDataDescriptor)
                generalPurposeBitFlag |= ZipEntryGeneralPurposeBitFlag.HasDataDescriptor;

            var zip64ExtraField = new Zip64ExtendedInformationExtraFieldForCentraHeader();
            var (rawSize, rawPackedSize, rawLocalHeaderOffset, rawDiskNumber) =
                zip64ExtraField.SetValues(
                    size,
                    packedSize,
                    localHeaderPosition.OffsetOnTheDisk,
                    localHeaderPosition.DiskNumber);
            extraFields.AddExtraField(zip64ExtraField);

            var (dosDate, dosTime) = GetDosDateTime(lastWriteTimeUtc);

            return
                new ZipEntryCentralDirectoryHeader(
                    (UInt16)(((UInt16)zipWriter.HostSystem << 8) | zipWriter.ThisSoftwareVersion),
                    zipWriter.GetVersionNeededToExtract(compressionMethodId, isDirectory, extraFields.Contains(Zip64ExtendedInformationExtraField.ExtraFieldId)),
                    generalPurposeBitFlag,
                    compressionMethodId,
                    dosDate,
                    dosTime,
                    crc,
                    rawSize,
                    rawPackedSize,
                    entryFullNameBytes,
                    entryCommentBytes,
                    extraFields.ToByteArray(),
                    externalAttributes,
                    rawDiskNumber,
                    rawLocalHeaderOffset);
        }

        private static (UInt16 dosDate, UInt16 dosTime) GetDosDateTime(DateTime? lastWriteTimeUtc)
        {
            try
            {
                return (lastWriteTimeUtc ?? DateTime.UtcNow).FromDateTimeToDosDateTime(DateTimeKind.Local);
            }
            catch (Exception)
            {
                return (0, 0);
            }
        }
    }
}
