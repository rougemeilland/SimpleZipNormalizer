using System;
using Utility;
using Utility.IO;

namespace ZipUtility.Headers.Builder
{
    internal class ZipEntryDataDescriptor
    {
        public const Int32 FixedHeaderSize = 16;
        public const Int32 FixedHeaderSizeForZip64 = 24;

        private static readonly UInt32 _dataDescriptorSignature;
        private readonly UInt32 _crc;
        private readonly UInt64 _size;
        private readonly UInt64 _packedSize;

        static ZipEntryDataDescriptor()
        {
            _dataDescriptorSignature = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x07, 0x08);
        }

        private ZipEntryDataDescriptor(
            UInt32 crc,
            UInt64 size,
            UInt64 packedSize)
        {
            _crc = crc;
            _size = size;
            _packedSize = packedSize;
        }

        public void WriteTo(IZipOutputStream outputStream)
        {
            if (_size >= UInt32.MaxValue || _packedSize >= UInt32.MaxValue)
            {
                // ZIP64 拡張が必要な場合

                // データディスクリプタの不可分書き込みを宣言する。
                // ※このとき書き込み対象のボリュームディスクが変化する可能性があることに注意。
                outputStream.ReserveAtomicSpace(FixedHeaderSizeForZip64);

                // 不可分書き込みのために出力先ボリュームをロックする。
                outputStream.LockVolumeDisk();
                try
                {
                    // データディスクリプタ を書き込む。
                    var headerBuffer = new Byte[FixedHeaderSizeForZip64];
                    headerBuffer.Slice(0, 4).SetValueLE(_dataDescriptorSignature);
                    headerBuffer.Slice(4, 4).SetValueLE(_crc);
                    headerBuffer.Slice(8, 8).SetValueLE(_packedSize);
                    headerBuffer.Slice(16, 8).SetValueLE(_size);
                    outputStream.WriteBytes(headerBuffer);
                }
                finally
                {
                    // 出力先ボリュームのロックを解除する。
                    outputStream.UnlockVolumeDisk();
                }
            }
            else
            {
                // ZIP64 拡張が不要な場合

                // データディスクリプタの不可分書き込みを宣言する。
                // ※このとき書き込み対象のボリュームディスクが変化する可能性があることに注意。
                outputStream.ReserveAtomicSpace(FixedHeaderSize);

                // 不可分書き込みのために出力先ボリュームをロックする。
                outputStream.LockVolumeDisk();
                try
                {
                    // データディスクリプタ を書き込む。
                    var headerBuffer = new Byte[FixedHeaderSize];
                    headerBuffer.Slice(0, 4).SetValueLE(_dataDescriptorSignature);
                    headerBuffer.Slice(4, 4).SetValueLE(_crc);
                    headerBuffer.Slice(8, 4).SetValueLE(checked((UInt32)_packedSize));
                    headerBuffer.Slice(12, 4).SetValueLE(checked((UInt32)_size));
                    outputStream.WriteBytes(headerBuffer);
                }
                finally
                {
                    // 出力先ボリュームのロックを解除する。
                    outputStream.UnlockVolumeDisk();
                }
            }
        }

        public static ZipEntryDataDescriptor Build(
            UInt32 crc,
            UInt64 size,
            UInt64 packedSize)
            => new(crc, size, packedSize);
    }
}
