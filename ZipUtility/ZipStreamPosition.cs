using System;
using System.IO;

namespace ZipUtility
{
    internal readonly struct ZipStreamPosition
        : IEquatable<ZipStreamPosition>, IComparable<ZipStreamPosition>
    {
        private readonly IVirtualZipFile _multiVolumeInfo;

        internal ZipStreamPosition(UInt32 diskNumber, UInt64 offsetOnTheDisk, IVirtualZipFile multiVolumeInfo)
        {
            if (multiVolumeInfo is null)
                throw new ArgumentNullException(nameof(multiVolumeInfo));

            DiskNumber = diskNumber;
            OffsetOnTheDisk = offsetOnTheDisk;
            _multiVolumeInfo = multiVolumeInfo;
        }

        public UInt32 DiskNumber { get; }
        public UInt64 OffsetOnTheDisk { get; }

        #region operator +

        public static ZipStreamPosition operator +(ZipStreamPosition x, Int64 y) => x.Add(y);
        public static ZipStreamPosition operator +(ZipStreamPosition x, UInt64 y) => x.Add(y);
        public static ZipStreamPosition operator +(ZipStreamPosition x, Int32 y) => x.Add(y);

        #endregion

        #region operator -

        public static UInt64 operator -(ZipStreamPosition x, ZipStreamPosition y) => x.Subtract(y);
        public static ZipStreamPosition operator -(ZipStreamPosition x, Int64 y) => x.Subtract(y);
        public static ZipStreamPosition operator -(ZipStreamPosition x, UInt64 y) => x.Subtract(y);
        public static ZipStreamPosition operator -(ZipStreamPosition x, Int32 y) => x.Subtract(y);

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

        public ZipStreamPosition Add(Int64 x)
            => x >= 0
                ? checked(Add((UInt64)x))
                : x != Int64.MinValue
                ? checked(Subtract((UInt64)(-x)))
                : checked(Subtract(-(Int64.MinValue + 1)).Subtract(1));

        public ZipStreamPosition Add(UInt64 x)
            => _multiVolumeInfo is null
                ? throw new InvalidOperationException("multiVolumeInfo not set")
                : _multiVolumeInfo.Add(this, x) ?? throw new OverflowException();

        public ZipStreamPosition Add(Int32 x)
            => x >= 0
                ? checked(Add((UInt64)x))
                : checked(Subtract(checked((UInt64)(-(Int64)x))));

        #endregion

        #region Subtract

        public UInt64 Subtract(ZipStreamPosition x)
            => _multiVolumeInfo is null
                ? throw new InvalidOperationException("multiVolumeInfo not set")
                : _multiVolumeInfo.Subtract(this, x);

        public ZipStreamPosition Subtract(Int64 x)
            => x >= 0
                ? checked(Subtract((UInt64)x))
                : x != Int64.MinValue
                ? checked(Add((UInt64)(-x)))
                : checked(Add(-(Int64.MinValue + 1)).Add(1));

        public ZipStreamPosition Subtract(UInt64 x)
            => _multiVolumeInfo is null
                ? throw new InvalidOperationException("multiVolumeInfo not set")
                : _multiVolumeInfo.Subtract(this, x) ?? throw new IOException("Invalid file position");

        public ZipStreamPosition Subtract(Int32 x)
            => x >= 0
                ? checked(Subtract((UInt64)x))
                : checked(Add((UInt64)(-(Int64)x)));

        #endregion

        public Int32 CompareTo(ZipStreamPosition other)
        {
            Int32 c;
            return
                (c = DiskNumber.CompareTo(other.DiskNumber)) != 0
                ? c
                : (c = OffsetOnTheDisk.CompareTo(other.OffsetOnTheDisk)) != 0
                ? c
                : 0;
        }

        public Boolean Equals(ZipStreamPosition other)
            => DiskNumber.Equals(other.DiskNumber) &&
                OffsetOnTheDisk.Equals(other.OffsetOnTheDisk);

        public override Boolean Equals(Object? other)
            => other is not null
                && GetType() == other.GetType()
                && Equals((ZipStreamPosition)other);

        public override Int32 GetHashCode() => HashCode.Combine(DiskNumber, OffsetOnTheDisk);

        public override String ToString() => $"0x{DiskNumber:x8}:0x{OffsetOnTheDisk:x16}";
    }
}
