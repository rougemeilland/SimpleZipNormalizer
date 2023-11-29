using System;
using System.Collections.Generic;

namespace Utility.Text
{
    /// <summary>
    /// 日本語文字の面区点表現を示す構造体です。
    /// </summary>
    public readonly struct PlaneRowCellNumber
        : IEquatable<PlaneRowCellNumber>
    {
        [Obsolete("Do not call the default constructor.")]
        public PlaneRowCellNumber()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// シングルバイト文字コードによって初期化するコンストラクタです。
        /// </summary>
        /// <param name="cell">
        /// 文字コードです。
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 文字コードが範囲外です。
        /// </exception>
        public PlaneRowCellNumber(Int32 cell)
        {
            if (!cell.IsBetween(Byte.MinValue, (Int32)Byte.MaxValue))
                throw new ArgumentOutOfRangeException(nameof(cell));

            Plane = 0;
            Row = 0;
            Cell = (Byte)cell;
        }

        /// <summary>
        /// 面区点コードによって初期化するコンストラクタです。
        /// </summary>
        /// <param name="plane">
        /// 面の値です。
        /// </param>
        /// <param name="row">
        /// 区の値です。
        /// </param>
        /// <param name="cell">
        /// 点の値です。
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="plane"/>, <paramref name="row"/>, <paramref name="cell"/> の何れかの値が範囲外です。
        /// </exception>
        public PlaneRowCellNumber(Int32 plane, Int32 row, Int32 cell)
        {
            if (!plane.IsBetween(1, 2))
                throw new ArgumentOutOfRangeException(nameof(plane));
            if (!row.IsBetween(1, 94))
                throw new ArgumentOutOfRangeException(nameof(row));
            if (!cell.IsBetween(1, 94))
                throw new ArgumentOutOfRangeException(nameof(cell));

            Plane = (Byte)plane;
            Row = (Byte)row;
            Cell = (Byte)cell;
        }

        /// <summary>
        /// 面の値を取得します。
        /// </summary>
        public Byte Plane { get; }

        /// <summary>
        /// 区の値を取得します。
        /// </summary>
        public Byte Row { get; }

        /// <summary>
        /// 点の値を取得します。
        /// </summary>
        public Byte Cell { get; }

        /// <summary>
        /// 文字がシングルバイト文字かどうかを示す値を取得します。
        /// </summary>
        public Boolean IsSingleByte
        {
            get
            {
                if (Plane > 0)
                    return false;
                else if (Row > 0)
                    throw new InternalLogicalErrorException();
                else
                    return true;
            }
        }

        public Boolean Equals(PlaneRowCellNumber other)
            => Plane == other.Plane
                && Row == other.Row
                && Cell == other.Cell;

        public override Boolean Equals(Object? other)
            => other is not null
                && GetType() == other.GetType()
                && Equals((PlaneRowCellNumber)other);

        public override Int32 GetHashCode() => HashCode.Combine(Plane, Row, Cell);

        public override String ToString()
            => Plane > 0
                ? $"{Plane}-{Row}-{Cell}"
                : Row > 0
                ? $"{Row}-{Cell}"
                : $"{Cell}";

        public static IEnumerable<PlaneRowCellNumber> EnumerateAllCharacters()
        {
            for (var cell = 0; cell <= 0x7f; ++cell)
                yield return new PlaneRowCellNumber(cell);
            for (var plane = 1; plane <= 2; ++plane)
            {
                for (var row = 1; row <= 94; ++row)
                {
                    for (var cell = 1; cell <= 94; ++cell)
                        yield return new PlaneRowCellNumber(plane, row, cell);
                }
            }
        }

        public static Boolean operator ==(PlaneRowCellNumber left, PlaneRowCellNumber right) => left.Equals(right);

        public static Boolean operator !=(PlaneRowCellNumber left, PlaneRowCellNumber right) => !(left == right);
    }
}
