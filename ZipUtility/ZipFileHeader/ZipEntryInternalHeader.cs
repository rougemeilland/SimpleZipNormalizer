using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Utility;
using Utility.Text;
using ZipUtility.ZipExtraField;

namespace ZipUtility.ZipFileHeader
{
    internal abstract class ZipEntryInternalHeader
    {
        private static readonly Encoding _utf8EncodingWithoutBOM;

        static ZipEntryInternalHeader()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _utf8EncodingWithoutBOM = Encoding.UTF8.WithFallback(null, null).WithoutPreamble();
        }

        protected ZipEntryInternalHeader(
            IZipEntryNameEncodingProvider zipEntryNameEncodingProvider,
            ZipStreamPosition localHeaderPosition,
            UInt16 versionNeededToExtract,
            ZipEntryGeneralPurposeBitFlag generalPurposeBitFlag,
            ZipEntryCompressionMethodId compressionMethodId,
            DateTime? dosDateTime,
            UInt32 rawCrc,
            UInt32 rawPackedSize,
            UInt32 rawSize,
            UInt64 packedSize,
            UInt64 size,
            ReadOnlyMemory<Byte> fullNameBytes,
            ReadOnlyMemory<Byte> commentBytes,
            ExtraFieldStorage extraFields,
            Boolean requiredZip64)
        {
            LocalHeaderPosition = localHeaderPosition;
            VersionNeededToExtract = versionNeededToExtract;
            GeneralPurposeBitFlag = generalPurposeBitFlag;
            CompressionMethodId = compressionMethodId;
            DosDateTime = dosDateTime is not null ? (dosDateTime.Value, TimeSpan.FromSeconds(2)) : null;
            RawCrc = rawCrc;
            RawPackedSize = rawPackedSize;
            RawSize = rawSize;
            PackedSize = packedSize;
            Size = size;
            FullName = "";
            FullNameBytes = fullNameBytes;
            Comment = "";
            CommentBytes = commentBytes;
            ExtraFields = extraFields;
            RequiredZip64 = requiredZip64;
            LastWriteTimeUtc = null;
            LastAccessTimeUtc = null;
            CreationTimeUtc = null;
            ExactEntryEncoding = null;
            PossibleEntryEncodings = Array.Empty<Encoding>();

            #region タイムスタンプを設定する

            //
            // タイムスタンプを設定する
            //

            // 拡張フィールドの優先順位は以下の通り
            // 1) まず、日時の最小単位 (precition) が小さいものを優先
            // 2) 次に、以下の配列で先に記述されているものを優先
            var timeStampExtraFields = new[]
            {
                ExtraFields.GetExtraField<NtfsExtraField>() as ITimestampExtraField,
                ExtraFields.GetExtraField<ExtendedTimestampExtraField>(),
                ExtraFields.GetExtraField<UnixExtraFieldType1>(),
                ExtraFields.GetExtraField<UnixExtraFieldType0>(),
            };
            foreach (var timeStampExtraField in timeStampExtraFields)
            {
                if (timeStampExtraField is not null)
                {
                    if (timeStampExtraField.LastWriteTimeUtc is not null && (LastWriteTimeUtc is null || LastWriteTimeUtc.Value.precition > timeStampExtraField.DateTimePrecision))
                        LastWriteTimeUtc = (timeStampExtraField.LastWriteTimeUtc.Value, timeStampExtraField.DateTimePrecision);

                    if (timeStampExtraField.LastAccessTimeUtc is not null && (LastAccessTimeUtc is null || LastAccessTimeUtc.Value.precition > timeStampExtraField.DateTimePrecision))
                        LastAccessTimeUtc = (timeStampExtraField.LastAccessTimeUtc.Value, timeStampExtraField.DateTimePrecision);

                    if (timeStampExtraField.CreationTimeUtc is not null && (CreationTimeUtc is null || CreationTimeUtc.Value.precition > timeStampExtraField.DateTimePrecision))
                        CreationTimeUtc = (timeStampExtraField.CreationTimeUtc.Value, timeStampExtraField.DateTimePrecision);
                }
            }

            #endregion

            #region エントリ名とコメントを設定する

            //
            // エントリ名とコメントを設定する
            //

            var encodingIsKnown = false;

            // 1. 汎用フラグで UTF-8 であることの指定がされているかどうかをチェックする
            if ((GeneralPurposeBitFlag & ZipEntryGeneralPurposeBitFlag.UseUnicodeEncodingForNameAndComment) != ZipEntryGeneralPurposeBitFlag.None)
            {
                ExactEntryEncoding = _utf8EncodingWithoutBOM;
                PossibleEntryEncodings = Array.Empty<Encoding>();
                FullName = _utf8EncodingWithoutBOM.GetString(fullNameBytes);
                Comment = _utf8EncodingWithoutBOM.GetString(commentBytes);
                encodingIsKnown = true;
            }

            // 2. エンコーディングが未解決であれば、拡張フィールド CodePageExtraField の参照を試みる
            if (!encodingIsKnown)
            {
                if (TryResolveEncodingByCodePadeExtraField(ExtraFields, zipEntryNameEncodingProvider, fullNameBytes, commentBytes, out var fullName, out var comment, out var originalEncoding))
                {
                    ExactEntryEncoding = originalEncoding;
                    PossibleEntryEncodings = Array.Empty<Encoding>();
                    FullName = fullName;
                    Comment = comment;
                    encodingIsKnown = true;
                }
            }

            // 3. エンコーディングが未解決であれば、拡張フィールド XceedUnicodeExtraField の参照を試みる
            if (!encodingIsKnown)
            {
                if (TryResolveEncodingByXceedUnicodeExtraField(ExtraFields, zipEntryNameEncodingProvider, fullNameBytes, commentBytes, out var fullName, out var comment, out var originalEncodings))
                {
                    ExactEntryEncoding = null;
                    PossibleEntryEncodings = originalEncodings.ToList();
                    FullName = fullName;
                    Comment = comment;
                    encodingIsKnown = true;
                }
            }

            // 4. エンコーディングが未解決であれば、拡張フィールド UnicodePathExtraField および UnicodeCommentExtraField の参照を試みる
            if (!encodingIsKnown)
            {
                if (TryResolveEncodingByUnicodePathCommentExtraField(ExtraFields, zipEntryNameEncodingProvider, fullNameBytes, commentBytes, out var fullName, out var comment, out var originalEncodings))
                {
                    ExactEntryEncoding = null;
                    PossibleEntryEncodings = originalEncodings.ToList();
                    FullName = fullName;
                    Comment = comment;
                    encodingIsKnown = true;
                }
            }

            // 5. エンコーディングが未解決であれば、適用可能なエンコーディングを探す
            if (!encodingIsKnown)
            {
                var originalEncodings = zipEntryNameEncodingProvider.GetBestEncodings(fullNameBytes, null, commentBytes, null).ToList();
                var bestEncoding = originalEncodings.FirstOrDefault();
                if (bestEncoding is not null)
                {
                    ExactEntryEncoding = null;
                    PossibleEntryEncodings = originalEncodings.ToList();
                    FullName = bestEncoding.GetString(fullNameBytes);
                    Comment = bestEncoding.GetString(commentBytes);
                    encodingIsKnown = true;
                }
            }

            // 6. 最後に、エンコーディングが未解決であれば、最低限の設定を行う
            if (!encodingIsKnown)
            {
                ExactEntryEncoding = null;
                PossibleEntryEncodings = Array.Empty<Encoding>();
                FullName = fullNameBytes.GetStringByUnknownDecoding();
                Comment = commentBytes.GetStringByUnknownDecoding();
            }

            #endregion
        }

        public ZipStreamPosition LocalHeaderPosition { get; }
        public UInt16 VersionNeededToExtract { get; }
        public ZipEntryGeneralPurposeBitFlag GeneralPurposeBitFlag { get; }
        public ZipEntryCompressionMethodId CompressionMethodId { get; }
        public (DateTime dateTime, TimeSpan precition)? DosDateTime { get; }
        public UInt32 RawCrc { get; }
        public abstract UInt32 Crc { get; }
        public UInt32 RawPackedSize { get; }
        public UInt32 RawSize { get; }
        public UInt64 PackedSize { get; }
        public UInt64 Size { get; }
        public ReadOnlyMemory<Byte> FullNameBytes { get; }
        public String FullName { get; }
        public virtual ReadOnlyMemory<Byte> CommentBytes { get; }
        public virtual String Comment { get; }
        public ExtraFieldStorage ExtraFields { get; }
        public Boolean RequiredZip64 { get; }
        public Encoding? ExactEntryEncoding { get; }
        public IEnumerable<Encoding> PossibleEntryEncodings { get; }
        public (DateTime dateTime, TimeSpan precition)? LastWriteTimeUtc { get; }
        public (DateTime dateTime, TimeSpan precition)? LastAccessTimeUtc { get; }
        public (DateTime dateTime, TimeSpan precition)? CreationTimeUtc { get; }

        private static Boolean TryResolveEncodingByCodePadeExtraField(ExtraFieldStorage extraFields, IZipEntryNameEncodingProvider zipEntryNameEncodingProvider, ReadOnlyMemory<Byte> fullNameBytes, ReadOnlyMemory<Byte> commentBytes, [MaybeNullWhen(false)] out String fullName, [MaybeNullWhen(false)] out String comment, [MaybeNullWhen(false)] out Encoding originalEncoding)
        {
            var extraField = extraFields.GetExtraField<CodePageExtraField>();
            if (extraField is null)
            {
                fullName = null;
                comment = null;
                originalEncoding = null;
                return false;
            }

            var encoding =
                zipEntryNameEncodingProvider.SupportedEncodings
                .Where(encoding => encoding.CodePage == extraField.CodePage)
                .FirstOrDefault();

            if (encoding is null)
            {
                fullName = null;
                comment = null;
                originalEncoding = null;
                return false;
            }

            // 拡張フィールドに指定されたコードページによっては UNICODE にマッピングできない文字を含むことがあるので、絶対に文字化けしないというわけではないことに注意。
            fullName = encoding.GetString(fullNameBytes);
            comment = encoding.GetString(commentBytes);
            originalEncoding = encoding;
            return true;
        }

        private static Boolean TryResolveEncodingByXceedUnicodeExtraField(ExtraFieldStorage extraFields, IZipEntryNameEncodingProvider zipEntryNameEncodingProvider, ReadOnlyMemory<Byte> fullNameBytes, ReadOnlyMemory<Byte> commentBytes, [MaybeNullWhen(false)] out String fullName, [MaybeNullWhen(false)] out String comment, out IEnumerable<Encoding> originalEncodings)
        {
            var xceedUnicodeExtraField = extraFields.GetExtraField<XceedUnicodeExtraField>();
            var extraFieldfullName = xceedUnicodeExtraField?.FullName;
            var extraFieldComment = xceedUnicodeExtraField?.Comment;
            if (extraFieldfullName is null || extraFieldComment is null)
            {
                fullName = null;
                comment = null;
                originalEncodings = Array.Empty<Encoding>();
                return false;
            }

            fullName = extraFieldfullName;
            comment = extraFieldComment;
            originalEncodings = zipEntryNameEncodingProvider.GetBestEncodings(fullNameBytes, extraFieldfullName, commentBytes, extraFieldComment).ToList();
            return true;
        }

        private static Boolean TryResolveEncodingByUnicodePathCommentExtraField(ExtraFieldStorage extraFields, IZipEntryNameEncodingProvider zipEntryNameEncodingProvider, ReadOnlyMemory<Byte> fullNameBytes, ReadOnlyMemory<Byte> commentBytes, [MaybeNullWhen(false)] out String fullName, [MaybeNullWhen(false)] out String comment, out IEnumerable<Encoding> originalEncodings)
        {
            var unicodePathExtraField = extraFields.GetExtraField<UnicodePathExtraField>();
            var unicodeCommentExtraField = extraFields.GetExtraField<UnicodeCommentExtraField>();
            var extrafieldFullName = unicodePathExtraField?.GetFullName(fullNameBytes.Span);
            var extraFieldComment = unicodeCommentExtraField?.GetComment(commentBytes.Span);
            if (extrafieldFullName is null && extraFieldComment is null)
            {
                fullName = null;
                comment = null;
                originalEncodings = Array.Empty<Encoding>();
                return false;
            }

            originalEncodings =
                zipEntryNameEncodingProvider.GetBestEncodings(
                    fullNameBytes,
                    extrafieldFullName,
                    commentBytes,
                    extraFieldComment)
                .ToList();

            fullName = extrafieldFullName ?? originalEncodings.FirstOrDefault()?.GetString(fullNameBytes) ?? fullNameBytes.GetStringByUnknownDecoding();
            comment = extraFieldComment ?? originalEncodings.FirstOrDefault()?.GetString(commentBytes) ?? commentBytes.GetStringByUnknownDecoding();
            return true;
        }
    }
}
