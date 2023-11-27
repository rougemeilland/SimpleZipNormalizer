using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Utility;

namespace ZipUtility
{
    /// <summary>
    /// <see cref="FileInfo"/> オブジェクトが示すファイルを ZIP アーカイブとして扱うための拡張メソッドのクラスです。
    /// </summary>
    public static class FileExtensions
    {
        private static readonly Regex _sevenZipMultiVolumeZipFileNamePattern;
        private static readonly Regex _generalMultiVolumeZipFileNamePattern;

        static FileExtensions()
        {
            _sevenZipMultiVolumeZipFileNamePattern = new Regex(@"^(?<body>[^\\/]+\.zip)\.[0-9]{3,}$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            _generalMultiVolumeZipFileNamePattern = new Regex(@"^(?<body>[^\\/]+)\.zip$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        /// <summary>
        /// ZIP アーカイブの内容を検証します。
        /// </summary>
        /// <param name="zipFile">
        /// 検証する ZIP アーカイブファイルです。
        /// </param>
        /// <param name="zipEntryNameEncodingProvider">
        /// ZIP アーカイブのエントリのエンコーディングを解決するプロバイダです。
        /// </param>
        /// <param name="progress">
        /// <para>
        /// 処理の進行状況の通知を受け取るためのオブジェクトです。通知を受け取らない場合は null です。
        /// </para>
        /// <para>
        /// 進行状況は、0 以上 1 以下の <see cref="Double"/> 値です。初期値は 0 で、作業が進行するごとに増加していき、作業が完了すると 1 になります。
        /// </para>
        /// </param>
        /// <returns>
        /// ZIP アーカイブを読み込むためのオブジェクトです。
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="zipFile"/> または <paramref name="zipEntryNameEncodingProvider"/> が null です。
        /// </exception>
        public static ZipArchiveValidationResult ValidateAsZipFile(this FileInfo zipFile, IZipEntryNameEncodingProvider zipEntryNameEncodingProvider, IProgress<Double>? progress = null)
        {
            if (zipFile is null)
                throw new ArgumentNullException(nameof(zipFile));
            if (zipEntryNameEncodingProvider is null)
                throw new ArgumentNullException(nameof(zipEntryNameEncodingProvider));

            return InternalValidateZipFile(zipFile, zipEntryNameEncodingProvider, progress);
        }

        /// <summary>
        /// ZIP アーカイブを読み込むためのオブジェクトを取得します。
        /// </summary>
        /// <param name="sourceZipFile">
        /// 読み込む ZIP アーカイブファイルです。
        /// </param>
        /// <param name="zipEntryNameEncodingProvider">
        /// ZIP アーカイブのエントリのエンコーディングを解決するプロバイダです。
        /// </param>
        /// <returns>
        /// ZIP アーカイブを読み込むためのオブジェクトです。
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sourceZipFile"/> または <paramref name="zipEntryNameEncodingProvider"/> が null です。
        /// </exception>
        public static ZipArchiveFileReader OpenAsZipFile(this FileInfo sourceZipFile, IZipEntryNameEncodingProvider zipEntryNameEncodingProvider)
        {
            if (sourceZipFile is null)
                throw new ArgumentNullException(nameof(sourceZipFile));
            if (zipEntryNameEncodingProvider is null)
                throw new ArgumentNullException(nameof(zipEntryNameEncodingProvider));

            var sourceStream = GetSourceStreamByFileNamePattern(sourceZipFile);
            while (true)
            {
                var success = false;
                try
                {
                    var zipFile = ZipArchiveFileReader.Parse(zipEntryNameEncodingProvider, sourceZipFile, sourceStream);
                    success = true;
                    return zipFile;
                }
                catch (MultiVolumeDetectedException ex)
                {
                    sourceStream.Dispose();
                    var lastDiskNumber = ex.LastDiskNumber;
                    sourceStream = GetSourceStreamByLastDiskNumber(sourceZipFile, lastDiskNumber);
                }
                finally
                {
                    if (!success)
                        sourceStream.Dispose();
                }
            }
        }

        /// <summary>
        /// ZIP アーカイブを新規に作成するためのオブジェクトを取得します。
        /// </summary>
        /// <param name="destinationZipFile">
        /// 作成する ZIP アーカイブファイルです。
        /// </param>
        /// <param name="zipEntryNameEncodingProvider">
        /// ZIP アーカイブのエントリのエンコーディングを解決するプロバイダです。
        /// </param>
        /// <returns>
        /// ZIP アーカイブを作成するためのオブジェクトです。
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="destinationZipFile"/> または <paramref name="zipEntryNameEncodingProvider"/> が null です。
        /// </exception>
        public static ZipArchiveFileWriter CreateAsZipFile(this FileInfo destinationZipFile, IZipEntryNameEncodingProvider zipEntryNameEncodingProvider)
        {
            if (destinationZipFile is null)
                throw new ArgumentNullException(nameof(destinationZipFile));
            if (zipEntryNameEncodingProvider is null)
                throw new ArgumentNullException(nameof(zipEntryNameEncodingProvider));

            return
                new ZipArchiveFileWriter(
                    new SingleVolumeZipOutputStream(destinationZipFile),
                    zipEntryNameEncodingProvider,
                    destinationZipFile);
        }

        private static ZipArchiveValidationResult InternalValidateZipFile(FileInfo file, IZipEntryNameEncodingProvider zipEntryNameEncodingProvider, IProgress<Double>? progress)
        {
            // progress 値は以下のように定義される
            //   処理できたエントリの非圧縮サイズ の合計 / ZIP ファイルのサイズ
            // ヘッダ部分のサイズもあるので、この定義では最後まで終了しても100%にはならないが、ヘッダだけを読むだけではエントリのサイズが事前にわからないこともあるので致し方なし。
            try
            {
                if (!file.Exists)
                    throw new FileNotFoundException($"ZIP archive file does not exist.: \"{file.FullName}\"");
                if (file.Length <= 0)
                    throw new BadZipFileFormatException($"ZIP archive file size is zero.: \"{file.FullName}\"");

                try
                {
                    progress?.Report(0);
                }
                catch (Exception)
                {
                }

                var entryCount = 0UL;
                var zipArchiveSize = (UInt64)file.Length;
                var processedUnpackedSize = 0UL;
                var totalProcessedRate = 0.0;

                using (var zipFile = file.OpenAsZipFile(zipEntryNameEncodingProvider))
                {
                    // 進捗率の配分は、GetEntries() が 10% で、データの比較が 90% とする。

                    var entries = zipFile.GetEntries(
                        SafetyProgress.CreateIncreasingProgress<Double, Double>(
                            progress,
                            value => value * 0.1,
                            0,
                            1));
                    totalProcessedRate += 0.1;
                    progress?.Report(totalProcessedRate);
                    foreach (var entry in entries)
                    {
                        try
                        {
                            entry.ValidateData(
                                SafetyProgress.CreateProgress<(UInt64 unpackedCount, UInt64 packedCount), Double>(
                                    progress,
                                    value => totalProcessedRate + (Double)value.packedCount / zipArchiveSize * 0.9));
                            ++entryCount;
                            processedUnpackedSize += entry.Size;
                            totalProcessedRate += (Double)entry.PackedSize / zipArchiveSize * 0.9;
                        }
                        finally
                        {
                            try
                            {
                                progress?.Report((Double)totalProcessedRate);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }

                try
                {
                    progress?.Report(1);
                }
                catch (Exception)
                {
                }

                return new ZipArchiveValidationResult(ZipArchiveValidationResultId.Ok, $"entries = {entryCount}, total entry size = {processedUnpackedSize:N0} bytes, total compressed entry size = {totalProcessedRate:N0} bytes", null);
            }
            catch (EncryptedZipFileNotSupportedException ex)
            {
                return new ZipArchiveValidationResult(ZipArchiveValidationResultId.Encrypted, ex.Message, ex);
            }
            catch (CompressionMethodNotSupportedException ex)
            {
                return new ZipArchiveValidationResult(ZipArchiveValidationResultId.UnsupportedCompressionMethod, ex.Message, ex);
            }
            catch (NotSupportedSpecificationException ex)
            {
                return new ZipArchiveValidationResult(ZipArchiveValidationResultId.UnsupportedFunction, ex.Message, ex);
            }
            catch (BadZipFileFormatException ex)
            {
                return new ZipArchiveValidationResult(ZipArchiveValidationResultId.Corrupted, ex.Message, ex);
            }
            catch (Exception ex)
            {
                return new ZipArchiveValidationResult(ZipArchiveValidationResultId.InternalError, ex.Message, ex);
            }
        }

        private static IZipInputStream GetSourceStreamByFileNamePattern(FileInfo sourceFile)
        {
            var match = _sevenZipMultiVolumeZipFileNamePattern.Match(sourceFile.Name);
            if (match.Success)
            {
                var body = match.Groups["body"].Value;
                var files = new List<FileInfo>();
                for (var index = 1UL; index <= UInt32.MaxValue; ++index)
                {
                    var file = new FileInfo(Path.Combine(sourceFile.DirectoryName ?? ".", $"{body}.{index:D3}"));
                    if (!file.Exists)
                        break;
                    files.Add(file);
                }

                return GetMultiVolumeInputStream(files.ToArray().AsReadOnly());
            }
            else
            {
                return new SingleVolumeZipInputStream(sourceFile);
            }
        }

        private static IZipInputStream GetSourceStreamByLastDiskNumber(FileInfo sourceFile, UInt32 lastDiskNumber)
        {
            var match = _generalMultiVolumeZipFileNamePattern.Match(sourceFile.Name);
            if (!match.Success)
                throw new NotSupportedSpecificationException("Unknown format as multi-volume ZIP file.");
            var body = match.Groups["body"].Value;
            var files = new List<FileInfo>();
            for (var index = 1U; index < lastDiskNumber; ++index)
            {

                var file = new FileInfo(Path.Combine(sourceFile.DirectoryName ?? ".", $"{body}.z{index:D2}"));
                if (!file.Exists)
                    throw new BadZipFileFormatException("There is a missing disk in a multi-volume ZIP file.");
                files.Add(file);
            }

            files.Add(sourceFile);
            return GetMultiVolumeInputStream(files.ToArray().AsReadOnly());
        }

        private static IZipInputStream GetMultiVolumeInputStream(ReadOnlyMemory<FileInfo> disks)
            => throw new NotSupportedSpecificationException($"Not supported \"Multi-Volume Zip File\".; disk count={disks.Length}");
    }
}
