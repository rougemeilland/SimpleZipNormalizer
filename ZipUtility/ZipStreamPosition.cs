using System;
using System.Numerics;
using Utility;

namespace ZipUtility
{
    internal readonly struct ZipStreamPosition
        : IEquatable<ZipStreamPosition>, IComparable<ZipStreamPosition>, IAdditionOperators<ZipStreamPosition, UInt64, ZipStreamPosition>, ISubtractionOperators<ZipStreamPosition, UInt64, ZipStreamPosition>, ISubtractionOperators<ZipStreamPosition, ZipStreamPosition, UInt64>
    {
        internal ZipStreamPosition(UInt32 diskNumber, UInt64 offsetOnTheDisk, IVirtualZipFile ownerVirtualDisk)
        {
            if (ownerVirtualDisk is null)
                throw new ArgumentNullException(nameof(ownerVirtualDisk));

            DiskNumber = diskNumber;
            OffsetOnTheDisk = offsetOnTheDisk;
            Owner = ownerVirtualDisk;
        }

        public UInt32 DiskNumber { get; }
        public UInt64 OffsetOnTheDisk { get; }
        #region operator +

        public static ZipStreamPosition operator +(ZipStreamPosition x, UInt64 y) => x.Add(y);

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
            => Owner.Add(this, x);

        #endregion

        #region Subtract

        public UInt64 Subtract(ZipStreamPosition x)
            => Owner.Subtract(this, x);

        public ZipStreamPosition Subtract(UInt64 x)
            => Owner.Subtract(this, x);

        #endregion

        public Int32 CompareTo(ZipStreamPosition other)
        {
            if (!Owner.Equals(other.Owner))
                throw new InternalLogicalErrorException();

            Int32 c;
            return
                (c = DiskNumber.CompareTo(other.DiskNumber)) != 0
                ? c
                : (c = OffsetOnTheDisk.CompareTo(other.OffsetOnTheDisk)) != 0
                ? c
                : 0;
        }

        public Boolean Equals(ZipStreamPosition other)
            => Owner.Equals(other.Owner)
                && DiskNumber.Equals(other.DiskNumber)
                && OffsetOnTheDisk.Equals(other.OffsetOnTheDisk);

        public override Boolean Equals(Object? other)
            => other is not null
                && GetType() == other.GetType()
                && Equals((ZipStreamPosition)other);

        public override Int32 GetHashCode() => HashCode.Combine(DiskNumber, OffsetOnTheDisk);

        public override String ToString() => $"0x{DiskNumber:x8}:0x{OffsetOnTheDisk:x16}";

        internal IVirtualZipFile Owner { get; }
    }
}
