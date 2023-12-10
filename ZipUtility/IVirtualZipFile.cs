using System;

namespace ZipUtility
{
    /// <summary>
    /// 複数の物理的なファイルを一つの仮想的なファイルとみなしてファイル情報をアクセスするインターフェースです。
    /// </summary>
    internal interface IVirtualZipFile
        : IEquatable<IVirtualZipFile>
    {
        /// <summary>
        /// 仮想的なファイル上の指定された位置から指定されたオフセットだけ前方に相当する物理的なファイルの情報を取得します。
        /// </summary>
        /// <param name="position">
        /// 仮想的なファイル上の位置を示す <see cref="ZipStreamPosition"/> 値です。
        /// </param>
        /// <param name="offset">
        /// 仮想的なファイル上のオフセットを示す <see cref="UInt64"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="position"/> + <paramref name="offset"/> の位置に相当する <see cref="ZipStreamPosition"/> オブジェクトを返します。
        /// 該当する位置が存在しない場合はnullを返します。
        /// </returns>
        ZipStreamPosition Add(ZipStreamPosition position, UInt64 offset);

        /// <summary>
        /// 仮想的なファイル上の指定された位置から指定されたオフセットだけ後方に相当する物理的なファイルの情報を取得します。
        /// </summary>
        /// <param name="position">
        /// 仮想的なファイル上の位置を示す <see cref="ZipStreamPosition"/> 値です。
        /// </param>
        /// <param name="offset">
        /// 仮想的なファイル上のオフセットを示す <see cref="UInt64"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="position"/> - <paramref name="offset"/> の位置に相当する <see cref="ZipStreamPosition"/> オブジェクトを返します。
        /// </returns>
        ZipStreamPosition Subtract(ZipStreamPosition position, UInt64 offset);

        /// <summary>
        /// 仮想的なファイル上の二つの位置の間の距離を取得します。
        /// </summary>
        /// <param name="position1">
        /// 仮想的なファイル上の位置を示す <see cref="ZipStreamPosition"/> 値です。
        /// </param>
        /// <param name="position2">
        /// 仮想的なファイル上の位置を示す <see cref="ZipStreamPosition"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="position1"/> と<paramref name="position2"/> の差を示す <see cref="UInt64"/> 値です。
        /// ( == <paramref name="position2"/> - <paramref name="position1"/>)
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="position1"/> が<paramref name="position2"/> より後方にあります。
        /// ( <paramref name="position1"/> &lt; <paramref name="position2"/> )
        /// </exception>
        /// <remarks>
        /// このメソッドを呼び出すと複数の物理ファイルの検索が発生するため、パフォーマンスが低下する可能性があることに留意してください。
        /// 特に、物理ファイルの数が多い場合や、物理ファイルが別々のリムーバブルメディアに格納されている場合は、それが顕著になります。
        /// </remarks>
        UInt64 Subtract(ZipStreamPosition position1, ZipStreamPosition position2);

        /// <summary>
        /// 仮想的なファイル上の二つの位置を等値比較します。
        /// </summary>
        /// <param name="position1">
        /// 仮想的なファイル上の位置を示す <see cref="ZipStreamPosition"/> 値です。
        /// </param>
        /// <param name="position2">
        /// 仮想的なファイル上の位置を示す <see cref="ZipStreamPosition"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="position1"/> と <paramref name="position2"/> が等しい場合は true、そうではない場合は false を返します。
        /// </returns>
        Boolean Equal(ZipStreamPosition position1, ZipStreamPosition position2);

        /// <summary>
        /// 仮想的なファイル上の位置を示す値のハッシュコードを取得します。
        /// </summary>
        /// <param name="position"></param>
        /// 仮想的なファイル上の位置を示す <see cref="ZipStreamPosition"/> 値です。
        /// <returns>
        /// <paramref name="position"/> のハッシュコードを返します。
        /// </returns>
        Int32 GetHashCode(ZipStreamPosition position);

        /// <summary>
        /// 仮想的なファイル上の二つの位置を大小比較します。
        /// </summary>
        /// <param name="position1">
        /// 仮想的なファイル上の位置を示す <see cref="ZipStreamPosition"/> 値です。
        /// </param>
        /// <param name="position2">
        /// 仮想的なファイル上の位置を示す <see cref="ZipStreamPosition"/> 値です。
        /// </param>
        /// <returns>
        /// 以下の値を返します。
        /// <list type="bullet">
        /// <item>
        /// <term><paramref name="position1"/> が <paramref name="position2"/> より大きい場合 </term>
        /// <description>正の <see cref="Int32"/> 値</description>
        /// </item>
        /// <item>
        /// <term><paramref name="position1"/> が <paramref name="position2"/> と等しい場合 </term>
        /// <description>0</description>
        /// </item>
        /// <item>
        /// <term><paramref name="position1"/> が <paramref name="position2"/> より小さい場合 </term>
        /// <description>負の <see cref="Int32"/> 値</description>
        /// </item>
        /// </list>
        /// </returns>
        Int32 Compare(ZipStreamPosition position1, ZipStreamPosition position2);
    }
}
