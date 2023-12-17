using System;

namespace ZipUtility.ExtraFields
{
    /// <summary>
    /// Info-ZIP Unicode Path Extra Field の拡張フィールドのクラスです。
    /// </summary>
    public class UnicodePathExtraField
        : UnicodeStringExtraField
    {
        /// <summary>
        /// デフォルトコンストラクタです。
        /// </summary>
        public UnicodePathExtraField()
            : base(ExtraFieldId)
        {
        }

        /// <summary>
        /// 拡張フィールドの ID です。
        /// </summary>
        public const UInt16 ExtraFieldId = 0x7075;

        /// <summary>
        /// 拡張フィールドに格納されているエントリ名のバイト列を UTF-8 エンコーディングでデコードします。
        /// </summary>
        /// <param name="rawFullNameBytes">
        /// エントリに格納されているエントリ名の生のバイト列です。
        /// </param>
        /// <returns>
        /// <paramref name="rawFullNameBytes"/> から計算された CRC 値と拡張フィールドに格納されている CRC 値が等しい場合は、
        /// 拡張フィールドに格納されているバイト列が UTF-8 エンコーディングでデコードされ、その文字列が返ります。
        /// もし CRC 値が一致しなかった場合は null が返ります。
        /// </returns>
        public String? GetFullName(ReadOnlySpan<Byte> rawFullNameBytes)
            => GetUnicodeString(rawFullNameBytes);

        /// <summary>
        /// 指定されたエントリ名の文字列を UTF-8 エンコーディングでエンコードして、拡張フィールドに格納します。
        /// </summary>
        /// <param name="fullName">
        /// 拡張フィールドに格納するエントリ名の文字列です。
        /// </param>
        /// <param name="rawFullNameBytes">
        /// エントリに格納されているエントリ名の生のバイト列です。
        /// </param>
        /// <remarks>
        /// <list type="bullet">
        /// <item><paramref name="rawFullNameBytes"/> で指定されたバイト列の CRC が計算され、その CRC 値も拡張フィールドに格納されます。</item>
        /// </list>
        /// </remarks>
        public void SetFullName(String fullName, ReadOnlySpan<Byte> rawFullNameBytes)
            => SetUnicodeString(fullName, rawFullNameBytes);

        /// <summary>
        /// サポートされている拡張フィールドのバージョンを取得します。
        /// </summary>
        protected override Byte SupportedVersion => 1;
    }
}
