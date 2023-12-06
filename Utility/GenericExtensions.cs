using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Utility
{
    public static class GenericExtensions
    {
        #region IsBetween

        /// <summary>
        /// 指定された値が範囲内にあるかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE1_T">
        /// <para>
        /// 調べる値の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IComparable{VALUE2_T}">IComparable&lt;<typeparamref name="VALUE2_T"/>&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <typeparam name="VALUE2_T">
        /// 範囲の上限/下限の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="lowerValue">
        /// 下限値です。
        /// </param>
        /// <param name="upperValue">
        /// 上限値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="lowerValue"/> 以上でありかつ <paramref name="upperValue"/> 以下である場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code><paramref name="value"/>.CompareTo(<paramref name="lowerValue"/>) &gt;= 0 &amp;&amp; <paramref name="value"/>.CompareTo(<paramref name="upperValue"/>) &lt;= 0</code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsBetween<VALUE1_T, VALUE2_T>(this VALUE1_T value, VALUE2_T lowerValue, VALUE2_T upperValue)
            where VALUE1_T : IComparable<VALUE2_T>
            => value is null
                ? lowerValue is null
                : value.CompareTo(lowerValue) >= 0 && value.CompareTo(upperValue) <= 0;

        /// <summary>
        /// 指定された比較子を使用して、指定された値が範囲内にあるかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// 調べる値の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="lowerValue">
        /// 下限値です。
        /// </param>
        /// <param name="upperValue">
        /// 上限値です。
        /// </param>
        /// <param name="comparer">
        /// 値を比較するための比較子です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="lowerValue"/> 以上でありかつ <paramref name="upperValue"/> 以下である場合は true を返します。
        /// そうではない場合は false を返します。
        /// 値の比較には比較子 <paramref name="comparer"/> を使用します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code><paramref name="comparer"/>.Compare(<paramref name="value"/>, <paramref name="lowerValue"/>) &gt;= 0 &amp;&amp; <paramref name="comparer"/>.Compare(<paramref name="value"/>, <paramref name="upperValue"/>) &lt;= 0</code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsBetween<VALUE_T>(this VALUE_T value, VALUE_T lowerValue, VALUE_T upperValue, IComparer<VALUE_T> comparer)
            => comparer.Compare(value, lowerValue) >= 0 && comparer.Compare(value, upperValue) <= 0;

        #endregion

        #region InRange

        /// <summary>
        /// 指定された値が範囲内にあるかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE1_T">
        /// <para>
        /// 調べる値の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IComparable{VALUE2_T}">IComparable&lt;<typeparamref name="VALUE2_T"/>&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <typeparam name="VALUE2_T">
        /// 範囲の上限/下限の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="lowerValue">
        /// 下限値です。
        /// </param>
        /// <param name="upperValue">
        /// 上限値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="lowerValue"/> 以上でありかつ <paramref name="upperValue"/> 未満である場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code><paramref name="value"/>.CompareTo(<paramref name="lowerValue"/>) &gt;= 0 &amp;&amp; <paramref name="value"/>.CompareTo(<paramref name="upperValue"/>) &lt; 0</code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean InRange<VALUE1_T, VALUE2_T>(this VALUE1_T value, VALUE2_T lowerValue, VALUE2_T upperValue)
            where VALUE1_T : IComparable<VALUE2_T>
            => value is null
                ? lowerValue is null
                : value.CompareTo(lowerValue) >= 0 && value.CompareTo(upperValue) < 0;

        /// <summary>
        /// 指定された比較子を使用して、指定された値が範囲内にあるかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// 調べる値の型です。
        /// </typeparam>
        /// <param name="value">
        /// 調べる値です。
        /// </param>
        /// <param name="lowerValue">
        /// 下限値です。
        /// </param>
        /// <param name="upperValue">
        /// 上限値です。
        /// </param>
        /// <param name="comparer">
        /// 値を比較するための比較子です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="lowerValue"/> 以上でありかつ <paramref name="upperValue"/> 未満である場合は true を返します。
        /// そうではない場合は false を返します。
        /// 値の比較には比較子 <paramref name="comparer"/> を使用します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code><paramref name="comparer"/>.Compare(<paramref name="value"/>, <paramref name="lowerValue"/>) &gt;= 0 &amp;&amp; <paramref name="comparer"/>.Compare(<paramref name="value"/>, <paramref name="upperValue"/>) &lt; 0</code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean InRange<VALUE_T>(this VALUE_T value, VALUE_T lowerValue, VALUE_T upperValue, IComparer<VALUE_T> comparer)
            => comparer.Compare(value, lowerValue) >= 0 && comparer.Compare(value, upperValue) < 0;

        #endregion

        #region IsNoneOf

        /// <summary>
        /// 指定された値が指定された別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE1_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IEquatable{VALUE2_T}">IEquatable&lt;<typeparamref name="VALUE2_T"/>&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <typeparam name="VALUE2_T">
        /// 比較対象の値の型です。
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
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// !<paramref name="value"/>.Equals(<paramref name="value1"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value2"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf<VALUE1_T, VALUE2_T>(this VALUE1_T value, VALUE2_T value1, VALUE2_T value2)
            where VALUE1_T : IEquatable<VALUE2_T>
            => value is null
                ? value1 is not null && value2 is not null
                : !value.Equals(value1) && !value.Equals(value2);

        /// <summary>
        /// 指定された値が指定された別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE1_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IEquatable{VALUE2_T}">IEquatable&lt;<typeparamref name="VALUE2_T"/>&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <typeparam name="VALUE2_T">
        /// 比較対象の値の型です。
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
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// !<paramref name="value"/>.Equals(<paramref name="value1"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value2"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value3"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf<VALUE1_T, VALUE2_T>(this VALUE1_T value, VALUE2_T value1, VALUE2_T value2, VALUE2_T value3)
            where VALUE1_T : IEquatable<VALUE2_T>
            => value is null
                ? value1 is not null && value2 is not null && value3 is not null
                : !value.Equals(value1) && !value.Equals(value2) && !value.Equals(value3);

        /// <summary>
        /// 指定された値が指定された別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE1_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IEquatable{VALUE2_T}">IEquatable&lt;<typeparamref name="VALUE2_T"/>&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <typeparam name="VALUE2_T">
        /// 比較対象の値の型です。
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
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// !<paramref name="value"/>.Equals(<paramref name="value1"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value2"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value3"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value4"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf<VALUE1_T, VALUE2_T>(this VALUE1_T value, VALUE2_T value1, VALUE2_T value2, VALUE2_T value3, VALUE2_T value4)
            where VALUE1_T : IEquatable<VALUE2_T>
            => value is null
                ? value1 is not null && value2 is not null && value3 is not null && value4 is not null
                : !value.Equals(value1) && !value.Equals(value2) && !value.Equals(value3) && !value.Equals(value4);

        /// <summary>
        /// 指定された値が指定された別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE1_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IEquatable{VALUE2_T}">IEquatable&lt;<typeparamref name="VALUE2_T"/>&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <typeparam name="VALUE2_T">
        /// 比較対象の値の型です。
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
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// !<paramref name="value"/>.Equals(<paramref name="value1"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value2"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value3"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value4"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value5"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf<VALUE1_T, VALUE2_T>(this VALUE1_T value, VALUE2_T value1, VALUE2_T value2, VALUE2_T value3, VALUE2_T value4, VALUE2_T value5)
            where VALUE1_T : IEquatable<VALUE2_T>
            => value is null
                ? value1 is not null && value2 is not null && value3 is not null && value4 is not null && value5 is not null
                : !value.Equals(value1) && !value.Equals(value2) && !value.Equals(value3) && !value.Equals(value4) && !value.Equals(value5);

        /// <summary>
        /// 指定された値が指定された別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE1_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IEquatable{VALUE2_T}">IEquatable&lt;<typeparamref name="VALUE2_T"/>&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <typeparam name="VALUE2_T">
        /// 比較対象の値の型です。
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
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// !<paramref name="value"/>.Equals(<paramref name="value1"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value2"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value3"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value4"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value5"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value6"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf<VALUE1_T, VALUE2_T>(this VALUE1_T value, VALUE2_T value1, VALUE2_T value2, VALUE2_T value3, VALUE2_T value4, VALUE2_T value5, VALUE2_T value6)
            where VALUE1_T : IEquatable<VALUE2_T>
            => value is null
                ? value1 is not null && value2 is not null && value3 is not null && value4 is not null && value5 is not null && value6 is not null
                : !value.Equals(value1) && !value.Equals(value2) && !value.Equals(value3) && !value.Equals(value4) && !value.Equals(value5) && !value.Equals(value6);

        /// <summary>
        /// 指定された値が指定された別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE1_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IEquatable{VALUE2_T}">IEquatable&lt;<typeparamref name="VALUE2_T"/>&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <typeparam name="VALUE2_T">
        /// 比較対象の値の型です。
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
        /// <param name="value7">
        /// 7番目の比較対象の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/>, <paramref name="value5"/>, <paramref name="value6"/>, <paramref name="value7"/> の何れとも等しくない場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// !<paramref name="value"/>.Equals(<paramref name="value1"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value2"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value3"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value4"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value5"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value6"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value7"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf<VALUE1_T, VALUE2_T>(this VALUE1_T value, VALUE2_T value1, VALUE2_T value2, VALUE2_T value3, VALUE2_T value4, VALUE2_T value5, VALUE2_T value6, VALUE2_T value7)
            where VALUE1_T : IEquatable<VALUE2_T>
            => value is null
                ? value1 is not null && value2 is not null && value3 is not null && value4 is not null && value5 is not null && value6 is not null && value7 is not null
                : !value.Equals(value1) && !value.Equals(value2) && !value.Equals(value3) && !value.Equals(value4) && !value.Equals(value5) && !value.Equals(value6) && !value.Equals(value7);

        /// <summary>
        /// 指定された値が指定された別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE1_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IEquatable{VALUE2_T}">IEquatable&lt;<typeparamref name="VALUE2_T"/>&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <typeparam name="VALUE2_T">
        /// 比較対象の値の型です。
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
        /// <param name="value7">
        /// 7番目の比較対象の値です。
        /// </param>
        /// <param name="value8">
        /// 8番目の比較対象の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/>, <paramref name="value5"/>, <paramref name="value6"/>, <paramref name="value7"/>, <paramref name="value8"/> の何れとも等しくない場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// !<paramref name="value"/>.Equals(<paramref name="value1"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value2"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value3"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value4"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value5"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value6"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value7"/>) &amp;&amp; !<paramref name="value"/>.Equals(<paramref name="value8"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf<VALUE1_T, VALUE2_T>(this VALUE1_T value, VALUE2_T value1, VALUE2_T value2, VALUE2_T value3, VALUE2_T value4, VALUE2_T value5, VALUE2_T value6, VALUE2_T value7, VALUE2_T value8)
            where VALUE1_T : IEquatable<VALUE2_T>
            => value is null
                ? value1 is not null && value2 is not null && value3 is not null && value4 is not null && value5 is not null && value6 is not null && value7 is not null && value8 is not null
                : !value.Equals(value1) && !value.Equals(value2) && !value.Equals(value3) && !value.Equals(value4) && !value.Equals(value5) && !value.Equals(value6) && !value.Equals(value7) && !value.Equals(value8);

        /// <summary>
        /// 指定された等値比較子を使用して、指定された値が指定された別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
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
        /// <param name="equalityComparer">
        /// 値の比較のための等値比較子です。
        /// </param>
        /// <returns>
        /// <para>
        /// 等値比較子 <paramref name="equalityComparer"/> を使用し、<paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/> の何れとも等しくない場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value1"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value2"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T value1, VALUE_T value2, IEqualityComparer<VALUE_T> equalityComparer)
            => !equalityComparer.Equals(value, value1) &&
                !equalityComparer.Equals(value, value2);

        /// <summary>
        /// 指定された等値比較子を使用して、指定された値が指定された別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
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
        /// <param name="equalityComparer">
        /// 値の比較のための等値比較子です。
        /// </param>
        /// <returns>
        /// <para>
        /// 等値比較子 <paramref name="equalityComparer"/> を使用し、<paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/> の何れとも等しくない場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value1"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value2"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value3"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T value1, VALUE_T value2, VALUE_T value3, IEqualityComparer<VALUE_T> equalityComparer)
            => !equalityComparer.Equals(value, value1) &&
                !equalityComparer.Equals(value, value2) &&
                !equalityComparer.Equals(value, value3);

        /// <summary>
        /// 指定された等値比較子を使用して、指定された値が指定された別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
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
        /// <param name="equalityComparer">
        /// 値の比較のための等値比較子です。
        /// </param>
        /// <returns>
        /// <para>
        /// 等値比較子 <paramref name="equalityComparer"/> を使用し、<paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/> の何れとも等しくない場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value1"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value2"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value3"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value4"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T value1, VALUE_T value2, VALUE_T value3, VALUE_T value4, IEqualityComparer<VALUE_T> equalityComparer)
            => !equalityComparer.Equals(value, value1) &&
                !equalityComparer.Equals(value, value2) &&
                !equalityComparer.Equals(value, value3) &&
                !equalityComparer.Equals(value, value4);

        /// <summary>
        /// 指定された等値比較子を使用して、指定された値が指定された別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
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
        /// <param name="equalityComparer">
        /// 値の比較のための等値比較子です。
        /// </param>
        /// <returns>
        /// <para>
        /// 等値比較子 <paramref name="equalityComparer"/> を使用し、<paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/>, <paramref name="value5"/> の何れとも等しくない場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value1"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value2"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value3"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value4"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value5"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T value1, VALUE_T value2, VALUE_T value3, VALUE_T value4, VALUE_T value5, IEqualityComparer<VALUE_T> equalityComparer)
            => !equalityComparer.Equals(value, value1) &&
                !equalityComparer.Equals(value, value2) &&
                !equalityComparer.Equals(value, value3) &&
                !equalityComparer.Equals(value, value4) &&
                !equalityComparer.Equals(value, value5);

        /// <summary>
        /// 指定された等値比較子を使用して、指定された値が指定された別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
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
        /// <param name="equalityComparer">
        /// 値の比較のための等値比較子です。
        /// </param>
        /// <returns>
        /// <para>
        /// 等値比較子 <paramref name="equalityComparer"/> を使用し、<paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/>, <paramref name="value5"/>, <paramref name="value6"/> の何れとも等しくない場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value1"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value2"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value3"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value4"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value5"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value6"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T value1, VALUE_T value2, VALUE_T value3, VALUE_T value4, VALUE_T value5, VALUE_T value6, IEqualityComparer<VALUE_T> equalityComparer)
            => !equalityComparer.Equals(value, value1) &&
                !equalityComparer.Equals(value, value2) &&
                !equalityComparer.Equals(value, value3) &&
                !equalityComparer.Equals(value, value4) &&
                !equalityComparer.Equals(value, value5) &&
                !equalityComparer.Equals(value, value6);

        /// <summary>
        /// 指定された等値比較子を使用して、指定された値が指定された別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
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
        /// <param name="value7">
        /// 7番目の比較対象の値です。
        /// </param>
        /// <param name="equalityComparer">
        /// 値の比較のための等値比較子です。
        /// </param>
        /// <returns>
        /// <para>
        /// 等値比較子 <paramref name="equalityComparer"/> を使用し、<paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/>, <paramref name="value5"/>, <paramref name="value6"/>, <paramref name="value7"/> の何れとも等しくない場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value1"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value2"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value3"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value4"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value5"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value6"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value7"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T value1, VALUE_T value2, VALUE_T value3, VALUE_T value4, VALUE_T value5, VALUE_T value6, VALUE_T value7, IEqualityComparer<VALUE_T> equalityComparer)
            => !equalityComparer.Equals(value, value1) &&
                !equalityComparer.Equals(value, value2) &&
                !equalityComparer.Equals(value, value3) &&
                !equalityComparer.Equals(value, value4) &&
                !equalityComparer.Equals(value, value5) &&
                !equalityComparer.Equals(value, value6) &&
                !equalityComparer.Equals(value, value7);

        /// <summary>
        /// 指定された等値比較子を使用して、指定された値が指定された別の複数の値の何れとも等しくないかどうかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
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
        /// <param name="value7">
        /// 7番目の比較対象の値です。
        /// </param>
        /// <param name="value8">
        /// 8番目の比較対象の値です。
        /// </param>
        /// <param name="equalityComparer">
        /// 値の比較のための等値比較子です。
        /// </param>
        /// <returns>
        /// <para>
        /// 等値比較子 <paramref name="equalityComparer"/> を使用し、<paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/>, <paramref name="value5"/>, <paramref name="value6"/>, <paramref name="value7"/>, <paramref name="value8"/> の何れとも等しくない場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value1"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value2"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value3"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value4"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value5"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value6"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value7"/>) &amp;&amp; !<paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value8"/>) 
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T value1, VALUE_T value2, VALUE_T value3, VALUE_T value4, VALUE_T value5, VALUE_T value6, VALUE_T value7, VALUE_T value8, IEqualityComparer<VALUE_T> equalityComparer)
            => !equalityComparer.Equals(value, value1) &&
                !equalityComparer.Equals(value, value2) &&
                !equalityComparer.Equals(value, value3) &&
                !equalityComparer.Equals(value, value4) &&
                !equalityComparer.Equals(value, value5) &&
                !equalityComparer.Equals(value, value6) &&
                !equalityComparer.Equals(value, value7) &&
                !equalityComparer.Equals(value, value8);

        #endregion

        #region IsAnyOf

        /// <summary>
        /// 指定された値が指定された別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="VALUE1_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IEquatable{VALUE2_T}">IEquatable&lt;<typeparamref name="VALUE2_T"/>&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <typeparam name="VALUE2_T">
        /// 比較対象の値の型です。
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
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// <paramref name="value"/>.Equals(<paramref name="value1"/>) || <paramref name="value"/>.Equals(<paramref name="value2"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf<VALUE1_T, VALUE2_T>(this VALUE1_T value, VALUE2_T value1, VALUE2_T value2)
            where VALUE1_T : IEquatable<VALUE2_T>
            => value is null
                ? value1 is null || value2 is null
                : value.Equals(value1) || value.Equals(value2);

        /// <summary>
        /// 指定された値が指定された別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="VALUE1_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IEquatable{VALUE2_T}">IEquatable&lt;<typeparamref name="VALUE2_T"/>&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <typeparam name="VALUE2_T">
        /// 比較対象の値の型です。
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
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// <paramref name="value"/>.Equals(<paramref name="value1"/>) || <paramref name="value"/>.Equals(<paramref name="value2"/>) || <paramref name="value"/>.Equals(<paramref name="value3"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf<VALUE1_T, VALUE2_T>(this VALUE1_T value, VALUE2_T value1, VALUE2_T value2, VALUE2_T value3)
            where VALUE1_T : IEquatable<VALUE2_T>
            => value is null
                ? value1 is null || value2 is null || value3 is null
                : value.Equals(value1) || value.Equals(value2) || value.Equals(value3);

        /// <summary>
        /// 指定された値が指定された別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="VALUE1_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IEquatable{VALUE2_T}">IEquatable&lt;<typeparamref name="VALUE2_T"/>&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <typeparam name="VALUE2_T">
        /// 比較対象の値の型です。
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
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// <paramref name="value"/>.Equals(<paramref name="value1"/>) || <paramref name="value"/>.Equals(<paramref name="value2"/>) || <paramref name="value"/>.Equals(<paramref name="value3"/>) || <paramref name="value"/>.Equals(<paramref name="value4"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf<VALUE1_T, VALUE2_T>(this VALUE1_T value, VALUE2_T value1, VALUE2_T value2, VALUE2_T value3, VALUE2_T value4)
            where VALUE1_T : IEquatable<VALUE2_T>
            => value is null
                ? value1 is null || value2 is null || value3 is null || value4 is null
                : value.Equals(value1) || value.Equals(value2) || value.Equals(value3) || value.Equals(value4);

        /// <summary>
        /// 指定された値が指定された別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="VALUE1_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IEquatable{VALUE2_T}">IEquatable&lt;<typeparamref name="VALUE2_T"/>&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <typeparam name="VALUE2_T">
        /// 比較対象の値の型です。
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
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// <paramref name="value"/>.Equals(<paramref name="value1"/>) || <paramref name="value"/>.Equals(<paramref name="value2"/>) || <paramref name="value"/>.Equals(<paramref name="value3"/>) || <paramref name="value"/>.Equals(<paramref name="value4"/>) || <paramref name="value"/>.Equals(<paramref name="value5"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf<VALUE1_T, VALUE2_T>(this VALUE1_T value, VALUE2_T value1, VALUE2_T value2, VALUE2_T value3, VALUE2_T value4, VALUE2_T value5)
            where VALUE1_T : IEquatable<VALUE2_T>
            => value is null
                ? value1 is null || value2 is null || value3 is null || value4 is null || value5 is null
                : value.Equals(value1) || value.Equals(value2) || value.Equals(value3) || value.Equals(value4) || value.Equals(value5);

        /// <summary>
        /// 指定された値が指定された別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="VALUE1_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IEquatable{VALUE2_T}">IEquatable&lt;<typeparamref name="VALUE2_T"/>&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <typeparam name="VALUE2_T">
        /// 比較対象の値の型です。
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
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// <paramref name="value"/>.Equals(<paramref name="value1"/>) || <paramref name="value"/>.Equals(<paramref name="value2"/>) || <paramref name="value"/>.Equals(<paramref name="value3"/>) || <paramref name="value"/>.Equals(<paramref name="value4"/>) || <paramref name="value"/>.Equals(<paramref name="value5"/>) || <paramref name="value"/>.Equals(<paramref name="value6"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf<VALUE1_T, VALUE2_T>(this VALUE1_T value, VALUE2_T value1, VALUE2_T value2, VALUE2_T value3, VALUE2_T value4, VALUE2_T value5, VALUE2_T value6)
            where VALUE1_T : IEquatable<VALUE2_T>
            => value is null
                ? value1 is null || value2 is null || value3 is null || value4 is null || value5 is null || value6 is null
                : value.Equals(value1) || value.Equals(value2) || value.Equals(value3) || value.Equals(value4) || value.Equals(value5) || value.Equals(value6);

        /// <summary>
        /// 指定された値が指定された別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="VALUE1_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IEquatable{VALUE2_T}">IEquatable&lt;<typeparamref name="VALUE2_T"/>&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <typeparam name="VALUE2_T">
        /// 比較対象の値の型です。
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
        /// <param name="value7">
        /// 7番目の比較対象の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/>, <paramref name="value5"/>, <paramref name="value6"/>, <paramref name="value7"/> の何れかと等しい場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// <paramref name="value"/>.Equals(<paramref name="value1"/>) || <paramref name="value"/>.Equals(<paramref name="value2"/>) || <paramref name="value"/>.Equals(<paramref name="value3"/>) || <paramref name="value"/>.Equals(<paramref name="value4"/>) || <paramref name="value"/>.Equals(<paramref name="value5"/>) || <paramref name="value"/>.Equals(<paramref name="value6"/>) || <paramref name="value"/>.Equals(<paramref name="value7"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf<VALUE1_T, VALUE2_T>(this VALUE1_T value, VALUE2_T value1, VALUE2_T value2, VALUE2_T value3, VALUE2_T value4, VALUE2_T value5, VALUE2_T value6, VALUE2_T value7)
            where VALUE1_T : IEquatable<VALUE2_T>
            => value is null
                ? value1 is null || value2 is null || value3 is null || value4 is null || value5 is null || value6 is null || value7 is null
                : value.Equals(value1) || value.Equals(value2) || value.Equals(value3) || value.Equals(value4) || value.Equals(value5) || value.Equals(value6) || value.Equals(value7);

        /// <summary>
        /// 指定された値が指定された別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="VALUE1_T">
        /// <para>
        /// <paramref name="value"/> の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IEquatable{VALUE2_T}">IEquatable&lt;<typeparamref name="VALUE2_T"/>&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <typeparam name="VALUE2_T">
        /// 比較対象の値の型です。
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
        /// <param name="value7">
        /// 7番目の比較対象の値です。
        /// </param>
        /// <param name="value8">
        /// 8番目の比較対象の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/>, <paramref name="value5"/>, <paramref name="value6"/>, <paramref name="value7"/>, <paramref name="value8"/> の何れかと等しい場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// <paramref name="value"/>.Equals(<paramref name="value1"/>) || <paramref name="value"/>.Equals(<paramref name="value2"/>) || <paramref name="value"/>.Equals(<paramref name="value3"/>) || <paramref name="value"/>.Equals(<paramref name="value4"/>) || <paramref name="value"/>.Equals(<paramref name="value5"/>) || <paramref name="value"/>.Equals(<paramref name="value6"/>) || <paramref name="value"/>.Equals(<paramref name="value8"/>) || <paramref name="value"/>.Equals(<paramref name="value7"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf<VALUE1_T, VALUE2_T>(this VALUE1_T value, VALUE2_T value1, VALUE2_T value2, VALUE2_T value3, VALUE2_T value4, VALUE2_T value5, VALUE2_T value6, VALUE2_T value7, VALUE2_T value8)
            where VALUE1_T : IEquatable<VALUE2_T>
            => value is null
                ? value1 is null || value2 is null || value3 is null || value4 is null || value5 is null || value6 is null || value7 is null || value8 is null
                : value.Equals(value1) || value.Equals(value2) || value.Equals(value3) || value.Equals(value4) || value.Equals(value5) || value.Equals(value6) || value.Equals(value7) || value.Equals(value8);

        /// <summary>
        /// 指定された等値比較子を使用して、指定された値が指定された別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// <paramref name="value"/> の型です。
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
        /// 等値比較子 <paramref name="equalityComparer"/> を使用して、 <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/> の何れかと等しい場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value1"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value2"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T value1, VALUE_T value2, IEqualityComparer<VALUE_T> equalityComparer)
            => equalityComparer.Equals(value, value1) ||
                equalityComparer.Equals(value, value2);

        /// <summary>
        /// 指定された等値比較子を使用して、指定された値が指定された別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// <paramref name="value"/> の型です。
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
        /// 等値比較子 <paramref name="equalityComparer"/> を使用して、 <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/> の何れかと等しい場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value1"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value2"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value3"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T value1, VALUE_T value2, VALUE_T value3, IEqualityComparer<VALUE_T> equalityComparer)
            => equalityComparer.Equals(value, value1) ||
                equalityComparer.Equals(value, value2) ||
                equalityComparer.Equals(value, value3);

        /// <summary>
        /// 指定された等値比較子を使用して、指定された値が指定された別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// <paramref name="value"/> の型です。
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
        /// 等値比較子 <paramref name="equalityComparer"/> を使用して、 <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/> の何れかと等しい場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value1"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value2"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value3"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value4"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T value1, VALUE_T value2, VALUE_T value3, VALUE_T value4, IEqualityComparer<VALUE_T> equalityComparer)
            => equalityComparer.Equals(value, value1) ||
                equalityComparer.Equals(value, value2) ||
                equalityComparer.Equals(value, value3) ||
                equalityComparer.Equals(value, value4);

        /// <summary>
        /// 指定された等値比較子を使用して、指定された値が指定された別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// <paramref name="value"/> の型です。
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
        /// 等値比較子 <paramref name="equalityComparer"/> を使用して、 <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/>, <paramref name="value5"/> の何れかと等しい場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value1"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value2"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value3"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value4"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value5"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T value1, VALUE_T value2, VALUE_T value3, VALUE_T value4, VALUE_T value5, IEqualityComparer<VALUE_T> equalityComparer)
            => equalityComparer.Equals(value, value1) ||
                equalityComparer.Equals(value, value2) ||
                equalityComparer.Equals(value, value3) ||
                equalityComparer.Equals(value, value4) ||
                equalityComparer.Equals(value, value5);

        /// <summary>
        /// 指定された等値比較子を使用して、指定された値が指定された別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// <paramref name="value"/> の型です。
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
        /// 等値比較子 <paramref name="equalityComparer"/> を使用して、 <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/>, <paramref name="value5"/>, <paramref name="value6"/> の何れかと等しい場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value1"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value2"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value3"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value4"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value5"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value6"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T value1, VALUE_T value2, VALUE_T value3, VALUE_T value4, VALUE_T value5, VALUE_T value6, IEqualityComparer<VALUE_T> equalityComparer)
            => equalityComparer.Equals(value, value1) ||
                equalityComparer.Equals(value, value2) ||
                equalityComparer.Equals(value, value3) ||
                equalityComparer.Equals(value, value4) ||
                equalityComparer.Equals(value, value5) ||
                equalityComparer.Equals(value, value6);

        /// <summary>
        /// 指定された等値比較子を使用して、指定された値が指定された別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// <paramref name="value"/> の型です。
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
        /// <param name="value7">
        /// 7番目の比較対象の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// 等値比較子 <paramref name="equalityComparer"/> を使用して、 <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/>, <paramref name="value5"/>, <paramref name="value6"/>, <paramref name="value7"/> の何れかと等しい場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value1"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value2"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value3"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value4"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value5"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value6"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value7"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T value1, VALUE_T value2, VALUE_T value3, VALUE_T value4, VALUE_T value5, VALUE_T value6, VALUE_T value7, IEqualityComparer<VALUE_T> equalityComparer)
            => equalityComparer.Equals(value, value1) ||
                equalityComparer.Equals(value, value2) ||
                equalityComparer.Equals(value, value3) ||
                equalityComparer.Equals(value, value4) ||
                equalityComparer.Equals(value, value5) ||
                equalityComparer.Equals(value, value6) ||
                equalityComparer.Equals(value, value7);

        /// <summary>
        /// 指定された等値比較子を使用して、指定された値が指定された別の複数の値の何れかと等しいかどうか調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// <paramref name="value"/> の型です。
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
        /// <param name="value7">
        /// 7番目の比較対象の値です。
        /// </param>
        /// <param name="value8">
        /// 8番目の比較対象の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// 等値比較子 <paramref name="equalityComparer"/> を使用して、 <paramref name="value"/> が <paramref name="value1"/>, <paramref name="value2"/>, <paramref name="value3"/>, <paramref name="value4"/>, <paramref name="value5"/>, <paramref name="value6"/>, <paramref name="value7"/>, <paramref name="value8"/> の何れかと等しい場合は true を返します。
        /// そうではない場合は false を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// <code>
        /// <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value1"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value2"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value3"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value4"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value5"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value6"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value7"/>) || <paramref name="equalityComparer"/>.Equals(<paramref name="value"/>, <paramref name="value8"/>)
        /// </code>
        /// </para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Boolean IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T value1, VALUE_T value2, VALUE_T value3, VALUE_T value4, VALUE_T value5, VALUE_T value6, VALUE_T value7, VALUE_T value8, IEqualityComparer<VALUE_T> equalityComparer)
            => equalityComparer.Equals(value, value1) ||
                equalityComparer.Equals(value, value2) ||
                equalityComparer.Equals(value, value3) ||
                equalityComparer.Equals(value, value4) ||
                equalityComparer.Equals(value, value5) ||
                equalityComparer.Equals(value, value6) ||
                equalityComparer.Equals(value, value7) ||
                equalityComparer.Equals(value, value8);

        #endregion

        #region Minimum

        /// <summary>
        /// 2つの値を比較して最小値を取得します。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// <para>
        /// 値の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IComparable{VALUE_T}">IComparable&lt;VALUE_T&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <param name="x">
        /// 比較する1つ目の値です。
        /// </param>
        /// <param name="y">
        /// 比較する2つ目の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="x"/> と <paramref name="y"/> のうち小さい方の値を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// </para>
        /// <code>
        /// <paramref name="x"/>.CompareTo(<paramref name="y"/>) &gt; 0 ? <paramref name="y"/> : <paramref name="x"/>
        /// </code>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: MaybeNull]
        public static VALUE_T Minimum<VALUE_T>([AllowNull] this VALUE_T x, [AllowNull] VALUE_T y)
            where VALUE_T : IComparable<VALUE_T>
            => x is null
                ? default
                : x.CompareTo(y) > 0
                ? y
                : x;

        /// <summary>
        /// 比較子を指定して、2つの値を比較して最小値を取得します。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// 値の型です。
        /// </typeparam>
        /// <param name="x">
        /// 比較する1つ目の値です。
        /// </param>
        /// <param name="y">
        /// 比較する2つ目の値です。
        /// </param>
        /// <param name="comparer">
        /// 値を比較するための比較子です。
        /// </param>
        /// <returns>
        /// <para>
        /// 比較子 <paramref name="comparer"/> を使用して、<paramref name="x"/> と <paramref name="y"/> のうち小さい方の値を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// </para>
        /// <code>
        /// <paramref name="comparer"/>.Comparer(<paramref name="x"/>, <paramref name="y"/>) &gt; 0 ? <paramref name="y"/> : <paramref name="x"/>
        /// </code>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: MaybeNull, NotNullIfNotNull(nameof(x)), NotNullIfNotNull(nameof(y))]
        public static VALUE_T Minimum<VALUE_T>([AllowNull] this VALUE_T x, [AllowNull] VALUE_T y, IComparer<VALUE_T> comparer)
            => comparer.Compare(x, y) < 0 ? x : y;

        #endregion

        #region Maximum

        /// <summary>
        /// 2つの値を比較して最大値を取得します。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// <para>
        /// 値の型です。
        /// </para>
        /// <para>
        /// この型は <see cref="IComparable{VALUE_T}">IComparable&lt;VALUE_T&gt;</see> を実装している必要があります。
        /// </para>
        /// </typeparam>
        /// <param name="x">
        /// 比較する1つ目の値です。
        /// </param>
        /// <param name="y">
        /// 比較する2つ目の値です。
        /// </param>
        /// <returns>
        /// <para>
        /// <paramref name="x"/> と <paramref name="y"/> のうち大きい方の値を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// </para>
        /// <code>
        /// <paramref name="x"/>.CompareTo(<paramref name="y"/>) &gt; 0 ? <paramref name="x"/> : <paramref name="y"/>
        /// </code>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: MaybeNull, NotNullIfNotNull(nameof(x)), NotNullIfNotNull(nameof(y))]
        public static VALUE_T Maximum<VALUE_T>([AllowNull] this VALUE_T x, [AllowNull] VALUE_T y)
            where VALUE_T : IComparable<VALUE_T>
            => x is null
                ? y
                : x.CompareTo(y) > 0
                ? x
                : y;

        /// <summary>
        /// 比較子を指定して、2つの値を比較して最大値を取得します。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// 値の型です。
        /// </typeparam>
        /// <param name="x">
        /// 比較する1つ目の値です。
        /// </param>
        /// <param name="y">
        /// 比較する2つ目の値です。
        /// </param>
        /// <param name="comparer">
        /// 値を比較するための比較子です。
        /// </param>
        /// <returns>
        /// <para>
        /// 比較子 <paramref name="comparer"/> を使用して、<paramref name="x"/> と <paramref name="y"/> のうち大きい方の値を返します。
        /// </para>
        /// <para>
        /// 戻り値は以下のコードの実行結果と等価です。
        /// </para>
        /// <code>
        /// <paramref name="comparer"/>.Comparer(<paramref name="x"/>, <paramref name="y"/>) &gt; 0 ? <paramref name="x"/> : <paramref name="y"/>
        /// </code>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: MaybeNull, NotNullIfNotNull(nameof(x)), NotNullIfNotNull(nameof(y))]
        public static VALUE_T Maximum<VALUE_T>([AllowNull] this VALUE_T x, [AllowNull] VALUE_T y, IComparer<VALUE_T> comparer)
            => comparer.Compare(x, y) > 0 ? x : y;

        #endregion

        public static VALUE_T Duplicate<VALUE_T>(this VALUE_T value)
            where VALUE_T : ICloneable<VALUE_T>
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            return value.Clone();
        }
    }
}
