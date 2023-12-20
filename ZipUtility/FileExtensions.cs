using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Utility;
using Utility.IO;

namespace ZipUtility
{
    /// <summary>
    /// <see cref="FilePath"/> オブジェクトが示すファイルを ZIP アーカイブとして扱うための拡張メソッドのクラスです。
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
        /// <paramref name="zipFile"/>  が null です。
        /// </exception>
        public static ZipArchiveValidationResult ValidateAsZipFile(this FilePath zipFile, IProgress<Double>? progress = null)
            => zipFile.ValidateAsZipFile(ZipEntryNameEncodingProvider.CreateInstance(), ValidationStringency.Normal, progress);

        /// <summary>
        /// ZIP アーカイブの内容を検証します。
        /// </summary>
        /// <param name="zipFile">
        /// 検証する ZIP アーカイブファイルです。
        /// </param>
        /// <param name="stringency">
        /// 読み込む ZIP アーカイブに対する検証の厳格性を示す値です。
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
        /// <paramref name="zipFile"/> が null です。
        /// </exception>
        public static ZipArchiveValidationResult ValidateAsZipFile(this FilePath zipFile, ValidationStringency stringency, IProgress<Double>? progress = null)
            => zipFile.ValidateAsZipFile(ZipEntryNameEncodingProvider.CreateInstance(), stringency, progress);

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
        public static ZipArchiveValidationResult ValidateAsZipFile(this FilePath zipFile, IZipEntryNameEncodingProvider zipEntryNameEncodingProvider, IProgress<Double>? progress = null)
            => zipFile.ValidateAsZipFile(zipEntryNameEncodingProvider, ValidationStringency.Normal, progress);

        /// <summary>
        /// ZIP アーカイブの内容を検証します。
        /// </summary>
        /// <param name="zipFile">
        /// 検証する ZIP アーカイブファイルです。
        /// </param>
        /// <param name="zipEntryNameEncodingProvider">
        /// ZIP アーカイブのエントリのエンコーディングを解決するプロバイダです。
        /// </param>
        /// <param name="stringency">
        /// 読み込む ZIP アーカイブに対する検証の厳格性を示す値です。
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
        public static ZipArchiveValidationResult ValidateAsZipFile(this FilePath zipFile, IZipEntryNameEncodingProvider zipEntryNameEncodingProvider, ValidationStringency stringency, IProgress<Double>? progress = null)
        {
            if (zipFile is null)
                throw new ArgumentNullException(nameof(zipFile));
            if (zipEntryNameEncodingProvider is null)
                throw new ArgumentNullException(nameof(zipEntryNameEncodingProvider));

            return InternalValidateZipFile(zipFile, zipEntryNameEncodingProvider, stringency, progress);
        }

        /// <summary>
        /// ZIP アーカイブの内容を非同期的に検証します。
        /// </summary>
        /// <param name="zipFile">
        /// 検証する ZIP アーカイブファイルです。
        /// </param>
        /// <param name="progress">
        /// <para>
        /// 処理の進行状況の通知を受け取るためのオブジェクトです。通知を受け取らない場合は null です。
        /// </para>
        /// <para>
        /// 進行状況は、0 以上 1 以下の <see cref="Double"/> 値です。初期値は 0 で、作業が進行するごとに増加していき、作業が完了すると 1 になります。
        /// </para>
        /// </param>
        /// <param name="cancellationToken">
        /// キャンセル要求を監視するためのトークンです。既定値は <see cref="CancellationToken.None"/> です。
        /// </param>
        /// <returns>
        /// ZIP アーカイブを読み込むためのオブジェクトです。
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="zipFile"/>  が null です。
        /// </exception>
        public static Task<ZipArchiveValidationResult> ValidateAsZipFileAsync(this FilePath zipFile, IProgress<Double>? progress = null, CancellationToken cancellationToken = default)
            => zipFile.ValidateAsZipFileAsync(ZipEntryNameEncodingProvider.CreateInstance(), ValidationStringency.Normal, progress, cancellationToken);

        /// <summary>
        /// ZIP アーカイブの内容を非同期的に検証します。
        /// </summary>
        /// <param name="zipFile">
        /// 検証する ZIP アーカイブファイルです。
        /// </param>
        /// <param name="stringency">
        /// 読み込む ZIP アーカイブに対する検証の厳格性を示す値です。
        /// </param>
        /// <param name="progress">
        /// <para>
        /// 処理の進行状況の通知を受け取るためのオブジェクトです。通知を受け取らない場合は null です。
        /// </para>
        /// <para>
        /// 進行状況は、0 以上 1 以下の <see cref="Double"/> 値です。初期値は 0 で、作業が進行するごとに増加していき、作業が完了すると 1 になります。
        /// </para>
        /// </param>
        /// <param name="cancellationToken">
        /// キャンセル要求を監視するためのトークンです。既定値は <see cref="CancellationToken.None"/> です。
        /// </param>
        /// <returns>
        /// ZIP アーカイブを読み込むためのオブジェクトです。
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="zipFile"/>  が null です。
        /// </exception>
        public static Task<ZipArchiveValidationResult> ValidateAsZipFileAsync(this FilePath zipFile, ValidationStringency stringency, IProgress<Double>? progress = null, CancellationToken cancellationToken = default)
            => zipFile.ValidateAsZipFileAsync(ZipEntryNameEncodingProvider.CreateInstance(), stringency, progress, cancellationToken);

        /// <summary>
        /// ZIP アーカイブの内容を非同期的に検証します。
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
        /// <param name="cancellationToken">
        /// キャンセル要求を監視するためのトークンです。既定値は <see cref="CancellationToken.None"/> です。
        /// </param>
        /// <returns>
        /// ZIP アーカイブを読み込むためのオブジェクトです。
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="zipFile"/> または <paramref name="zipEntryNameEncodingProvider"/> が null です。
        /// </exception>
        public static Task<ZipArchiveValidationResult> ValidateAsZipFileAsync(this FilePath zipFile, IZipEntryNameEncodingProvider zipEntryNameEncodingProvider, IProgress<Double>? progress = null, CancellationToken cancellationToken = default)
            => zipFile.ValidateAsZipFileAsync(zipEntryNameEncodingProvider, ValidationStringency.Normal, progress, cancellationToken);

        /// <summary>
        /// ZIP アーカイブの内容を非同期的に検証します。
        /// </summary>
        /// <param name="zipFile">
        /// 検証する ZIP アーカイブファイルです。
        /// </param>
        /// <param name="zipEntryNameEncodingProvider">
        /// ZIP アーカイブのエントリのエンコーディングを解決するプロバイダです。
        /// </param>
        /// <param name="stringency">
        /// 読み込む ZIP アーカイブに対する検証の厳格性を示す値です。
        /// </param>
        /// <param name="progress">
        /// <para>
        /// 処理の進行状況の通知を受け取るためのオブジェクトです。通知を受け取らない場合は null です。
        /// </para>
        /// <para>
        /// 進行状況は、0 以上 1 以下の <see cref="Double"/> 値です。初期値は 0 で、作業が進行するごとに増加していき、作業が完了すると 1 になります。
        /// </para>
        /// </param>
        /// <param name="cancellationToken">
        /// キャンセル要求を監視するためのトークンです。既定値は <see cref="CancellationToken.None"/> です。
        /// </param>
        /// <returns>
        /// ZIP アーカイブを読み込むためのオブジェクトです。
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="zipFile"/> または <paramref name="zipEntryNameEncodingProvider"/> が null です。
        /// </exception>
        public static Task<ZipArchiveValidationResult> ValidateAsZipFileAsync(this FilePath zipFile, IZipEntryNameEncodingProvider zipEntryNameEncodingProvider, ValidationStringency stringency, IProgress<Double>? progress = null, CancellationToken cancellationToken = default)
        {
            if (zipFile is null)
                throw new ArgumentNullException(nameof(zipFile));
            if (zipEntryNameEncodingProvider is null)
                throw new ArgumentNullException(nameof(zipEntryNameEncodingProvider));

            return InternalValidateZipFileAsync(zipFile, zipEntryNameEncodingProvider, stringency, progress, cancellationToken);
        }

        /// <summary>
        /// ZIP アーカイブを読み込むためのオブジェクトを取得します。
        /// </summary>
        /// <param name="sourceZipFile">
        /// 読み込む ZIP アーカイブファイルです。
        /// </param>
        /// <param name="stringency">
        /// 読み込む ZIP アーカイブに対する検証の厳格性を示す値です。
        /// </param>
        /// <returns>
        /// ZIP アーカイブを読み込むためのオブジェクトです。
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sourceZipFile"/> が null です。
        /// </exception>
        public static ZipArchiveFileReader OpenAsZipFile(this FilePath sourceZipFile, ValidationStringency stringency = ValidationStringency.Normal)
            => sourceZipFile.OpenAsZipFile(ZipEntryNameEncodingProvider.CreateInstance(), stringency);

        /// <summary>
        /// ZIP アーカイブを読み込むためのオブジェクトを取得します。
        /// </summary>
        /// <param name="sourceZipFile">
        /// 読み込む ZIP アーカイブファイルです。
        /// </param>
        /// <param name="zipEntryNameEncodingProvider">
        /// ZIP アーカイブのエントリのエンコーディングを解決するプロバイダです。
        /// </param>
        /// <param name="stringency">
        /// 読み込む ZIP アーカイブに対する検証の厳格性を示す値です。
        /// </param>
        /// <returns>
        /// ZIP アーカイブを読み込むためのオブジェクトです。
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="sourceZipFile"/> または <paramref name="zipEntryNameEncodingProvider"/> が null です。
        /// </exception>
        public static ZipArchiveFileReader OpenAsZipFile(this FilePath sourceZipFile, IZipEntryNameEncodingProvider zipEntryNameEncodingProvider, ValidationStringency stringency = ValidationStringency.Normal)
        {
            if (sourceZipFile is null)
                throw new ArgumentNullException(nameof(sourceZipFile));
            if (zipEntryNameEncodingProvider is null)
                throw new ArgumentNullException(nameof(zipEntryNameEncodingProvider));
            var baseDirectory = sourceZipFile.Directory;
            if (baseDirectory is null)
                throw new ArgumentException($"The parent directory of the file specified by parameter {nameof(sourceZipFile)} does not exist.", nameof(baseDirectory));

            var sourceStream = GetSourceStreamByFileNamePattern(baseDirectory, sourceZipFile);
            while (true)
            {
                var success = false;
                try
                {
                    var zipFile = ZipArchiveFileReader.Parse(sourceZipFile, sourceStream, zipEntryNameEncodingProvider, stringency);
                    success = true;
                    return zipFile;
                }
                catch (MultiVolumeDetectedException ex)
                {
                    sourceStream.Dispose();
                    var lastDiskNumber = ex.LastDiskNumber;
                    sourceStream = GetSourceStreamByLastDiskNumber(baseDirectory, sourceZipFile, lastDiskNumber, stringency);
                    success = true;
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
        /// <param name="zipArchiveFile">
        /// 作成する ZIP アーカイブファイルです。
        /// </param>
        /// <param name="maximumVolumeSize">
        /// ZIP アーカイブの 1 ボリュームあたりの最大の長さのバイト数です。省略時の値は <see cref="UInt32.MaxValue"/> です。
        /// </param>
        /// <returns>
        /// ZIP アーカイブを作成するためのオブジェクトです。
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="zipArchiveFile"/> が null です。
        /// </exception>
        public static ZipArchiveFileWriter CreateAsZipFile(this FilePath zipArchiveFile, UInt64 maximumVolumeSize = UInt64.MaxValue)
            => zipArchiveFile.CreateAsZipFile(ZipEntryNameEncodingProvider.CreateInstance(), maximumVolumeSize);

        /// <summary>
        /// ZIP アーカイブを新規に作成するためのオブジェクトを取得します。
        /// </summary>
        /// <param name="zipArchiveFile">
        /// 作成する ZIP アーカイブファイルです。
        /// </param>
        /// <param name="zipEntryNameEncodingProvider">
        /// ZIP アーカイブのエントリのエンコーディングを解決するプロバイダです。
        /// </param>
        /// <param name="maximumVolumeSize">
        /// ZIP アーカイブの 1 ボリュームあたりの最大の長さのバイト数です。省略時の値は <see cref="UInt32.MaxValue"/> です。
        /// </param>
        /// <returns>
        /// ZIP アーカイブを作成するためのオブジェクトです。
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="zipArchiveFile"/> または <paramref name="zipEntryNameEncodingProvider"/> が null です。
        /// </exception>
        public static ZipArchiveFileWriter CreateAsZipFile(this FilePath zipArchiveFile, IZipEntryNameEncodingProvider zipEntryNameEncodingProvider, UInt64 maximumVolumeSize = UInt64.MaxValue)
        {
            if (zipArchiveFile is null)
                throw new ArgumentNullException(nameof(zipArchiveFile));
            if (zipEntryNameEncodingProvider is null)
                throw new ArgumentNullException(nameof(zipEntryNameEncodingProvider));

            return
                new ZipArchiveFileWriter(
                    GenericStyleZipOutputStream.CreateInstance(zipArchiveFile, maximumVolumeSize),
                    zipEntryNameEncodingProvider,
                    zipArchiveFile);
        }

        private static ZipArchiveValidationResult InternalValidateZipFile(FilePath file, IZipEntryNameEncodingProvider zipEntryNameEncodingProvider, ValidationStringency stringency, IProgress<Double>? progress)
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

                ReportDoubleProgress(progress, () => 0.0);

                var entryCount = 0UL;
                var zipArchiveSize = file.Length;
                var processedUnpackedSize = 0UL;
                var processedPackedSize = 0UL;
                var totalProcessedRate = 0.0;

                using (var zipFile = file.OpenAsZipFile(zipEntryNameEncodingProvider, stringency))
                {
                    // 進捗率の配分は、GetEntries() が 10% で、データの比較が 90% とする。

                    var entries = zipFile.EnumerateEntries(
                        SafetyProgress.CreateIncreasingProgress<Double, Double>(
                            progress,
                            value => value * 0.1,
                            0,
                            1));
                    totalProcessedRate += 0.1;
                    ReportDoubleProgress(progress, () => totalProcessedRate);
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
                            processedPackedSize += entry.PackedSize;
                            totalProcessedRate += (Double)entry.PackedSize / zipArchiveSize * 0.9;
                        }
                        finally
                        {
                            ReportDoubleProgress(progress, () => totalProcessedRate);
                        }
                    }
                }

                ReportDoubleProgress(progress, () => 1.0);
                return new ZipArchiveValidationResult(ZipArchiveValidationResultId.Ok, $"entries = {entryCount}, total entry size = {processedUnpackedSize:N0} bytes, total compressed entry size = {processedPackedSize:N0} bytes", null);
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

        private static async Task<ZipArchiveValidationResult> InternalValidateZipFileAsync(FilePath file, IZipEntryNameEncodingProvider zipEntryNameEncodingProvider, ValidationStringency stringency, IProgress<Double>? progress, CancellationToken cancellationToken = default)
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

                ReportDoubleProgress(progress, () => 0.0);

                var entryCount = 0UL;
                var zipArchiveSize = file.Length;
                var processedUnpackedSize = 0UL;
                var processedPackedSize = 0UL;
                var totalProcessedRate = 0.0;

                using (var zipFile = file.OpenAsZipFile(zipEntryNameEncodingProvider, stringency))
                {
                    // 進捗率の配分は、GetEntries() が 10% で、データの比較が 90% とする。

                    var entries = zipFile.EnumerateEntriesAsync(
                        SafetyProgress.CreateIncreasingProgress<Double, Double>(
                            progress,
                            value => value * 0.1,
                            0,
                            1),
                        cancellationToken);
                    totalProcessedRate += 0.1;
                    ReportDoubleProgress(progress, () => totalProcessedRate);
                    var enumerator = entries.GetAsyncEnumerator(cancellationToken);
                    await using (enumerator.ConfigureAwait(false))
                    {
                        while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                        {
                            var entry = enumerator.Current;
                            try
                            {
                                await entry.ValidateDataAsync(
                                    SafetyProgress.CreateProgress<(UInt64 unpackedCount, UInt64 packedCount), Double>(
                                        progress,
                                        value => totalProcessedRate + (Double)value.packedCount / zipArchiveSize * 0.9),
                                    cancellationToken).ConfigureAwait(false);
                                ++entryCount;
                                processedUnpackedSize += entry.Size;
                                processedPackedSize += entry.PackedSize;
                                totalProcessedRate += (Double)entry.PackedSize / zipArchiveSize * 0.9;
                            }
                            finally
                            {
                                ReportDoubleProgress(progress, () => totalProcessedRate);
                            }
                        }
                    }
                }

                ReportDoubleProgress(progress, () => 1.0);
                return new ZipArchiveValidationResult(ZipArchiveValidationResultId.Ok, $"entries = {entryCount}, total entry size = {processedUnpackedSize:N0} bytes, total compressed entry size = {processedPackedSize:N0} bytes", null);
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

        private static IZipInputStream GetSourceStreamByFileNamePattern(DirectoryPath baseDirectory, FilePath sourceFile)
        {
            var match = _sevenZipMultiVolumeZipFileNamePattern.Match(sourceFile.Name);
            if (match.Success)
            {
                var fileNameWithoutExtension = match.Groups["body"].Value;
                return
                    SevenZipStyleMultiVolumeZipInputStream.CreateInstance(
                        EnumerateVolumeDiskFilesForFileNamePattern(
                            baseDirectory,
                            fileNameWithoutExtension),
                        diskNumber => GetSevenZipStyleVolumeDiskFile(baseDirectory, fileNameWithoutExtension, diskNumber));
            }
            else
            {
                return SingleVolumeZipInputStream.CreateInstance(sourceFile);
            }
        }

        private static IZipInputStream GetSourceStreamByLastDiskNumber(
            DirectoryPath baseDirectory,
            FilePath sourceFile,
            UInt32 lastDiskNumber,
            ValidationStringency stringency)
        {
            var match = _generalMultiVolumeZipFileNamePattern.Match(sourceFile.Name);
            if (!match.Success)
                throw new NotSupportedSpecificationException("Unknown format as multi-volume ZIP file.");

            var fileNameWithoutExtension = match.Groups["body"].Value;
            var selfExtract = CheckIfSelfExtractingZipArchive(baseDirectory, fileNameWithoutExtension);
            return
                MultiVolumeZipInputStream.CreateInstance(
                    EnumerateVolumeDiskSizes(
                        baseDirectory,
                        sourceFile,
                        lastDiskNumber,
                        fileNameWithoutExtension,
                        selfExtract),
                    diskNumber => GetGenericStyleVolumeDiskFile(baseDirectory, fileNameWithoutExtension, sourceFile, lastDiskNumber, diskNumber, selfExtract),
                    stringency);
        }

        private static IEnumerable<UInt64> EnumerateVolumeDiskFilesForFileNamePattern(DirectoryPath baseDirectory, String zipFileNameWithoutExtension)
        {
            for (var diskNumber = 0U; diskNumber <= UInt32.MaxValue; ++diskNumber)
            {
                var file = GetSevenZipStyleVolumeDiskFile(baseDirectory, zipFileNameWithoutExtension, diskNumber);
                if (!file.Exists)
                    break;
                yield return file.Length;
            }
        }

        private static FilePath GetSevenZipStyleVolumeDiskFile(
            DirectoryPath baseDirectory,
            String fileNameWithoutExtension,
            UInt32 diskNumber)
            => baseDirectory.GetFile($"{fileNameWithoutExtension}.{diskNumber + 1:D3}");

        private static IEnumerable<UInt64> EnumerateVolumeDiskSizes(
            DirectoryPath baseDirectory,
            FilePath baseFile,
            UInt32 lastDiskNumber,
            String zipFileNameWithoutExtension,
            Boolean selfExtract)
        {
            for (var diskNumber = 0U; diskNumber < lastDiskNumber; ++diskNumber)
            {
                var file = GetGenericStyleVolumeDiskFile(baseDirectory, zipFileNameWithoutExtension, baseFile, lastDiskNumber, diskNumber, selfExtract);
                if (!file.Exists)
                    throw new BadZipFileFormatException($"There is a missing disk in a multi-volume ZIP file.: volume-file=\"{file.FullName}\"");
                yield return file.Length;
            }

            yield return baseFile.Length;
        }

        private static Boolean CheckIfSelfExtractingZipArchive(DirectoryPath baseDirectory, String fileNameWithoutExtension)
            => !baseDirectory.GetFile($"{fileNameWithoutExtension}.z01").Exists
                && baseDirectory.GetFile($"{fileNameWithoutExtension}.exe").Exists;

        private static FilePath GetGenericStyleVolumeDiskFile(
            DirectoryPath baseDirectory,
            String fileNameWithoutExtension,
            FilePath baseFile,
            UInt32 lastDiskNumber,
            UInt32 diskNumber,
            Boolean selfExtract)
            => diskNumber <= 0
                ? baseDirectory.GetFile($"{fileNameWithoutExtension}.{(selfExtract ? "exe" : "z01")}")
                : diskNumber < lastDiskNumber
                ? baseDirectory.GetFile($"{fileNameWithoutExtension}.z{diskNumber + 1:D2}")
                : baseFile;

        private static void ReportDoubleProgress(IProgress<Double>? progress, Func<Double> valueGetter)
        {
            if (progress is not null)
            {
                try
                {
                    progress.Report(valueGetter());
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
