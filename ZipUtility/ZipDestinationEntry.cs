using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private class LocalHeaderInfo
        {
            private LocalHeaderInfo(
                ZipStreamPosition localHeaderPosition,
                UInt16 versionNeededToExtract,
                ZipEntryGeneralPurposeBitFlag generalPurposeBitFlag,
                ZipEntryCompressionMethodId compressionMethodId,
                UInt16 dosDate,
                UInt16 dosTime,
                UInt32 crc,
                UInt32 rawSize,
                UInt32 rawPackedSize,
                ReadOnlyMemory<Byte> entryFullNameBytes,
                ExtraFieldStorage extraFields)
            {
                if (entryFullNameBytes.Length > UInt16.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(entryFullNameBytes));

                LocalHeaderPosition = localHeaderPosition;
                VersionNeededToExtract = versionNeededToExtract;
                GeneralPurposeBitFlag = generalPurposeBitFlag;
                CompressionMethodId = compressionMethodId;
                DosDate = dosDate;
                DosTime = dosTime;
                Crc = crc;
                RawSize = rawSize;
                RawPacked = rawPackedSize;
                EntryFullNameBytes = entryFullNameBytes;
                ExtraFields = extraFields ?? throw new ArgumentNullException(nameof(extraFields));
            }

            public ZipStreamPosition LocalHeaderPosition { get; }
            public UInt16 VersionNeededToExtract { get; }
            public ZipEntryGeneralPurposeBitFlag GeneralPurposeBitFlag { get; }
            public ZipEntryCompressionMethodId CompressionMethodId { get; }
            public UInt16 DosDate { get; }
            public UInt16 DosTime { get; }
            public UInt32 Crc { get; }
            public UInt32 RawSize { get; }
            public UInt32 RawPacked { get; }
            public ReadOnlyMemory<Byte> EntryFullNameBytes { get; }
            public ExtraFieldStorage ExtraFields { get; }

            public IEnumerable<ReadOnlyMemory<Byte>> ToBytes()
            {
                var extraFieldsBytes = ExtraFields.ToByteArray();
                var headerBytes = new Byte[30].AsMemory();
                headerBytes[..4].SetValueLE(_localHeaderSignature);
                headerBytes.Slice(4, 2).SetValueLE(VersionNeededToExtract);
                headerBytes.Slice(6, 2).SetValueLE((UInt16)GeneralPurposeBitFlag);
                headerBytes.Slice(8, 2).SetValueLE((UInt16)CompressionMethodId);
                headerBytes.Slice(10, 2).SetValueLE(DosTime);
                headerBytes.Slice(12, 2).SetValueLE(DosDate);
                headerBytes.Slice(14, 4).SetValueLE(Crc);
                headerBytes.Slice(18, 4).SetValueLE(RawPacked);
                headerBytes.Slice(22, 4).SetValueLE(RawSize);
                headerBytes.Slice(26, 2).SetValueLE((UInt16)EntryFullNameBytes.Length);
                headerBytes.Slice(28, 2).SetValueLE((UInt16)extraFieldsBytes.Length);
                return new[]
                {
                    headerBytes,
                    EntryFullNameBytes,
                    extraFieldsBytes,
                };
            }

            public static LocalHeaderInfo Create(
                ZipStreamPosition localHeaderPosition,
                ZipEntryGeneralPurposeBitFlag generalPurposeBitFlag,
                ZipEntryCompressionMethodId compressionMethodId,
                UInt64 size,
                UInt64 packedSize,
                UInt32 crc,
                ExtraFieldStorage extraFields,
                ReadOnlyMemory<Byte> entryFullNameBytes,
                DateTime? lastWriteTimeUtc,
                Boolean isDirectory)
            {
                var zip64ExtraField = new Zip64ExtendedInformationExtraFieldForLocalHeader();
                var (rawSize, rawPackedSize) = zip64ExtraField.SetValues(size, packedSize);
                extraFields.AddExtraField(zip64ExtraField);

                var (dosDate, dosTime) = GetDosDateTime(lastWriteTimeUtc);

                return
                    new LocalHeaderInfo(
                        localHeaderPosition,
                        GetVersionNeededToExtract(compressionMethodId, isDirectory, extraFields.Contains(Zip64ExtendedInformationExtraField.ExtraFieldId)),
                        generalPurposeBitFlag,
                        compressionMethodId,
                        dosDate,
                        dosTime,
                        crc,
                        rawSize,
                        rawPackedSize,
                        entryFullNameBytes,
                        extraFields);
            }

            public static LocalHeaderInfo Create(
                ZipStreamPosition localHeaderPosition,
                ZipEntryGeneralPurposeBitFlag generalPurposeBitFlag,
                ZipEntryCompressionMethodId compressionMethodId,
                ExtraFieldStorage extraFields,
                ReadOnlyMemory<Byte> entryFullNameBytes,
                DateTime? lastWriteTimeUtc,
                Boolean isDirectory)
            {
                generalPurposeBitFlag |= ZipEntryGeneralPurposeBitFlag.HasDataDescriptor;

                var (dosDate, dosTime) = GetDosDateTime(lastWriteTimeUtc);

                return
                    new LocalHeaderInfo(
                        localHeaderPosition,
                        GetVersionNeededToExtract(compressionMethodId, isDirectory, extraFields.Contains(Zip64ExtendedInformationExtraField.ExtraFieldId)),
                        generalPurposeBitFlag,
                        compressionMethodId,
                        dosDate,
                        dosTime,
                        0,
                        0,
                        0,
                        entryFullNameBytes,
                        extraFields);
            }
        }

        private class DataDescriptorInfo
        {
            private DataDescriptorInfo(
                Boolean requiedZip64,
                UInt32 crc,
                UInt64 size,
                UInt64 packedSize)
            {
                Crc = crc;
                Size = size;
                PackedSize = packedSize;
                RequiedZip64 = requiedZip64;
            }

            public UInt32 Crc { get; }
            public UInt64 Size { get; }
            public UInt64 PackedSize { get; }
            public Boolean RequiedZip64 { get; }

            public ReadOnlyMemory<Byte> ToBytes()
            {
                if (RequiedZip64)
                {
                    var headerBytes = new Byte[24].AsMemory();
                    headerBytes[..4].SetValueLE(_dataDescriptorSignature);
                    headerBytes.Slice(4, 4).SetValueLE(Crc);
                    headerBytes.Slice(8, 8).SetValueLE(PackedSize);
                    headerBytes.Slice(16, 8).SetValueLE(Size);
                    return headerBytes;
                }
                else
                {
                    var headerBytes = new Byte[16].AsMemory();
                    headerBytes[..4].SetValueLE(_dataDescriptorSignature);
                    headerBytes.Slice(4, 4).SetValueLE(Crc);
                    headerBytes.Slice(8, 4).SetValueLE(checked((UInt32)PackedSize));
                    headerBytes.Slice(12, 4).SetValueLE(checked((UInt32)Size));
                    return headerBytes;
                }
            }

            public static DataDescriptorInfo Create(
                UInt32 crc,
                UInt64 size,
                UInt64 packedSize)
                => new(
                    size >= UInt32.MaxValue || packedSize >= UInt32.MaxValue,
                    crc,
                    size,
                    packedSize);
        }

        private class CentralDirectoryHeaderInfo
        {
            private CentralDirectoryHeaderInfo(
                UInt16 versionMadeBy,
                UInt16 versionNeededToExtract,
                ZipEntryGeneralPurposeBitFlag generalPurposeBitFlag,
                ZipEntryCompressionMethodId compressionMethodId,
                UInt16 dosDate,
                UInt16 dosTime,
                UInt32 crc,
                UInt32 rawSize,
                UInt32 rawPackedSize,
                ReadOnlyMemory<Byte> entryFullNameBytes,
                ReadOnlyMemory<Byte> entryCommentBytes,
                ExtraFieldStorage extraFields,
                UInt32 externalFileAttributes,
                UInt16 rawDiskNumberStart,
                UInt32 rawRelativeOffsetOfLocalHeader)
            {
                if (entryFullNameBytes.Length > UInt16.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(entryFullNameBytes));
                if (entryCommentBytes.Length > UInt16.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(entryCommentBytes));

                VersionMadeBy = versionMadeBy;
                VersionNeededToExtract = versionNeededToExtract;
                GeneralPurposeBitFlag = generalPurposeBitFlag;
                CompressionMethodId = compressionMethodId;
                DosDate = dosDate;
                DosTime = dosTime;
                Crc = crc;
                RawSize = rawSize;
                RawPackedSize = rawPackedSize;
                EntryFullNameBytes = entryFullNameBytes;
                EntryCommentBytes = entryCommentBytes;
                ExtraFields = extraFields ?? throw new ArgumentNullException(nameof(extraFields));
                RawDiskNumberStart = rawDiskNumberStart;
                ExternalFileAttributes = externalFileAttributes;
                RawRelativeOffsetOfLocalHeader = rawRelativeOffsetOfLocalHeader;
            }

            public UInt16 VersionMadeBy { get; }
            public UInt16 VersionNeededToExtract { get; }
            public ZipEntryGeneralPurposeBitFlag GeneralPurposeBitFlag { get; }
            public ZipEntryCompressionMethodId CompressionMethodId { get; }
            public UInt16 DosDate { get; }
            public UInt16 DosTime { get; }
            public UInt32 Crc { get; }
            public UInt32 RawSize { get; }
            public UInt32 RawPackedSize { get; }
            public ReadOnlyMemory<Byte> EntryFullNameBytes { get; }
            public ReadOnlyMemory<Byte> EntryCommentBytes { get; }
            public ExtraFieldStorage ExtraFields { get; }
            public UInt32 ExternalFileAttributes { get; }
            public UInt16 RawDiskNumberStart { get; }
            public UInt32 RawRelativeOffsetOfLocalHeader { get; }

            public IEnumerable<ReadOnlyMemory<Byte>> ToBytes()
            {
                var extraFieldsBytes = ExtraFields.ToByteArray();
                var headerBytes = new Byte[46].AsMemory();
                headerBytes[..4].SetValueLE(_centralDirectoryHeaderSignature);
                headerBytes.Slice(4, 2).SetValueLE(VersionMadeBy);
                headerBytes.Slice(6, 2).SetValueLE(VersionNeededToExtract);
                headerBytes.Slice(8, 2).SetValueLE((UInt16)GeneralPurposeBitFlag);
                headerBytes.Slice(10, 2).SetValueLE((UInt16)CompressionMethodId);
                headerBytes.Slice(12, 2).SetValueLE(DosTime);
                headerBytes.Slice(14, 2).SetValueLE(DosDate);
                headerBytes.Slice(16, 4).SetValueLE(Crc);
                headerBytes.Slice(20, 4).SetValueLE(RawPackedSize);
                headerBytes.Slice(24, 4).SetValueLE(RawSize);
                headerBytes.Slice(28, 2).SetValueLE((UInt16)EntryFullNameBytes.Length);
                headerBytes.Slice(30, 2).SetValueLE((UInt16)extraFieldsBytes.Length);
                headerBytes.Slice(32, 2).SetValueLE((UInt16)EntryCommentBytes.Length);
                headerBytes.Slice(34, 2).SetValueLE(RawDiskNumberStart);
                headerBytes.Slice(36, 2).SetValueLE((UInt16)0); // internal attributes
                headerBytes.Slice(38, 4).SetValueLE(ExternalFileAttributes);
                headerBytes.Slice(42, 4).SetValueLE(RawRelativeOffsetOfLocalHeader);
                return new[]
                {
                    headerBytes,
                    EntryFullNameBytes,
                    extraFieldsBytes,
                    EntryCommentBytes,
                };
            }

            public static CentralDirectoryHeaderInfo Create(
                ZipArchiveFileWriter.IZipFileWriterEnvironment zipWriter,
                ZipStreamPosition localHeaderPosition,
                ZipEntryGeneralPurposeBitFlag generalPurposeBitFlag,
                ZipEntryCompressionMethodId compressionMethodId,
                UInt64 size,
                UInt64 packedSize,
                UInt32 crc,
                UInt32 externalAttributes,
                ExtraFieldStorage extraFields,
                ReadOnlyMemory<Byte> entryFullNameBytes,
                ReadOnlyMemory<Byte> entryCommentBytes,
                DateTime? lastWriteTimeUtc,
                Boolean isDirectory,
                Boolean useDataDescriptor)
            {
                if (useDataDescriptor)
                    generalPurposeBitFlag |= ZipEntryGeneralPurposeBitFlag.HasDataDescriptor;

                var zip64ExtraField = new Zip64ExtendedInformationExtraFieldForCentraHeader();
                var (rawSize, rawPackedSize, rawLocalHeaderOffset, rawDiskNumber) =
                    zip64ExtraField.SetValues(
                        size,
                        packedSize,
                        localHeaderPosition.OffsetOnTheDisk,
                        localHeaderPosition.DiskNumber);
                extraFields.AddExtraField(zip64ExtraField);

                var (dosDate, dosTime) = GetDosDateTime(lastWriteTimeUtc);

                return
                    new CentralDirectoryHeaderInfo(
                        (UInt16)(((UInt16)zipWriter.HostSystem << 8) | zipWriter.ThisSoftwareVersion),
                        GetVersionNeededToExtract(compressionMethodId, isDirectory, extraFields.Contains(Zip64ExtendedInformationExtraField.ExtraFieldId)),
                        generalPurposeBitFlag,
                        compressionMethodId,
                        dosDate,
                        dosTime,
                        crc,
                        rawSize,
                        rawPackedSize,
                        entryFullNameBytes,
                        entryCommentBytes,
                        extraFields,
                        externalAttributes,
                        rawDiskNumber,
                        rawLocalHeaderOffset);
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

        private static readonly UInt32 _localHeaderSignature;
        private static readonly UInt32 _dataDescriptorSignature;
        private static readonly UInt32 _centralDirectoryHeaderSignature;
        private static readonly Encoding _utf8Encoding;
        private static readonly Regex _dotEntryNamePattern;

        private readonly ZipArchiveFileWriter.IZipFileWriterEnvironment _zipFileWriter;
        private readonly ZipArchiveFileWriter.IZipFileWriterOutputStream _zipStream;
        private readonly ExtraFieldStorage _localHeaderExtraFields;
        private readonly ExtraFieldStorage _centralDirectoryHeaderExtraFields;
        private readonly ExtraFieldCollection _extraFields;
        private LocalHeaderInfo? _localHeaderInfo;
        private CentralDirectoryHeaderInfo? _centralDirectoryHeaderInfo;
        private Boolean _isFile;
        private ZipEntryGeneralPurposeBitFlag _generalPurposeBitFlag;
        private ZipEntryCompressionMethodId _compressionMethodId;
        private ZipEntryCompressionLevel _compressionLevel;
        private DateTime? _lastWriteTimeUtc;
        private DateTime? _lastAccessTimeUtc;
        private DateTime? _creationTimeUtc;
        private UInt32? _externalAttributes;
        private Boolean _useDataDescriptor;
        private UInt64 _size;
        private UInt64 _packedSize;
        private UInt32 _crc;
        private Boolean _written;

        static ZipDestinationEntry()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _localHeaderSignature = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x03, 0x04);
            _dataDescriptorSignature = Signature.MakeUInt32LESignature(0x50, 0x4b, 0x07, 0x08);
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
            String comment,
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
            if (comment is null)
                throw new ArgumentNullException(nameof(comment));
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

            _localHeaderInfo = null;
            _centralDirectoryHeaderInfo = null;
            _generalPurposeBitFlag = ZipEntryGeneralPurposeBitFlag.None;
            _isFile = true;
            _compressionMethodId = ZipEntryCompressionMethodId.Stored;
            _compressionLevel = ZipEntryCompressionLevel.Normal;
            _lastWriteTimeUtc = null;
            _lastAccessTimeUtc = null;
            _creationTimeUtc = null;
            _externalAttributes = null;
            _useDataDescriptor = false;
            _size = 0;
            _packedSize = 0;
            _crc = 0;
            _written = false;

            Index = index;
            FullName = fullName;
            FullNameBytes = fullNameBytes;
            Comment = comment;
            CommentBytes = commentBytes;
            LocalHeaderPosition = zipStream.Stream.Position;

            #region エントリのエンコーディングを決定する

            //
            // エントリのエンコーディングを決定する
            //

            _extraFields.Delete(UnicodePathExtraField.ExtraFieldId);
            _extraFields.Delete(UnicodeCommentExtraField.ExtraFieldId);
            _extraFields.Delete(CodePageExtraField.ExtraFieldId);

            if (exactEntryEncoding is not null)
            {
                // 確実なエンコーディングが与えられている場合

                if (ValidateEncoding(exactEntryEncoding, fullName, fullNameBytes.Span, comment, commentBytes.Span))
                {
                    // エントリのエンコーディングが明確に判明しており、かつ
                    // 与えられた文字列をエンコードしたバイト列と与えられたバイト列が一致している場合

                    // エンコーディングが何かはともかく、バイト列が示す文字はすべて UNICODE 文字セットにも含まれている文字である (.NET の文字列の内部表現は UNICODE (UTF-16) であるため)
                    // => 拡張フィールドの単純化のため、エントリ名とコメントを UTF-8 でエンコードしなおす

                    FullNameBytes = _utf8Encoding.GetReadOnlyBytes(fullName);
                    CommentBytes = _utf8Encoding.GetReadOnlyBytes(comment);

                    // エントリのエンコーディングが UTF-8 であることを示す汎用フラグを立てる
                    _generalPurposeBitFlag |= ZipEntryGeneralPurposeBitFlag.UseUnicodeEncodingForNameAndComment;
                }
                else
                {
                    // エントリのエンコーディングが明確に判明しており、かつ
                    // 与えられた文字列をエンコードしたバイト列と与えられたバイト列が一致していない場合

                    // バイト列が示す文字列の文字の中に、UNICODE にマッピングできない文字が含まれている (.NET の文字列の内部表現は UNICODE (UTF-16) であるため)

                    if ((fullNameBytes.Length > 0 || commentBytes.Length > 0)
                        && (fullName.Length > 0 || comment.Length > 0)
                        && !fullName.IsUnknownEncodingText()
                        && !comment.IsUnknownEncodingText())
                    {
                        // 有効なエントリ名またはコメントが与えられている場合

                        // 与えられたバイト列をテキストにデコードするための拡張フィールドを付加する
                        _extraFields.AddExtraField(
                            new CodePageExtraField
                            {
                                CodePage = exactEntryEncoding.CodePage,
                            });
                    }

                    if (fullNameBytes.Length > 0 && fullName.Length > 0 && !fullName.IsUnknownEncodingText())
                    {
                        // 有効なエントリ名文字列が与えられている場合

                        // 与えられたエントリ名文字列(UNICODE)の拡張フィールドを付加する
                        var extraField = new UnicodePathExtraField();
                        extraField.SetFullName(fullName, fullNameBytes.Span);
                        _extraFields.AddExtraField(extraField);
                    }

                    if (commentBytes.Length > 0 && comment.Length > 0 && !comment.IsUnknownEncodingText())
                    {
                        // 有効なコメント文字列が与えられている場合

                        // 与えられたコメント(UNICODE)の拡張フィールドを付加する
                        var extraField = new UnicodeCommentExtraField();
                        extraField.SetComment(comment, commentBytes.Span);
                        _extraFields.AddExtraField(extraField);
                    }
                }
            }
            else
            {
                // 確実なエンコーディングが与えられていない場合

                // 正しい可能性のあるエンコーディングを探す
                var possibleEntryEncoding =
                    possibleEntryEncodings
                    .Where(encoding => ValidateEncoding(encoding, fullName, fullNameBytes.Span, comment, commentBytes.Span))
                    .FirstOrDefault();

                if (possibleEntryEncoding is not null)
                {
                    // 確実に正しいエンコーディングが与えられておらず、かつ
                    // 正しい可能性のあるエンコーディングが存在する場合

                    // (エンコーディングが何かはともかく) バイト列に UNICODE 文字セットのみが含まれている
                    // エントリ名とコメントを UTF-8 でエンコードしなおす

                    FullNameBytes = _utf8Encoding.GetReadOnlyBytes(fullName);
                    CommentBytes = _utf8Encoding.GetReadOnlyBytes(comment);

                    // エントリのエンコーディングが UTF-8 であることを示す汎用フラグを立てる
                    _generalPurposeBitFlag |= ZipEntryGeneralPurposeBitFlag.UseUnicodeEncodingForNameAndComment;
                }
                else
                {
                    // 正しい可能性のあるエンコーディングが存在しない場合

                    if (fullNameBytes.Length > 0 && fullName.Length > 0 && !fullName.IsUnknownEncodingText())
                    {
                        // 有効なエントリ名文字列が与えられている場合

                        // 与えられたエントリ名文字列(UNICODE)の拡張フィールドを付加する
                        var extraField = new UnicodePathExtraField();
                        extraField.SetFullName(fullName, fullNameBytes.Span);
                        _extraFields.AddExtraField(extraField);
                    }

                    if (commentBytes.Length > 0 && comment.Length > 0 && !comment.IsUnknownEncodingText())
                    {
                        // 有効なコメント文字列が与えられている場合

                        // 与えられたコメント(UNICODE)の拡張フィールドを付加する
                        var extraField = new UnicodeCommentExtraField();
                        extraField.SetComment(comment, commentBytes.Span);
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
        /// このエントリがファイルかどうかを示す <see cref="Boolean"/> 値を取得または設定します。
        /// </summary>
        /// <value>
        /// ファイルであれば true、そうではないのなら false です。
        /// </value>
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
        /// このエントリがディレクトリかどうかを示す <see cref="Boolean"/> 値を取得または設定します。
        /// </summary>
        /// <value>
        /// ディレクトリであれば true、そうではないのなら false です。
        /// </value>
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
        /// 書き込むエントリの圧縮方式を示す値を取得または設定します。
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
        /// 書き込むエントリの圧縮率の高さを取得または設定します。
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
        /// 書き込むエントリの最終更新日時(UTC)を取得または設定します。
        /// </summary>
        /// <value>
        /// 最終更新日時(UTC)を示す <see cref="DateTime"/> オブジェクトです。 既定値は null です。
        /// </value>
        /// <remarks>
        /// <list type="bullet">
        /// <item>このプロパティの値が null である場合、エントリの最終更新日時として代わりに現在日時が付加されます。</item>
        /// </list>
        /// </remarks>
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
        /// 書き込むエントリの最終アクセス日時(UTC)を取得または設定します。
        /// </summary>
        /// <value>
        /// 最終アクセス日時(UTC)を示す <see cref="DateTime"/> オブジェクトです。 既定値は null です。
        /// </value>
        /// <remarks>
        /// <list type="bullet">
        /// <item>このプロパティの値が null である場合、エントリの最終アクセス日時は付加されません</item>
        /// </list>
        /// </remarks>
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
        /// 書き込むエントリの作成日時(UTC)を取得または設定します。
        /// </summary>
        /// <value>
        /// 作成日時(UTC)を示す <see cref="DateTime"/> オブジェクトです。 既定値は null です。
        /// </value>
        /// <remarks>
        /// <list type="bullet">
        /// <item>このプロパティの値が null である場合、エントリの作成日時は付加されません</item>
        /// </list>
        /// </remarks>
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
        /// 書き込むエントリのファイル属性を取得または設定します。
        /// </summary>
        /// <value>
        /// <list type="bullet">
        /// <item>このプロパティの意味は実行時の OS により異なります。
        /// <list type="bullet">
        /// <item>Windows または MS-DOS 系 OS の場合は <see cref="ExternalAttributesForDos"/> を参考にしてください。</item>
        /// <item>UNIX 系 OS の場合は <see cref="ExternalAttributesForUnix"/> を参考にしてください。</item>
        /// </list>
        /// </item>
        /// <item>既定値は実行時の OS による固有の値です。</item>
        /// </list>
        /// </value>
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
        /// エントリのデータの書き込みの際にデータディスクリプタを使用するかどうかを示す <see cref="Boolean"/> 値を取得または設定します。
        /// </summary>
        /// <value>
        /// データディスクリプタを使用する場合は true、そうではない場合は falseです。既定値は false です。
        /// </value>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// 特別な理由が無い限り、<see cref="UseDataDescriptor"/> の値は既定値 (false) から変更しないことをお勧めします。
        /// </item>
        /// </list>
        /// </remarks>
        public Boolean UseDataDescriptor
        {
            get => _useDataDescriptor;

            set
            {
                if (_written)
                    throw new InvalidOperationException();

                _useDataDescriptor = value;
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
        public ISequentialOutputByteStream GetContentStream(IProgress<UInt64>? unpackedCountProgress = null)
        {
            _zipStream.LockStream();

            if (_written)
                throw new InvalidOperationException();
            if (!_isFile)
                throw new InvalidOperationException();

            if (LastWriteTimeUtc is not null && LastWriteTimeUtc.Value.Kind == DateTimeKind.Unspecified)
                throw new InvalidOperationException($"The value of {nameof(LastWriteTimeUtc)}.{nameof(LastWriteTimeUtc.Value.Kind)} property must not be {nameof(DateTimeKind)}.{nameof(DateTimeKind.Unspecified)}.");
            if (LastAccessTimeUtc is not null && LastAccessTimeUtc.Value.Kind == DateTimeKind.Unspecified)
                throw new InvalidOperationException($"The value of {nameof(LastAccessTimeUtc)}.{nameof(LastAccessTimeUtc.Value.Kind)} property must not be {nameof(DateTimeKind)}.{nameof(DateTimeKind.Unspecified)}.");
            if (CreationTimeUtc is not null && CreationTimeUtc.Value.Kind == DateTimeKind.Unspecified)
                throw new InvalidOperationException($"The value of {nameof(CreationTimeUtc)}.{nameof(CreationTimeUtc.Value.Kind)} property must not be {nameof(DateTimeKind)}.{nameof(DateTimeKind.Unspecified)}.");

            return
                _useDataDescriptor
                ? GetContentStreamWithDataDescriptor(unpackedCountProgress)
                : GetContentStreamWithoutDataDescriptor(unpackedCountProgress);
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

        internal UInt16 VersionNeededToExtractForLocalHeader => _localHeaderInfo?.VersionNeededToExtract ?? throw new InvalidOperationException();
        internal UInt16 VersionNeededToExtractForCentralDirectoryHeader => _centralDirectoryHeaderInfo?.VersionNeededToExtract ?? throw new InvalidOperationException();

        internal void WriteCentralDirectoryHeader()
        {
            if (_centralDirectoryHeaderInfo is null)
                throw new InvalidOperationException();

            _zipStream.Stream.WriteBytes(_centralDirectoryHeaderInfo.ToBytes());
        }

        internal void InternalFlush()
        {
            if (!_written)
            {
                // GetContentStream() が呼ばれないまま Flush() または Dispose() が呼び出された場合

                _size = 0;
                _packedSize = 0;
                _crc = Array.Empty<Byte>().CalculateCrc32().Crc;

                SetupExtraFields(_extraFields, LastWriteTimeUtc, LastAccessTimeUtc, CreationTimeUtc);

                _localHeaderInfo =
                    LocalHeaderInfo.Create(
                        LocalHeaderPosition,
                        _generalPurposeBitFlag,
                        ZipEntryCompressionMethodId.Stored,
                        _size,
                        _packedSize,
                        _crc,
                        _localHeaderExtraFields,
                        FullNameBytes,
                        LastWriteTimeUtc,
                        IsDirectory);

                _centralDirectoryHeaderInfo =
                    CentralDirectoryHeaderInfo.Create(
                        _zipFileWriter,
                        LocalHeaderPosition,
                        _generalPurposeBitFlag,
                        ZipEntryCompressionMethodId.Stored,
                        _size,
                        _packedSize,
                        _crc,
                        ExternalAttributes,
                        _centralDirectoryHeaderExtraFields,
                        FullNameBytes,
                        CommentBytes,
                        LastWriteTimeUtc,
                        IsDirectory,
                        false);

                _zipStream.Stream.WriteBytes(_localHeaderInfo.ToBytes());
                _written = true;
            }
        }

        private ISequentialOutputByteStream GetContentStreamWithoutDataDescriptor(IProgress<UInt64>? unpackedCountProgress)
        {
            var temporaryFile = (FilePath?)null;
            var packedTemporaryFile = (FilePath?)null;
            var success = false;
            try
            {
                try
                {
                    unpackedCountProgress?.Report(0);
                }
                catch (Exception)
                {
                }

                SetupExtraFields(_extraFields, LastWriteTimeUtc, LastAccessTimeUtc, CreationTimeUtc);

                var compressionMethod = CompressionMethodId.GetCompressionMethod(CompressionLevel);

                temporaryFile = new FilePath(Path.GetTempFileName());

                packedTemporaryFile =
                    CompressionMethodId == ZipEntryCompressionMethodId.Stored
                    ? null
                    : new FilePath(Path.GetTempFileName());

                var outputStrem =
                    temporaryFile.Create();

                var packedOutputStream =
                    packedTemporaryFile is null
                    ? null
                    : compressionMethod.GetEncodingStream(
                        packedTemporaryFile.Create(),
                        SafetyProgress.CreateProgress<(UInt64 unpackedCount, UInt64 packedCount), UInt64>(
                            unpackedCountProgress,
                            value => value.unpackedCount / 2));

                var tempraryFileStream =
                    (packedOutputStream is null ? outputStrem : outputStrem.Branch(packedOutputStream))
                    .WithCrc32Calculation(EndOfCopyingToTemporaryFile);
                success = true;
                return tempraryFileStream;
            }
            finally
            {
                if (!success)
                {
                    temporaryFile?.SafetyDelete();
                    packedTemporaryFile?.SafetyDelete();
                    _zipStream.UnlockStream();
                }
            }

            void EndOfCopyingToTemporaryFile(UInt32 actualCrc, UInt64 actualSize)
            {
                try
                {
                    if (temporaryFile is not null && temporaryFile.Exists)
                    {
                        if (CompressionMethodId == ZipEntryCompressionMethodId.Stored != (packedTemporaryFile is null))
                            throw new InternalLogicalErrorException();

                        var size = (UInt64)temporaryFile.Length;
                        var packedSize = (UInt64)temporaryFile.Length;
                        var crc = actualCrc;

                        if (size != actualSize)
                            throw new InternalLogicalErrorException();

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

                        _localHeaderInfo =
                            LocalHeaderInfo.Create(
                                LocalHeaderPosition,
                                _generalPurposeBitFlag,
                                CompressionMethodId,
                                size,
                                packedSize,
                                crc,
                                _localHeaderExtraFields,
                                FullNameBytes,
                                LastWriteTimeUtc,
                                IsDirectory);

                        _centralDirectoryHeaderInfo =
                            CentralDirectoryHeaderInfo.Create(
                                _zipFileWriter,
                                LocalHeaderPosition,
                                _generalPurposeBitFlag,
                                CompressionMethodId,
                                size,
                                packedSize,
                                crc,
                                ExternalAttributes,
                                _centralDirectoryHeaderExtraFields,
                                FullNameBytes,
                                CommentBytes,
                                LastWriteTimeUtc,
                                IsDirectory,
                                false);

                        _zipStream.Stream.WriteBytes(_localHeaderInfo.ToBytes());
                        using var sourceStream = (packedTemporaryFile is null ? temporaryFile : packedTemporaryFile).OpenRead();
                        sourceStream.CopyTo(
                            _zipStream.Stream,
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
                    }
                }
                finally
                {
                    temporaryFile.SafetyDelete();
                    packedTemporaryFile?.SafetyDelete();
                    _zipStream.UnlockStream();
                }
            }
        }

        private ISequentialOutputByteStream GetContentStreamWithDataDescriptor(IProgress<UInt64>? unpackedCountProgress)
        {
            var packedSizeHolder = new ValueHolder<UInt64>();
            try
            {
                try
                {
                    unpackedCountProgress?.Report(0);
                }
                catch (Exception)
                {
                }

                SetupExtraFields(_extraFields, LastWriteTimeUtc, LastAccessTimeUtc, CreationTimeUtc);

                var compressionMethod = CompressionMethodId.GetCompressionMethod(CompressionLevel);

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

                _localHeaderInfo =
                    LocalHeaderInfo.Create(
                        LocalHeaderPosition,
                        _generalPurposeBitFlag,
                        CompressionMethodId,
                        _localHeaderExtraFields,
                        FullNameBytes,
                        LastWriteTimeUtc,
                        IsDirectory);

                _zipStream.Stream.WriteBytes(_localHeaderInfo.ToBytes());
                var contentStream =
                    compressionMethod.GetEncodingStream(
                        _zipStream.Stream
                            .WithEndAction(packedSize => packedSizeHolder.Value = packedSize, true),
                        SafetyProgress.CreateProgress<(UInt64 unpackedCount, UInt64 packedCount), UInt64>(
                            unpackedCountProgress,
                            value => value.unpackedCount / 2))
                    .WithCrc32Calculation(EndOfWrintingContents);
                return contentStream;

            }
            catch (Exception)
            {
                _zipStream.UnlockStream();
                throw;
            }

            void EndOfWrintingContents(UInt32 actualCrc, UInt64 actualSize)
            {
                try
                {
                    var actualPackedSize = packedSizeHolder.Value;

                    var dataDescriptor =
                        DataDescriptorInfo.Create(actualCrc, actualSize, actualPackedSize);

                    _centralDirectoryHeaderInfo =
                        CentralDirectoryHeaderInfo.Create(
                            _zipFileWriter,
                            LocalHeaderPosition,
                            _generalPurposeBitFlag,
                            CompressionMethodId,
                            actualSize,
                            actualPackedSize,
                            actualCrc,
                            ExternalAttributes,
                            _centralDirectoryHeaderExtraFields,
                            FullNameBytes,
                            CommentBytes,
                            LastWriteTimeUtc,
                            IsDirectory,
                            true);

                    _zipStream.Stream.WriteBytes(dataDescriptor.ToBytes());
                    _size = actualSize;
                    _packedSize = actualPackedSize;
                    _crc = actualCrc;

                    try
                    {
                        unpackedCountProgress?.Report(actualSize);
                    }
                    catch (Exception)
                    {
                    }

                    _written = true;
                }
                finally
                {
                    _zipStream.UnlockStream();
                }
            }
        }

        /// <summary>
        /// エンコーディングの検証をします。
        /// </summary>
        /// <param name="encoding">
        /// 検証対象のエンコーディングです。
        /// </param>
        /// <param name="fullName">
        /// 検証のために使用するエントリ名の文字列です。
        /// </param>
        /// <param name="fullNameBytes">
        /// 検証のために使用するエントリ名のバイト列です。
        /// </param>
        /// <param name="comment">
        /// 検証のために使用するコメントの文字列です。
        /// </param>
        /// <param name="commentBytes">
        /// 検証のために使用するコメントのバイト列です。
        /// </param>
        /// <returns>
        /// 検証が成功した場合は true、そうではない場合は false を返します。
        /// </returns>
        /// <remarks>
        /// <para>
        /// このメソッドは、以下の条件で true を返します。
        /// </para>
        /// <list type="number">
        /// <item> <paramref name="encoding"/> により <paramref name="fullName"/> をエンコードした結果が <paramref name="fullNameBytes"/> に等しく、かつ </item>
        /// <item> <paramref name="encoding"/> により <paramref name="fullNameBytes"/> をデコードした結果が <paramref name="fullName"/> に等しく、かつ </item>
        /// <item> <paramref name="encoding"/> により <paramref name="comment"/> をエンコードした結果が <paramref name="commentBytes"/> に等しく、かつ </item>
        /// <item> <paramref name="encoding"/> により <paramref name="commentBytes"/> をデコードした結果が <paramref name="comment"/> に等しい場合</item>
        /// </list>
        /// <para>
        /// エンコーディングが正しくても、<paramref name="fullNameBytes"/> または <paramref name="commentBytes"/> に UNICODE にマッピングできない文字が含まれている場合には
        /// 上記の条件は成立しないことに注意してください。
        /// </para>
        /// </remarks>
        private static Boolean ValidateEncoding(Encoding encoding, String fullName, ReadOnlySpan<Byte> fullNameBytes, String comment, ReadOnlySpan<Byte> commentBytes)
        {
            // このメソッドは、エンコーディングが正しくない場合以外にも、
            // fullNameBytes または commentBytes に UNICODE にマッピングできない文字が含まれている場合にも false を返すことに注意。
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

        private static void SetupExtraFields(ExtraFieldCollection extraFields, DateTime? lastWriteTimeUtc, DateTime? lastAccessTimeUtc, DateTime? creationTimeUtc)
        {
            //
            // 高精度日時の何れかが指定されている場合は、拡張フィールドに設定する
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
        }

        private static UInt16 GetVersionNeededToExtract(ZipEntryCompressionMethodId compressionMethodId, Boolean isDirectory, Boolean requiredZip64)
        {
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
                        requiredZip64 ? (UInt16)45 : (UInt16)0, // version if using zip 64 extensions
                }
                .Max();
            return versionNeededTiExtract;
        }
        private static (UInt16 dosDate, UInt16 dosTime) GetDosDateTime(DateTime? lastWriteTimeUtc)
        {
            try
            {
                return (lastWriteTimeUtc ?? DateTime.UtcNow).FromDateTimeToDosDateTime(DateTimeKind.Local);
            }
            catch (Exception)
            {
                return (0, 0);
            }
        }
    }
}
