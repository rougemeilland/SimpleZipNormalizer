using System;
using Utility;
using Utility.IO;

namespace ZipUtility.ZipFileHeader
{
    internal class ZipEntryDataDescriptor
    {
        public const Int32 MinimumHeaderSize = 12;
        public const Int32 MaximumHeaderSize = 16;
        public const Int32 MinimumHeaderSizeForZip64 = 20;
        public const Int32 MaximumHeaderSizeForZip64 = 24;

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
            // 以下の条件でこのデータディスクリプタは正しく読まれない。ZIP の仕様がこんなので大丈夫？
            // 1) データディスクリプタのシグニチャが存在せず、かつ
            // 2) CRC == 0x08074b50 となるデータが書き込まれている場合。

            try
            {
                zipInputStrem.Seek(dataPosition + packedSizeValueInCentralDirectoryHeader);
            }
            catch (ArgumentException ex)
            {
                throw new BadZipFileFormatException($"Unable to read data descriptor on ZIP archive.: {nameof(dataPosition)}=\"{dataPosition}\", {nameof(packedSizeValueInCentralDirectoryHeader)}=0x{packedSizeValueInCentralDirectoryHeader:x16}", ex);
            }

            // ZIP ストリームに対し、最低読み込みバイト数とともに、これからボリュームをロックした状態でのヘッダの読み込みを行うことを宣言する。
            // これが必要な理由は、もしこの時点でアクセス位置がボリュームの終端にある場合は、次のボリュームへの移動を促すため。
            // もし、現在のボリュームの残りバイト数がヘッダの最小サイズ未満である場合は、ヘッダがボリューム間で分割されていると判断し、ZIP アーカイブの破損とみなす。
            // ※ ZIP の仕様上、すべてのヘッダは ボリューム境界をまたいではならない。
            if (!zipInputStrem.CheckIfCanAtomicRead((UInt32)(isZip64 ? MinimumHeaderSizeForZip64 : MinimumHeaderSize)))
                throw new BadZipFileFormatException($"The data descriptor is not in the expected position or is fragmented.: position=\"{zipInputStrem.Position}\"");

            var dataDescriptorPosition = zipInputStrem.Position;

            // ボリュームをロックする。これ以降、ボリュームをまたいだ読み込みが禁止される。
            zipInputStrem.LockVolumeDisk();
            try
            {
                // 最初から最大サイズ (ZIP64 なら 24 バイト、そうでないなら 16 バイト) 分を一括で読み込んでしまうと、処理は楽でいいが読み込みのオーバーランが発生してしまう。
                // 大抵の場合はそれでも問題は発生しないが、以下の条件で ZIP アーカイブの破損と誤解されるため、オーバーランとなる読み込みが発生しないように考慮している。
                // 1) マルチボリューム ZIP アーカイブであり、かつ
                // 2) データディスクリプタにシグニチャが存在せず、かつ
                // 3) データディスクリプタがボリュームの末尾にある場合。
                // ※ すべての ZIP のヘッダはボリューム境界をまたいではならない仕様であり、これはデータディスクリプタも例外ではない。

                if (isZip64)
                {
                    // ZIP64 拡張仕様が要求されている場合

                    // まず、シグニチャを含まないサイズ (20 バイト) 分だけ読み込む
                    var headerBytes = zipInputStrem.ReadBytes(MinimumHeaderSizeForZip64);
                    if (headerBytes.Length != MinimumHeaderSizeForZip64)
                        throw new BadZipFileFormatException($"Unable to read data descriptor to the end.: position=\"{dataDescriptorPosition}\"");

                    // 読み込んだ内容を、シグニチャなしの仮定でセントラルディレクトリヘッダの内容と比較する
                    if (headerBytes[..4].ToUInt32LE() == crcValueInCentralDirectoryHeader
                        && headerBytes.Slice(4, 8).ToUInt64LE() == packedSizeValueInCentralDirectoryHeader
                        && headerBytes.Slice(12, 8).ToUInt64LE() == sizeValueInCentralDirectoryHeader)
                    {
                        // 内容が一致した場合

                        // OK
                        return
                            new ZipEntryDataDescriptor(
                                crcValueInCentralDirectoryHeader,
                                packedSizeValueInCentralDirectoryHeader,
                                sizeValueInCentralDirectoryHeader);
                    }
                    else
                    {
                        // シグニチャなしの仮定では一致しなかった場合

                        // シグニチャありの仮定で先頭から 16 バイト分だけをセントラルディレクトリヘッダの内容と比較する
                        if (headerBytes[..4].ToUInt32LE() == _dataDescriptorSignature
                            && headerBytes.Slice(4, 4).ToUInt32LE() == crcValueInCentralDirectoryHeader
                            && headerBytes.Slice(8, 8).ToUInt64LE() == packedSizeValueInCentralDirectoryHeader)
                        {
                            // 先頭から 16 バイト分が一致した場合

                            // header の最後の 4 バイトと、新たに読み込んだ 4 バイトを組み合わせて 8 バイト整数 (非圧縮サイズ) を作る
                            var lowPartOfSize = headerBytes.Slice(16, 4).ToUInt32LE();
                            var highPartOfSize = zipInputStrem.ReadUInt32LE();
                            var size = ((UInt64)highPartOfSize << 32) | lowPartOfSize;

                            if (size != sizeValueInCentralDirectoryHeader)
                            {
                                // 16 バイト目から 8 バイトのフィールド (非圧縮サイズ) がセントラルディレクトリヘッダと一致しなかった場合

                                // 存在するはずのデータディスクリプタが存在しない (または壊れている) と判断し、ZIP アーカイブの破損とみなす。
                                throw new BadZipFileFormatException($"Data descriptor does not exist.: position=\"{dataDescriptorPosition}\"");
                            }

                            // 16 バイト目から 8 バイトのフィールド (非圧縮サイズ) がセントラルディレクトリヘッダと一致した場合

                            // OK
                            return
                                new ZipEntryDataDescriptor(
                                    crcValueInCentralDirectoryHeader,
                                    packedSizeValueInCentralDirectoryHeader,
                                    sizeValueInCentralDirectoryHeader);
                        }
                        else
                        {
                            // シグニチャあり/なしどちらの仮定でも、読み込んだ内容がセントラルディレクトリヘッダの内容と一致しなかった場合

                            // 存在するはずのデータディスクリプタが存在しない (または壊れている) と判断し、ZIP アーカイブの破損とみなす。
                            throw new BadZipFileFormatException($"Data descriptor does not exist.: position=\"{dataDescriptorPosition}\"");
                        }
                    }
                }
                else
                {
                    // ZIP64 拡張仕様が要求されていない場合

                    // まず、シグニチャを含まないサイズ (12 バイト) 分だけ読み込む
                    var headerBytes = zipInputStrem.ReadBytes(MinimumHeaderSize);
                    if (headerBytes.Length != MinimumHeaderSize)
                        throw new BadZipFileFormatException($"Unable to read data descriptor to the end.: position=\"{dataDescriptorPosition}\"");

                    // 読み込んだ内容を、シグニチャなしの仮定でセントラルディレクトリヘッダの内容と比較する
                    if (headerBytes[..4].ToUInt32LE() == crcValueInCentralDirectoryHeader
                        && headerBytes.Slice(4, 4).ToUInt32LE() == packedSizeValueInCentralDirectoryHeader
                        && headerBytes.Slice(8, 4).ToUInt32LE() == sizeValueInCentralDirectoryHeader)
                    {
                        // 内容が一致した場合

                        // OK
                        return
                            new ZipEntryDataDescriptor(
                                crcValueInCentralDirectoryHeader,
                                packedSizeValueInCentralDirectoryHeader,
                                sizeValueInCentralDirectoryHeader);
                    }
                    else
                    {
                        // シグニチャなしの仮定では一致しなかった場合

                        // シグニチャありの仮定で先頭から 12 バイト分だけをセントラルディレクトリヘッダの内容と比較する
                        if (headerBytes[..4].ToUInt32LE() == _dataDescriptorSignature
                            && headerBytes.Slice(4, 4).ToUInt32LE() == crcValueInCentralDirectoryHeader
                            && headerBytes.Slice(8, 4).ToUInt32LE() == packedSizeValueInCentralDirectoryHeader)
                        {
                            // 先頭から 12 バイト分が一致した場合

                            // 新たに 4 バイト整数 (非圧縮サイズ) を読み込む。
                            var size = zipInputStrem.ReadUInt32LE();
                            if (size != sizeValueInCentralDirectoryHeader)
                            {
                                // 12 バイト目から 4 バイトのフィールド (非圧縮サイズ) がセントラルディレクトリヘッダと一致しなかった場合

                                // 存在するはずのデータディスクリプタが存在しない (または壊れている) と判断し、ZIP アーカイブの破損とみなす。
                                throw new BadZipFileFormatException($"Data descriptor does not exist.: position=\"{dataDescriptorPosition}\"");
                            }

                            // 12 バイト目から 4 バイトのフィールド (非圧縮サイズ) がセントラルディレクトリヘッダと一致した場合

                            // OK
                            return
                                new ZipEntryDataDescriptor(
                                    crcValueInCentralDirectoryHeader,
                                    packedSizeValueInCentralDirectoryHeader,
                                    sizeValueInCentralDirectoryHeader);
                        }
                        else
                        {
                            // シグニチャあり/なしどちらの仮定でも、読み込んだ内容がセントラルディレクトリヘッダの内容と一致しなかった場合

                            // 存在するはずのデータディスクリプタが存在しない (または壊れている) と判断し、ZIP アーカイブの破損とみなす。
                            throw new BadZipFileFormatException($"Data descriptor does not exist.: position=\"{dataDescriptorPosition}\"");
                        }
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                // ボリュームがロックされている最中に、ボリュームをまたいだ読み込みが行われた場合

                // ヘッダがボリュームをまたいでいると判断し、ZIP アーカイブの破損とみなす。
                throw new BadZipFileFormatException($"It is possible that the data descriptor is split across multiple disks.: position=\"{dataDescriptorPosition}\"", ex);
            }
            catch (UnexpectedEndOfStreamException ex)
            {
                // ヘッダの読み込み中に ZIP アーカイブの終端に達した場合

                // ZIP アーカイブの破損とみなす。
                throw new BadZipFileFormatException($"Unable to read data descriptor.: position=\"{dataDescriptorPosition}\"", ex);
            }
            finally
            {
                // ボリュームのロックを解除する。
                zipInputStrem.UnlockVolumeDisk();
            }
        }
    }
}
