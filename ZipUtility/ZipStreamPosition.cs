using System;
using System.Numerics;
using Utility;

namespace ZipUtility
{
    internal readonly struct ZipStreamPosition
        : IEquatable<ZipStreamPosition>, IComparable<ZipStreamPosition>, IAdditionOperators<ZipStreamPosition, UInt64, ZipStreamPosition>, ISubtractionOperators<ZipStreamPosition, UInt64, ZipStreamPosition>, ISubtractionOperators<ZipStreamPosition, ZipStreamPosition, UInt64>
    {
        internal ZipStreamPosition(UInt32 diskNumber, UInt64 offsetOnTheDisk, IVirtualZipFile hostVirtualDisk)
        {
            if (hostVirtualDisk is null)
                throw new ArgumentNullException(nameof(hostVirtualDisk));

            DiskNumber = diskNumber;
            OffsetOnTheDisk = offsetOnTheDisk;
            Host = hostVirtualDisk;
        }

        public UInt32 DiskNumber { get; }
        public UInt64 OffsetOnTheDisk { get; }

        #region operator +

        public static ZipStreamPosition operator +(ZipStreamPosition x, UInt64 y) => x.Add(y);
        public static ZipStreamPosition operator +(UInt64 x, ZipStreamPosition y) => y.Add(x);

        #endregion

        #region operator -

        public static UInt64 operator -(ZipStreamPosition x, ZipStreamPosition y) => x.Subtract(y);
        public static ZipStreamPosition operator -(ZipStreamPosition x, UInt64 y) => x.Subtract(y);

        #endregion

        #region other operator

        public static Boolean operator ==(ZipStreamPosition x, ZipStreamPosition y) => x.Equals(y);
        public static Boolean operator !=(ZipStreamPosition x, ZipStreamPosition y) => !x.Equals(y);
        public static Boolean operator >(ZipStreamPosition x, ZipStreamPosition y) => x.CompareTo(y) > 0;
        public static Boolean operator >=(ZipStreamPosition x, ZipStreamPosition y) => x.CompareTo(y) >= 0;
        public static Boolean operator <(ZipStreamPosition x, ZipStreamPosition y) => x.CompareTo(y) < 0;
        public static Boolean operator <=(ZipStreamPosition x, ZipStreamPosition y) => x.CompareTo(y) <= 0;

        #endregion

        #region Add

        public ZipStreamPosition Add(UInt64 x)
            => Host.Add(this, x);

        #endregion

        #region Subtract

        public UInt64 Subtract(ZipStreamPosition x)
            => Host.Subtract(this, x);

        public ZipStreamPosition Subtract(UInt64 x)
            => Host.Subtract(this, x);

        #endregion

        public Int32 CompareTo(ZipStreamPosition other) => Host.Compare(this, other);
        public Boolean Equals(ZipStreamPosition other) => Host.Equal(this, other);

        public override Boolean Equals(Object? other)
            => other is not null
                && GetType() == other.GetType()
                && Equals((ZipStreamPosition)other);

        public override Int32 GetHashCode() => Host.GetHashCode(this);

        public override String ToString() => $"0x{DiskNumber:x8}:0x{OffsetOnTheDisk:x16}";

        internal IVirtualZipFile Host { get; }
    }
}
