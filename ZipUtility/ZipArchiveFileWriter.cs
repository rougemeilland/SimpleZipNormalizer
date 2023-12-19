using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.IO;
using Utility.Text;
using ZipUtility.Headers.Builder;

namespace ZipUtility
{
    /// <summary>
    /// 新規に ZIP アーカイブを作成するクラスです。
    /// </summary>
    public class ZipArchiveFileWriter
        : IDisposable, IAsyncDisposable, IZipFileWriterParameter, IZipFileWriterOutputStreamAccesser
    {
        private enum WriterState
        {
            Initial = 0,
            EntryCreated,
            WritingContent,
            Completed,
            Error,
        }

        private const Byte _zipWriterVersion = 63;

        private static readonly Encoding _standardUnicodeEncoding;

        private readonly IZipOutputStream _zipOutputStream;
        private readonly IZipEntryNameEncodingProvider _entryNameEncodingProvider;
        private readonly FilePath _zipArchiveFile;
        private readonly FilePath _temporaryFileForCentoralDirectories;
        private readonly ISequentialOutputByteStream _outStreamForCentoralDirectories;
        private Boolean _isDisposed;
        private Boolean _isLocked;
        private UInt64 _currentEntryCount;
        private WriterState _writerState;
        private ReadOnlyMemory<Byte> _commentBytes;
        private ZipDestinationEntry? _lastEntry;

        static ZipArchiveFileWriter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _standardUnicodeEncoding = Encoding.UTF8.WithFallback(null, null).WithoutPreamble();
        }

        internal ZipArchiveFileWriter(IZipOutputStream zipStream, IZipEntryNameEncodingProvider entryNameEncodingProvider, FilePath zipArchiveFile)
        {
            _zipOutputStream = zipStream ?? throw new ArgumentNullException(nameof(zipStream));
            _entryNameEncodingProvider = entryNameEncodingProvider;
            _zipArchiveFile = zipArchiveFile;
            _temporaryFileForCentoralDirectories = new FilePath(Path.GetTempFileName());
            _outStreamForCentoralDirectories = _temporaryFileForCentoralDirectories.Create().WithCache();
            _isDisposed = false;
            _isLocked = false;
            _currentEntryCount = 0;
            _writerState = WriterState.Initial;
            _lastEntry = null;
            CommentBytes = ReadOnlyMemory<Byte>.Empty;
        }

        /// <summary>
        /// ZIP アーカイブのコメントのバイト列を取得または設定します。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>コメントのバイト列の長さの上限は 65535 バイトです。</item>
        /// <item>
        /// <para>
        /// コメントのバイト列には、ZIP アーカイブ の仕様上の脆弱性を避けるため、一部の値を含むことが出来ません。
        /// </para>
        /// <para>
        /// それらの値は大抵のエンコーディング (例: ASCII および、UTF-8 や SHIFT-JIS などの ASCII の上位互換である他のエンコーディング) では制御文字に該当します。
        /// </para>
        /// <para>
        /// それらの制御文字は一般的なテキストには使用されないため、通常の用途では問題ありません。
        /// </para>
        /// </item>
        /// <item>
        /// <para>
        /// コメントのバイト列のエンコーディングは規定されていないので、ZIP アーカイブを読み取るソフトウェアがコメントのデコード方法を知る手段がないことに注意してください。
        /// </para>
        /// <para>
        /// 可能な限り、よく使われているエンコーディングを使用することを推奨します。(例: ASCII または UTF-8 など)
        /// </para>
        /// </item>
        /// </list>
        /// </remarks>
        public ReadOnlyMemory<Byte> CommentBytes
        {
            get => _commentBytes;

            set
            {
                if (_commentBytes.Length > UInt16.MaxValue)
                    throw new ArgumentException($"{nameof(value)} of {nameof(CommentBytes)} is too long. {nameof(value)} of {nameof(CommentBytes)} length must be at most 65535 bytes.");
                if (_commentBytes.Span.IndexOfAny((Byte)0x00, (Byte)0x05, (Byte)0x06) >= 0)
                    throw new ArgumentException($"{nameof(value)} of {nameof(CommentBytes)} contains inappropriate characters.");

                _commentBytes = value;
            }
        }

        /// <summary>
        /// ZIP アーカイブの作成方法を制御するフラグを示す値を取得または設定します。
        /// </summary>
        public ZipWriteFlags Flags { get; set; }

        /// <summary>
        /// サポートされている圧縮方式のIDのコレクションを取得します。
        /// </summary>
        public static IEnumerable<ZipEntryCompressionMethodId> SupportedCompressionIds
            => ZipEntryCompressionMethod.SupportedCompresssionMethodIds;

        /// <summary>
        /// ZIP アーカイブに書き込む新たなエントリを追加します。
        /// </summary>
        /// <param name="entryFullName">
        /// エントリのフルパス名を示す文字列です。
        /// </param>
        /// <param name="entryComment">
        /// エントリのコメントを示す文字列です。省略時は空文字列として扱われます。
        /// </param>
        /// <returns>
        /// 追加されたエントリの <see cref="ZipDestinationEntry"/> オブジェクトです。
        /// </returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>エントリに各種属性を設定したりデータを書き込んだりする場合は、このメソッドにより返る <see cref="ZipDestinationEntry"/> オブジェクトを使用してください。</item>
        /// <item>エントリ名に空文字を指定することはできません。</item>
        /// <item>エントリ名およびコメントは UTF-8 でエンコードされます。</item>
        /// <item>エントリ名およびコメントの長さの上限は、それぞれ、エンコードされたバイト列の長さが 65535 バイトになるまでです。</item>
        /// </list>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        /// <see cref="ZipArchiveFileWriter"/> オブジェクトが既に破棄されています。
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="entryFullName"/> または <paramref name="entryComment"/> が長すぎます。
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entryFullName"/> または <paramref name="entryComment"/> が null です。
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// 既に <see cref="Close"/> が呼び出されているため、新たなエントリの追加はできません。
        /// </exception>
        public ZipDestinationEntry CreateEntry(String entryFullName, String entryComment = "")
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (String.IsNullOrEmpty(entryFullName))
                throw new ArgumentException($"{nameof(entryFullName)} must not be either null or an empty string.");
            if (entryComment is null)
                throw new ArgumentNullException(nameof(entryComment));
            var entryFullNameBytes = _standardUnicodeEncoding.GetReadOnlyBytes(entryFullName);
            if (entryFullNameBytes.Length > UInt16.MaxValue)
                throw new ArgumentException($"{nameof(entryFullName)} is too long. {nameof(entryFullName)} must be less than or equal to {UInt16.MaxValue} bytes.");
            var entryCommentBytes = _standardUnicodeEncoding.GetReadOnlyBytes(entryComment);
            if (entryCommentBytes.Length > UInt16.MaxValue)
                throw new ArgumentException($"{nameof(entryComment)} is too long. {nameof(entryComment)} must be less than or equal to {UInt16.MaxValue} bytes.");

            return
                InternalCreateEntry(
                    entryFullName,
                    entryFullNameBytes,
                    entryComment,
                    entryCommentBytes,
                    _standardUnicodeEncoding,
                    new[] { _standardUnicodeEncoding });
        }

        /// <summary>
        /// エントリ名およびコメントのバイト列およびエンコード方式を明示的に指定することにより、ZIP アーカイブに書き込む新たなエントリを追加します。
        /// </summary>
        /// <param name="entryFullName">
        /// エントリのフルパス名を示す文字列です。
        /// </param>
        /// <param name="entryFullNameBytes">
        /// エントリのフルパス名を示すバイト列です。
        /// </param>
        /// <param name="entryComment">
        /// エントリのコメントを示す文字列です。
        /// </param>
        /// <param name="entryCommentBytes">
        /// エントリのコメントを示すバイト列です。
        /// </param>
        /// <param name="exactEntryEncoding">
        /// エントリ名およびコメントのエンコーディング方式として明確に判明しているエンコーディングがあればその <see cref="Encoding"/> オブジェクトです。それ以外は null です。
        /// </param>
        /// <param name="possibleEntryEncodings">
        /// エントリ名およびコメントのエンコーディング方式として明確にではないが候補として挙げることが可能なエンコーディング方式の <see cref="Encoding"/> オブジェクトのコレクションです。候補として挙げることのできるエンコーディング方式が存在しない場合は空のコレクションです。
        /// </param>
        /// <returns>
        /// 追加されたエントリの <see cref="ZipDestinationEntry"/> オブジェクトです。
        /// </returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>通常は、以下の場合を除き <see cref="CreateEntry(String, String)"/> のオーバーロードを使用してください。
        /// <list type="bullet">
        /// <item>エントリ名およびコメントの文字列とバイト列が別々の手段で判明しているが、それらが一致する確証が持てない場合</item>
        /// </list>
        /// このような状況は、例えば以下のような条件で発生し得ます。
        /// <list type="number">
        /// <item>読み込んだ ZIP アーカイブのエントリに UTF-8 エンコーディングであることを示すフラグが立っておらず、かつ</item>
        /// <item>同じエントリの拡張フィールドにて明示的に UNICODE であると示されているバイト列が取得可能な場合。</item>
        /// </list>
        /// 通常はこれらのバイト列は同じ文字列を意味しているのですが、生のバイト列のエンコーディングによっては UNICODE にマップできない文字が含まれている可能性があります。
        /// そのような場合、このオーバーロードでは <paramref name="entryFullNameBytes"/> で与えられたバイト列をエントリ名として設定し、<paramref name="entryFullName"/> で与えられた文字列を拡張フィールドに設定します。
        /// </item>
        /// <item>エントリに各種属性を設定したりデータを書き込んだりする場合は、このメソッドにより返る <see cref="ZipDestinationEntry"/> オブジェクトを使用してください。</item>
        /// <item>エントリ名およびコメントの長さの上限は、それぞれ、エンコードされたバイト列の長さが 65535 バイトになるまでです。</item>
        /// </list>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">
        /// <see cref="ZipArchiveFileWriter"/> オブジェクトが既に破棄されています。
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entryFullName"/> または <paramref name="entryFullName"/>、<paramref name="possibleEntryEncodings"/> の何れかに null が与えられました。
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// 既に <see cref="Close"/> が呼び出されているため、新たなエントリの追加はできません。
        /// </exception>
        /// <exception cref="ArgumentException">
        /// 以下の何れかの原因で発生します。
        /// <list type="bullet">
        /// <item><paramref name="entryFullName"/> が空文字列である。</item>
        /// <item><paramref name="entryFullNameBytes"/> の長さが 0 であるかまたは 65535 バイトを超えている。</item>
        /// <item><paramref name="entryCommentBytes"/> の長さが 65535 バイトを超えている。</item>
        /// </list>
        /// </exception>
        public ZipDestinationEntry CreateEntry(
            String entryFullName,
            ReadOnlyMemory<Byte> entryFullNameBytes,
            String entryComment,
            ReadOnlyMemory<Byte> entryCommentBytes,
            Encoding? exactEntryEncoding,
            IEnumerable<Encoding> possibleEntryEncodings)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (String.IsNullOrEmpty(entryFullName))
                throw new ArgumentException($"'{nameof(entryFullName)}' must not be null or empty string.", nameof(entryFullName));
            if (entryFullNameBytes.Length <= 0)
                throw new ArgumentException($"'{nameof(entryFullNameBytes)}' must not be empty.", nameof(entryFullNameBytes));
            if (entryFullNameBytes.Length > UInt16.MaxValue)
                throw new ArgumentException($"{nameof(entryFullNameBytes)} is too long. {nameof(entryFullNameBytes)} must be less than or equal to {UInt16.MaxValue} bytes.");
            if (entryComment is null)
                throw new ArgumentNullException(nameof(entryComment));
            if (entryCommentBytes.Length > UInt16.MaxValue)
                throw new ArgumentException($"{nameof(entryCommentBytes)} is too long. {nameof(entryCommentBytes)} must be less than or equal to {UInt16.MaxValue} bytes.");
            if (possibleEntryEncodings is null)
                throw new ArgumentNullException(nameof(possibleEntryEncodings));
            if (_writerState is not WriterState.Initial and not WriterState.EntryCreated)
                throw new InvalidOperationException();

            return
                InternalCreateEntry(
                    entryFullName,
                    entryFullNameBytes,
                    entryComment,
                    entryCommentBytes,
                    exactEntryEncoding,
                    possibleEntryEncodings);
        }

        /// <summary>
        /// ZIP アーカイブの書き込みを明示的に終了します。
        /// </summary>
        public void Close()
        {
            if (!_isDisposed)
            {
                InternalFlush();
                Dispose();
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
        public override String ToString() => $"\"{_zipArchiveFile.FullName}\"";

        FilePath IZipFileWriterParameter.ZipArchiveFile => new(_zipArchiveFile.FullName);
        IZipEntryNameEncodingProvider IZipFileWriterParameter.EntryNameEncodingProvider => _entryNameEncodingProvider;
        Byte IZipFileWriterParameter.ThisSoftwareVersion => _zipWriterVersion;

        ZipEntryHostSystem IZipFileWriterParameter.HostSystem
            => OperatingSystem.IsWindows()
                ? ZipEntryHostSystem.FAT
                : OperatingSystem.IsLinux()
                ? ZipEntryHostSystem.UNIX
                : OperatingSystem.IsMacOS()
                ? ZipEntryHostSystem.Macintosh
                : ZipEntryHostSystem.FAT; // 未知の OS の場合には MS-DOS とみなす

        UInt16 IZipFileWriterParameter.GetVersionNeededToExtract(ZipEntryCompressionMethodId compressionMethodId, Boolean? supportDirectory, Boolean? requiredZip64)
        {
            var versionNeededTiExtract =
                new[]
                {
                    (UInt16)10, // minimum version (supported Stored compression)
                    supportDirectory is not null && supportDirectory.Value == true  ? (UInt16)20 : (UInt16)0, // version if it contains directory entries
                    compressionMethodId == ZipEntryCompressionMethodId.Deflate ? (UInt16)20 : (UInt16)0, // version if using Deflate compression
                    compressionMethodId == ZipEntryCompressionMethodId.Deflate64 ? (UInt16)21 : (UInt16)0, // version if using Deflate64 compression
                    compressionMethodId == ZipEntryCompressionMethodId.BZIP2 ? (UInt16)46 : (UInt16)0, // version if using BZIP2 compression
                    compressionMethodId == ZipEntryCompressionMethodId.LZMA ? (UInt16)63 : (UInt16)0, // version if using LZMA compression
                    compressionMethodId == ZipEntryCompressionMethodId.PPMd ? (UInt16)63 : (UInt16)0, // version if using PPMd+ compression
                    requiredZip64 is not null && requiredZip64.Value ? (UInt16)45 : (UInt16)0, // version if using zip 64 extensions
                }
                .Max();
            return versionNeededTiExtract;
        }

        IZipOutputStream IZipFileWriterOutputStreamAccesser.MainStream => _zipOutputStream;
        ISequentialOutputByteStream IZipFileWriterOutputStreamAccesser.StreamForCentralDirectoryHeaders => _outStreamForCentoralDirectories;

        void IZipFileWriterOutputStreamAccesser.BeginToWriteContent()
        {
            if (_writerState != WriterState.EntryCreated)
                throw new InternalLogicalErrorException();

            _writerState = WriterState.WritingContent;
        }

        void IZipFileWriterOutputStreamAccesser.EndToWritingContent()
        {
            if (_writerState != WriterState.WritingContent)
                throw new InternalLogicalErrorException();

            _writerState = WriterState.Initial;
        }

        void IZipFileWriterOutputStreamAccesser.SetErrorMark() => _writerState = WriterState.Error;
        void IZipFileWriterOutputStreamAccesser.LockZipStream() => LockZipStream();
        void IZipFileWriterOutputStreamAccesser.UnlockZipStream() => UnlockZipStream();

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
                try
                {
                    InternalFlush();
                }
                catch (Exception)
                {
                }

                if (disposing)
                {
                    _outStreamForCentoralDirectories.Dispose();
                    _zipOutputStream.Dispose();
                }

                if (_temporaryFileForCentoralDirectories.Exists)
                    _temporaryFileForCentoralDirectories.Delete();
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
                try
                {
                    await Task.Run(InternalFlush).ConfigureAwait(false);
                }
                catch (Exception)
                {
                }

                await _outStreamForCentoralDirectories.DisposeAsync().ConfigureAwait(false);
                await _zipOutputStream.DisposeAsync().ConfigureAwait(false);
                if (_temporaryFileForCentoralDirectories.Exists)
                    _temporaryFileForCentoralDirectories.Delete();
                _isDisposed = true;
            }
        }

        private ZipDestinationEntry InternalCreateEntry(String entryFullName, ReadOnlyMemory<Byte> entryFullNameBytes, String entryComment, ReadOnlyMemory<Byte> entryCommentBytes, Encoding? exactEntryEncoding, IEnumerable<Encoding> possibleEntryEncodings)
        {
            FlushLatestEntry();

            _writerState = WriterState.EntryCreated;
            var newEntry =
                new ZipDestinationEntry(
                    this,
                    this,
                    _currentEntryCount,
                    entryFullName,
                    entryFullNameBytes,
                    entryComment,
                    entryCommentBytes,
                    exactEntryEncoding,
                    possibleEntryEncodings);
            checked
            {
                ++_currentEntryCount;
            }

            _lastEntry = newEntry;
            return newEntry;
        }

        private void InternalFlush()
        {
            FlushLatestEntry();

            LockZipStream();
            var success = false;
            try
            {
                if (_writerState is not WriterState.Initial and not WriterState.EntryCreated)
                    throw new InvalidOperationException();

                // セントラルディレクトリヘッダの書き込み
                _outStreamForCentoralDirectories.Dispose();
                var currentDiskNumber = 0U;
                var numberOfCentralDirectoryHeadersOnCurrenetDisk = 0U;
                var startOfCentralDirectories = (ZipStreamPosition?)null;
                try
                {
                    using var inStream = _temporaryFileForCentoralDirectories.OpenRead().WithCache();
                    for (var count = 0UL; count < _currentEntryCount; ++count)
                    {
                        var length = inStream.ReadUInt32LE();
                        var headerBytes = inStream.ReadBytes(length);
                        if (checked((UInt32)headerBytes.Length) != length)
                            throw new UnexpectedEndOfStreamException();
                        var position = WriteChunkToZipStream(_zipOutputStream, headerBytes);
                        if (position.DiskNumber == currentDiskNumber)
                        {
                            checked
                            {
                                ++numberOfCentralDirectoryHeadersOnCurrenetDisk;
                            }
                        }
                        else
                        {
                            currentDiskNumber = position.DiskNumber;
                            numberOfCentralDirectoryHeadersOnCurrenetDisk = 1;
                        }

                        startOfCentralDirectories ??= position;
                    }
                }
                catch (Exception ex)
                {
                    _writerState = WriterState.Error;
                    throw new InternalLogicalErrorException("Failed to write to central directories.", ex);
                }

                var endOfCentralDirectories = _zipOutputStream.Position;

                // ZIP64 EOCDR, ZIP64 EOCDL, EOCDL の書き込み
                var lastHeaders =
                    ZipFileLastDiskHeader.Build(
                        this,
                        startOfCentralDirectories ?? endOfCentralDirectories,
                        endOfCentralDirectories,
                        _currentEntryCount,
                        currentDiskNumber,
                        numberOfCentralDirectoryHeadersOnCurrenetDisk,
                        _commentBytes,
                        (Flags & ZipWriteFlags.AlwaysWriteZip64EOCDR) != ZipWriteFlags.None);
                lastHeaders.WriteTo(_zipOutputStream);

                // ZIP アーカイブの出力が完了したことの宣言
                _zipOutputStream.CompletedSuccessfully();

                _writerState = WriterState.Completed;
                success = true;
            }
            finally
            {
                if (!success)
                    _writerState = WriterState.Error;
                UnlockZipStream();
            }
        }

        private void FlushLatestEntry()
        {
            // 前回のまだ書き込まれていないかもしれないコンテンツの書き込み
            // データがないエントリ、特にディレクトリエントリの場合に該当する
            if (_lastEntry is not null)
            {
                _lastEntry.Flush();
                _lastEntry = null;
            }
        }

        private void LockZipStream()
        {
            lock (this)
            {
                if (_isLocked)
                    throw new InvalidOperationException("An attempt was made to create the next entry before finishing writing the previous entry.");
                _isLocked = true;
            }
        }

        private void UnlockZipStream()
        {
            lock (this)
            {
                _isLocked = false;
            }
        }

        private static ZipStreamPosition WriteChunkToZipStream(IZipOutputStream outputStream, ReadOnlyMemory<Byte> chunk)
        {
            // 不可分書き込みを宣言する。
            // ※このとき書き込み対象のボリュームディスクが変化する可能性があることに注意。
            outputStream.ReserveAtomicSpace(checked((UInt64)chunk.Length));

            // 不可分書き込みのために出力先ボリュームをロックする。
            outputStream.LockVolumeDisk();
            try
            {
                // 先頭位置を保存する
                var position = outputStream.Position;

                // データを書き込む
                outputStream.WriteBytes(chunk);

                // 先頭位置を返す。
                return position;
            }
            finally
            {
                // 出力先ボリュームのロックを解除する。
                outputStream.UnlockVolumeDisk();
            }
        }
    }
}
