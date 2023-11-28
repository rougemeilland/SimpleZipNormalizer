using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utility;
using Utility.IO;
using ZipUtility.ZipFileHeader;

namespace ZipUtility
{
    /// <summary>
    /// 読み込み専用の ZIP アーカイバ のクラスです。
    /// </summary>
    public class ZipArchiveFileReader
        : IDisposable, IAsyncDisposable, ZipArchiveFileReader.IZipReaderStream
    {
        internal interface IZipReaderEnvironment
        {
            IZipEntryNameEncodingProvider ZipEntryNameEncodingProvider { get; }
            Boolean CheckVersion(UInt16 versionNeededToExtract);
            Byte ThisSoftwareVersion { get; }
            FilePath ZipArchiveFile { get; }
        }

        internal interface IZipReaderStream
        {
            IZipInputStream Stream { get; }
        }

        private class ReaderParameter
            : IZipReaderEnvironment
        {
            private readonly FilePath _zipArchiveFile;

            public ReaderParameter(IZipEntryNameEncodingProvider entryNameEncodingProvider, FilePath zipArchiveFile)
            {
                ZipEntryNameEncodingProvider = entryNameEncodingProvider;
                _zipArchiveFile = zipArchiveFile;
            }

            public IZipEntryNameEncodingProvider ZipEntryNameEncodingProvider { get; }

            public Boolean CheckVersion(UInt16 versionNeededToExtract)
                => versionNeededToExtract <= _zipReadVersion;

            public Byte ThisSoftwareVersion => _zipReadVersion;

            public FilePath ZipArchiveFile => new(_zipArchiveFile.FullName);
        }

        private const Byte _zipReadVersion = 63; // 開発時点での APPNOTE のバージョン
        private readonly ReaderParameter _paramter;
        private readonly IZipInputStream _zipInputStream;
        private readonly ZipStreamPosition _centralDirectoryPosition;
        private readonly UInt64 _totalNumberOfCentralDirectoryRecords;

        private Boolean _isDisposed;

        private ZipArchiveFileReader(IZipEntryNameEncodingProvider entryNameEncodingProvider, IZipInputStream zipInputStream, FilePath zipArchiveFile, ZipStreamPosition centralDirectoryPosition, UInt64 totalNumberOfCentralDirectoryRecords, ReadOnlyMemory<Byte> commentBytes)
        {
            _isDisposed = false;
            _paramter = new ReaderParameter(entryNameEncodingProvider, zipArchiveFile);
            _zipInputStream = zipInputStream;
            _centralDirectoryPosition = centralDirectoryPosition;
            _totalNumberOfCentralDirectoryRecords = totalNumberOfCentralDirectoryRecords;
            CommentBytes = commentBytes;
            var commentEncoding = _paramter.ZipEntryNameEncodingProvider.GetBestEncodings(ReadOnlyMemory<Byte>.Empty, null, commentBytes, null).FirstOrDefault();
            Comment =
                commentEncoding is not null
                ? commentEncoding.GetString(commentBytes)
                : commentBytes.GetStringByUnknownDecoding();
#if DEBUG && false
            System.Diagnostics.Debug.WriteLine($"Created {GetType().FullName}: \"{zipArchiveFile.FullName}\"");
#endif
        }

        /// <summary>
        /// ZIP アーカイブのコメントの文字列を取得します。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// ZIP アーカイブのコメントのエンコーディング方式は規定されていないので、正しくデコードできていない可能性があります。
        /// </item>
        /// </list>
        /// </remarks>
        public String Comment { get; }

        /// <summary>
        /// ZIP アーカイブのコメントのバイト列を取得します。
        /// </summary>
        public ReadOnlyMemory<Byte> CommentBytes { get; }

        /// <summary>
        /// サポートされている圧縮方式のIDのコレクションを取得します。
        /// </summary>
        public static IEnumerable<ZipEntryCompressionMethodId> SupportedCompressionIds
            => ZipEntryCompressionMethod.SupportedCompresssionMethodIds;

        internal static ZipArchiveFileReader Parse(IZipEntryNameEncodingProvider entryNameEncodingProvider, FilePath zipArchiveFile, IZipInputStream zipInputStream)
        {
            if (entryNameEncodingProvider is null)
                throw new ArgumentNullException(nameof(entryNameEncodingProvider));
            if (zipArchiveFile is null)
                throw new ArgumentNullException(nameof(zipArchiveFile));
            if (zipInputStream is null)
                throw new ArgumentNullException(nameof(zipInputStream));

            var paramter = new ReaderParameter(entryNameEncodingProvider, zipArchiveFile);

            var lastDiskHeader = ZipFileLastDiskHeader.Parse(zipInputStream);
            if (lastDiskHeader.EOCDR.IsRequiresZip64 || lastDiskHeader.Zip64EOCDL is not null)
            {
                if (lastDiskHeader.Zip64EOCDL is null)
                    throw new BadZipFileFormatException("Not found 'zip64 end of central directory locator' in Zip file");
                if (!zipInputStream.IsMultiVolumeZipStream && lastDiskHeader.Zip64EOCDL.TotalNumberOfDisks > 1)
                    throw new MultiVolumeDetectedException(lastDiskHeader.Zip64EOCDL.TotalNumberOfDisks - 1U);
                var zip64EOCDR = ZipFileZip64EOCDR.Parse(paramter, zipInputStream, lastDiskHeader.Zip64EOCDL);
                var centralDirectoryPosition =
                    zipInputStream.GetPosition(
                        zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory,
                        zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber);
                return
                    new ZipArchiveFileReader(
                        entryNameEncodingProvider,
                        zipInputStream,
                        zipArchiveFile,
                        centralDirectoryPosition,
                        zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory,
                        lastDiskHeader.EOCDR.CommentBytes);
            }
            else
            {
                if (!zipInputStream.IsMultiVolumeZipStream && lastDiskHeader.EOCDR.NumberOfThisDisk >= 1)
                    throw new MultiVolumeDetectedException(lastDiskHeader.EOCDR.NumberOfThisDisk);
                var centralDirectoryPosition =
                    zipInputStream.GetPosition(
                        lastDiskHeader.EOCDR.DiskWhereCentralDirectoryStarts,
                        lastDiskHeader.EOCDR.OffsetOfStartOfCentralDirectory);
                return
                    new ZipArchiveFileReader(
                        entryNameEncodingProvider,
                        zipInputStream,
                        zipArchiveFile,
                        centralDirectoryPosition,
                        lastDiskHeader.EOCDR.TotalNumberOfCentralDirectoryRecords,
                        lastDiskHeader.EOCDR.CommentBytes);
            }
        }

        /// <summary>
        /// ZIP アーカイバに含まれているエントリのコレクションを取得します。
        /// </summary>
        /// <param name="progress">
        /// <para>
        /// 処理の進行状況の通知を受け取るためのオブジェクトです。通知を受け取らない場合は null です。
        /// </para>
        /// <para>
        /// 進行状況は、0 以上 1 以下の <see cref="Double"/> 値です。初期値は 0 で、作業が進行するごとに増加していき、作業が完了すると 1 になります。
        /// </para>
        /// </param>
        /// <returns>
        /// ZIP アーカイバに含まれているエントリのコレクションです。
        /// </returns>
        public ZipArchiveEntryCollection GetEntries(IProgress<Double>? progress = null)
            => InternalGetEntries(default, progress);

        /// <summary>
        /// ZIP アーカイバに含まれているエントリのコレクションを取得します。
        /// </summary>
        /// <param name="cancellationToken">
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
        /// ZIP アーカイバに含まれているエントリのコレクションです。
        /// </returns>
        public Task<ZipArchiveEntryCollection> GetEntriesAsync(IProgress<Double>? progress = null, CancellationToken cancellationToken = default)
            => Task.FromResult(InternalGetEntries(cancellationToken, progress));

        /// <summary>
        /// オブジェクトに関連付けられたリソースを解放します。
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// オブジェクトに関連付けられたリソースを非同期的に解放します。
        /// </summary>
        /// <returns>
        /// オブジェクトに関連付けられたリソースを解放するタスクです。
        /// </returns>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// オブジェクトの内容を分かりやすい文字列に変換します。
        /// </summary>
        /// <returns>
        /// オブジェクトの内容を示す文字列です。
        /// </returns>
        public override String ToString() => $"\"{_paramter.ZipArchiveFile.FullName}\"";

        IZipInputStream IZipReaderStream.Stream
        {
            get
            {
#if DEBUG && false
                System.Diagnostics.Debug.WriteLine($"Reference {GetType().FullName}.Stream: \"{_paramter.ZipArchiveFile.FullName}\"");
#endif
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                return _zipInputStream;
            }
        }

        /// <summary>
        /// オブジェクトに関連付けられたリソースを解放します。
        /// </summary>
        /// <param name="disposing">
        /// <see cref="Dispose()"/> から呼び出された場合は true です。
        /// </param>
        protected virtual void Dispose(Boolean disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                    _zipInputStream.Dispose();
                _isDisposed = true;
#if DEBUG && false
                System.Diagnostics.Debug.WriteLine($"Disposed {GetType().FullName}: \"{_paramter.ZipArchiveFile.FullName}\"");
#endif
            }
        }

        /// <summary>
        /// オブジェクトに関連付けられたリソースを非同期的に解放します。
        /// </summary>
        /// <returns>
        /// オブジェクトに関連付けられたリソースを解放するタスクです。
        /// </returns>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!_isDisposed)
            {
                await _zipInputStream.DisposeAsync().ConfigureAwait(false);
                _isDisposed = true;
            }
        }

        private ZipArchiveEntryCollection InternalGetEntries(CancellationToken cancellationToken, IProgress<Double>? progress)
        {
            progress?.Report(0);
            var totalCount = _totalNumberOfCentralDirectoryRecords;
            var count = 0UL;

            cancellationToken.ThrowIfCancellationRequested();

            var collection =
                new ZipArchiveEntryCollection(
                    ZipEntryCentralDirectoryHeader.Enumerate(
                        _paramter,
                        this,
                        _centralDirectoryPosition,
                        _totalNumberOfCentralDirectoryRecords,
                        cancellationToken)
                    .Select(centralHeader =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var headerObject =
                            new ZipEntryHeader(
                                centralDirectoryHeader: centralHeader,
                                ZipEntryLocalHeader.Parse(_paramter, this, centralHeader));
                        ++count;
                        var progressValue = (Double)count / totalCount;
#if DEBUG
                        if (!progressValue.IsBetween(0.0, 1.0))
                            throw new Exception();
#endif
                        progress?.Report(progressValue);
                        return headerObject;
                    })
                    .OrderBy(header => header.CentralDirectoryHeader.LocalHeaderPosition)
                    .Select((header, order) => new ZipSourceEntry(_paramter, this, header, order)));
            progress?.Report(1);
            return collection;
        }
    }
}
