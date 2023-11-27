using System;
using Utility;

namespace ZipUtility.ZipExtraField
{
    /// <summary>
    /// すべての拡張フィールドの基底クラスです。
    /// </summary>
    public abstract class ExtraField
        : IExtraField
    {
        private readonly UInt16 _extraFieldId;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="extraFieldId">
        /// 拡張フィールドの ID です。
        /// </param>
        protected ExtraField(UInt16 extraFieldId)
        {
            _extraFieldId = extraFieldId;
        }

        UInt16 IExtraField.ExtraFieldId => _extraFieldId;

        /// <summary>
        /// 拡張フィールドの内容がシリアライズされたバイト列を取得します。
        /// </summary>
        /// <param name="headerType">
        /// ZIP のヘッダの種類 (ローカルヘッダあるいはセントラルディレクトリヘッダ) を示す値です。
        /// </param>
        /// <returns>
        /// <paramref name="headerType"/> に格納される拡張フィールドの内容がシリアライズされたバイト列です。
        /// もし拡張フィールドに有効なデータがない場合は null が返ります。
        /// </returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <term>[このメソッドを実装する場合の注意点]</term>
        /// <description>
        /// <list type="bullet">
        /// <item>
        /// 以下の何れにも該当しない場合は、<paramref name="headerType"/> で示されるヘッダに設定すべきバイト列を復帰値として返してください。
        /// 以下の何れかの該当する場合には null を返してください。
        /// <list type="bullet">
        /// <item>拡張フィールドとして出力するための有効なデータが存在しない、または不足している。</item>
        /// <item><paramref name="headerType"/> で示されるヘッダにこの拡張フィールドを出力してはならない。</item>
        /// </list>
        /// </item>
        /// </list>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        public abstract ReadOnlyMemory<Byte>? GetData(ZipEntryHeaderType headerType);

        /// <summary>
        /// 与えられたバイト列を解析して拡張フィールドに値を設定します。
        /// </summary>
        /// <param name="headerType">
        /// ZIP のヘッダの種類 (ローカルヘッダあるいはセントラルディレクトリヘッダ) を示す値です。
        /// </param>
        /// <param name="data">
        /// 解析対象のバイト列です。
        /// </param>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <term>実装する場合の注意点</term>
        /// <description>
        /// <para>
        /// 必ず <paramref name="headerType"/> に従って <paramref name="data"/> を解析してください。
        /// </para>
        /// <para>
        /// もし ZIPファイルの破損が疑われる状況である場合には、<see cref="GetBadFormatException(ZipEntryHeaderType, ReadOnlyMemory{Byte})"/> メソッドをを呼び出して取得した例外をスローしてください。
        /// </para>
        /// <para>
        /// このメソッドに与えるパラメタは、<see cref="SetData(ZipEntryHeaderType, ReadOnlyMemory{Byte})"/> で与えられた <paramref name="headerType"/> と <paramref name="data"/> です。
        /// </para>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        public abstract void SetData(ZipEntryHeaderType headerType, ReadOnlyMemory<Byte> data);

        /// <summary>
        /// 拡張フィールドの解析中に続行不可能なエラーが発生した場合にスローする例外オブジェクトを取得します。
        /// </summary>
        /// <param name="headerType">
        /// ZIP のヘッダの種類 (ローカルヘッダあるいはセントラルディレクトリヘッダ) を示す値です。
        /// </param>
        /// <param name="data">
        /// 解析対象のバイト列です。
        /// </param>
        /// <returns>
        /// スローする例外オブジェクトです。
        /// </returns>
        protected Exception GetBadFormatException(ZipEntryHeaderType headerType, ReadOnlyMemory<Byte> data)
            => new BadZipFileFormatException($"Bad extra field: header={headerType}, type=0x{_extraFieldId:x4}, data=\"{data.ToFriendlyString()}\"");

        /// <summary>
        /// 拡張フィールドの解析中に続行不可能なエラーが発生した場合にスローする例外オブジェクトを取得します。
        /// </summary>
        /// <param name="headerType">
        /// ZIP のヘッダの種類 (ローカルヘッダあるいはセントラルディレクトリヘッダ) を示す値です。
        /// </param>
        /// <param name="data">
        /// 解析対象のバイト列です。
        /// </param>
        /// <param name="innerException">
        /// 現在の例外の原因である例外のオブジェクトです。
        /// </param>
        /// <returns>
        /// スローする例外オブジェクトです。
        /// </returns>
        protected Exception GetBadFormatException(ZipEntryHeaderType headerType, ReadOnlyMemory<Byte> data, Exception innerException)
            => new BadZipFileFormatException($"Bad extra field: header={headerType}, type=0x{_extraFieldId:x4}, data=\"{data.ToFriendlyString()}\"", innerException);
    }
}
