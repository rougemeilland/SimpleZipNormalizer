using System;

namespace ZipUtility.Headers.Builder
{
    internal class ZipFileLastDiskHeader
    {
        private readonly IZipFileWriterParameter _zipWriterParameter;
        private readonly ZipStreamPosition _startOfCentralDirectoryHeaders;
        private readonly ZipStreamPosition _endOfCentralDirectoryHeaders;
        private readonly UInt64 _totalNumberOfCentralDirectoryHeaders;
        private readonly UInt32 _diskNumberOfDiskWithLastCentralDirectoryHeader;
        private readonly UInt32 _numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader;
        private readonly ReadOnlyMemory<Byte> _commentBytes;
        private readonly Boolean _alwaysApplyZip64EOCDR;

        private ZipFileLastDiskHeader(
            IZipFileWriterParameter zipWriterParameter,
            ZipStreamPosition startOfCentralDirectoryHeaders,
            ZipStreamPosition endOfCentralDirectoryHeaders,
            UInt64 totalNumberOfCentralDirectoryHeaders,
            UInt32 diskNumberOfDiskWithLastCentralDirectoryHeader,
            UInt32 numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader,
            ReadOnlyMemory<Byte> commentBytes,
            Boolean alwaysApplyZip64EOCDR)
        {
            _zipWriterParameter = zipWriterParameter;
            _startOfCentralDirectoryHeaders = startOfCentralDirectoryHeaders;
            _endOfCentralDirectoryHeaders = endOfCentralDirectoryHeaders;
            _totalNumberOfCentralDirectoryHeaders = totalNumberOfCentralDirectoryHeaders;
            _diskNumberOfDiskWithLastCentralDirectoryHeader = diskNumberOfDiskWithLastCentralDirectoryHeader;
            _numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader = numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader;
            _commentBytes = commentBytes;
            _alwaysApplyZip64EOCDR = alwaysApplyZip64EOCDR;
        }

        public void WriteTo(IZipOutputStream outputStream)
        {
            var requiredZip64 =
                _alwaysApplyZip64EOCDR
                || IsRequiredZip64(
                    _startOfCentralDirectoryHeaders,
                    _endOfCentralDirectoryHeaders,
                    _totalNumberOfCentralDirectoryHeaders,
                    _diskNumberOfDiskWithLastCentralDirectoryHeader,
                    _numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader,
                    outputStream.Position);
            if (requiredZip64)
            {
                // ZIP64 拡張が必要である場合

                // [ZIP64 EOCDRを ZIP64 EOCDL および EOCDR と不可分としている理由について]
                // ZIP64 EOCDR Ver2 以降ではどのような仕様であるか不明であるが、少なくとも ZIP64 EOCDR Ver1 においては、
                // ZIP64 EOCDR は ZIP64 EOCDL および EOCDR と同じボリュームディスクに (つまり最後のボリュームディスクに) 存在しなければならない模様。
                // 根拠は、ZIP64 EOCDR と ZIP64 EOCDL を別のボリュームディスクに分けた場合に、それをエラーとみなす実装が複数存在することに拠る。(例: PKZIP, 7-zip)
                // そのため、本ソフトウェアでは ZIP64 EOCDR を ZIP64 EOCDL および EOCDR と不可分に書き込むこととする。

                // ZIP64 EOCDR および ZIP64 EOCDL、EOCDR の不可分書き込みを宣言する
                // ※このとき書き込み対象のボリュームディスクが変化する可能性があることに注意。
                outputStream.ReserveAtomicSpace(checked(ZipFileZip64EOCDR_Ver1.GetLength() + ZipFileZip64EOCDL.GetLength() + ZipFileEOCDR.GetLength(_commentBytes)));

                // 不可分書き込みのために出力先ボリュームをロックする。
                outputStream.LockVolumeDisk();
                try
                {
                    // ZIP64 EOCDR を書き込む
                    var zip64EOCDR =
                        ZipFileZip64EOCDR_Ver1.Build(
                            _zipWriterParameter,
                            _startOfCentralDirectoryHeaders,
                            _endOfCentralDirectoryHeaders,
                            _totalNumberOfCentralDirectoryHeaders,
                            _diskNumberOfDiskWithLastCentralDirectoryHeader,
                            _numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader);
                    var positionOfZip64EOCDR = zip64EOCDR.WriteTo(outputStream);

                    // ZIP64 EOCDL を書き込む。
                    var zip64EOCDL = ZipFileZip64EOCDL.Build(positionOfZip64EOCDR);
                    zip64EOCDL.WriteTo(outputStream);

                    // EOCDR を書き込む。
                    var eocdr =
                        ZipFileEOCDR.Build(
                            _startOfCentralDirectoryHeaders,
                            _endOfCentralDirectoryHeaders,
                            _totalNumberOfCentralDirectoryHeaders,
                            _diskNumberOfDiskWithLastCentralDirectoryHeader,
                            _numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader,
                            _commentBytes);
                    eocdr.WriteTo(outputStream);
                }
                finally
                {
                    // 出力先ボリュームのロックを解除する。
                    outputStream.UnlockVolumeDisk();
                }
            }
            else
            {
                // ZIP64 拡張が不要である場合

                // EOCDR の不可分書き込みを宣言する。
                // ※このとき書き込み対象のボリュームディスクが変化する可能性があることに注意。
                outputStream.ReserveAtomicSpace(ZipFileEOCDR.GetLength(_commentBytes));

                // 不可分書き込みのために出力先ボリュームをロックする。
                outputStream.LockVolumeDisk();
                try
                {
                    // EOCDR を書き込む。
                    var eocdr =
                        ZipFileEOCDR.Build(
                            _startOfCentralDirectoryHeaders,
                            _endOfCentralDirectoryHeaders,
                            _totalNumberOfCentralDirectoryHeaders,
                            _diskNumberOfDiskWithLastCentralDirectoryHeader,
                            _numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader,
                            _commentBytes);
                    eocdr.WriteTo(outputStream);
                }
                finally
                {
                    // 出力先ボリュームのロックを解除する。
                    outputStream.UnlockVolumeDisk();
                }
            }
        }

        public static ZipFileLastDiskHeader Build(
            IZipFileWriterParameter zipWriterParameter,
            ZipStreamPosition startOfCentralDirectoryHeaders,
            ZipStreamPosition endOfCentralDirectoryHeaders,
            UInt64 totalNumberOfCentralDirectoryHeaders,
            UInt32 diskNumberOfDiskWithLastCentralDirectoryHeader,
            UInt32 numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader,
            ReadOnlyMemory<Byte> commentBytes,
            Boolean alwaysApplyZip64EOCDR)
            => new(
                zipWriterParameter,
                startOfCentralDirectoryHeaders,
                endOfCentralDirectoryHeaders,
                totalNumberOfCentralDirectoryHeaders,
                diskNumberOfDiskWithLastCentralDirectoryHeader,
                numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader,
                commentBytes,
                alwaysApplyZip64EOCDR);

        // ※ ZIP64 拡張仕様の条件のうち、
        //   currentPosition.DiskNumber >= UInt16.MaxValue ではなく currentPosition.DiskNumber >= UInt16.MaxValue - 1
        //   になっている理由について。
        //
        // この時点でのボリュームディスク番号が UInt16.MaxValue - 1 であり、かつ他の値は ZIP64 拡張仕様を使用する条件を満たしていない場合について考える。
        // 通常の ZIP64 の仕様によれば、次は EOCDR を書き込まなければならないことになっている。
        // もし、この EOCDR の書き込みがボリューム境界をまたいでしまう場合は、EOCDR は次のボリュームに書かれねばならない。
        // しかし、そうすると、EOCDR が書かれるディスク番号が UInt16.MaxValue となって、結局 ZIP64 拡張仕様を使用しなければならなくなる。
        //
        // もし、このような事態は発生する可能性がある場合は最初から ZIP64 拡張仕様を使用するようにするために、
        // ZIP64 拡張仕様の対象かどうかのチェックにおいて「EOCDRのあるディスク番号」の比較は「UInt16.MaxValue 以上」ではなく 「UInt16.MaxValue - 1 以上」としている。
        private static Boolean IsRequiredZip64(
            ZipStreamPosition startOfCentralDirectoryHeaders,
            ZipStreamPosition endOfCentralDirectoryHeaders,
            UInt64 totalNumberOfCentralDirectoryHeaders,
            UInt32 diskNumberOfDiskWithLastCentralDirectoryHeader,
            UInt32 numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader,
            ZipStreamPosition currentPosition)
            => currentPosition.DiskNumber >= UInt16.MaxValue - 1
                || totalNumberOfCentralDirectoryHeaders > 0
                    && (startOfCentralDirectoryHeaders.DiskNumber >= UInt16.MaxValue
                        || startOfCentralDirectoryHeaders.OffsetOnTheDisk >= UInt32.MaxValue)
                || diskNumberOfDiskWithLastCentralDirectoryHeader == currentPosition.DiskNumber && numberOfCentralDirectoryHeadersOnDiskWithLastCentralDirectoryHeader >= UInt16.MaxValue
                || totalNumberOfCentralDirectoryHeaders >= UInt16.MaxValue
                || endOfCentralDirectoryHeaders - startOfCentralDirectoryHeaders >= UInt32.MaxValue;
    }
}
