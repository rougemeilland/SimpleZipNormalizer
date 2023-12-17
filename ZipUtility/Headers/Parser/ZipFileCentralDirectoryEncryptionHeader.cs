using System;
using System.Collections.Generic;
using Utility;
using Utility.IO;

namespace ZipUtility.Headers.Parser
{
    internal class ZipFileCentralDirectoryEncryptionHeader
    {
        public const UInt32 MinimumHeaderSize = 28U;

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
                var minimumHeaderBytes = stream.ReadBytes(MinimumHeaderSize);
                if (minimumHeaderBytes.Length <= 0)
                    return null;
                if (minimumHeaderBytes.Length != checked((Int32)MinimumHeaderSize))
                    throw new BadZipFileFormatException("Unable to read central directory encryption header to the end.");
                var compressionMethodId = (ZipEntryCompressionMethodId)minimumHeaderBytes[..2].ToUInt16LE();
                var packedSize = minimumHeaderBytes.Slice(2, 8).ToUInt64LE();
                var size = minimumHeaderBytes.Slice(10, 8).ToUInt64LE();
                var algorithmId = (ZipEntryEncryptionAlgorithmId)minimumHeaderBytes.Slice(18, 2).ToUInt16LE();
                var bitLength = minimumHeaderBytes.Slice(20, 2).ToUInt16LE();
                var flag = (ZipEntryEncryptionFlag)minimumHeaderBytes.Slice(22, 2).ToUInt16LE();
                var hashAlgorithmId = (ZipEntryHashAlgorithmId)minimumHeaderBytes.Slice(24, 2).ToUInt16LE();
                var hashhDataLength = minimumHeaderBytes.Slice(26, 2).ToUInt16LE();
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
