using System;
using System.Collections.Generic;
using Utility.IO;

namespace ZipUtility.ZipFileHeader
{
    class ZipFileCentralDirectoryEncryptionHeader
    {
        public ZipFileCentralDirectoryEncryptionHeader(ZipEntryCompressionMethodId compressionMethodId, UInt64 packedSize, UInt64 size, ZipEntryEncryptionAlgorithmId algorithmId, UInt16 bitLength, ZipEntryEncryptionFlag flag, ZipEntryHashAlgorithmId hashAlgorithmId, ReadOnlyMemory<Byte> hashData)
        {
            CompressionMethodId = compressionMethodId;
            PackedSize = packedSize;
            Size = size;
            AlgorithmId = algorithmId;
            BitLength = bitLength == 32 ? (UInt16)448 : bitLength;
            Flag = flag;
            HashAlgorithmId = hashAlgorithmId;
            HashData = hashData;
        }

        public ZipEntryCompressionMethodId CompressionMethodId { get; }
        public UInt64 PackedSize { get; }
        public UInt64 Size { get; }
        public ZipEntryEncryptionAlgorithmId AlgorithmId { get; }
        public UInt16 BitLength { get; }
        public ZipEntryEncryptionFlag Flag { get; }
        public ZipEntryHashAlgorithmId HashAlgorithmId { get; }
        public ReadOnlyMemory<Byte> HashData { get; }

        public static ZipFileCentralDirectoryEncryptionHeader? Parse(IEnumerable<Byte> source)
        {
            using var stream = source.AsByteStream();
            try
            {
                var compressionMethodId = (ZipEntryCompressionMethodId)stream.ReadUInt16LE();
                var packedSize = stream.ReadUInt64LE();
                var size = stream.ReadUInt64LE();
                var algorithmId = (ZipEntryEncryptionAlgorithmId)stream.ReadUInt16LE();
                var bitLength = stream.ReadUInt16LE();
                var flag = (ZipEntryEncryptionFlag)stream.ReadUInt16LE();
                var hashAlgorithmId = (ZipEntryHashAlgorithmId)stream.ReadUInt16LE();
                var hashhDataLength = stream.ReadUInt16LE();
                var hashData = stream.ReadBytes(hashhDataLength);
                if (hashData.Length != hashhDataLength)
                    return null;
                var otherData = stream.ReadByteOrNull();
                return
                    otherData is not null
                    ? null
                    : new ZipFileCentralDirectoryEncryptionHeader(
                        compressionMethodId,
                        packedSize,
                        size,
                        algorithmId,
                        bitLength,
                        flag,
                        hashAlgorithmId,
                        hashData);
            }
            catch (UnexpectedEndOfStreamException)
            {
                return null;
            }
        }
    }
}
