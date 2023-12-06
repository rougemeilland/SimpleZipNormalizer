using System;
using System.Collections.Generic;
using Utility;

namespace ZipUtility.ZipFileHeader
{
    class ZipFileEOCDR
    {
        public const UInt32 MinimumHeaderSize = 22U;
        public const UInt32 MaximumHeaderSize = MinimumHeaderSize + UInt16.MaxValue;

        private static readonly UInt32 _eocdSignature;

        static ZipFileEOCDR()
        {
            _eocdSignature = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x05, 0x06);
        }

        private ZipFileEOCDR(UInt64 offsetOnLastDisk, UInt16 numberOfThisDisk, UInt16 diskWhereCentralDirectoryStarts, UInt16 numberOfCentralDirectoryRecordsOnThisDisk, UInt16 totalNumberOfCentralDirectoryRecords, UInt32 sizeOfCentralDirectory, UInt32 offsetOfStartOfCentralDirectory, ReadOnlyMemory<Byte> commentBytes)
        {
            OffsetOnLastDisk = offsetOnLastDisk;
            NumberOfThisDisk = numberOfThisDisk;
            DiskWhereCentralDirectoryStarts = diskWhereCentralDirectoryStarts;
            NumberOfCentralDirectoryRecordsOnThisDisk = numberOfCentralDirectoryRecordsOnThisDisk;
            TotalNumberOfCentralDirectoryRecords = totalNumberOfCentralDirectoryRecords;
            SizeOfCentralDirectory = sizeOfCentralDirectory;
            OffsetOfStartOfCentralDirectory = offsetOfStartOfCentralDirectory;
            CommentBytes = commentBytes;
            IsRequiresZip64 =
                NumberOfThisDisk == UInt16.MaxValue ||
                DiskWhereCentralDirectoryStarts == UInt16.MaxValue ||
                NumberOfCentralDirectoryRecordsOnThisDisk == UInt16.MaxValue ||
                TotalNumberOfCentralDirectoryRecords == UInt16.MaxValue ||
                SizeOfCentralDirectory == UInt32.MaxValue ||
                OffsetOfStartOfCentralDirectory == UInt32.MaxValue;
        }

        public UInt64 OffsetOnLastDisk { get; }
        public UInt16 NumberOfThisDisk { get; }
        public UInt16 DiskWhereCentralDirectoryStarts { get; }

        /// <summary>
        /// EOCDRのあるディスクに含まれるセントラルディレクトリレコードの数。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <term>[注意]</term>
        /// <description>
        /// <para>
        /// PKZIPの実装では、マルチボリュームにおいては、このプロパティの値は常に 0x0001 となる。
        /// </para>
        /// <para>
        /// そのため、このプロパティの値をもとにセントラルディレクトリヘッダを探してはならない。
        /// </para>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        public UInt16 NumberOfCentralDirectoryRecordsOnThisDisk { get; }

        public UInt16 TotalNumberOfCentralDirectoryRecords { get; }
        public UInt32 SizeOfCentralDirectory { get; }
        public UInt32 OffsetOfStartOfCentralDirectory { get; }
        public ReadOnlyMemory<Byte> CommentBytes { get; }
        public Boolean IsRequiresZip64 { get; }

        public Boolean CheckDiskNumber(UInt32 actualLastDiskNumber)
            => NumberOfThisDisk == actualLastDiskNumber
                && DiskWhereCentralDirectoryStarts <= actualLastDiskNumber;

        public static IEnumerable<ZipFileEOCDR> EnumerateEOCDR(ReadOnlyMemory<Byte> buffer, UInt64 bufferStartOffsetOnLastDisk)
        {
            foreach (var offset in EnumerateIndexOfSignature(buffer))
            {
                var header = buffer[offset..];
                var offsetOnLastDisk = checked(bufferStartOffsetOnLastDisk + (UInt32)offset);
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"最後のディスクの 0x{offsetOnLastDisk:x16}バイト目に EOCDR シグニチャが見つかりました。");
#endif
                // シグニチャは既に一致しているのでチェックしない
                var numberOfThisDisk = header.Slice(4, 2).ToUInt16LE();
                var diskWhereCentralDirectoryStarts = header.Slice(6, 2).ToUInt16LE();
                var numberOfCentralDirectoryRecordsOnThisDisk = header.Slice(8, 2).ToUInt16LE();
                var totalNumberOfCentralDirectoryRecords = header.Slice(10, 2).ToUInt16LE();
                var sizeOfCentralDirectory = header.Slice(12, 4).ToUInt32LE();
                var offsetOfStartOfCentralDirectory = header.Slice(16, 4).ToUInt32LE();
                var commentLength = header.Slice(20, 2).ToUInt16LE();
                if (checked((UInt32)header.Length) >= MinimumHeaderSize + commentLength)
                {
                    var commentBytes = header.Slice(checked((Int32)MinimumHeaderSize), commentLength);
                    yield return
                        new ZipFileEOCDR(
                            offsetOnLastDisk,
                            numberOfThisDisk,
                            diskWhereCentralDirectoryStarts,
                            numberOfCentralDirectoryRecordsOnThisDisk,
                            totalNumberOfCentralDirectoryRecords,
                            sizeOfCentralDirectory,
                            offsetOfStartOfCentralDirectory,
                            commentBytes);
                }
            }
        }

        private static IEnumerable<Int32> EnumerateIndexOfSignature(ReadOnlyMemory<Byte> buffer)
        {
            if (buffer.Length < MinimumHeaderSize)
                throw new InternalLogicalErrorException();

            var signatureByte0 = unchecked((Byte)(_eocdSignature >> 8 * 0));
            var signatureByte1 = unchecked((Byte)(_eocdSignature >> 8 * 1));
            var signatureByte2 = unchecked((Byte)(_eocdSignature >> 8 * 2));
            var signatureByte3 = unchecked((Byte)(_eocdSignature >> 8 * 3));

            for (var index = buffer.Length - checked((Int32)MinimumHeaderSize); index >= 0; --index)
            {
                var region = buffer.Span[index..];
                if (region[0] == signatureByte0
                    && region[1] == signatureByte1
                    && region[2] == signatureByte2
                    && region[3] == signatureByte3)
                {
                    yield return index;
                }
            }
        }
    }
}
