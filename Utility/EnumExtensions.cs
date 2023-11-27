using System;

namespace Utility
{
    public static class EnumExtensions
    {
        /// <summary>
        /// 指定された値が、別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// 値の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="value1">
        /// 1番目の比較対象の値です。
        /// </param>
        /// <param name="value2">
        /// 2番目の比較対象の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/> の何れとも等しくない場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードと等価です。
        /// <code>
        /// !<paramref name="value"/>.Equals(<paramref name="value1"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value2"/>)
        /// </code>
        /// </para>
        /// </returns>
        public static Boolean IsNoneOf<ELEMENT_T>(this ELEMENT_T value, ELEMENT_T value1, ELEMENT_T value2)
            where ELEMENT_T : Enum
            => value is null
                ? value1 is not null && value2 is not null
                : !value.Equals(value1) && !value.Equals(value2);

        /// <summary>
        /// 指定された値が、別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// 値の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="value1">
        /// 1番目の比較対象の値です。
        /// </param>
        /// <param name="value2">
        /// 2番目の比較対象の値です。
        /// </param>
        /// <param name="value3">
        /// 3番目の比較対象の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/> の何れとも等しくない場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードと等価です。
        /// <code>
        /// !<paramref name="value"/>.Equals(<paramref name="value1"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value2"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value3"/>)
        /// </code>
        /// </para>
        /// </returns>
        public static Boolean IsNoneOf<ELEMENT_T>(this ELEMENT_T value, ELEMENT_T value1, ELEMENT_T value2, ELEMENT_T value3)
            where ELEMENT_T : Enum
            => value is null
                ? value1 is not null && value2 is not null && value3 is not null
                : !value.Equals(value1) && !value.Equals(value2) && !value.Equals(value3);

        /// <summary>
        /// 指定された値が、別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// 値の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="value1">
        /// 1番目の比較対象の値です。
        /// </param>
        /// <param name="value2">
        /// 2番目の比較対象の値です。
        /// </param>
        /// <param name="value3">
        /// 3番目の比較対象の値です。
        /// </param>
        /// <param name="value4">
        /// 4番目の比較対象の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/> の何れとも等しくない場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードと等価です。
        /// <code>
        /// !<paramref name="value"/>.Equals(<paramref name="value1"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value2"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value3"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value4"/>)
        /// </code>
        /// </para>
        /// </returns>
        public static Boolean IsNoneOf<ELEMENT_T>(this ELEMENT_T value, ELEMENT_T value1, ELEMENT_T value2, ELEMENT_T value3, ELEMENT_T value4)
            where ELEMENT_T
            : Enum => value is null
                ? value1 is not null && value2 is not null && value3 is not null && value4 is not null
                : !value.Equals(value1) && !value.Equals(value2) && !value.Equals(value3) && !value.Equals(value4);

        /// <summary>
        /// 指定された値が、別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// 値の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="value1">
        /// 1番目の比較対象の値です。
        /// </param>
        /// <param name="value2">
        /// 2番目の比較対象の値です。
        /// </param>
        /// <param name="value3">
        /// 3番目の比較対象の値です。
        /// </param>
        /// <param name="value4">
        /// 4番目の比較対象の値です。
        /// </param>
        /// <param name="value5">
        /// 5番目の比較対象の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/>, <paramref name="value5"/> の何れとも等しくない場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードと等価です。
        /// <code>
        /// !<paramref name="value"/>.Equals(<paramref name="value1"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value2"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value3"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value4"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value5"/>)
        /// </code>
        /// </para>
        /// </returns>
        public static Boolean IsNoneOf<ELEMENT_T>(this ELEMENT_T value, ELEMENT_T value1, ELEMENT_T value2, ELEMENT_T value3, ELEMENT_T value4, ELEMENT_T value5)
            where ELEMENT_T : Enum
            => value is null
                ? value1 is not null && value2 is not null && value3 is not null && value4 is not null && value5 is not null
                : !value.Equals(value1) && !value.Equals(value2) && !value.Equals(value3) && !value.Equals(value4) && !value.Equals(value5);

        /// <summary>
        /// 指定された値が、別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// 値の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="value1">
        /// 1番目の比較対象の値です。
        /// </param>
        /// <param name="value2">
        /// 2番目の比較対象の値です。
        /// </param>
        /// <param name="value3">
        /// 3番目の比較対象の値です。
        /// </param>
        /// <param name="value4">
        /// 4番目の比較対象の値です。
        /// </param>
        /// <param name="value5">
        /// 5番目の比較対象の値です。
        /// </param>
        /// <param name="value6">
        /// 6番目の比較対象の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/>, <paramref name="value5"/>, <paramref name="value6"/> の何れとも等しくない場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードと等価です。
        /// <code>
        /// !<paramref name="value"/>.Equals(<paramref name="value1"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value2"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value3"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value4"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value5"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value6"/>)
        /// </code>
        /// </para>
        /// </returns>
        public static Boolean IsNoneOf<ELEMENT_T>(this ELEMENT_T value, ELEMENT_T value1, ELEMENT_T value2, ELEMENT_T value3, ELEMENT_T value4, ELEMENT_T value5, ELEMENT_T value6)
            where ELEMENT_T : Enum
            => value is null
                ? value1 is not null && value2 is not null && value3 is not null && value4 is not null && value5 is not null && value6 is not null
                : !value.Equals(value1) && !value.Equals(value2) && !value.Equals(value3) && !value.Equals(value4) && !value.Equals(value5) && !value.Equals(value6);

        /// <summary>
        /// 指定された値が、別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// 値の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="value1">
        /// 1番目の比較対象の値です。
        /// </param>
        /// <param name="value2">
        /// 2番目の比較対象の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/> の何れかと等しい場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードと等価です。
        /// <code>
        /// <paramref name="value"/>.Equals(<paramref name="value1"/>) || <paramref name="value"/>.Equals(<paramref name="value2"/>)
        /// </code>
        /// </para>
        /// </returns>
        public static Boolean IsAnyOf<ELEMENT_T>(this ELEMENT_T value, ELEMENT_T value1, ELEMENT_T value2)
            where ELEMENT_T : Enum
            => value is null
                ? value1 is null || value2 is null : value.Equals(value1) || value.Equals(value2);

        /// <summary>
        /// 指定された値が、別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// 値の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="value1">
        /// 1番目の比較対象の値です。
        /// </param>
        /// <param name="value2">
        /// 2番目の比較対象の値です。
        /// </param>
        /// <param name="value3">
        /// 3番目の比較対象の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/> の何れかと等しい場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードと等価です。
        /// <code>
        /// <paramref name="value"/>.Equals(<paramref name="value1"/>) || <paramref name="value"/>.Equals(<paramref name="value2"/>) || <paramref name="value"/>.Equals(<paramref name="value3"/>)
        /// </code>
        /// </para>
        /// </returns>
        public static Boolean IsAnyOf<ELEMENT_T>(this ELEMENT_T value, ELEMENT_T value1, ELEMENT_T value2, ELEMENT_T value3)
            where ELEMENT_T : Enum
            => value is null
                ? value1 is null || value2 is null || value3 is null
                : value.Equals(value1) || value.Equals(value2) || value.Equals(value3);

        /// <summary>
        /// 指定された値が、別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// 値の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="value1">
        /// 1番目の比較対象の値です。
        /// </param>
        /// <param name="value2">
        /// 2番目の比較対象の値です。
        /// </param>
        /// <param name="value3">
        /// 3番目の比較対象の値です。
        /// </param>
        /// <param name="value4">
        /// 4番目の比較対象の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/> の何れかと等しい場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードと等価です。
        /// <code>
        /// <paramref name="value"/>.Equals(<paramref name="value1"/>) || <paramref name="value"/>.Equals(<paramref name="value2"/>) || <paramref name="value"/>.Equals(<paramref name="value3"/>) || <paramref name="value"/>.Equals(<paramref name="value4"/>)
        /// </code>
        /// </para>
        /// </returns>
        public static Boolean IsAnyOf<ELEMENT_T>(this ELEMENT_T value, ELEMENT_T value1, ELEMENT_T value2, ELEMENT_T value3, ELEMENT_T value4)
            where ELEMENT_T : Enum
            => value is null
                ? value1 is null || value2 is null || value3 is null || value4 is null
                : value.Equals(value1) || value.Equals(value2) || value.Equals(value3) || value.Equals(value4);

        /// <summary>
        /// 指定された値が、別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// 値の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="value1">
        /// 1番目の比較対象の値です。
        /// </param>
        /// <param name="value2">
        /// 2番目の比較対象の値です。
        /// </param>
        /// <param name="value3">
        /// 3番目の比較対象の値です。
        /// </param>
        /// <param name="value4">
        /// 4番目の比較対象の値です。
        /// </param>
        /// <param name="value5">
        /// 5番目の比較対象の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/>, <paramref name="value5"/> の何れかと等しい場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードと等価です。
        /// <code>
        /// <paramref name="value"/>.Equals(<paramref name="value1"/>) || <paramref name="value"/>.Equals(<paramref name="value2"/>) || <paramref name="value"/>.Equals(<paramref name="value3"/>) || <paramref name="value"/>.Equals(<paramref name="value4"/>) || <paramref name="value"/>.Equals(<paramref name="value5"/>)
        /// </code>
        /// </para>
        /// </returns>
        public static Boolean IsAnyOf<ELEMENT_T>(this ELEMENT_T value, ELEMENT_T value1, ELEMENT_T value2, ELEMENT_T value3, ELEMENT_T value4, ELEMENT_T value5)
            where ELEMENT_T : Enum
            => value is null
                ? value1 is null || value2 is null || value3 is null || value4 is null || value5 is null
                : value.Equals(value1) || value.Equals(value2) || value.Equals(value3) || value.Equals(value4) || value.Equals(value5);

        /// <summary>
        /// 指定された値が、別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// 値の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="value1">
        /// 1番目の比較対象の値です。
        /// </param>
        /// <param name="value2">
        /// 2番目の比較対象の値です。
        /// </param>
        /// <param name="value3">
        /// 3番目の比較対象の値です。
        /// </param>
        /// <param name="value4">
        /// 4番目の比較対象の値です。
        /// </param>
        /// <param name="value5">
        /// 5番目の比較対象の値です。
        /// </param>
        /// <param name="value6">
        /// 6番目の比較対象の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/>, <paramref name="value5"/>, <paramref name="value6"/> の何れかと等しい場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードと等価です。
        /// <code>
        /// <paramref name="value"/>.Equals(<paramref name="value1"/>) || <paramref name="value"/>.Equals(<paramref name="value2"/>) || <paramref name="value"/>.Equals(<paramref name="value3"/>) || <paramref name="value"/>.Equals(<paramref name="value4"/>) || <paramref name="value"/>.Equals(<paramref name="value5"/>) || <paramref name="value"/>.Equals(<paramref name="value6"/>)
        /// </code>
        /// </para>
        /// </returns>
        public static Boolean IsAnyOf<ELEMENT_T>(this ELEMENT_T value, ELEMENT_T value1, ELEMENT_T value2, ELEMENT_T value3, ELEMENT_T value4, ELEMENT_T value5, ELEMENT_T value6)
            where ELEMENT_T : Enum
            => value is null
                ? value1 is null || value2 is null || value3 is null || value4 is null || value5 is null || value6 is null
                : value.Equals(value1) || value.Equals(value2) || value.Equals(value3) || value.Equals(value4) || value.Equals(value5) || value.Equals(value6);
    }
}
