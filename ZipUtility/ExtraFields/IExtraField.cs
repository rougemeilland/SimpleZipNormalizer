using System;

namespace ZipUtility.ExtraFields
{
    /// <summary>
    /// 拡張フィールドにアクセス可能なインターフェースです。
    /// </summary>
    public interface IExtraField
    {
        /// <summary>
        /// 拡張フィールドの ID を示す <see cref="UInt16"/> 値を取得します。
        /// </summary>
        UInt16 ExtraFieldId { get; }

        /// <summary>
        /// 拡張フィールドのバイト配列を構築します。
        /// </summary>
        /// <param name="headerType">
        /// 構築されたバイト配列が格納されるヘッダの種類を示す列挙体です。
        /// </param>
        /// <param name="parameter">
        /// バイト配列の構築の際の参照可能なパラメタです。
        /// </param>
        /// <returns>
        /// <para>
        /// もし null ではない場合は、それは構築されたバイト配列です。
        /// </para>
        /// <para>
        /// もし null であれば、拡張フィールドを構築できなかったことを意味します。その原因は主に以下のようなものがあります。
        /// <list type="bullet">
        /// <item>拡張フィールドの構築のために必要な情報が不足している。</item>
        /// <item>この拡張フィールドは <paramref name="headerType"/> で示されているヘッダに格納されるべきではない。</item>
        /// </list>
        /// </para>
        /// </returns>
        ReadOnlyMemory<Byte>? GetData(ZipEntryHeaderType headerType, IExtraFieldEncodingParameter parameter);

        /// <summary>
        /// バイト配列から拡張フィールドを解析します。
        /// </summary>
        /// <param name="headerType">
        /// バイト配列の提供元であるヘッダを示す列挙体です。
        /// </param>
        /// <param name="data">
        /// 解析対象のバイト配列です。
        /// </param>
        /// <param name="parameter">
        /// バイト配列の解析の際に参照可能なパラメタです。
        /// </param>
        void SetData(ZipEntryHeaderType headerType, ReadOnlyMemory<Byte> data, IExtraFieldDecodingParameter parameter);
    }
}
