using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utility;
using Utility.IO;
using ZipUtility.Headers.Parser;

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
        private readonly UInt32 _eocdrDiskNumber;
        private readonly UInt64 _numberOfCentralDirectoryHeadersOnTheSameDiskAsEOCDR;
        private readonly ValidationStringency _stringency;

        private Boolean _isDisposed;

        private ZipArchiveFileReader(
            IZipEntryNameEncodingProvider entryNameEncodingProvider,
            IZipInputStream zipInputStream,
            FilePath zipArchiveFile,
            ZipStreamPosition centralDirectoryPosition,
            UInt64 totalNumberOfCentralDirectoryRecords,
            UInt32 eocdrDiskNumber,
            UInt64 numberOfCentralDirectoryHeadersOnTheSameDiskAsEOCDR,
            ReadOnlyMemory<Byte> commentBytes,
            ZipStreamPosition eocdrPosition,
            ValidationStringency stringency)
        {
            _isDisposed = false;
            _paramter = new ReaderParameter(entryNameEncodingProvider, zipArchiveFile);
            _zipInputStream = zipInputStream;
            _centralDirectoryPosition = centralDirectoryPosition;
            _totalNumberOfCentralDirectoryRecords = totalNumberOfCentralDirectoryRecords;
            _eocdrDiskNumber = eocdrDiskNumber;
            _numberOfCentralDirectoryHeadersOnTheSameDiskAsEOCDR = numberOfCentralDirectoryHeadersOnTheSameDiskAsEOCDR;
            CommentBytes = commentBytes;
            var commentEncoding = _paramter.ZipEntryNameEncodingProvider.GetBestEncodings(ReadOnlyMemory<Byte>.Empty, null, commentBytes, null).FirstOrDefault();
            Comment =
                commentEncoding is not null
                ? commentEncoding.GetString(commentBytes)
                : commentBytes.GetStringByUnknownDecoding();
            EOCDRPosition = eocdrPosition;
            _stringency = stringency;
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

        internal ZipStreamPosition EOCDRPosition { get; }

        /// <summary>
        /// サポートされている圧縮方式のIDのコレクションを取得します。
        /// </summary>
        public static IEnumerable<ZipEntryCompressionMethodId> SupportedCompressionIds
            => ZipEntryCompressionMethod.SupportedCompresssionMethodIds;

        internal static ZipArchiveFileReader Parse(FilePath zipArchiveFile, IZipInputStream zipInputStream, IZipEntryNameEncodingProvider entryNameEncodingProvider, ValidationStringency stringency)
        {
            if (entryNameEncodingProvider is null)
                throw new ArgumentNullException(nameof(entryNameEncodingProvider));
            if (zipArchiveFile is null)
                throw new ArgumentNullException(nameof(zipArchiveFile));
            if (zipInputStream is null)
                throw new ArgumentNullException(nameof(zipInputStream));

            var paramter = new ReaderParameter(entryNameEncodingProvider, zipArchiveFile);

            var lastDiskHeader = ZipFileLastDiskHeader.Parse(zipInputStream, stringency);
            if (lastDiskHeader.Zip64EOCDL is not null)
            {
                var zip64EOCDR = ZipFileZip64EOCDR.Parse(paramter, zipInputStream, lastDiskHeader.Zip64EOCDL);
                if (stringency > ValidationStringency.Normal)
                {
                    // ZIP64 EOCDR の末尾に ZIP64 EOCDL の先頭が隣接していることの確認

                    var unknownPayloadSize = lastDiskHeader.Zip64EOCDL.Zip64EOCDLPosition - zip64EOCDR.Zip64EOCDRPosition - zip64EOCDR.HeaderSize;
                    if (unknownPayloadSize > 0)
                        throw new BadZipFileFormatException($"Unknown payload exists between ZIP64 EOCDR and ZIP64 EOCDL.: position=\"{lastDiskHeader.Zip64EOCDL.Zip64EOCDLPosition - unknownPayloadSize}\", size=0x{unknownPayloadSize:x16}");
                }

                ValidateEOCDR(lastDiskHeader.EOCDR, zip64EOCDR, stringency);
                var centralDirectoryPosition =
                    zipInputStream.GetPosition(
                        zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory,
                        zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber)
                    ?? throw new BadZipFileFormatException($"The central directory header position read from ZIP64 EOCDR does not point to the correct disk position.: {nameof(zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory)}=0x{zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory:x8}, {nameof(zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber)}=0x{zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber:x16}");

                return
                    new ZipArchiveFileReader(
                        entryNameEncodingProvider,
                        zipInputStream,
                        zipArchiveFile,
                        centralDirectoryPosition,
                        zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory,
                        zip64EOCDR.NumberOfThisDisk,
                        zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk,
                        lastDiskHeader.EOCDR.CommentBytes,
                        zip64EOCDR.Zip64EOCDRPosition,
                        stringency);
            }
            else
            {
                if (lastDiskHeader.EOCDR.IsRequiresZip64)
                    throw new InternalLogicalErrorException();

                var centralDirectoryPosition =
                    zipInputStream.GetPosition(
                        lastDiskHeader.EOCDR.DiskWhereCentralDirectoryStarts,
                        lastDiskHeader.EOCDR.OffsetOfStartOfCentralDirectory)
                    ?? throw new BadZipFileFormatException($"The central directory header position read from EOCDR does not point to the correct disk position.: {nameof(lastDiskHeader.EOCDR.DiskWhereCentralDirectoryStarts)}=0x{lastDiskHeader.EOCDR.DiskWhereCentralDirectoryStarts:x4}, {nameof(lastDiskHeader.EOCDR.OffsetOfStartOfCentralDirectory)}=0x{lastDiskHeader.EOCDR.OffsetOfStartOfCentralDirectory:x8}");

                return
                    new ZipArchiveFileReader(
                        entryNameEncodingProvider,
                        zipInputStream,
                        zipArchiveFile,
                        centralDirectoryPosition,
                        lastDiskHeader.EOCDR.TotalNumberOfCentralDirectoryRecords,
                        lastDiskHeader.EOCDR.NumberOfThisDisk,
                        lastDiskHeader.EOCDR.NumberOfCentralDirectoryRecordsOnThisDisk,
                        lastDiskHeader.EOCDR.CommentBytes,
                        lastDiskHeader.EOCDR.EOCDRPosition,
                        stringency);
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

            var centralDirectoryHeaders =
                ZipEntryCentralDirectoryHeader.Enumerate(
                    _paramter,
                    this,
                    _centralDirectoryPosition,
                    _totalNumberOfCentralDirectoryRecords,
                    _stringency,
                    cancellationToken)
                .ToList();
            if (_stringency > ValidationStringency.Normal)
            {
                var centralDirectoriesCount = 0UL;
                for (var index = centralDirectoryHeaders.Count - 1; index >= 0; --index)
                {
                    if (centralDirectoryHeaders[index].CentralDirectoryHeaderPosition.DiskNumber != _eocdrDiskNumber)
                        break;
                    checked
                    {
                        ++centralDirectoriesCount;
                    }
                }

                if (centralDirectoriesCount != _numberOfCentralDirectoryHeadersOnTheSameDiskAsEOCDR)
                    throw new BadZipFileFormatException($"The number of central directory headers on the same volume as EOCDR is different. : expected number=0x{_numberOfCentralDirectoryHeadersOnTheSameDiskAsEOCDR:x16}, actual number=0x{centralDirectoriesCount:x16}");

                if (centralDirectoryHeaders.Count > 0)
                {
                    // セントラルディレクトリヘッダ同士が隣接していることの確認

                    for (var index = 0; index < centralDirectoryHeaders.Count - 1; ++index)
                    {
                        var centralDirectoryHeader1 = centralDirectoryHeaders[index];
                        var centralDirectoryHeader2 = centralDirectoryHeaders[index + 1];
                        var unknownPayloadSize = centralDirectoryHeader2.CentralDirectoryHeaderPosition - centralDirectoryHeader1.CentralDirectoryHeaderPosition - centralDirectoryHeader1.HeaderSize;
                        if (unknownPayloadSize > 0)
                            throw new BadImageFormatException($"An unknown payload exists between central directory headers.: position=\"{centralDirectoryHeader2.CentralDirectoryHeaderPosition - unknownPayloadSize}\", size=0x{unknownPayloadSize:x16}");
                    }

                    // 最後のセントラルディレクトリヘッダと EOCDR (or ZIP64 EOCDR) が隣接していることの確認

                    var lastCentralDirectoryHeader = centralDirectoryHeaders[^1];
                    var lastUnknownPayloadSize = EOCDRPosition - lastCentralDirectoryHeader.CentralDirectoryHeaderPosition - lastCentralDirectoryHeader.HeaderSize;
                    if (lastUnknownPayloadSize > 0)
                        throw new BadImageFormatException($"An unknown payload exists between the last central directory header and the EOCDR (or ZIP64 EOCDR).: position=\"{EOCDRPosition - lastUnknownPayloadSize}\", size=0x{lastUnknownPayloadSize:x16}");
                }
            }

            var contentHeaders =
                centralDirectoryHeaders
                .Select(centralHeader =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var headerObject =
                        new ZipEntryHeader(
                            centralHeader,
                            ZipEntryLocalHeader.Parse(_paramter, this, centralHeader, _stringency));
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
                .ToList();

            if (_stringency > ValidationStringency.Normal)
            {
                if (contentHeaders.Count > 0)
                {
                    // ローカルヘッダ間に未知のペイロードがないことを確認する

                    for (var index = 0; index < contentHeaders.Count - 1; ++index)
                    {
                        var localHeader = contentHeaders[index].LocalHeader;
                        var endOfContent = localHeader.DataPosition + localHeader.PackedSize + (localHeader.DataDescriptor?.HeaderSize ?? 0);
                        var nextLocalHeader = contentHeaders[index + 1].LocalHeader;
                        var unknownPayloadSize = nextLocalHeader.LocalHeaderPosition - endOfContent;
                        if (unknownPayloadSize > 0)
                            throw new BadImageFormatException($"An unknown payload exists between local headers.: position=\"{nextLocalHeader.LocalHeaderPosition - unknownPayloadSize}\", size=0x{unknownPayloadSize:x16}");
                    }

                    var firstCentralDirectoryHeaderPosition =
                        centralDirectoryHeaders.Count > 0
                        ? centralDirectoryHeaders[0].CentralDirectoryHeaderPosition
                        : EOCDRPosition;

                    var lastLocalheader = contentHeaders[^1].LocalHeader;
                    var endOfLastContent = lastLocalheader.DataPosition + lastLocalheader.PackedSize + (lastLocalheader.DataDescriptor?.HeaderSize ?? 0);
                    var lastUnknownPayloadSize = firstCentralDirectoryHeaderPosition - endOfLastContent;
                    if (lastUnknownPayloadSize > 0)
                        throw new BadImageFormatException($"An unknown payload exists between the last local header and the first central directory header.: position=\"{firstCentralDirectoryHeaderPosition - lastUnknownPayloadSize}\", size=0x{lastUnknownPayloadSize:x16}");
                }
            }

            var entries =
                new ZipArchiveEntryCollection(
                    contentHeaders
                    .Select((header, order) => new ZipSourceEntry(_paramter, this, header, order)));
            progress?.Report(1);
            return entries;
        }

        private static void ValidateEOCDR(ZipFileEOCDR eocdr, ZipFileZip64EOCDR zip64EOCDR, ValidationStringency stringency)
        {
            if (stringency > ValidationStringency.Normal)
            {
                // ZIP64 EOCDR と EOCDR の整合性を確認する。

                if (zip64EOCDR.NumberOfThisDisk == eocdr.EOCDRPosition.DiskNumber)
                {
                    // ZIP64 EOCDR が EOCDR と同じボリュームディスクにある場合

                    if (eocdr.NumberOfCentralDirectoryRecordsOnThisDisk != UInt16.MaxValue)
                    {
                        // EOCDR の フィールド NumberOfCentralDirectoryRecordsOnThisDisk が UInt16.MaxValue ではない場合
                        // => ZIP64 EOCDR の TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk も同じ値であるはず。

                        if (eocdr.NumberOfCentralDirectoryRecordsOnThisDisk != zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk)
                            throw new BadZipFileFormatException($"Since ZIP64 EOCDR and EOCDR are on the same volume disk, and the value of field {nameof(eocdr.NumberOfCentralDirectoryRecordsOnThisDisk)} in EOCDR is not 0x{UInt16.MaxValue:x4}, the value of field {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk)} in ZIP64 EOCDR should be equal to the value of field {nameof(eocdr.NumberOfCentralDirectoryRecordsOnThisDisk)} in EOCDR. However, the value of field {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk)} of ZIP64 EOCDR is actually different from the value of field {nameof(eocdr.NumberOfCentralDirectoryRecordsOnThisDisk)} of EOCDR.: {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk)}=0x{zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk:x16}, {nameof(eocdr.NumberOfCentralDirectoryRecordsOnThisDisk)}=0x{eocdr.NumberOfCentralDirectoryRecordsOnThisDisk:x4}");
                    }
                    else
                    {
                        // EOCDR の フィールド NumberOfCentralDirectoryRecordsOnThisDisk が UInt16.MaxValue である場合
                        // => ZIP64 EOCDR の TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk は UInt16.MaxValue 以上であるはず。

                        if (zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk < UInt16.MaxValue)
                            throw new BadZipFileFormatException($"Since ZIP64 EOCDR and EOCDR are on the same volume disk, and EOCDR's field {nameof(eocdr.NumberOfCentralDirectoryRecordsOnThisDisk)} has a value of 0x{UInt16.MaxValue:x4}, ZIP64 EOCDR's field {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk)} should have a value greater than or equal to 0x{UInt16.MaxValue:x16}. But actually the value of field {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk)} in ZIP64 EOCDR is less than 0x{UInt16.MaxValue:x16}.: {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk)}=0x{zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk:x16}");
                    }
                }
                else
                {
                    // ZIP64 EOCDR が EOCDR と異なるボリュームディスクにある場合
                    // => 少なくとも、最後のボリュームディスク (つまり EOCDR があるボリュームディスク) にはセントラルディレクトリヘッダは存在しないはず。

                    if (eocdr.NumberOfCentralDirectoryRecordsOnThisDisk != 0)
                        throw new BadZipFileFormatException($"Since ZIP64 ECDR and ECDR are on different volume disks, the volume disk where ECDR is located (that is, the last volume disk) should not have a central directory header. However, the value of the ECDR field {nameof(eocdr.NumberOfCentralDirectoryRecordsOnThisDisk)} is actually not 0. : {nameof(eocdr.NumberOfCentralDirectoryRecordsOnThisDisk)}={eocdr.NumberOfCentralDirectoryRecordsOnThisDisk}");
                }

                if (eocdr.DiskWhereCentralDirectoryStarts == UInt16.MaxValue)
                {
                    if (zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory < UInt16.MaxValue)
                        throw new BadZipFileFormatException($"The value of field {nameof(eocdr.DiskWhereCentralDirectoryStarts)} in EOCDR is 0x{UInt16.MaxValue:x4}, so the value of field {nameof(zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory)} in ZIP64 EOCDR must be greater than or equal to 0x{UInt16.MaxValue:x8}. But actually the value of field {nameof(zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory)} of ZIP64 EOCDR is less than 0x{UInt16.MaxValue:x8}. : {nameof(zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory)}=0x{zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory:x8}");
                }
                else
                {
                    if (eocdr.DiskWhereCentralDirectoryStarts != zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory)
                        throw new BadZipFileFormatException($"The value of field {nameof(eocdr.DiskWhereCentralDirectoryStarts)} in EOCDR is not 0x{UInt16.MaxValue:x4}, so the value of field {nameof(zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory)} in ZIP64 EOCDR must be equal to the value of field {nameof(eocdr.DiskWhereCentralDirectoryStarts)} in EOCDR. But actually, the value of field {nameof(zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory)} in ZIP64 EOCDR is different from the value of field {nameof(eocdr.DiskWhereCentralDirectoryStarts)} in EOCDR.: {nameof(zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory)}=0x{zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory:x8}, {nameof(eocdr.DiskWhereCentralDirectoryStarts)}=0x{eocdr.DiskWhereCentralDirectoryStarts:x4}");
                }

                if (eocdr.OffsetOfStartOfCentralDirectory == UInt32.MaxValue)
                {
                    if (zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber < UInt32.MaxValue)
                        throw new BadZipFileFormatException($"The value of field {nameof(eocdr.OffsetOfStartOfCentralDirectory)} in EOCDR is 0x{UInt32.MaxValue:x8}, so the value of field {nameof(zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber)} in ZIP64 EOCDR must be greater than or equal to 0x{UInt32.MaxValue:x16}. But actually the value of field {nameof(zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber)} of ZIP64 EOCDR is less than 0x{UInt32.MaxValue:x16}. : {nameof(zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber)}=0x{zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber:x16}");
                }
                else
                {
                    if (eocdr.OffsetOfStartOfCentralDirectory != zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber)
                        throw new BadZipFileFormatException($"The value of field {nameof(eocdr.OffsetOfStartOfCentralDirectory)} in EOCDR is not 0x{UInt32.MaxValue:x8}, so the value of field {nameof(zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber)} in ZIP64 EOCDR must be equal to the value of field {nameof(eocdr.OffsetOfStartOfCentralDirectory)} in EOCDR. But actually, the value of field {nameof(zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber)} in ZIP64 EOCDR is different from the value of field {nameof(eocdr.OffsetOfStartOfCentralDirectory)} in EOCDR.: {nameof(zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber)}=0x{zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber:x16}, {nameof(eocdr.OffsetOfStartOfCentralDirectory)}=0x{eocdr.OffsetOfStartOfCentralDirectory:x8}");
                }

                if (eocdr.TotalNumberOfCentralDirectoryRecords == UInt16.MaxValue)
                {
                    if (zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory < UInt16.MaxValue)
                        throw new BadZipFileFormatException($"The value of field {nameof(eocdr.TotalNumberOfCentralDirectoryRecords)} in EOCDR is 0x{UInt16.MaxValue:x4}, so the value of field {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory)} in ZIP64 EOCDR must be greater than or equal to 0x{UInt16.MaxValue:x16}. But actually the value of field {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory)} of ZIP64 EOCDR is less than 0x{UInt16.MaxValue:x16}. : {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory)}=0x{zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory:x16}");
                }
                else
                {
                    if (eocdr.TotalNumberOfCentralDirectoryRecords != zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory)
                        throw new BadZipFileFormatException($"The value of field {nameof(eocdr.TotalNumberOfCentralDirectoryRecords)} in EOCDR is not 0x{UInt16.MaxValue:x4}, so the value of field {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory)} in ZIP64 EOCDR must be equal to the value of field {nameof(eocdr.TotalNumberOfCentralDirectoryRecords)} in EOCDR. But actually, the value of field {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory)} in ZIP64 EOCDR is different from the value of field {nameof(eocdr.TotalNumberOfCentralDirectoryRecords)} in EOCDR.: {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory)}=0x{zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory:x16}, {nameof(eocdr.TotalNumberOfCentralDirectoryRecords)}=0x{eocdr.TotalNumberOfCentralDirectoryRecords:x4}");
                }

                if (eocdr.SizeOfCentralDirectory == UInt32.MaxValue)
                {
                    if (zip64EOCDR.SizeOfTheCentralDirectory < UInt32.MaxValue)
                        throw new BadZipFileFormatException($"The value of field {nameof(eocdr.SizeOfCentralDirectory)} in EOCDR is 0x{UInt32.MaxValue:x8}, so the value of field {nameof(zip64EOCDR.SizeOfTheCentralDirectory)} in ZIP64 EOCDR must be greater than or equal to 0x{UInt32.MaxValue:x16}. But actually the value of field {nameof(zip64EOCDR.SizeOfTheCentralDirectory)} of ZIP64 EOCDR is less than 0x{UInt32.MaxValue:x16}. : {nameof(zip64EOCDR.SizeOfTheCentralDirectory)}=0x{zip64EOCDR.SizeOfTheCentralDirectory:x16}");
                }
                else
                {
                    if (eocdr.SizeOfCentralDirectory != zip64EOCDR.SizeOfTheCentralDirectory)
                        throw new BadZipFileFormatException($"The value of field {nameof(eocdr.SizeOfCentralDirectory)} in EOCDR is not 0x{UInt32.MaxValue:x8}, so the value of field {nameof(zip64EOCDR.SizeOfTheCentralDirectory)} in ZIP64 EOCDR must be equal to the value of field {nameof(eocdr.SizeOfCentralDirectory)} in EOCDR. But actually, the value of field {nameof(zip64EOCDR.SizeOfTheCentralDirectory)} in ZIP64 EOCDR is different from the value of field {nameof(eocdr.SizeOfCentralDirectory)} in EOCDR.: {nameof(zip64EOCDR.SizeOfTheCentralDirectory)}=0x{zip64EOCDR.SizeOfTheCentralDirectory:x16}, {nameof(eocdr.SizeOfCentralDirectory)}=0x{eocdr.SizeOfCentralDirectory:x8}");
                }
            }
        }
    }
}
