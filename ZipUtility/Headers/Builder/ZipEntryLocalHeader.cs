using System;
using Utility;
using Utility.IO;
using ZipUtility.ExtraFields;

namespace ZipUtility.Headers.Builder
{
    internal class ZipEntryLocalHeader
    {
        public const Int32 MaximumHeaderSize = MinimumHeaderSize + UInt16.MaxValue + UInt16.MaxValue;
        public const Int32 MinimumHeaderSize = 30;

        private static readonly UInt32 _localHeaderSignature;
        private readonly ZipEntryGeneralPurposeBitFlag _generalPurposeBitFlag;
        private readonly ZipEntryCompressionMethodId _compressionMethodId;
        private readonly UInt16 _dosDate;
        private readonly UInt16 _dosTime;
        private readonly UInt32 _crc;
        private readonly UInt32 _rawSize;
        private readonly UInt32 _rawPacked;
        private readonly ReadOnlyMemory<Byte> _entryFullNameBytes;
        private readonly ReadOnlyMemory<Byte> _extraFieldsBytes;

        static ZipEntryLocalHeader()
        {
            _localHeaderSignature = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x03, 0x04);
        }

        private ZipEntryLocalHeader(
            UInt16 versionNeededToExtract,
            ZipEntryGeneralPurposeBitFlag generalPurposeBitFlag,
            ZipEntryCompressionMethodId compressionMethodId,
            UInt16 dosDate,
            UInt16 dosTime,
            UInt32 crc,
            UInt32 rawSize,
            UInt32 rawPackedSize,
            ReadOnlyMemory<Byte> entryFullNameBytes,
            ReadOnlyMemory<Byte> extraFieldsBytes)
        {
            if (entryFullNameBytes.Length > UInt16.MaxValue)
                throw new ArgumentException($"The byte array specified by parameter {nameof(entryFullNameBytes)} is too long.: 0x{entryFullNameBytes.Length:x8}", nameof(entryFullNameBytes));
            if (extraFieldsBytes.Length > UInt16.MaxValue)
                throw new ArgumentException($"The byte array specified by parameter {nameof(extraFieldsBytes)} is too long.: 0x{extraFieldsBytes.Length:x8}", nameof(extraFieldsBytes));

            VersionNeededToExtract = versionNeededToExtract;
            _generalPurposeBitFlag = generalPurposeBitFlag;
            _compressionMethodId = compressionMethodId;
            _dosDate = dosDate;
            _dosTime = dosTime;
            _crc = crc;
            _rawSize = rawSize;
            _rawPacked = rawPackedSize;
            _entryFullNameBytes = entryFullNameBytes;
            _extraFieldsBytes = extraFieldsBytes;
        }

        public UInt16 VersionNeededToExtract { get; }

        public ZipStreamPosition WriteTo(IZipOutputStream outputStream)
        {
            // ローカルヘッダの不可分書き込みを宣言する。
            // ※このとき書き込み対象のボリュームディスクが変化する可能性があることに注意。
            outputStream.ReserveAtomicSpace(checked((UInt64)(MinimumHeaderSize + _entryFullNameBytes.Length + _extraFieldsBytes.Length)));

            // 不可分書き込みのために出力先ボリュームをロックする。
            outputStream.LockVolumeDisk();
            try
            {
                // ローカルヘッダの先頭位置を保存する
                var position = outputStream.Position;

                // ローカルヘッダを書き込む。
                var headerBuffer = new Byte[MinimumHeaderSize];
                headerBuffer.Slice(0, 4).SetValueLE(_localHeaderSignature);
                headerBuffer.Slice(4, 2).SetValueLE(VersionNeededToExtract);
                headerBuffer.Slice(6, 2).SetValueLE((UInt16)_generalPurposeBitFlag);
                headerBuffer.Slice(8, 2).SetValueLE((UInt16)_compressionMethodId);
                headerBuffer.Slice(10, 2).SetValueLE(_dosTime);
                headerBuffer.Slice(12, 2).SetValueLE(_dosDate);
                headerBuffer.Slice(14, 4).SetValueLE(_crc);
                headerBuffer.Slice(18, 4).SetValueLE(_rawPacked);
                headerBuffer.Slice(22, 4).SetValueLE(_rawSize);
                headerBuffer.Slice(26, 2).SetValueLE(checked((UInt16)_entryFullNameBytes.Length));
                headerBuffer.Slice(28, 2).SetValueLE(checked((UInt16)_extraFieldsBytes.Length));
                outputStream.WriteBytes(headerBuffer);
                outputStream.WriteBytes(_entryFullNameBytes);
                outputStream.WriteBytes(_extraFieldsBytes);

                // ローカルヘッダの先頭位置を返す。
                return position;
            }
            finally
            {
                // 出力先ボリュームのロックを解除する。
                outputStream.UnlockVolumeDisk();
            }
        }

        public static ZipEntryLocalHeader Build(
            ZipArchiveFileWriter.IZipFileWriterEnvironment zipWriter,
            ZipEntryGeneralPurposeBitFlag generalPurposeBitFlag,
            ZipEntryCompressionMethodId compressionMethodId,
            UInt64 size,
            UInt64 packedSize,
            UInt32 crc,
            ExtraFieldCollection extraFields,
            ReadOnlyMemory<Byte> entryFullNameBytes,
            DateTime? lastWriteTimeUtc,
            Boolean isDirectory)
        {
            var zip64ExtraField = new Zip64ExtendedInformationExtraFieldForLocalHeader();
            var (rawSize, rawPackedSize) = zip64ExtraField.SetValues(size, packedSize);
            extraFields.AddExtraField(zip64ExtraField);

            var (dosDate, dosTime) = GetDosDateTime(lastWriteTimeUtc);

            return
                new ZipEntryLocalHeader(
                    zipWriter.GetVersionNeededToExtract(compressionMethodId, isDirectory, extraFields.Contains(Zip64ExtendedInformationExtraField.ExtraFieldId)),
                    generalPurposeBitFlag,
                    compressionMethodId,
                    dosDate,
                    dosTime,
                    crc,
                    rawSize,
                    rawPackedSize,
                    entryFullNameBytes,
                    extraFields.ToByteArray());
        }

        public static ZipEntryLocalHeader Build(
            ZipArchiveFileWriter.IZipFileWriterEnvironment zipWriter,
            ZipEntryGeneralPurposeBitFlag generalPurposeBitFlag,
            ZipEntryCompressionMethodId compressionMethodId,
            ExtraFieldCollection extraFields,
            ReadOnlyMemory<Byte> entryFullNameBytes,
            DateTime? lastWriteTimeUtc,
            Boolean isDirectory)
        {
            generalPurposeBitFlag |= ZipEntryGeneralPurposeBitFlag.HasDataDescriptor;

            var (dosDate, dosTime) = GetDosDateTime(lastWriteTimeUtc);

            return
                new ZipEntryLocalHeader(
                    zipWriter.GetVersionNeededToExtract(compressionMethodId, isDirectory, extraFields.Contains(Zip64ExtendedInformationExtraField.ExtraFieldId)),
                    generalPurposeBitFlag,
                    compressionMethodId,
                    dosDate,
                    dosTime,
                    0,
                    0,
                    0,
                    entryFullNameBytes,
                    extraFields.ToByteArray());
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
