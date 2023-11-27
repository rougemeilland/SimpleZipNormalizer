using System;
using System.Linq;
using Utility;
using Utility.IO;

namespace ZipUtility.ZipFileHeader
{
    class ZipEntryDataDescriptor
    {
        private static readonly UInt32 _dataDescriptorSignature;

        static ZipEntryDataDescriptor()
        {
            _dataDescriptorSignature = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x07, 0x08);
        }

        private ZipEntryDataDescriptor(UInt32 crc, UInt64 packedSize, UInt64 size)
        {
            Crc = crc;
            PackedSize = packedSize;
            Size = size;
        }

        public UInt32 Crc { get; }
        public UInt64 PackedSize { get; }
        public UInt64 Size { get; }

        public static ZipEntryDataDescriptor Parse(
            IZipInputStream zipInputStrem,
            ZipStreamPosition dataPosition,
            UInt64 packedSizeValueInCentralDirectoryHeader,
            UInt64 sizeValueInCentralDirectoryHeader,
            UInt32 crcValueInCentralDirectoryHeader,
            Boolean isZip64)
        {
            zipInputStrem.Seek(dataPosition + packedSizeValueInCentralDirectoryHeader);
            var sourceData = zipInputStrem.ReadBytes(isZip64 ? 24 : 16);
            var foundDataDescriptor =
                new Func<ZipEntryDataDescriptor?>[]
                {
                    () => Create(sourceData, true, isZip64),
                    () => Create(sourceData, false, isZip64),
                }
                .Select(creater => creater())
                .Where(dataDescriptor =>
                    dataDescriptor is not null &&
                    dataDescriptor.IsCorrect(crcValueInCentralDirectoryHeader, packedSizeValueInCentralDirectoryHeader, sizeValueInCentralDirectoryHeader) &&
                    dataDescriptor.PackedSize == packedSizeValueInCentralDirectoryHeader &&
                    dataDescriptor.Size == sizeValueInCentralDirectoryHeader)
                .FirstOrDefault();
            return foundDataDescriptor ?? throw new BadZipFileFormatException("Not found data descriptor.");
        }

        private static ZipEntryDataDescriptor? Create(ReadOnlyMemory<Byte> source, Boolean containsSignature, Boolean isZip64)
        {
            // データディスクリプタの先頭にシグニチャを書かない ZIP アーカイバ の実装も存在するので、シグニチャがないことも想定しなければならない。
            if (containsSignature)
            {
                // シグニチャが存在すると仮定する場合

                if (isZip64)
                {
                    // シグニチャが存在すると仮定し、かつ ZIP64 である場合

                    var signature = source[..4].ToUInt32LE();
                    if (signature != _dataDescriptorSignature)
                        return null;
                    var crc = source.Slice(4, 4).ToUInt32LE();
                    var packedSize = source.Slice(8, 8).ToUInt64LE();
                    var size = source.Slice(16, 8).ToUInt64LE();
                    return new ZipEntryDataDescriptor(crc, packedSize, size);
                }
                else
                {
                    // シグニチャが存在すると仮定し、かつ ZIP64 ではない場合

                    var signature = source[..4].ToUInt32LE();
                    if (signature != _dataDescriptorSignature)
                        return null;
                    var crc = source.Slice(4, 4).ToUInt32LE();
                    var packedSize = source.Slice(8, 4).ToUInt32LE();
                    var size = source.Slice(12, 4).ToUInt32LE();
                    return new ZipEntryDataDescriptor(crc, packedSize, size);
                }
            }
            else
            {
                // シグニチャが存在しないと仮定する場合

                if (isZip64)
                {
                    // シグニチャが存在しないと仮定し、かつ ZIP64 である場合

                    var crc = source[..4].ToUInt32LE();
                    var packedSize = source.Slice(4, 8).ToUInt64LE();
                    var size = source.Slice(12, 8).ToUInt64LE();
                    return new ZipEntryDataDescriptor(crc, packedSize, size);
                }
                else
                {
                    // シグニチャが存在しないと仮定し、かつ ZIP64 ではない場合

                    var crc = source[..4].ToUInt32LE();
                    var packedSize = source.Slice(4, 4).ToUInt32LE();
                    var size = source.Slice(8, 4).ToUInt32LE();
                    return new ZipEntryDataDescriptor(crc, packedSize, size);
                }
            }
        }

        private Boolean IsCorrect(UInt32 crc, UInt64 packedSize, UInt64 size)
            => Crc == crc &&
                PackedSize == packedSize &&
                Size == size;
    }
}
