using System;
using Utility;
using Utility.IO;

namespace ZipUtility.Headers.Builder
{
    internal class ZipFileZip64EOCDL
    {
        public const Int32 FixedHeaderSize = 20;

        private static readonly UInt32 _signatureOfZip64EOCDL;
        private readonly ZipStreamPosition _startOfZip64EOCDR;

        static ZipFileZip64EOCDL()
        {
            _signatureOfZip64EOCDL = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x06, 0x07);
        }

        private ZipFileZip64EOCDL(ZipStreamPosition startOfZip64EOCDR)
        {
            _startOfZip64EOCDR = startOfZip64EOCDR;
        }

        public void WriteTo(IZipOutputStream outputStream)
        {
            var positionOfThisHeader = outputStream.Position;

            var headerBuffer = new Byte[FixedHeaderSize];
            headerBuffer.Slice(0, 4).SetValueLE(_signatureOfZip64EOCDL);
            headerBuffer.Slice(4, 4).SetValueLE(_startOfZip64EOCDR.DiskNumber);
            headerBuffer.Slice(8, 8).SetValueLE(_startOfZip64EOCDR.OffsetOnTheDisk);
            headerBuffer.Slice(16, 4).SetValueLE(checked(positionOfThisHeader.DiskNumber + 1)); // ZIP 64 EOCDL は常に最後のボリュームディスクに存在するので、"現在のボリュームディスク番号 + 1 == 合計ボリュームディスク数" が成り立つ。
            outputStream.WriteBytes(headerBuffer);
        }

        public static UInt64 GetLength() => FixedHeaderSize;

        public static ZipFileZip64EOCDL Build(ZipStreamPosition startOfZip64EOCDR)
            => new(startOfZip64EOCDR);
    }
}
