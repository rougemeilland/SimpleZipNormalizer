using System;
using Utility;

namespace ZipUtility.ExtraFields
{
    /// <summary>
    /// Info-ZIP Unicode Comment Extra Field の拡張フィールドのクラスです。
    /// </summary>
    public class UnicodeCommentExtraField
        : UnicodeStringExtraField
    {
        /// <summary>
        /// デフォルトコンストラクタです。
        /// </summary>
        public UnicodeCommentExtraField()
            : base(ExtraFieldId)
        {
        }

        /// <summary>
        /// 拡張フィールドの ID です。
        /// </summary>
        public const UInt16 ExtraFieldId = 0x6375;

        /// <inheritdoc/>
        public override ReadOnlyMemory<Byte>? GetData(ZipEntryHeaderType headerType, IExtraFieldEncodingParameter parameter)
            => headerType switch
            {
                ZipEntryHeaderType.LocalHeader => null,
                ZipEntryHeaderType.CentralDirectoryHeader => base.GetData(headerType, parameter),
                _ => throw new InternalLogicalErrorException($"Unknown header type: {nameof(headerType)}={headerType}"),
            };

        /// <inheritdoc/>
        public override void SetData(ZipEntryHeaderType headerType, ReadOnlyMemory<Byte> data, IExtraFieldDecodingParameter parameter)
        {
            switch (headerType)
            {
                case ZipEntryHeaderType.LocalHeader:
                    break;
                case ZipEntryHeaderType.CentralDirectoryHeader:
                    base.SetData(headerType, data, parameter);
                    break;
                default:
                    throw new InternalLogicalErrorException($"Unknown header type: {nameof(headerType)}={headerType}");
            }
        }

        /// <summary>
        /// 拡張フィールドに格納されているエントリのコメントのバイト列を UTF-8 エンコーディングでデコードします。
        /// </summary>
        /// <param name="rawCommentBytes">
        /// エントリに格納されているエントリのコメントの生のバイト列です。
        /// </param>
        /// <returns>
        /// <paramref name="rawCommentBytes"/> から計算された CRC 値と拡張フィールドに格納されている CRC 値が等しい場合は、
        /// 拡張フィールドに格納されているバイト列が UTF-8 エンコーディングでデコードされ、その文字列が返ります。
        /// もし CRC 値が一致しなかった場合は null が返ります。
        /// </returns>
        public String? GetComment(ReadOnlySpan<Byte> rawCommentBytes)
            => GetUnicodeString(rawCommentBytes);

        /// <summary>
        /// 指定されたエントリのコメントの文字列を UTF-8 エンコーディングでエンコードして、拡張フィールドに格納します。
        /// </summary>
        /// <param name="comment">
        /// 拡張フィールドに格納するエントリのコメントの文字列です。
        /// </param>
        /// <param name="rawCommentBytes">
        /// エントリに格納されているエントリのコメントの生のバイト列です。
        /// </param>
        /// <remarks>
        /// <list type="bullet">
        /// <item><paramref name="rawCommentBytes"/> で指定されたバイト列の CRC が計算され、その CRC 値も拡張フィールドに格納されます。</item>
        /// </list>
        /// </remarks>
        public void SetComment(String comment, ReadOnlySpan<Byte> rawCommentBytes)
            => SetUnicodeString(comment, rawCommentBytes);

        /// <summary>
        /// サポートされている拡張フィールドのバージョンを取得します。
        /// </summary>
        protected override Byte SupportedVersion => 1;
    }
}
