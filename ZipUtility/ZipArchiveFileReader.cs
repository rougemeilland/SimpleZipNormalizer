using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            void LockZipStream();
            Task LockZipStreamAsync(CancellationToken cancellationToken);
            void UnlockZipStream();
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
                => versionNeededToExtract <= _ZIP_READER_VERSION;

            public Byte ThisSoftwareVersion => _ZIP_READER_VERSION;

            public FilePath ZipArchiveFile => new(_zipArchiveFile.FullName);
        }

        private const Byte _ZIP_READER_VERSION = 63; // 開発時点での APPNOTE のバージョン
        private readonly ReaderParameter _paramter;
        private readonly IZipInputStream _zipInputStream;
        private readonly ZipStreamPosition _startOfCentralDirectoryHeaders;
        private readonly UInt64 _totalNumberOfCentralDirectoryHeaders;
        private readonly UInt32 _eocdrDiskNumber;
        private readonly UInt64 _numberOfCentralDirectoryHeadersOnTheSameDiskAsEOCDR;
        private readonly ValidationStringency _stringency;
        private readonly SemaphoreSlim _zipStreamLockObject;
        private readonly FragmentSet<ZipStreamPosition, UInt64> _unknownPayloads;

        private Boolean _isDisposed;
        private Boolean _isEnumerating;

        private ZipArchiveFileReader(
            IZipEntryNameEncodingProvider entryNameEncodingProvider,
            IZipInputStream zipInputStream,
            FilePath zipArchiveFile,
            ZipStreamPosition startOfCentralDirectoryHeaders,
            UInt64 totalNumberOfCentralDirectoryHeaders,
            UInt32 eocdrDiskNumber,
            UInt64 numberOfCentralDirectoryHeadersOnTheSameDiskAsEOCDR,
            ReadOnlyMemory<Byte> commentBytes,
            ZipStreamPosition eocdrPosition,
            FragmentSet<ZipStreamPosition, UInt64> unknownPayloads,
            ValidationStringency stringency)
        {
            _paramter = new ReaderParameter(entryNameEncodingProvider, zipArchiveFile);
            _zipInputStream = zipInputStream;
            _startOfCentralDirectoryHeaders = startOfCentralDirectoryHeaders;
            _totalNumberOfCentralDirectoryHeaders = totalNumberOfCentralDirectoryHeaders;
            _eocdrDiskNumber = eocdrDiskNumber;
            _numberOfCentralDirectoryHeadersOnTheSameDiskAsEOCDR = numberOfCentralDirectoryHeadersOnTheSameDiskAsEOCDR;
            CommentBytes = commentBytes;
            var commentEncoding = _paramter.ZipEntryNameEncodingProvider.GetBestEncodings(ReadOnlyMemory<Byte>.Empty, null, commentBytes, null).FirstOrDefault();
            Comment =
                commentEncoding is not null
                ? commentEncoding.GetString(commentBytes)
                : commentBytes.GetStringByUnknownDecoding();
            EOCDRPosition = eocdrPosition;
            _unknownPayloads = unknownPayloads;
            _stringency = stringency;
            _zipStreamLockObject = new SemaphoreSlim(1, 1);

            _isDisposed = false;
            _isEnumerating = false;
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

        /// <summary>
        /// ZIP アーカイブのエントリを列挙します。
        /// </summary>
        /// <param name="progress">
        /// 列挙の進捗状況を受け取るオブジェクトです。進捗状況の値は 0.0 から始まって 1.0 で終わります。
        /// </param>
        /// <returns>
        /// ZIP アーカイブのエントリの列挙子です。
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// 既に列挙が行われています。
        /// </exception>
        public IEnumerable<ZipSourceEntry> EnumerateEntries(IProgress<Double>? progress = null)
        {
            PrepareToEnumerate();
            try
            {
                var count = 0UL;
                ReportDoubleProgress(progress, () => 0.0);
                foreach (var centralDirectoryHeader in EnumerateCentralDirectoryHeaders())
                {
                    var localHeader = ParseLocalHeader(centralDirectoryHeader);

                    yield return
                        new ZipSourceEntry(
                            _paramter,
                            this,
                            new ZipEntryHeader(centralDirectoryHeader, localHeader));

                    MarkAsKnownPayload(
                        localHeader.LocalHeaderPosition,
                        localHeader.HeaderSize + localHeader.PackedSize + (localHeader.DataDescriptor?.HeaderSize ?? 0));

                    checked
                    {
                        ++count;
                    }

                    ReportDoubleProgress(progress, () => (Double)count / _totalNumberOfCentralDirectoryHeaders);
                }

                CheckUnknownPayloads();
                ReportDoubleProgress(progress, () => 1.0);
            }
            finally
            {
                FinishToEnumerate();
            }
        }

        /// <summary>
        /// ZIP アーカイブのエントリを非同期的に列挙します。
        /// </summary>
        /// <param name="progress">
        /// 列挙の進捗状況を受け取るオブジェクトです。進捗状況の値は 0.0 から始まって 1.0 で終わります。
        /// </param>
        /// <param name="cancellationToken">
        /// キャンセル要求を監視するためのトークンです。既定値は <see cref="CancellationToken.None"/> です。
        /// </param>
        /// <returns>
        /// ZIP アーカイブのエントリの列挙子です。
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// 既に列挙が行われています。
        /// </exception>
        public async IAsyncEnumerable<ZipSourceEntry> EnumerateEntriesAsync(IProgress<Double>? progress = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            PrepareToEnumerate();
            try
            {
                var count = 0UL;
                ReportDoubleProgress(progress, () => 0.0);
                var localHeaderFragments = new FragmentSet<ZipStreamPosition, UInt64>(_zipInputStream.StartOfThisStream, _startOfCentralDirectoryHeaders - _zipInputStream.StartOfThisStream);
                var enumerator = EnumerateCentralDirectoryHeadersAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);
                await using (enumerator.ConfigureAwait(false))
                {
                    while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                    {
                        var centralDirectoryHeader = enumerator.Current;
                        var localHeader = await ParseLocalHeaderAsync(centralDirectoryHeader, cancellationToken).ConfigureAwait(false);

                        yield return
                            new ZipSourceEntry(
                                _paramter,
                                this,
                                new ZipEntryHeader(centralDirectoryHeader, localHeader));

                        MarkAsKnownPayload(
                            localHeader.LocalHeaderPosition,
                            localHeader.HeaderSize + localHeader.PackedSize + (localHeader.DataDescriptor?.HeaderSize ?? 0));

                        checked
                        {
                            ++count;
                        }

                        ReportDoubleProgress(progress, () => (Double)count / _totalNumberOfCentralDirectoryHeaders);
                    }
                }

                CheckUnknownPayloads();
                ReportDoubleProgress(progress, () => 1.0);
            }
            finally
            {
                FinishToEnumerate();
            }
        }

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

        void IZipReaderStream.LockZipStream() => LockZipStream();
        Task IZipReaderStream.LockZipStreamAsync(CancellationToken cancellationToken) => LockZipStreamAsync(cancellationToken);
        void IZipReaderStream.UnlockZipStream() => UnlockZipStream();

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

        internal static ZipArchiveFileReader Parse(FilePath zipArchiveFile, IZipInputStream zipInputStream, IZipEntryNameEncodingProvider entryNameEncodingProvider, ValidationStringency stringency)
        {
            if (entryNameEncodingProvider is null)
                throw new ArgumentNullException(nameof(entryNameEncodingProvider));
            if (zipArchiveFile is null)
                throw new ArgumentNullException(nameof(zipArchiveFile));
            if (zipInputStream is null)
                throw new ArgumentNullException(nameof(zipInputStream));

            var paramter = new ReaderParameter(entryNameEncodingProvider, zipArchiveFile);

            var unknownPayloads = new FragmentSet<ZipStreamPosition, UInt64>(zipInputStream.StartOfThisStream, zipInputStream.Length);
            var lastDiskHeader = ZipFileLastDiskHeader.Parse(zipInputStream, stringency);
            MarkAsKnownPayload(unknownPayloads, lastDiskHeader.EOCDR.HeaderPosition, lastDiskHeader.EOCDR.HeaderSize);
            if (lastDiskHeader.Zip64EOCDL is not null)
            {
                MarkAsKnownPayload(unknownPayloads, lastDiskHeader.Zip64EOCDL.HeaderPosition, lastDiskHeader.Zip64EOCDL.HeaderSize);
                var zip64EOCDR = ZipFileZip64EOCDR.Parse(paramter, zipInputStream, lastDiskHeader.Zip64EOCDL);
                MarkAsKnownPayload(unknownPayloads, zip64EOCDR.HeaderPosition, zip64EOCDR.HeaderSize);
                if (stringency > ValidationStringency.Normal)
                {
                    // ZIP64 EOCDR の末尾に ZIP64 EOCDL の先頭が隣接していることの確認

                    var unknownPayloadSize = lastDiskHeader.Zip64EOCDL.HeaderPosition - zip64EOCDR.HeaderPosition - zip64EOCDR.HeaderSize;
                    if (unknownPayloadSize > 0)
                        throw new BadZipFileFormatException($"Unknown payload exists between ZIP64 EOCDR and ZIP64 EOCDL.: position=\"{lastDiskHeader.Zip64EOCDL.HeaderPosition - unknownPayloadSize}\", size=0x{unknownPayloadSize:x16}");
                }

                ValidateEOCDR(lastDiskHeader.EOCDR, zip64EOCDR, stringency);
                var startOfCentralDirectoryHeaders =
                    zipInputStream.GetPosition(
                        zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory,
                        zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber)
                    ?? throw new BadZipFileFormatException($"The central directory header position read from ZIP64 EOCDR does not point to the correct disk position.: {nameof(zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory)}=0x{zip64EOCDR.NumberOfTheDiskWithTheStartOfTheCentralDirectory:x8}, {nameof(zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber)}=0x{zip64EOCDR.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber:x16}");
                MarkAsKnownPayload(unknownPayloads, startOfCentralDirectoryHeaders, zip64EOCDR.SizeOfTheCentralDirectory);
                return
                    new ZipArchiveFileReader(
                        entryNameEncodingProvider,
                        zipInputStream,
                        zipArchiveFile,
                        startOfCentralDirectoryHeaders,
                        zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory,
                        zip64EOCDR.NumberOfThisDisk,
                        zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk,
                        lastDiskHeader.EOCDR.CommentBytes,
                        zip64EOCDR.HeaderPosition,
                        unknownPayloads,
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
                MarkAsKnownPayload(unknownPayloads, centralDirectoryPosition, lastDiskHeader.EOCDR.SizeOfCentralDirectory);
                return
                    new ZipArchiveFileReader(
                        entryNameEncodingProvider,
                        zipInputStream,
                        zipArchiveFile,
                        centralDirectoryPosition,
                        lastDiskHeader.EOCDR.TotalNumberOfCentralDirectoryHeaders,
                        lastDiskHeader.EOCDR.NumberOfThisDisk,
                        lastDiskHeader.EOCDR.NumberOfCentralDirectoryHeadersOnThisDisk,
                        lastDiskHeader.EOCDR.CommentBytes,
                        lastDiskHeader.EOCDR.HeaderPosition,
                        unknownPayloads,
                        stringency);
            }
        }

        private void PrepareToEnumerate()
        {
            lock (this)
            {
                if (_isEnumerating)
                    throw new InvalidOperationException();
                _isEnumerating = true;
            }
        }

        private void FinishToEnumerate()
        {
            lock (this)
            {
                _isEnumerating = false;
            }
        }

        private ZipEntryLocalHeader ParseLocalHeader(ZipEntryCentralDirectoryHeader centralDirectoryHeader)
        {
            LockZipStream();
            try
            {
                return ZipEntryLocalHeader.Parse(_paramter, this, centralDirectoryHeader, _stringency);
            }
            finally
            {
                UnlockZipStream();
            }
        }

        private async Task<ZipEntryLocalHeader> ParseLocalHeaderAsync(ZipEntryCentralDirectoryHeader centralDirectoryHeader, CancellationToken cancellationToken)
        {
            await LockZipStreamAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await ZipEntryLocalHeader.ParseAsync(_paramter, this, centralDirectoryHeader, _stringency, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                UnlockZipStream();
            }
        }

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

        private void CheckUnknownPayloads()
        {
            if (_stringency > ValidationStringency.Normal)
            {
                // ローカルヘッダ間に未知のペイロードがないことの確認
                foreach (var fragment in _unknownPayloads.EnumerateFragments())
                {
                    // 自己解凍型 ZIP アーカイブや一部のマルチボリューム実装ではファイルの先頭からは ZIP フォーマットに無関係のデータが存在するため、
                    // ファイルの先頭から始まるペイロードはエラーとしない。
                    if (fragment.StartPosition != _zipInputStream.StartOfThisStream)
                        throw new BadImageFormatException($"Unknown payload exists between local headers.: position=\"{fragment.StartPosition}\", size=0x{fragment.Size:x16}");
                }
            }
        }

        private IEnumerable<ZipEntryCentralDirectoryHeader> EnumerateCentralDirectoryHeaders()
        {
            const UInt64 _MAXIMUM_CENTRAL_DIRECTORY_HEADER_CHUNK_SIZE = 64 * 1024UL;
            var centralDirectoryPosition = _startOfCentralDirectoryHeaders;
            var numberOfCentralDirectoryOnLastDisk = 0UL;
            var count = 0UL;
            while (count < _totalNumberOfCentralDirectoryHeaders)
            {
                var centralDirectoryHeaders = new List<ZipEntryCentralDirectoryHeader>();
                LockZipStream();
                try
                {
                    try
                    {
                        _zipInputStream.Seek(centralDirectoryPosition);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new BadZipFileFormatException($"Unable to read central directory header on ZIP archive.: {nameof(centralDirectoryPosition)}=\"{centralDirectoryPosition}\"", ex);
                    }

                    var chunkSize = 0UL;

                    while (count < _totalNumberOfCentralDirectoryHeaders && chunkSize <= _MAXIMUM_CENTRAL_DIRECTORY_HEADER_CHUNK_SIZE)
                    {
                        var centralDirectoryHeader = ZipEntryCentralDirectoryHeader.Parse(_paramter, this, _stringency);
                        centralDirectoryPosition += centralDirectoryHeader.HeaderSize;
                        centralDirectoryHeaders.Add(centralDirectoryHeader);
                        checked
                        {
                            chunkSize += centralDirectoryHeader.HeaderSize;
                            ++count;
                        }

                        if (centralDirectoryHeader.CentralDirectoryHeaderPosition.DiskNumber == _eocdrDiskNumber)
                        {
                            checked
                            {
                                ++numberOfCentralDirectoryOnLastDisk;
                            }
                        }
                    }
                }
                finally
                {
                    UnlockZipStream();
                }

                foreach (var centralDirectoryHeader in centralDirectoryHeaders)
                    yield return centralDirectoryHeader;
            }

            if (_stringency > ValidationStringency.Normal)
            {
                // 最後のディスクにあるセントラルディレクトリヘッダの数が一致していることの確認
                if (numberOfCentralDirectoryOnLastDisk != _numberOfCentralDirectoryHeadersOnTheSameDiskAsEOCDR)
                    throw new BadZipFileFormatException($"The number of central directory headers on the same volume as EOCDR is different. : expected number=0x{_numberOfCentralDirectoryHeadersOnTheSameDiskAsEOCDR:x16}, actual number=0x{_numberOfCentralDirectoryHeadersOnTheSameDiskAsEOCDR - numberOfCentralDirectoryOnLastDisk:x16}");

                // 最後のセントラルディレクトリヘッダと EOCDR (or ZIP64 EOCDR) が隣接していることの確認
                var unknownPayloadSize = EOCDRPosition - centralDirectoryPosition;
                if (unknownPayloadSize > 0)
                    throw new BadImageFormatException($"An unknown payload exists between the last central directory header and the EOCDR (or ZIP64 EOCDR).: position=\"{centralDirectoryPosition}\", size=0x{unknownPayloadSize:x16}");
            }
        }

        private async IAsyncEnumerable<ZipEntryCentralDirectoryHeader> EnumerateCentralDirectoryHeadersAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            const UInt64 _MAXIMUM_CENTRAL_DIRECTORY_HEADER_CHUNK_SIZE = 64 * 1024UL;
            var centralDirectoryPosition = _startOfCentralDirectoryHeaders;
            var numberOfCentralDirectoryOnLastDisk = 0UL;
            var count = 0UL;
            while (count < _totalNumberOfCentralDirectoryHeaders)
            {
                var centralDirectoryHeaders = new List<ZipEntryCentralDirectoryHeader>();
                await LockZipStreamAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    try
                    {
                        _zipInputStream.Seek(centralDirectoryPosition);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new BadZipFileFormatException($"Unable to read central directory header on ZIP archive.: {nameof(centralDirectoryPosition)}=\"{centralDirectoryPosition}\"", ex);
                    }

                    var chunkSize = 0UL;

                    while (count < _totalNumberOfCentralDirectoryHeaders && chunkSize <= _MAXIMUM_CENTRAL_DIRECTORY_HEADER_CHUNK_SIZE)
                    {
                        var centralDirectoryHeader = await ZipEntryCentralDirectoryHeader.ParseAsync(_paramter, this, _stringency, cancellationToken).ConfigureAwait(false);
                        centralDirectoryPosition += centralDirectoryHeader.HeaderSize;
                        centralDirectoryHeaders.Add(centralDirectoryHeader);
                        checked
                        {
                            chunkSize += centralDirectoryHeader.HeaderSize;
                            ++count;
                        }

                        if (centralDirectoryHeader.CentralDirectoryHeaderPosition.DiskNumber == _eocdrDiskNumber)
                        {
                            checked
                            {
                                ++numberOfCentralDirectoryOnLastDisk;
                            }
                        }
                    }
                }
                finally
                {
                    UnlockZipStream();
                }

                foreach (var centralDirectoryHeader in centralDirectoryHeaders)
                    yield return centralDirectoryHeader;
            }

            if (_stringency > ValidationStringency.Normal)
            {
                // 最後のディスクにあるセントラルディレクトリヘッダの数が一致していることの確認
                if (numberOfCentralDirectoryOnLastDisk != _numberOfCentralDirectoryHeadersOnTheSameDiskAsEOCDR)
                    throw new BadZipFileFormatException($"The number of central directory headers on the same volume as EOCDR is different. : expected number=0x{_numberOfCentralDirectoryHeadersOnTheSameDiskAsEOCDR:x16}, actual number=0x{_numberOfCentralDirectoryHeadersOnTheSameDiskAsEOCDR - numberOfCentralDirectoryOnLastDisk:x16}");

                // 最後のセントラルディレクトリヘッダと EOCDR (or ZIP64 EOCDR) が隣接していることの確認
                var unknownPayloadSize = EOCDRPosition - centralDirectoryPosition;
                if (unknownPayloadSize > 0)
                    throw new BadImageFormatException($"An unknown payload exists between the last central directory header and the EOCDR (or ZIP64 EOCDR).: position=\"{centralDirectoryPosition}\", size=0x{unknownPayloadSize:x16}");
            }
        }

        private static void ValidateEOCDR(ZipFileEOCDR eocdr, ZipFileZip64EOCDR zip64EOCDR, ValidationStringency stringency)
        {
            if (stringency > ValidationStringency.Normal)
            {
                // ZIP64 EOCDR と EOCDR の整合性を確認する。

                if (zip64EOCDR.NumberOfThisDisk == eocdr.HeaderPosition.DiskNumber)
                {
                    // ZIP64 EOCDR が EOCDR と同じボリュームディスクにある場合

                    if (eocdr.NumberOfCentralDirectoryHeadersOnThisDisk != UInt16.MaxValue)
                    {
                        // EOCDR の フィールド NumberOfCentralDirectoryHeadersOnThisDisk が UInt16.MaxValue ではない場合
                        // => ZIP64 EOCDR の NumberOfCentralDirectoryHeadersOnThisDisk も同じ値であるはず。

                        if (eocdr.NumberOfCentralDirectoryHeadersOnThisDisk != zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk)
                            throw new BadZipFileFormatException($"Since ZIP64 EOCDR and EOCDR are on the same volume disk, and the value of field {nameof(eocdr.NumberOfCentralDirectoryHeadersOnThisDisk)} in EOCDR is not 0x{UInt16.MaxValue:x4}, the value of field {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk)} in ZIP64 EOCDR should be equal to the value of field {nameof(eocdr.NumberOfCentralDirectoryHeadersOnThisDisk)} in EOCDR. However, the value of field {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk)} of ZIP64 EOCDR is actually different from the value of field {nameof(eocdr.NumberOfCentralDirectoryHeadersOnThisDisk)} of EOCDR.: {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk)}=0x{zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk:x16}, {nameof(eocdr.NumberOfCentralDirectoryHeadersOnThisDisk)}=0x{eocdr.NumberOfCentralDirectoryHeadersOnThisDisk:x4}");
                    }
                    else
                    {
                        // EOCDR の フィールド NumberOfCentralDirectoryHeadersOnThisDisk が UInt16.MaxValue である場合
                        // => ZIP64 EOCDR の TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk は UInt16.MaxValue 以上であるはず。

                        if (zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk < UInt16.MaxValue)
                            throw new BadZipFileFormatException($"Since ZIP64 EOCDR and EOCDR are on the same volume disk, and EOCDR's field {nameof(eocdr.NumberOfCentralDirectoryHeadersOnThisDisk)} has a value of 0x{UInt16.MaxValue:x4}, ZIP64 EOCDR's field {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk)} should have a value greater than or equal to 0x{UInt16.MaxValue:x16}. But actually the value of field {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk)} in ZIP64 EOCDR is less than 0x{UInt16.MaxValue:x16}.: {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk)}=0x{zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectoryOnThisDisk:x16}");
                    }
                }
                else
                {
                    // ZIP64 EOCDR が EOCDR と異なるボリュームディスクにある場合
                    // => 少なくとも、最後のボリュームディスク (つまり EOCDR があるボリュームディスク) にはセントラルディレクトリヘッダは存在しないはず。

                    if (eocdr.NumberOfCentralDirectoryHeadersOnThisDisk != 0)
                        throw new BadZipFileFormatException($"Since ZIP64 ECDR and ECDR are on different volume disks, the volume disk where ECDR is located (that is, the last volume disk) should not have a central directory header. However, the value of the ECDR field {nameof(eocdr.NumberOfCentralDirectoryHeadersOnThisDisk)} is actually not 0. : {nameof(eocdr.NumberOfCentralDirectoryHeadersOnThisDisk)}={eocdr.NumberOfCentralDirectoryHeadersOnThisDisk}");
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

                if (eocdr.TotalNumberOfCentralDirectoryHeaders == UInt16.MaxValue)
                {
                    if (zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory < UInt16.MaxValue)
                        throw new BadZipFileFormatException($"The value of field {nameof(eocdr.TotalNumberOfCentralDirectoryHeaders)} in EOCDR is 0x{UInt16.MaxValue:x4}, so the value of field {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory)} in ZIP64 EOCDR must be greater than or equal to 0x{UInt16.MaxValue:x16}. But actually the value of field {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory)} of ZIP64 EOCDR is less than 0x{UInt16.MaxValue:x16}. : {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory)}=0x{zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory:x16}");
                }
                else
                {
                    if (eocdr.TotalNumberOfCentralDirectoryHeaders != zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory)
                        throw new BadZipFileFormatException($"The value of field {nameof(eocdr.TotalNumberOfCentralDirectoryHeaders)} in EOCDR is not 0x{UInt16.MaxValue:x4}, so the value of field {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory)} in ZIP64 EOCDR must be equal to the value of field {nameof(eocdr.TotalNumberOfCentralDirectoryHeaders)} in EOCDR. But actually, the value of field {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory)} in ZIP64 EOCDR is different from the value of field {nameof(eocdr.TotalNumberOfCentralDirectoryHeaders)} in EOCDR.: {nameof(zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory)}=0x{zip64EOCDR.TotalNumberOfEntriesInTheCentralDirectory:x16}, {nameof(eocdr.TotalNumberOfCentralDirectoryHeaders)}=0x{eocdr.TotalNumberOfCentralDirectoryHeaders:x4}");
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

        private void MarkAsKnownPayload(ZipStreamPosition position, UInt64 size) => MarkAsKnownPayload(_unknownPayloads, position, size);

        private static void MarkAsKnownPayload(FragmentSet<ZipStreamPosition, UInt64> unknownPayloads, ZipStreamPosition position, UInt64 size)
        {
            if (!unknownPayloads.IsEmpty)
            {
                try
                {
                    unknownPayloads.RemoveFragment(new FragmentSetElement<ZipStreamPosition, UInt64>(position, size));
                }
                catch (Exception ex)
                {
                    throw new InternalLogicalErrorException("Unknown payload location update failed.", ex);
                }
            }
        }

        private void LockZipStream() => _zipStreamLockObject.Wait();
        private Task LockZipStreamAsync(CancellationToken cancellationToken) => _zipStreamLockObject.WaitAsync(cancellationToken);
        private void UnlockZipStream() => _ = _zipStreamLockObject.Release();
    }
}
