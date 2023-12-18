using System;
using System.Numerics;

namespace Utility.IO.StreamFilters
{
    internal readonly struct InvalidPositionType
        : IComparable<InvalidPositionType>, IAdditionOperators<InvalidPositionType, UInt64, InvalidPositionType>, ISubtractionOperators<InvalidPositionType, UInt64, InvalidPositionType>, ISubtractionOperators<InvalidPositionType, InvalidPositionType, UInt64>
    {
        public readonly Int32 CompareTo(InvalidPositionType other) => throw new InternalLogicalErrorException();
        public static InvalidPositionType operator +(InvalidPositionType left, UInt64 right) => throw new InternalLogicalErrorException();
        public static InvalidPositionType operator -(InvalidPositionType left, UInt64 right) => throw new InternalLogicalErrorException();
        public static UInt64 operator -(InvalidPositionType left, InvalidPositionType right) => throw new InternalLogicalErrorException();
    }
}
