﻿using System;
using System.Numerics;

namespace ZipUtility
{
    /// <summary>
    /// ZIP アーカイブ上にエントリのデータが配置されている順序を示す値の構造体です。
    /// </summary>
    public readonly struct ZipEntryLocationOrder
        : IEquatable<ZipEntryLocationOrder>, IComparable<ZipEntryLocationOrder>, IEqualityOperators<ZipEntryLocationOrder, ZipEntryLocationOrder, Boolean>, IComparisonOperators<ZipEntryLocationOrder, ZipEntryLocationOrder, Boolean>
    {
        private readonly UInt32 _diskNumber;
        private readonly UInt64 _offsetOnTheDisk;

        internal ZipEntryLocationOrder(ZipStreamPosition position)
        {
            _diskNumber = position.DiskNumber;
            _offsetOnTheDisk = position.OffsetOnTheDisk;
        }

        /// <summary>
        /// このインスタンスが指定された <see cref="ZipEntryLocationOrder"/> 値と等しいかどうかを調べます。
        /// </summary>
        /// <param name="other">
        /// このインスタンスと比較するオブジェクトです。
        /// </param>
        /// <returns>
        /// このインスタンスと <paramref name="other"/> は同じ値である場合は true です。
        /// そうではない場合は false です。
        /// </returns>
        public Boolean Equals(ZipEntryLocationOrder other) => _diskNumber == other._diskNumber && _offsetOnTheDisk == other._offsetOnTheDisk;

        /// <summary>
        /// このインスタンスと指定された <see cref="ZipEntryLocationOrder"/> 値の大小関係を調べます。
        /// </summary>
        /// <param name="other">
        /// このインスタンスと比較する <see cref="ZipEntryLocationOrder"/> 値です。
        /// </param>
        /// <returns>
        /// <list type="table">
        /// <item>
        /// <term>0 未満</term><description>このインスタンスは <paramref name="other"/> 未満です。</description>
        /// </item>
        /// <item>
        /// <term>0</term><description>このインスタンスは <paramref name="other"/> と等しいです。</description>
        /// </item>
        /// <item>
        /// <term>0 より大きい</term><description>このインスタンスは <paramref name="other"/> より大きいです。</description>
        /// </item>
        /// </list>
        /// </returns>
        public Int32 CompareTo(ZipEntryLocationOrder other)
        {
            var c = _diskNumber.CompareTo(other._diskNumber);
            return
                c != 0
                ? c
                : _offsetOnTheDisk.CompareTo(other._offsetOnTheDisk);
        }

        /// <summary>
        /// このインスタンスが指定されたオブジェクトと等しいかどうかを調べます。
        /// </summary>
        /// <param name="other">
        /// このインスタンスと比較するオブジェクトです。
        /// </param>
        /// <returns>
        /// このインスタンスと <paramref name="other"/> は同じ値である場合は true です。
        /// そうではない場合は false です。
        /// </returns>
        public override Boolean Equals(Object? other) => other is not null && GetType() == other.GetType() && Equals((ZipEntryId)other);

        /// <summary>
        /// このインスタンスのハッシュコードを返します。
        /// </summary>
        /// <returns>
        /// 現在の <see cref="ZipEntryLocationOrder"/> のハッシュコードです。
        /// </returns>
        public override Int32 GetHashCode() => HashCode.Combine(_diskNumber, _offsetOnTheDisk);

        /// <summary>
        /// 現在の <see cref="ZipEntryLocationOrder"/> オブジェクトの値を等価な文字列に変換します。
        /// </summary>
        /// <returns>
        /// このオブジェクトの値の文字列形式です。
        /// </returns>
        public override String ToString() => $"{_diskNumber:x8}:{_offsetOnTheDisk:x16}";

        /// <summary>
        /// 2 つの <see cref="ZipEntryLocationOrder"/> 値が等しいかどうかを調べます。
        /// </summary>
        /// <param name="left">
        /// 比較する片方の <see cref="ZipEntryLocationOrder"/> 値です。
        /// </param>
        /// <param name="right">
        /// 比較するもう片方の <see cref="ZipEntryLocationOrder"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="left"/> と <paramref name="right"/> が等しい場合は true を返します。
        /// そうではない場合は false を返します。
        /// </returns>
        public static Boolean operator ==(ZipEntryLocationOrder left, ZipEntryLocationOrder right) => left.Equals(right);

        /// <summary>
        /// 2 つの <see cref="ZipEntryLocationOrder"/> 値が等しくないかどうかを調べます。
        /// </summary>
        /// <param name="left">
        /// 比較する片方の <see cref="ZipEntryLocationOrder"/> 値です。
        /// </param>
        /// <param name="right">
        /// 比較するもう片方の <see cref="ZipEntryLocationOrder"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="left"/> と <paramref name="right"/> が等しくない場合は true を返します。
        /// そうではない場合は false を返します。
        /// </returns>
        public static Boolean operator !=(ZipEntryLocationOrder left, ZipEntryLocationOrder right) => !left.Equals(right);

        /// <summary>
        /// ある <see cref="ZipEntryLocationOrder"/> 値が別の <see cref="ZipEntryLocationOrder"/> 値未満かどうかを調べます。
        /// </summary>
        /// <param name="left">
        /// 比較する片方の <see cref="ZipEntryLocationOrder"/> 値です。
        /// </param>
        /// <param name="right">
        /// 比較するもう片方の <see cref="ZipEntryLocationOrder"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="left"/> が <paramref name="right"/> 未満である場合は true を返します。
        /// そうではない場合は false を返します。
        /// </returns>
        public static Boolean operator <(ZipEntryLocationOrder left, ZipEntryLocationOrder right) => left.CompareTo(right) < 0;

        /// <summary>
        /// ある <see cref="ZipEntryLocationOrder"/> 値が別の <see cref="ZipEntryLocationOrder"/> 値以下かどうかを調べます。
        /// </summary>
        /// <param name="left">
        /// 比較する片方の <see cref="ZipEntryLocationOrder"/> 値です。
        /// </param>
        /// <param name="right">
        /// 比較するもう片方の <see cref="ZipEntryLocationOrder"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="left"/> が <paramref name="right"/> 以下である場合は true を返します。
        /// そうではない場合は false を返します。
        /// </returns>
        public static Boolean operator <=(ZipEntryLocationOrder left, ZipEntryLocationOrder right) => left.CompareTo(right) <= 0;

        /// <summary>
        /// ある <see cref="ZipEntryLocationOrder"/> 値が別の <see cref="ZipEntryLocationOrder"/> 値より大きいかどうかを調べます。
        /// </summary>
        /// <param name="left">
        /// 比較する片方の <see cref="ZipEntryLocationOrder"/> 値です。
        /// </param>
        /// <param name="right">
        /// 比較するもう片方の <see cref="ZipEntryLocationOrder"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="left"/> が <paramref name="right"/> より大きい場合は true を返します。
        /// そうではない場合は false を返します。
        /// </returns>
        public static Boolean operator >(ZipEntryLocationOrder left, ZipEntryLocationOrder right) => left.CompareTo(right) > 0;

        /// <summary>
        /// ある <see cref="ZipEntryLocationOrder"/> 値が別の <see cref="ZipEntryLocationOrder"/> 値以上であるかどうかを調べます。
        /// </summary>
        /// <param name="left">
        /// 比較する片方の <see cref="ZipEntryLocationOrder"/> 値です。
        /// </param>
        /// <param name="right">
        /// 比較するもう片方の <see cref="ZipEntryLocationOrder"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="left"/> が <paramref name="right"/> 以上である場合は true を返します。
        /// そうではない場合は false を返します。
        /// </returns>
        public static Boolean operator >=(ZipEntryLocationOrder left, ZipEntryLocationOrder right) => left.CompareTo(right) >= 0;
    }
}
