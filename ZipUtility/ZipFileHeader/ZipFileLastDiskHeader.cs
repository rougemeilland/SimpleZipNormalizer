using System;
using System.Collections.Generic;
using System.Linq;
using Utility;
using Utility.IO;

namespace ZipUtility.ZipFileHeader
{
    internal class ZipFileLastDiskHeader
    {
        private class DiskHeaderEnumeratorParameter
        {
            private readonly IZipInputStream _zipStream;

            public DiskHeaderEnumeratorParameter(IZipInputStream zipStream)
            {
                _zipStream = zipStream;
            }

            public Boolean AssumedToBeSingleVolume => !_zipStream.IsMultiVolumeZipStream;
            public UInt32 LastDiskNumber => _zipStream.LastDiskStartPosition.DiskNumber;
            public UInt64 LastDiskSize => _zipStream.LastDiskSize;
            public UInt64 TotalVolumeSize => _zipStream.Length;

            public Boolean ValidatePosition(UInt32 diskNumber, UInt64 offsetOnTheDisk)
                => _zipStream.GetPosition(diskNumber, offsetOnTheDisk) is not null;

        }

        private ZipFileLastDiskHeader(ZipFileEOCDR eocdr, ZipFileZip64EOCDL? zip64EOCDL)
        {
            EOCDR = eocdr;
            Zip64EOCDL = zip64EOCDL;
        }

        public ZipFileEOCDR EOCDR { get; }
        public ZipFileZip64EOCDL? Zip64EOCDL { get; }

        public static ZipFileLastDiskHeader Parse(IZipInputStream zipInputStream)
        {
            var assumedToBeSingleVolume = !zipInputStream.IsMultiVolumeZipStream;
            var lastDiskNumber = zipInputStream.LastDiskStartPosition.DiskNumber;
            var lastDiskSize = zipInputStream.LastDiskSize;
            var zipArchiveSizeExceptLastDisk = zipInputStream.Length - zipInputStream.LastDiskSize;

            // EOCDR (および ZIP64 EOCDL) が存在し得る最初の場所を求める
            var possibleFirstHeaderOffsetOnLastDisk =
                zipInputStream.LastDiskSize >= (ZipFileEOCDR.MaximumHeaderSize + ZipFileZip64EOCDL.FixedHeaderSize)
                ? zipInputStream.LastDiskSize - (ZipFileEOCDR.MaximumHeaderSize + ZipFileZip64EOCDL.FixedHeaderSize)
                : 0;
            if (zipInputStream.LastDiskSize - possibleFirstHeaderOffsetOnLastDisk < ZipFileEOCDR.MinimumHeaderSize)
                throw new BadZipFileFormatException("The length of the ZIP archive file (or its last file in case of multi-volume) is too short.");
            var possibleFirstHeaderPosition = zipInputStream.LastDiskStartPosition + possibleFirstHeaderOffsetOnLastDisk;

            // ヘッダのありそうな位置に Seek し、それ以降のデータをすべて読み込む。
            zipInputStream.Seek(possibleFirstHeaderPosition);
            var buffer = new Byte[checked((Int32)(zipInputStream.LastDiskSize - possibleFirstHeaderOffsetOnLastDisk))];
            if (zipInputStream.ReadBytes(buffer) != buffer.Length)
                throw new InternalLogicalErrorException();

            var foundHeaders =
                EnumerateLastDiskHeaders(buffer, possibleFirstHeaderOffsetOnLastDisk, new DiskHeaderEnumeratorParameter(zipInputStream))
                .OrderBy(item => item.mayBeMultiVolume) // シングルボリュームと仮定されていて実はマルチボリュームである疑いがあるものは後回し
                .ThenByDescending(item => item.header.EOCDR.OffsetOnLastDisk) // オフセットが大きい (つまりディスクの末尾に近い) ものを優先
                .Take(1) // 候補を最大 1 つまで絞り込む
                .ToArray();

            if (foundHeaders.Length <= 0)
            {
                // 該当するヘッダの候補が一つも見つからなかった場合
                throw new BadZipFileFormatException($"EOCDR (and ZIP64 EOCDL) is missing or has incorrect contents.");
            }

            if (foundHeaders[0].mayBeMultiVolume)
            {
                // シングルボリュームと仮定された上で実はマルチボリュームであることが判明した場合
                throw new MultiVolumeDetectedException(foundHeaders[0].lastDiskNumber);
            }

            return foundHeaders[0].header;
        }

        private static IEnumerable<(ZipFileLastDiskHeader header, Boolean mayBeMultiVolume, UInt32 lastDiskNumber)> EnumerateLastDiskHeaders(ReadOnlyMemory<Byte> buffer, UInt64 possibleFirstHeaderOffsetOnLastDisk, DiskHeaderEnumeratorParameter parameter)
        {
            foreach (var eocdr in ZipFileEOCDR.EnumerateEOCDR(buffer, possibleFirstHeaderOffsetOnLastDisk))
            {
                var zip64EOCDL = (ZipFileZip64EOCDL?)null;
                if (eocdr.OffsetOnLastDisk >= ZipFileZip64EOCDL.FixedHeaderSize)
                {
                    // EOCDR の前に ZIP64 EOCDL のサイズ以上の余白がある場合

                    var offsetOnLastDiskWhereZip64EOCDLMayBe = checked(eocdr.OffsetOnLastDisk - ZipFileZip64EOCDL.FixedHeaderSize);
                    var eocdlBuffer = buffer.Slice(checked((Int32)(offsetOnLastDiskWhereZip64EOCDLMayBe - possibleFirstHeaderOffsetOnLastDisk)), checked((Int32)ZipFileZip64EOCDL.FixedHeaderSize));

                    // ZIP64 EOCDL を解析する
                    zip64EOCDL = ZipFileZip64EOCDL.Parse(eocdlBuffer.Span);
                }

                var mayBeMultiVolume = false;
                if (zip64EOCDL is null)
                {
                    // ZIP64 EOCDL が存在しない場合

                    if (eocdr.IsRequiresZip64)
                    {
                        // ZIP64 EOCDL が存在しないにもかかわらず、EOCDR は ZIP64 拡張仕様を要求している場合

                        // これは正しい EOCDR ではない。
                        continue;
                    }

                    if (parameter.AssumedToBeSingleVolume)
                    {
                        // シングルボリュームと仮定されている場合

                        if (!eocdr.CheckDiskNumber(parameter.LastDiskNumber))
                        {
                            // シングルボリュームと仮定されており、かつ EOCDR のディスク番号のフィールドに正しくない (0 でない値) が含まれている場合

                            // EOCDR は正しいかもしれないが、しかしアーカイブはマルチボリュームかもしれない
                            mayBeMultiVolume = true;
                        }
                    }
                    else
                    {
                        // マルチボリュームであると確定している場合

                        if (!eocdr.CheckDiskNumber(parameter.LastDiskNumber))
                        {
                            // マルチボリュームと確定しており、かつ EOCDR のディスク番号のフィールドに正しくない (0 でない値) が含まれている場合

                            // これは正しい EOCDR ではない。
                            continue;
                        }
                    }

                    if (!mayBeMultiVolume)
                    {
                        if (eocdr.DiskWhereCentralDirectoryStarts > eocdr.NumberOfThisDisk)
                        {
                            // 最初のセントラルディレクトリがあるディスクの番号が最後のディスクの番号より大きい場合

                            // これは正しい EOCDR ではない。
                            continue;
                        }

                        if (eocdr.NumberOfCentralDirectoryRecordsOnThisDisk != 1
                            && eocdr.NumberOfCentralDirectoryRecordsOnThisDisk > eocdr.TotalNumberOfCentralDirectoryRecords)
                        {
                            // 最後のディスクに存在するセントラルディレクトリの個数が 1 でなく、かつ、合計のセントラルディレクトリの数より大きいならば、これは正しい EOCDR ではない。
                            // ※最後のディスクに存在するセントラルディレクトリの個数と 1 を比較しているのは、このフィールドが常に 1 となる ZIP アーカイバの実装が存在するため。

                            // これは正しい EOCDR ではない。
                            continue;
                        }

                        if (eocdr.SizeOfCentralDirectory > checked(parameter.TotalVolumeSize - parameter.LastDiskSize + eocdr.OffsetOnLastDisk))
                        {
                            // セントラルディレクトリの合計サイズが、現在調べているヘッダを除く全ボリュームのサイズより大きい場合

                            // これは正しい EOCDR ではない。
                            continue;
                        }

                        if (!parameter.ValidatePosition(eocdr.DiskWhereCentralDirectoryStarts, eocdr.OffsetOfStartOfCentralDirectory))
                        {
                            // セントラルディレクトリの位置が ZIP アーカイブ上の正しい位置を指していない場合

                            // これは正しい EOCDR ではない。
                            continue;
                        }

                        if (checked(eocdr.OffsetOnLastDisk + ZipFileEOCDR.MinimumHeaderSize + (UInt32)eocdr.CommentBytes.Length) != parameter.LastDiskSize)
                        {
                            // コメントを含めた EOCDR の末尾が最後のディスクの末尾と一致しない場合

                            // これは正しい EOCDR ではない。
                            continue;
                        }
                    }

                    yield return (new ZipFileLastDiskHeader(eocdr, zip64EOCDL), mayBeMultiVolume, eocdr.NumberOfThisDisk);
                }
                else
                {
                    // ZIP64 EOCDL が存在する場合
                    // この場合、コメントを除き、基本的に EOCDR の内容を参照してはならない。

                    if (parameter.AssumedToBeSingleVolume)
                    {
                        // シングルボリュームと仮定されている場合
                        if (!zip64EOCDL.CheckDiskNumber(parameter.LastDiskNumber))
                        {
                            // シングルボリュームであると仮定されており、かつ ZIP64 EOCDL のディスク数/ディスク番号を示すフィールドの値が正しくない場合

                            // ZIP64 EOCDL は正しいかもしれないが、しかしアーカイブはマルチボリュームかもしれない
                            mayBeMultiVolume = true;
                        }
                    }
                    else
                    {
                        // マルチボリュームであると確定している場合

                        if (!zip64EOCDL.CheckDiskNumber(parameter.LastDiskNumber))
                        {
                            // マルチボリュームと確定しており、かつ ZIP64 EOCDL のディスク数/ディスク番号を示すフィールドの値が正しくない場合

                            // これは正しい ZIP64 EOCDL ではない。
                            continue;
                        }
                    }

                    if (!mayBeMultiVolume)
                    {
                        if (zip64EOCDL.NumberOfTheDiskWithTheStartOfTheZip64EndOfCentralDirectory >= zip64EOCDL.TotalNumberOfDisks)
                        {
                            // 最初のセントラルディレクトリがあるディスクの番号が合計ディスク数以上である場合

                            // これは正しい ZIP64 EOCDL ではない。
                            continue;
                        }

                        if (!parameter.ValidatePosition(zip64EOCDL.NumberOfTheDiskWithTheStartOfTheZip64EndOfCentralDirectory, zip64EOCDL.OffsetOfTheZip64EndOfCentralDirectoryRecord))
                        {
                            // ZIP64 EOCDR の位置が正しいディスク上の場所ではない場合

                            // これは正しい ZIP64 EOCDL ではない。
                            continue;
                        }
                    }

                    yield return (new ZipFileLastDiskHeader(eocdr, zip64EOCDL), mayBeMultiVolume, checked(zip64EOCDL.TotalNumberOfDisks - 1));
                }
            }
        }
    }
}
