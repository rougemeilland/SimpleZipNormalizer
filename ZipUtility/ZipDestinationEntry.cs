using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Utility;
using Utility.IO;
using Utility.Linq;
using Utility.Text;
using ZipUtility.ZipExtraField;

namespace ZipUtility
{
    /// <summary>
    /// 出力先 ZIP ファイルのエントリのクラスです。
    /// </summary>
    public class ZipDestinationEntry
    {
        private class PassThroughOutputStream
            : IBasicOutputByteStream, IReportableOnStreamClosed<UInt64>
        {
            private readonly IBasicOutputByteStream _baseStream1;
            private readonly IBasicOutputByteStream? _baseStream2;
            private Boolean _isDisposed;
            private UInt64 _writtenTotalCount;

            public event EventHandler<OnStreamClosedEventArgs<UInt64>>? OnStreamClosed;

            public PassThroughOutputStream(IBasicOutputByteStream baseStream1, IBasicOutputByteStream? baseStream2 = null)
            {
                _baseStream1 = baseStream1;
                _baseStream2 = baseStream2;
                _isDisposed = false;
                _writtenTotalCount = 0;
            }

            public Int32 Write(ReadOnlySpan<Byte> buffer)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                _baseStream1.WriteBytes(buffer);
                _baseStream2?.WriteBytes(buffer);
                checked
                {
                    _writtenTotalCount += (UInt64)buffer.Length;
                }

                return buffer.Length;
            }

            public async Task<Int32> WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                await _baseStream1.WriteBytesAsync(buffer, cancellationToken).ConfigureAwait(false);
                if (_baseStream2 is not null)
                    await _baseStream2.WriteBytesAsync(buffer, cancellationToken).ConfigureAwait(false);
                checked
                {
                    _writtenTotalCount += (UInt64)buffer.Length;
                }

                return buffer.Length;
            }

            public void Flush()
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                _baseStream1.Flush();
                _baseStream2?.Flush();
            }

            public async Task FlushAsync(CancellationToken cancellationToken = default)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                await _baseStream1.FlushAsync(cancellationToken).ConfigureAwait(false);
                if (_baseStream2 is not null)
                    await _baseStream2.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            public async ValueTask DisposeAsync()
            {
                await DisposeAsyncCore().ConfigureAwait(false);
                Dispose(disposing: false);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(Boolean disposing)
            {
                if (!_isDisposed)
                {
                    try
                    {
                        if (disposing)
                        {
                            _baseStream1.Dispose();
                            _baseStream2?.Dispose();
                        }

                        _isDisposed = true;
                    }
                    finally
                    {
                        try
                        {
                            OnStreamClosed?.Invoke(this, new OnStreamClosedEventArgs<UInt64>(_writtenTotalCount));
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }

            protected virtual async ValueTask DisposeAsyncCore()
            {
                if (!_isDisposed)
                {
                    try
                    {
                        await _baseStream1.DisposeAsync().ConfigureAwait(false);
                        if (_baseStream2 is not null)
                            await _baseStream2.DisposeAsync().ConfigureAwait(false);
                        _isDisposed = true;
                    }
                    finally
                    {
                        try
                        {
                            OnStreamClosed?.Invoke(this, new OnStreamClosedEventArgs<UInt64>(_writtenTotalCount));
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        private class ContentHeaderInfo
        {
            private ContentHeaderInfo(
                ZipStreamPosition localHeaderPosition,
                UInt16 versionMadeBy,
                UInt16 versionNeededToExtractForLocalHeader,
                UInt16 versionNeededToExtractForCentralDirectoryHeader,
                ZipEntryGeneralPurposeBitFlag generalPurposeBitFlag,
                ZipEntryCompressionMethodId compressionMethodId,
                UInt16 dosDate,
                UInt16 dosTime,
                UInt32 crc,
                UInt32 rawSizeForLocalHeader,
                UInt32 rawSizeForCentralDirectoryHeader,
                UInt32 rawPackedSizeForLocalHeader,
                UInt32 rawPackedSizeForCentralDirectoryHeader,
                ReadOnlyMemory<Byte> entryFullNameBytes,
                ReadOnlyMemory<Byte> entryCommentBytes,
                ExtraFieldStorage localHeaderExtraFields,
                ExtraFieldStorage centralDirectoryHeaderExtraFields,
                UInt16 rawDiskNumberStartForCentralDirectoryHeader,
                UInt32 rawRelativeOffsetOfLocalHeaderForCentralDirectoryHeader,
                UInt32 externalFileAttributes)
            {
                if (entryFullNameBytes.Length > UInt16.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(entryFullNameBytes));
                if (entryCommentBytes.Length > UInt16.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(entryCommentBytes));

                LocalHeaderPosition = localHeaderPosition;
                VersionMadeBy = versionMadeBy;
                VersionNeededToExtractForLocalHeader = versionNeededToExtractForLocalHeader;
                VersionNeededToExtractForCentralDirectoryHeader = versionNeededToExtractForCentralDirectoryHeader;
                GeneralPurposeBitFlag = generalPurposeBitFlag;
                CompressionMethodId = compressionMethodId;
                DosDate = dosDate;
                DosTime = dosTime;
                Crc = crc;
                RawSizeForLocalHeader = rawSizeForLocalHeader;
                RawSizeForCentralDirectoryHeader = rawSizeForCentralDirectoryHeader;
                RawPackedSizeForLocalHeader = rawPackedSizeForLocalHeader;
                RawPackedSizeForCentralDirectoryHeader = rawPackedSizeForCentralDirectoryHeader;
                EntryFullNameBytes = entryFullNameBytes;
                EntryCommentBytes = entryCommentBytes;
                LocalHeaderExtraFields = localHeaderExtraFields ?? throw new ArgumentNullException(nameof(localHeaderExtraFields));
                CentralDirectoryHeaderExtraFields = centralDirectoryHeaderExtraFields ?? throw new ArgumentNullException(nameof(centralDirectoryHeaderExtraFields));
                RawDiskNumberStartForCentralDirectoryHeader = rawDiskNumberStartForCentralDirectoryHeader;
                ExternalFileAttributes = externalFileAttributes;
                RawRelativeOffsetOfLocalHeaderForCentralDirectoryHeader = rawRelativeOffsetOfLocalHeaderForCentralDirectoryHeader;
            }

            public ZipStreamPosition LocalHeaderPosition { get; }
            public UInt16 VersionMadeBy { get; }
            public UInt16 VersionNeededToExtractForLocalHeader { get; }
            public UInt16 VersionNeededToExtractForCentralDirectoryHeader { get; }
            public ZipEntryGeneralPurposeBitFlag GeneralPurposeBitFlag { get; }
            public ZipEntryCompressionMethodId CompressionMethodId { get; }
            public UInt16 DosDate { get; }
            public UInt16 DosTime { get; }
            public UInt32 Crc { get; }
            public UInt32 RawSizeForLocalHeader { get; }
            public UInt32 RawSizeForCentralDirectoryHeader { get; }
            public UInt32 RawPackedSizeForLocalHeader { get; }
            public UInt32 RawPackedSizeForCentralDirectoryHeader { get; }
            public ReadOnlyMemory<Byte> EntryFullNameBytes { get; }
            public ReadOnlyMemory<Byte> EntryCommentBytes { get; }
            public ExtraFieldStorage LocalHeaderExtraFields { get; }
            public ExtraFieldStorage CentralDirectoryHeaderExtraFields { get; }
            public UInt16 RawDiskNumberStartForCentralDirectoryHeader { get; }
            public UInt32 ExternalFileAttributes { get; }
            public UInt32 RawRelativeOffsetOfLocalHeaderForCentralDirectoryHeader { get; }

            public IEnumerable<ReadOnlyMemory<Byte>> ToLocalHeaderBytes()
            {
                var extraFieldsBytes = LocalHeaderExtraFields.ToByteArray();
                var headerBytes = new Byte[_localHeaderSizeOfFixedPart].AsMemory();
                headerBytes[..4].SetValueLE(_localHeaderSignature);
                headerBytes.Slice(4, 2).SetValueLE(VersionNeededToExtractForLocalHeader);
                headerBytes.Slice(6, 2).SetValueLE((UInt16)GeneralPurposeBitFlag);
                headerBytes.Slice(8, 2).SetValueLE((UInt16)CompressionMethodId);
                headerBytes.Slice(10, 2).SetValueLE(DosTime);
                headerBytes.Slice(12, 2).SetValueLE(DosDate);
                headerBytes.Slice(14, 4).SetValueLE(Crc);
                headerBytes.Slice(18, 4).SetValueLE(RawPackedSizeForLocalHeader);
                headerBytes.Slice(22, 4).SetValueLE(RawSizeForLocalHeader);
                headerBytes.Slice(26, 2).SetValueLE((UInt16)EntryFullNameBytes.Length);
                headerBytes.Slice(28, 2).SetValueLE((UInt16)extraFieldsBytes.Length);
                return new[]
                {
                    headerBytes,
                    EntryFullNameBytes,
                    extraFieldsBytes,
                };
            }

            public IEnumerable<ReadOnlyMemory<Byte>> ToCentralDirectoryHeaderBytes()
            {
                var extraFieldsBytes = CentralDirectoryHeaderExtraFields.ToByteArray();
                var headerBytes = new Byte[_centralDirectoryHeaderSizeOfFixedPart].AsMemory();
                headerBytes[..4].SetValueLE(_centralDirectoryHeaderSignature);
                headerBytes.Slice(4, 2).SetValueLE(VersionMadeBy);
                headerBytes.Slice(6, 2).SetValueLE(VersionNeededToExtractForCentralDirectoryHeader);
                headerBytes.Slice(8, 2).SetValueLE((UInt16)GeneralPurposeBitFlag);
                headerBytes.Slice(10, 2).SetValueLE((UInt16)CompressionMethodId);
                headerBytes.Slice(12, 2).SetValueLE(DosTime);
                headerBytes.Slice(14, 2).SetValueLE(DosDate);
                headerBytes.Slice(16, 4).SetValueLE(Crc);
                headerBytes.Slice(20, 4).SetValueLE(RawPackedSizeForCentralDirectoryHeader);
                headerBytes.Slice(24, 4).SetValueLE(RawSizeForCentralDirectoryHeader);
                headerBytes.Slice(28, 2).SetValueLE((UInt16)EntryFullNameBytes.Length);
                headerBytes.Slice(30, 2).SetValueLE((UInt16)extraFieldsBytes.Length);
                headerBytes.Slice(32, 2).SetValueLE((UInt16)EntryCommentBytes.Length);
                headerBytes.Slice(34, 2).SetValueLE(RawDiskNumberStartForCentralDirectoryHeader);
                headerBytes.Slice(36, 2).SetValueLE((UInt16)0); // internal attributes
                headerBytes.Slice(38, 4).SetValueLE(ExternalFileAttributes);
                headerBytes.Slice(42, 4).SetValueLE(RawRelativeOffsetOfLocalHeaderForCentralDirectoryHeader);
                return new[]
                {
                    headerBytes,
                    EntryFullNameBytes,
                    extraFieldsBytes,
                    EntryCommentBytes,
                };
            }

            public static ContentHeaderInfo Create(
                ZipArchiveFileWriter.IZipFileWriterEnvironment zipWriter,
                ZipStreamPosition localHeaderPosition,
                ZipEntryGeneralPurposeBitFlag generalPurposeBitFlag,
                ZipEntryCompressionMethodId compressionMethodId,
                UInt64 size,
                UInt64 packedSize,
                UInt32 crc,
                UInt32 externalAttributes,
                ExtraFieldStorage localHeaderExtraFields,
                ExtraFieldStorage centralDirectoryHeaderExtraFields,
                ReadOnlyMemory<Byte> entryFullNameBytes,
                ReadOnlyMemory<Byte> entryCommentBytes,
                DateTime? lastWriteTimeUtc,
                DateTime? lastAccessTimeUtc,
                DateTime? creationTimeUtc,
                Boolean isDirectory)
            {
                // 拡張フィールドの設定のためのカプセルオブジェクトを作る
                var extraFields = new ExtraFieldCollection(localHeaderExtraFields, centralDirectoryHeaderExtraFields);

                //
                // ZIP64 ヘッダを付加する
                //
                var zip64ExtraFieldForLocalHeader = new Zip64ExtendedInformationExtraFieldForLocalHeader();
                var (rawSizeForLocalHeader, rawPackedSizeForLocalHeader) =
                    zip64ExtraFieldForLocalHeader.SetValues(size, packedSize);
                extraFields.AddExtraField(zip64ExtraFieldForLocalHeader);

                var zip64ExtraFieldForCentralDirectoryHeader = new Zip64ExtendedInformationExtraFieldForCentraHeader();
                var (rawSizeForCentralDirectoryHeader, rawPackedSizeForCentralDirectoryHeader, rawLocalHeaderOffsetForCentralDirectoryHeader, rawDiskNumberForCentralDirectoryHeader) =
                    zip64ExtraFieldForCentralDirectoryHeader.SetValues(size, packedSize, localHeaderPosition.OffsetOnTheDisk, localHeaderPosition.DiskNumber);
                extraFields.AddExtraField(zip64ExtraFieldForCentralDirectoryHeader);

                //
                // 高精度時刻の何れかが指定されている場合は、拡張フィールドに設定する
                //

                extraFields.Delete(NtfsExtraField.ExtraFieldId);
                extraFields.Delete(ExtendedTimestampExtraField.ExtraFieldId);

                var windowsTimestampExtraField = new NtfsExtraField()
                {
                    LastWriteTimeUtc = lastWriteTimeUtc,
                    LastAccessTimeUtc = lastAccessTimeUtc,
                    CreationTimeUtc = creationTimeUtc,
                };
                var unixTimeStampExtraField = new ExtendedTimestampExtraField()
                {

                    LastWriteTimeUtc = lastWriteTimeUtc,
                    LastAccessTimeUtc = lastAccessTimeUtc,
                    CreationTimeUtc = creationTimeUtc,
                };
                extraFields.AddExtraField(windowsTimestampExtraField);
                extraFields.AddExtraField(unixTimeStampExtraField);

                UInt16 dosDate;
                UInt16 dosTime;
                try
                {
                    (dosDate, dosTime) = (lastWriteTimeUtc ?? DateTime.UtcNow).FromDateTimeToDosDateTime(DateTimeKind.Local);
                }
                catch (Exception)
                {
                    dosDate = 0;
                    dosTime = 0;
                }

                var versionNeededTiExtract =
                    new[]
                    {
                        (UInt16)10, // minimum version (supported Stored compression)
                        isDirectory  ? (UInt16)20 : (UInt16)0, // version if it contains directory entries
                        compressionMethodId == ZipEntryCompressionMethodId.Deflate ? (UInt16)20 : (UInt16)0, // version if using Deflate compression
                        compressionMethodId == ZipEntryCompressionMethodId.Deflate64 ? (UInt16)21 : (UInt16)0, // version if using Deflate64 compression
                        compressionMethodId == ZipEntryCompressionMethodId.BZIP2 ? (UInt16)46 : (UInt16)0, // version if using BZIP2 compression
                        compressionMethodId == ZipEntryCompressionMethodId.LZMA ? (UInt16)63 : (UInt16)0, // version if using LZMA compression
                        compressionMethodId == ZipEntryCompressionMethodId.PPMd ? (UInt16)63 : (UInt16)0, // version if using PPMd+ compression
                    }
                    .Max();

                return
                    new ContentHeaderInfo(
                        localHeaderPosition,
                        (UInt16)(((UInt16)zipWriter.HostSystem << 8) | zipWriter.ThisSoftwareVersion),
                        versionNeededTiExtract
                            .Maximum(
                                localHeaderExtraFields.Contains(Zip64ExtendedInformationExtraField.ExtraFieldId)
                                ? (UInt16)45 // version if using zip 64 extensions
                                : (UInt16)0),
                        versionNeededTiExtract
                            .Maximum(
                                centralDirectoryHeaderExtraFields.Contains(Zip64ExtendedInformationExtraField.ExtraFieldId)
                                ? (UInt16)45 // version if using zip 64 extensions
                                : (UInt16)0),
                        generalPurposeBitFlag,
                        compressionMethodId,
                        dosDate,
                        dosTime,
                        crc,
                        rawSizeForLocalHeader,
                        rawSizeForCentralDirectoryHeader,
                        rawPackedSizeForLocalHeader,
                        rawPackedSizeForCentralDirectoryHeader,
                        entryFullNameBytes,
                        entryCommentBytes,
                        localHeaderExtraFields,
                        centralDirectoryHeaderExtraFields,
                        rawDiskNumberForCentralDirectoryHeader,
                        rawLocalHeaderOffsetForCentralDirectoryHeader,
                        externalAttributes);
            }
        }

        private class ExtraFieldCollection
            : IWriteOnlyExtraFieldCollection
        {
            private readonly ExtraFieldStorage _localHeaderExtraFields;
            private readonly ExtraFieldStorage _centralDirectoryHeaderExtraFields;

            public ExtraFieldCollection(ExtraFieldStorage localHeaderExtraFields, ExtraFieldStorage centralDirectoryHeaderExtraFields)
            {
                _localHeaderExtraFields = localHeaderExtraFields;
                _centralDirectoryHeaderExtraFields = centralDirectoryHeaderExtraFields;
            }

            public void Delete(UInt16 extraFieldId)
            {
                _localHeaderExtraFields.Delete(extraFieldId);
                _centralDirectoryHeaderExtraFields.Delete(extraFieldId);
            }

            public void AddExtraField<EXTRA_FIELD_T>(EXTRA_FIELD_T extraField)
                where EXTRA_FIELD_T : IExtraField
            {
                _localHeaderExtraFields.AddExtraField(extraField);
                _centralDirectoryHeaderExtraFields.AddExtraField(extraField);
            }
        }

        private const Int32 _localHeaderSizeOfFixedPart = 30;
        private const Int32 _centralDirectoryHeaderSizeOfFixedPart = 46;

        private static readonly UInt32 _localHeaderSignature;
        private static readonly UInt32 _centralDirectoryHeaderSignature;
        private static readonly Encoding _utf8Encoding;
        private static readonly Regex _dotEntryNamePattern;

        private readonly ZipArchiveFileWriter.IZipFileWriterEnvironment _zipFileWriter;
        private readonly ZipArchiveFileWriter.IZipFileWriterOutputStream _zipStream;
        private readonly ExtraFieldStorage _localHeaderExtraFields;
        private readonly ExtraFieldStorage _centralDirectoryHeaderExtraFields;
        private readonly ExtraFieldCollection _extraFields;
        private ContentHeaderInfo? _contentHeaderInfo;
        private Boolean _isFile;
        private ZipEntryGeneralPurposeBitFlag _generalPurposeBitFlag;
        private ZipEntryCompressionMethodId _compressionMethodId;
        private ZipEntryCompressionLevel _compressionLevel;
        private DateTime? _lastWriteTimeUtc;
        private DateTime? _lastAccessTimeUtc;
        private DateTime? _creationTimeUtc;
        private UInt32? _externalAttributes;
        private UInt64 _size;
        private UInt64 _packedSize;
        private UInt32 _crc;
        private Boolean _written;

        static ZipDestinationEntry()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _localHeaderSignature = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x03, 0x04);
            _centralDirectoryHeaderSignature = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x01, 0x02);
            _utf8Encoding = Encoding.UTF8.WithFallback(null, null).WithoutPreamble();
            _dotEntryNamePattern = new Regex(@"(^|/|\\)\.{1,2}($|/|\\)", RegexOptions.Compiled);
        }

        internal ZipDestinationEntry(
            ZipArchiveFileWriter.IZipFileWriterEnvironment zipFileWriter,
            ZipArchiveFileWriter.IZipFileWriterOutputStream zipStream,
            UInt32 index,
            String fullName,
            ReadOnlyMemory<Byte> fullNameBytes,
            String entryComment,
            ReadOnlyMemory<Byte> commentBytes,
            Encoding? exactEntryEncoding,
            IEnumerable<Encoding> possibleEntryEncodings)
        {
            if (String.IsNullOrEmpty(fullName))
                throw new ArgumentException($"{nameof(fullName)} must not be null or empty.", nameof(fullName));
            if (fullNameBytes.Length <= 0)
                throw new InvalidOperationException($"The {nameof(fullNameBytes)} value must not be empty.");
            if (fullNameBytes.Length > UInt16.MaxValue)
                throw new InvalidOperationException($"The value of the {nameof(fullNameBytes)} is too long.: {fullNameBytes.Length} bytes");
            if (entryComment is null)
                throw new ArgumentNullException(nameof(entryComment));
            if (commentBytes.Length > UInt16.MaxValue)
                throw new InvalidOperationException($"The value of the {nameof(commentBytes)} is too long.: {commentBytes.Length} bytes");
            if (possibleEntryEncodings is null)
                throw new ArgumentNullException(nameof(possibleEntryEncodings));
            if (_dotEntryNamePattern.IsMatch(fullName))
                throw new ArgumentException($"Entry names containing directory names \".\" or \"..\" are not allowed.: {fullName}", nameof(fullName));

            _zipFileWriter = zipFileWriter ?? throw new ArgumentNullException(nameof(zipFileWriter));
            _zipStream = zipStream ?? throw new ArgumentNullException(nameof(zipStream));
            _localHeaderExtraFields = new ExtraFieldStorage(ZipEntryHeaderType.LocalFileHeader);
            _centralDirectoryHeaderExtraFields = new ExtraFieldStorage(ZipEntryHeaderType.CentralDirectoryHeader);
            _extraFields = new ExtraFieldCollection(_localHeaderExtraFields, _centralDirectoryHeaderExtraFields);

            _contentHeaderInfo = null;
            _generalPurposeBitFlag = ZipEntryGeneralPurposeBitFlag.None;
            _isFile = true;
            _compressionMethodId = ZipEntryCompressionMethodId.Stored;
            _compressionLevel = ZipEntryCompressionLevel.Normal;
            _lastWriteTimeUtc = null;
            _lastAccessTimeUtc = null;
            _creationTimeUtc = null;
            _externalAttributes = null;
            _size = 0;
            _packedSize = 0;
            _crc = 0;
            _written = false;

            Index = index;
            FullName = fullName;
            FullNameBytes = fullNameBytes;
            Comment = entryComment;
            CommentBytes = commentBytes;
            LocalHeaderPosition = zipStream.Stream.Position;

            #region エントリのエンコーディングを決定する

            //
            // エントリのエンコーディングを決定する
            //

            _extraFields.Delete(UnicodePathExtraField.ExtraFieldId);
            _extraFields.Delete(UnicodeCommentExtraField.ExtraFieldId);
            _extraFields.Delete(CodePageExtraField.ExtraFieldId);

            var entryEncodings = possibleEntryEncodings;
            if (exactEntryEncoding is not null)
                entryEncodings = entryEncodings.Prepend(exactEntryEncoding);
            entryEncodings = entryEncodings.ToList();
            if (entryEncodings.None())
                entryEncodings = _zipFileWriter.EntryNameEncodingProvider.GetBestEncodings(fullNameBytes, fullName, commentBytes, entryComment);
            var entryEncoding = entryEncodings.FirstOrDefault();

            if (entryEncoding is null)
            {
                // 与えられたバイト列のデコード方式が不明である場合

                if (fullNameBytes.Length > 0 && fullName.Length > 0 && !fullName.IsUnknownEncodingText())
                {
                    // 有効なエントリ名文字列が与えられている場合

                    // 与えられたエントリ名文字列(UNICODE)を拡張フィールドに設定する
                    var extrafield = new UnicodePathExtraField();
                    extrafield.SetFullName(fullName, fullNameBytes.Span);
                    _extraFields.AddExtraField(extrafield);
                }

                if (commentBytes.Length > 0 && entryComment.Length > 0 && !entryComment.IsUnknownEncodingText())
                {
                    // 有効なコメント文字列が与えられている場合

                    // 与えられたコメント(UNICODE)を拡張フィールドに設定する
                    var extrafield = new UnicodeCommentExtraField();
                    extrafield.SetComment(Comment, CommentBytes.Span);
                    _extraFields.AddExtraField(extrafield);
                }
            }
            else
            {
                // 与えられたバイト列のデコード方式が判明している場合

                if (exactEntryEncoding is not null &&
                    IsMatchedBytesAndString(entryEncoding, FullName, fullNameBytes.Span, Comment, commentBytes.Span))
                {
                    // エントリのエンコーディングが明確に判明しており、かつ
                    // 与えられた文字列をエンコードしたバイト列と与えられたバイト列が一致している場合

                    // (エンコーディングが何かはともかく) バイト列に UNICODE 文字セットのみが含まれている
                    // エントリ名とコメントを UTF-8 でエンコードしなおす

                    FullNameBytes = _utf8Encoding.GetReadOnlyBytes(FullName);
                    CommentBytes = _utf8Encoding.GetReadOnlyBytes(Comment);

                    // エントリのエンコーディングが UTF-8 であることを示す汎用フラグを立てる
                    _generalPurposeBitFlag |= ZipEntryGeneralPurposeBitFlag.UseUnicodeEncodingForNameAndComment;
                }
                else
                {
                    // エントリのエンコーディングが明確には判明していない、または
                    // 与えられた文字列をエンコードしたバイト列と与えられたバイト列が一致しない場合

                    // エントリのエンコーディングに確信が持てないか、あるいはおそらくバイト列に UNICODE 文字セットに含まれていない文字が含まれている

                    if ((fullNameBytes.Length > 0 || commentBytes.Length > 0)
                        && (fullName.Length > 0 || entryComment.Length > 0)
                        && !fullName.IsUnknownEncodingText()
                        && !entryComment.IsUnknownEncodingText())
                    {
                        // 有効なエントリ名またはコメントが与えられている場合

                        // 与えられたバイト列をテキストにデコードするためのコードページを拡張フィールドに設定する
                        _extraFields.AddExtraField(
                            new CodePageExtraField
                            {
                                CodePage = entryEncoding.CodePage,
                            });
                    }

                    if (fullNameBytes.Length > 0 && fullName.Length > 0 && !fullName.IsUnknownEncodingText())
                    {
                        // 有効なエントリ名文字列が与えられている場合

                        // 与えられたエントリ名文字列(UNICODE)を拡張フィールドに設定する
                        var extraField = new UnicodePathExtraField();
                        extraField.SetFullName(FullName, fullNameBytes.Span);
                        _extraFields.AddExtraField(extraField);
                    }

                    if (commentBytes.Length > 0 && entryComment.Length > 0 && !entryComment.IsUnknownEncodingText())
                    {
                        // 有効なコメント文字列が与えられている場合

                        // 与えられたコメント(UNICODE)を拡張フィールドに設定する
                        var extraField = new UnicodeCommentExtraField();
                        extraField.SetComment(Comment, commentBytes.Span);
                        _extraFields.AddExtraField(extraField);
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// このエントリが追加された順番を示す整数です。
        /// </summary>
        public UInt32 Index { get; }

        /// <summary>
        /// このエントリのエントリ名の文字列です。
        /// </summary>
        public String FullName { get; }

        /// <summary>
        /// このエントリのエントリ名のバイト列です。
        /// </summary>
        public ReadOnlyMemory<Byte> FullNameBytes { get; }

        /// <summary>
        /// このエントリのコメントの文字列です。
        /// </summary>
        public String Comment { get; }

        /// <summary>
        /// このエントリのコメントのバイト列です。
        /// </summary>
        public ReadOnlyMemory<Byte> CommentBytes { get; }

        internal ZipStreamPosition LocalHeaderPosition { get; }

        /// <summary>
        /// このエントリがファイルかどうかを示す <see cref="Boolean"/> 値を取得または設定します。ファイルであれば true、そうではないのなら false です。
        /// </summary>
        public Boolean IsFile
        {
            get => _isFile;

            set
            {
                if (_written)
                    throw new InvalidOperationException();

                _isFile = value;
            }
        }

        /// <summary>
        /// このエントリがディレクトリかどうかを示す <see cref="Boolean"/> 値を取得または設定します。ディレクトリであれば true、そうではないのなら false です。
        /// </summary>
        public Boolean IsDirectory
        {
            get => !_isFile;

            set
            {
                if (_written)
                    throw new InvalidOperationException();

                _isFile = !value;
            }
        }

        /// <summary>
        /// 書き込むデータの圧縮方式を示す値を取得または設定します。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>必ずしもすべての圧縮方式がサポートされているわけではないことに注意してください。</item>
        /// </list>
        /// </remarks>
        public ZipEntryCompressionMethodId CompressionMethodId
        {
            get => _compressionMethodId;

            set
            {
                if (_written)
                    throw new InvalidOperationException();
                if (!ZipEntryCompressionMethod.SupportedCompresssionMethodIds.Contains(value))
                    throw new ArgumentException($"An unsupported compression method was specified.: {value}", nameof(value));

                _compressionMethodId = value;
            }
        }

        /// <summary>
        /// 書き込むデータの圧縮率の高さを取得または設定します。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>このプロパティの値は、非圧縮 (<see cref="_compressionMethodId"/> プロパティの値が <see cref="ZipEntryCompressionMethodId.Stored"/>) 以外の場合に意味を持ちます。</item>
        /// <item>このプロパティの値が意味を持たない圧縮方式も存在します。</item>
        /// </list>
        /// </remarks>
        public ZipEntryCompressionLevel CompressionLevel
        {
            get => _compressionLevel;

            set
            {
                if (_written)
                    throw new InvalidOperationException();

                _compressionLevel = value;
            }
        }

        /// <summary>
        /// 書き込むデータの最終更新時刻(UTC)を取得または設定します。既定値は null です。null が設定されている場合は書き込むデータには最終更新時刻として現在時刻が付加されます。
        /// </summary>
        public DateTime? LastWriteTimeUtc
        {
            get => _lastWriteTimeUtc;

            set
            {
                if (_written)
                    throw new InvalidOperationException();
                if (value is not null && value.Value.Kind == DateTimeKind.Unspecified)
                    throw new ArgumentException($"Setting a value where the {nameof(value.Value.Kind)} property is {nameof(DateTimeKind.Unspecified)} is prohibited.", nameof(value));

                _lastWriteTimeUtc = value?.ToUniversalTime();
            }
        }

        /// <summary>
        /// 書き込むデータの最終アクセス日時(UTC)を取得または設定します。既定値は null です。null が設定されている場合は書き込むデータに最終アクセス日時は付加されません。
        /// </summary>
        public DateTime? LastAccessTimeUtc
        {
            get => _lastAccessTimeUtc;

            set
            {
                if (_written)
                    throw new InvalidOperationException();
                if (value is not null && value.Value.Kind == DateTimeKind.Unspecified)
                    throw new ArgumentException($"Setting a value where the {nameof(value.Value.Kind)} property is {nameof(DateTimeKind.Unspecified)} is prohibited.", nameof(value));

                _lastAccessTimeUtc = value?.ToUniversalTime();
            }
        }

        /// <summary>
        /// 書き込むデータの作成日時(UTC)を取得または設定します。既定値は null です。null が設定されている場合は書き込むデータに作成日時は付加されません。
        /// </summary>
        public DateTime? CreationTimeUtc
        {
            get => _creationTimeUtc;

            set
            {
                if (_written)
                    throw new InvalidOperationException();
                if (value is not null && value.Value.Kind == DateTimeKind.Unspecified)
                    throw new ArgumentException($"Setting a value where the {nameof(value.Value.Kind)} property is {nameof(DateTimeKind.Unspecified)} is prohibited.", nameof(value));

                _creationTimeUtc = value?.ToUniversalTime();
            }
        }

        /// <summary>
        /// 書き込むデータのファイル属性を取得または設定します。既定値は OS 固有の値です。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>このプロパティの意味は実行時の OS により異なります。
        /// <list type="bullet">
        /// <item>Windows または MS-DOS 系 OS の場合は <see cref="ExternalAttributesForDos"/> を参考にしてください。</item>
        /// <item>UNIX 系 OS の場合は <see cref="ExternalAttributesForUnix"/> を参考にしてください。</item>
        /// </list>
        /// </item>
        /// </list>
        /// </remarks>
        public UInt32 ExternalAttributes
        {
            get
            {
                if (_externalAttributes is not null)
                    return _externalAttributes.Value;

                if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                {
                    if (IsDirectory)
                        return (UInt32)(ExternalAttributesForUnix.DOS_DIRECTORY | ExternalAttributesForUnix.UNX_IFDIR | ExternalAttributesForUnix.UNX_IROTH | ExternalAttributesForUnix.UNX_IXOTH | ExternalAttributesForUnix.UNX_IRGRP | ExternalAttributesForUnix.UNX_IXGRP | ExternalAttributesForUnix.UNX_IRUSR | ExternalAttributesForUnix.UNX_IWUSR | ExternalAttributesForUnix.UNX_IXUSR);
                    else
                        return (UInt32)(ExternalAttributesForUnix.DOS_ARCHIVE | ExternalAttributesForUnix.UNX_IFREG | ExternalAttributesForUnix.UNX_IROTH | ExternalAttributesForUnix.UNX_IRGRP | ExternalAttributesForUnix.UNX_IRUSR | ExternalAttributesForUnix.UNX_IWUSR);
                }
                else
                {
                    if (IsDirectory)
                        return (UInt32)ExternalAttributesForDos.DOS_DIRECTORY;
                    else
                        return (UInt32)ExternalAttributesForDos.DOS_ARCHIVE;
                }
            }

            set
            {
                if (_written)
                    throw new InvalidOperationException();

                _externalAttributes = value;
            }
        }

        /// <summary>
        /// 拡張フィールドを設定するためのオブジェクトを取得します。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <term>[拡張フィールドについて]</term>
        /// <description>
        /// <para>拡張フィールドとは、ZIP の正式フォーマットには含まれていない様々な追加情報です。</para>
        /// <para>拡張フィールドには多くの種類があります。例えば以下のようなものがあります。</para>
        /// <list type="bullet">
        /// <item>NTFS 上でのファイル/ディレクトリのセキュリティディスクリプタを保持する拡張フィールド</item>
        /// <item>NTFS 上でのファイル/ディレクトリのタイムスタンプを保持する拡張フィールド</item>
        /// <item>UNIX上でのファイル/ディレクトリのタイムスタンプやユーザID/グループIDを保持する拡張フィールド</item>
        /// <item>エントリ名やコメントのUNICODE文字列を保持する拡張フィールド</item>
        /// <item>エントリ名やコメントのコードページを保持する拡張フィールド</item>
        /// </list>
        /// <para>これはごく一部の例ですが、明らかに目的が重複している拡張フィールドもありますし、特定のオペレーティングシステムでしか意味を持たない拡張フィールドも存在します。</para>
        /// <para>そして、これらの拡張フィールドをZIPアーカイバソフトウェアがどう扱うかは、ZIPアーカイバソフトウェアに任されています。適切に対応されることもあれば、無視されることもあるでしょう。異なる実行環境での拡張フィールドの互換性には注意してください。</para>
        /// </description>
        /// </item>
        /// <item>
        /// <term>[拡張フィールドの仕様について]</term>
        /// <description>
        /// <para>よく知られている拡張フィールドの仕様については、<see href="https://libzip.org/specifications/extrafld.txt">info-zip の記事</see> が一番詳しいようです。</para>
        /// </description>
        /// </item>
        /// <item>
        /// <term>[拡張フィールドの設定方法]</term>
        /// <description>
        /// <para>NTFS のセキュリティディスクリプタを保持する拡張フィールドを設定するサンプルプログラムを以下に示します。</para>
        /// <code>
        /// using System;
        /// using System.IO;
        /// using ZipUtility;
        /// using ZipUtility.ZipExtraField;
        ///
        /// internal class Program
        /// {
        ///     private static void Main(string[] args)
        ///     {
        ///         using var writer = new FilePath(args[0]).CreateAsZipFile(ZipEntryNameEncodingProvider.Create(Array.Empty&lt;string&gt;(), Array.Empty&lt;string&gt;()));
        ///
        ///         // "note.txt" というエントリ名を作る。
        ///         var entry = writer.CreateEntry("note.txt");
        ///
        ///         // "note.txt"の NTFS セキュリティディスクリプタ の設定を行う
        ///         // NTFS のセキュリティディスクリプタを保持する拡張フィールドを実装しているクラスは <see cref="WindowsSecurityDescriptorExtraField"/> なので、<see cref="WindowsSecurityDescriptorExtraField"/> 型のオブジェクトを ExtraFields AddExtraField メソッドに与える。
        ///         entry.ExtraFields.AddExtraField(
        ///             new WindowsSecurityDescriptorExtraField()
        ///             {
        ///                 // 各種プロパティの設定を行う
        ///             });
        ///         using var dataStream = entry.GetContentStream();
        ///
        ///         //これ以降、dataStream に対してデータの書き込みを行う
        ///
        ///     }
        /// }
        /// </code>
        /// </description>
        /// </item>
        /// <item>
        /// <term>[拡張フィールドのカスタマイズについて]</term>
        /// <description>
        /// <para>もし、あなたがこのソフトウェアでサポートされていない拡張フィールドの設定を取得したい場合には、以下の手順に従ってください。</para>
        /// <list type="number">
        /// <item><see cref="ExtraField"/> を継承した、拡張フィールドのクラスを定義する。</item>
        /// <item>前項で定義した拡張フィールドのクラスのインスタンスを作成し、必要なプロパティを設定する。</item>
        /// <item><see cref="IWriteOnlyExtraFieldCollection.AddExtraField{EXTRA_FIELD_T}(EXTRA_FIELD_T)"/>メソッドを使用して前項で作成した拡張フィールドのインスタンスをコレクションに追加する。</item>
        /// </list>
        /// <para>拡張フィールドのクラスの実装例については、<see cref="WindowsSecurityDescriptorExtraField"/> クラスまたは <see cref="XceedUnicodeExtraField"/> クラスのソースコードを参照してください。</para>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        public IWriteOnlyExtraFieldCollection ExtraFields => _extraFields;

        /// <summary>
        /// 書き込まれたデータの長さを取得します。まだデータが書き込まれていない場合は null です。
        /// </summary>
        public UInt64 Size
        {
            get
            {
                if (!_written)
                    throw new InvalidOperationException();

                return _size;
            }
        }

        /// <summary>
        /// 書き込まれたデータの圧縮された長さを取得します。まだデータが書き込まれていない場合は null です。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>圧縮方式が <see cref="ZipEntryCompressionMethodId.Stored"/> の場合は、このプロパティの値は <see cref="Size"/> の値に等しくなります。</item>
        /// </list>
        /// </remarks>
        public UInt64 PackedSize
        {
            get
            {
                if (!_written)
                    throw new InvalidOperationException();

                return _packedSize;
            }
        }

        /// <summary>
        /// 書き込まれたデータの CRC 値を取得します。まだデータが書き込まれていない場合は null です。
        /// </summary>
        public UInt32 Crc
        {
            get
            {
                if (!_written)
                    throw new InvalidOperationException();

                return _crc;
            }
        }

        /// <summary>
        /// エントリのデータを書き込むためのストリームを取得します。
        /// </summary>
        /// <param name="unpackedCountProgress">
        /// <para>
        /// 処理の進行状況の通知を受け取るためのオブジェクトです。通知を受け取らない場合は null です。
        /// </para>
        /// <para>
        /// 進行状況は、書き込みが完了したデータのバイト数です。
        /// </para>
        /// </param>
        /// <returns>
        /// エントリのデータの出力先ストリームのオブジェクトです。
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// 既にデータは出力済みです。
        /// </exception>
        public IBasicOutputByteStream GetContentStream(IProgress<UInt64>? unpackedCountProgress = null)
        {
            _zipStream.LockStream();

            if (_written)
                throw new InvalidOperationException();

            if (LastWriteTimeUtc is not null && LastWriteTimeUtc.Value.Kind == DateTimeKind.Unspecified)
                throw new InvalidOperationException($"The value of {nameof(LastWriteTimeUtc)}.{nameof(LastWriteTimeUtc.Value.Kind)} property must not be {nameof(DateTimeKind)}.{nameof(DateTimeKind.Unspecified)}.");
            if (LastAccessTimeUtc is not null && LastAccessTimeUtc.Value.Kind == DateTimeKind.Unspecified)
                throw new InvalidOperationException($"The value of {nameof(LastAccessTimeUtc)}.{nameof(LastAccessTimeUtc.Value.Kind)} property must not be {nameof(DateTimeKind)}.{nameof(DateTimeKind.Unspecified)}.");
            if (CreationTimeUtc is not null && CreationTimeUtc.Value.Kind == DateTimeKind.Unspecified)
                throw new InvalidOperationException($"The value of {nameof(CreationTimeUtc)}.{nameof(CreationTimeUtc.Value.Kind)} property must not be {nameof(DateTimeKind)}.{nameof(DateTimeKind.Unspecified)}.");

            var temporaryFile = (FilePath?)null;
            var packedTemporaryFile = (FilePath?)null;
            var crcValueHolder = new ValueHolder<(UInt32 Crc, UInt64 Size)>();

            try
            {
                try
                {
                    unpackedCountProgress?.Report(0);
                }
                catch (Exception)
                {
                }

                var compressionMethod = CompressionMethodId.GetCompressionMethod(CompressionLevel);

                temporaryFile = new FilePath(Path.GetTempFileName());

                packedTemporaryFile =
                    CompressionMethodId == ZipEntryCompressionMethodId.Stored
                    ? null
                    : new FilePath(Path.GetTempFileName());

                var outputStrem =
                    temporaryFile.Create()
                    .WithCrc32Calculation(crcValueHolder);

                var packedOutputStream =
                    packedTemporaryFile is null
                    ? null
                    : compressionMethod.GetEncodingStream(
                        packedTemporaryFile.Create()
                        .WithCache(),
                        null,
                        SafetyProgress.CreateProgress<(UInt64 unpackedCount, UInt64 packedCount), UInt64>(
                            unpackedCountProgress,
                            value => value.unpackedCount / 2))
                        .WithCache();

                var tempraryFileStream =
                    new PassThroughOutputStream(outputStrem, packedOutputStream);

                tempraryFileStream.OnStreamClosed += EndOfCopyingToTemporaryFile;
                return tempraryFileStream;
            }
            catch (Exception)
            {
                temporaryFile?.SafetyDelete();
                packedTemporaryFile?.SafetyDelete();
                _zipStream.UnlockStream();
                throw;
            }

            void EndOfCopyingToTemporaryFile(Object? sender, OnStreamClosedEventArgs<UInt64> e)
            {
                if (sender is IReportableOnStreamClosed<UInt64> eventSender)
                    eventSender.OnStreamClosed -= EndOfCopyingToTemporaryFile;

                var success = false;
                try
                {
                    if (temporaryFile is not null && temporaryFile.Exists)
                    {
                        if (CompressionMethodId == ZipEntryCompressionMethodId.Stored != (packedTemporaryFile is null))
                            throw new InternalLogicalErrorException();

                        var size = (UInt64)temporaryFile.Length;
                        var packedSize = (UInt64)temporaryFile.Length;
                        var crc = crcValueHolder.Value.Crc;

                        if (size != crcValueHolder.Value.Size)
                            throw new Exception("Faital error !");

                        if (packedTemporaryFile is not null)
                        {
                            if (!packedTemporaryFile.Exists)
                                throw new InternalLogicalErrorException();

                            packedSize = (UInt64)packedTemporaryFile.Length;

                            if (size <= 0 || packedSize >= size)
                            {
                                // 圧縮前のサイズが 0、または圧縮後のサイズが圧縮前のサイズより小さくなっていない場合

                                // 圧縮方式を強制的に Stored に変更する。
                                CompressionMethodId = ZipEntryCompressionMethodId.Stored;
                                CompressionLevel = ZipEntryCompressionLevel.Normal;
                                packedSize = size;
                                packedTemporaryFile.SafetyDelete();
                                packedTemporaryFile = null;
                            }
                        }

                        //
                        // 圧縮方式が Deflate の場合、圧縮レベルをフラグとして設定する
                        //
                        if (CompressionMethodId.IsAnyOf(ZipEntryCompressionMethodId.Deflate, ZipEntryCompressionMethodId.Deflate64))
                        {
                            switch (CompressionLevel)
                            {
                                case ZipEntryCompressionLevel.Normal:
                                default:
                                    break;
                                case ZipEntryCompressionLevel.Maximum:
                                    _generalPurposeBitFlag |= ZipEntryGeneralPurposeBitFlag.CompresssionOption0;
                                    break;
                                case ZipEntryCompressionLevel.Fast:
                                    _generalPurposeBitFlag |= ZipEntryGeneralPurposeBitFlag.CompresssionOption1;
                                    break;
                                case ZipEntryCompressionLevel.SuperFast:
                                    _generalPurposeBitFlag |= ZipEntryGeneralPurposeBitFlag.CompresssionOption0 | ZipEntryGeneralPurposeBitFlag.CompresssionOption1;
                                    break;
                            }
                        }

                        _contentHeaderInfo =
                            ContentHeaderInfo.Create(
                                _zipFileWriter,
                                LocalHeaderPosition,
                                _generalPurposeBitFlag,
                                CompressionMethodId,
                                size,
                                packedSize,
                                crc,
                                ExternalAttributes,
                                _localHeaderExtraFields,
                                _centralDirectoryHeaderExtraFields,
                                FullNameBytes,
                                CommentBytes,
                                LastWriteTimeUtc,
                                LastAccessTimeUtc,
                                CreationTimeUtc,
                                IsDirectory);

                        using var destinationStream = _zipStream.Stream.AsPartial(_contentHeaderInfo.LocalHeaderPosition, null);
                        destinationStream.WriteBytes(_contentHeaderInfo.ToLocalHeaderBytes());
                        using var sourceStream = (packedTemporaryFile is null ? temporaryFile : packedTemporaryFile).OpenRead();
                        sourceStream.CopyTo(
                            destinationStream,
                            SafetyProgress.CreateProgress<UInt64, UInt64>(
                                unpackedCountProgress,
                                value =>
                                    CompressionMethodId == ZipEntryCompressionMethodId.Stored
                                    ? checked((size + value) / 2)
                                    : packedSize <= 0
                                    ? 0
                                    : checked((UInt64)(size + (Double)value / packedSize * size) / 2)));
                        _size = size;
                        _packedSize = packedSize;
                        _crc = crc;

                        try
                        {
                            unpackedCountProgress?.Report(size);
                        }
                        catch (Exception)
                        {
                        }

                        _written = true;
                        success = true;
                    }
                }
                finally
                {
                    temporaryFile.SafetyDelete();
                    packedTemporaryFile?.SafetyDelete();

                    try
                    {
                        if (!success)
                            _zipStream.Stream.Seek(LocalHeaderPosition);
                    }
                    catch (Exception)
                    {
                    }

                    _zipStream.UnlockStream();
                }
            }
        }

        /// <summary>
        /// まだ書き込まれていないデータを書き込みます。
        /// </summary>
        public void Flush()
        {
            _zipStream.LockStream();
            try
            {
                InternalFlush();
            }
            finally
            {
                _zipStream.UnlockStream();
            }
        }

        /// <summary>
        /// オブジェクトの内容を分かりやすい文字列に変換します。
        /// </summary>
        /// <returns>
        /// オブジェクトの内容を示す文字列です。
        /// </returns>
        public override String ToString() => $"\"{_zipFileWriter.ZipArchiveFile.FullName}/{FullName}\"";

        internal UInt16 VersionNeededToExtractForLocalHeader => _contentHeaderInfo?.VersionNeededToExtractForLocalHeader ?? throw new InvalidOperationException();
        internal UInt16 VersionNeededToExtractForCentralDirectoryHeader => _contentHeaderInfo?.VersionNeededToExtractForCentralDirectoryHeader ?? throw new InvalidOperationException();

        internal void WriteCentralDirectoryHeader()
        {
            if (_contentHeaderInfo is null)
                throw new InvalidOperationException();

            using var destinationStream = _zipStream.Stream.AsPartial(_zipStream.Stream.Position, null);
            destinationStream.WriteBytes(_contentHeaderInfo.ToCentralDirectoryHeaderBytes());
        }

        internal void InternalFlush()
        {
            if (!_written)
            {
                // GetContentStream() が呼ばれないまま Flush() または Dispose() が呼び出された場合

                _size = 0;
                _packedSize = 0;
                _crc = Array.Empty<Byte>().CalculateCrc32().Crc;
                _contentHeaderInfo =
                    ContentHeaderInfo.Create(
                        _zipFileWriter,
                        LocalHeaderPosition,
                        _generalPurposeBitFlag,
                        ZipEntryCompressionMethodId.Stored,
                        _size,
                        _packedSize,
                        _crc,
                        ExternalAttributes,
                        _localHeaderExtraFields,
                        _centralDirectoryHeaderExtraFields,
                        FullNameBytes,
                        CommentBytes,
                        LastWriteTimeUtc,
                        LastAccessTimeUtc,
                        CreationTimeUtc,
                        IsDirectory);

                using var destinationStream = _zipStream.Stream.AsPartial(_contentHeaderInfo.LocalHeaderPosition, null);
                destinationStream.WriteBytes(_contentHeaderInfo.ToLocalHeaderBytes());
                _written = true;
            }
        }

        private static Boolean IsMatchedBytesAndString(Encoding encoding, String fullName, ReadOnlySpan<Byte> fullNameBytes, String comment, ReadOnlySpan<Byte> commentBytes)
        {
            try
            {
                var encodingForTest = encoding.WithFallback(null, null).WithoutPreamble();
                var triedFullName = encodingForTest.GetString(fullNameBytes);
                var triedComment = encodingForTest.GetString(commentBytes);
                var triedFullNameBytes = encodingForTest.GetBytes(fullName);
                var triedCommentBytes = encodingForTest.GetBytes(comment);
                return
                    triedFullName == fullName
                    && triedComment == comment
                    && triedFullNameBytes.SequenceEqual(fullNameBytes)
                    && triedCommentBytes.SequenceEqual(commentBytes);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
