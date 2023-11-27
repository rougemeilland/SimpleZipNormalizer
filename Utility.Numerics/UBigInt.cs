using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace Utility.Numerics
{
    public readonly struct UBigInt
        : IComparable, IComparable<BigInt>, IComparable<UBigInt>, IComparable<BigInteger>, IComparable<Int64>, IComparable<UInt64>, IComparable<Int32>, IComparable<UInt32>, IEquatable<BigInt>, IEquatable<UBigInt>, IEquatable<BigInteger>, IEquatable<Int64>, IEquatable<UInt64>, IEquatable<Int32>, IEquatable<UInt32>, IFormattable, IBigIntInternalValue
    {
        private readonly BigInteger _value;

        #region constructor

        static UBigInt()
        {
            One = new UBigInt(BigInteger.One);
            Zero = new UBigInt(BigInteger.Zero);
        }

        public UBigInt()
            : this(BigInteger.Zero)
        {
        }

        public UBigInt(Int32 value)
            : this(new BigInteger(value))
        {
        }

        public UBigInt(UInt32 value)
            : this(new BigInteger(value))
        {
        }

        public UBigInt(Int64 value)
            : this(new BigInteger(value))
        {
        }

        public UBigInt(UInt64 value)
            : this(new BigInteger(value))
        {
        }

        public UBigInt(BigInteger value)
        {
            if (value < 0)
                throw new OverflowException();

            _value = value;
        }

        public UBigInt(Single value)
            : this(new BigInteger(value))
        {
        }

        public UBigInt(Double value)
            : this(new BigInteger(value))
        {
        }

        public UBigInt(Decimal value)
            : this(new BigInteger(value))
        {
        }

        public UBigInt(ReadOnlyMemory<Byte> value, Boolean isBigEndian = false)
            : this(new BigInteger(value.Span, true, isBigEndian))
        {
        }

        public UBigInt(ReadOnlySpan<Byte> value, Boolean isBigEndian = false)
            : this(new BigInteger(value, true, isBigEndian))
        {
        }

        #endregion

        #region properties

        public static UBigInt Zero { get; }
        public static UBigInt One { get; }
        BigInteger IBigIntInternalValue.Value => _value;

        #endregion

        public UBigInt Add(UBigInt other) => new(_value + other._value);
        public UBigInt Subtract(UBigInt other) => new(_value - other._value);
        public UBigInt Multiply(UBigInt other) => new(_value * other._value);
        public UBigInt Divide(UBigInt other) => new(_value / other._value);

        #region Remainder

        public UBigInt Remainder(UBigInt other) => new(_value % other._value);
        public UInt64 Remainder(UInt64 other) => (UInt64)(_value % other);
        public UInt32 Remainder(UInt32 other) => (UInt32)(_value % other);

        #endregion

        #region DivRem

        public (UBigInt Quotient, UBigInt Remainder) DivRem(UBigInt divisor)
        {
            var quotient = BigInteger.DivRem(_value, divisor._value, out var remainder);
            return (new UBigInt(quotient), new UBigInt(remainder));
        }

        public (UBigInt Quotient, UInt64 Remainder) DivRem(UInt64 divisor)
        {
            var quotient = BigInteger.DivRem(_value, divisor, out var remainder);
            return (new UBigInt(quotient), checked((UInt64)remainder));
        }

        public (UBigInt Quotient, UInt32 Remainder) DivRem(UInt32 divisor)
        {
            var quotient = BigInteger.DivRem(_value, divisor, out var remainder);
            return (new UBigInt(quotient), checked((UInt32)remainder));
        }

        #endregion

        public UBigInt Xor(UBigInt other) => new(_value ^ other._value);

        #region BitwiseAnd

        public UBigInt BitwiseAnd(UBigInt other) => new(_value & other._value);
        public UInt64 BitwiseAnd(UInt64 other) => (UInt64)(_value & other);
        public UInt32 BitwiseAnd(UInt32 other) => (UInt32)(_value & other);

        #endregion

        public UBigInt BitwiseOr(UBigInt other) => new(_value | other._value);
        public UBigInt LeftShift(Int32 shiftCount) => new(_value << shiftCount);
        public UBigInt RightShift(Int32 shiftCount) => new(_value >> shiftCount);
        public UBigInt Decrement() => new(_value - 1);
        public UBigInt Increment() => new(_value + 1);
        public BigInt Negate() => new(BigInteger.Negate(_value));
        public UBigInt Plus() => this;
        public BigInt OnesComplement() => new(~_value);
        public Int64 GetBitLength() => _value.GetBitLength();
        public Int32 GetByteCount() => _value.GetByteCount(false);
        public UBigInt GreatestCommonDivisor(UBigInt other) => new(BigInteger.GreatestCommonDivisor(_value, other._value));
        public UBigInt Pow(Int32 exponent) => new(BigInteger.Pow(_value, exponent));
        public UBigInt ModPow(UBigInt exponent, UBigInt modulus) => new(BigInteger.ModPow(_value, exponent._value, modulus._value));
        public Double Log() => BigInteger.Log(_value);
        public Double Log(Double baseValue) => BigInteger.Log(_value, baseValue);
        public Double Log10() => BigInteger.Log10(_value);

        #region Parse

        public static UBigInt Parse(String value) => new(BigInteger.Parse(value));
        public static UBigInt Parse(String value, NumberStyles style) => new(BigInteger.Parse(value, style));
        public static UBigInt Parse(String value, IFormatProvider? provider) => new(BigInteger.Parse(value, provider));
        public static UBigInt Parse(String value, NumberStyles style, IFormatProvider? provider) => new(BigInteger.Parse(value, style, provider));

        #endregion

        #region TryParse

        public static Boolean TryParse([NotNullWhen(true)] String? value, NumberStyles style, IFormatProvider? provider, out UBigInt result)
        {
            if (!BigInteger.TryParse(value, style, provider, out BigInteger bigIntegerValue))
            {
                result = Zero;
                return false;
            }

            result = new UBigInt(bigIntegerValue);
            return true;
        }

        public static Boolean TryParse([NotNullWhen(true)] String? value, out UBigInt result)
        {
            if (!BigInteger.TryParse(value, out BigInteger bigIntegerValue))
            {
                result = Zero;
                return false;
            }

            result = new UBigInt(bigIntegerValue);
            return true;
        }

        #endregion

        #region ToByteArray

        public Byte[] ToByteArray(Boolean isBigEndian = false) => _value.ToByteArray(true, isBigEndian);

        #endregion

        #region CompareTo

        public Int32 CompareTo(BigInt other) => _value.CompareTo(((IBigIntInternalValue)other).Value);
        public Int32 CompareTo(UBigInt other) => _value.CompareTo(other._value);
        public Int32 CompareTo(BigInteger other) => _value.CompareTo(other);
        public Int32 CompareTo(Int64 other) => _value.CompareTo(other);
        public Int32 CompareTo(UInt64 other) => _value.CompareTo(other);
        public Int32 CompareTo(Int32 other) => _value.CompareTo(other);
        public Int32 CompareTo(UInt32 other) => _value.CompareTo(other);

        public Int32 CompareTo(Object? obj)
            => obj is null
                ? 1
                : obj is BigInt BigIntValue
                ? CompareTo(BigIntValue)
                : obj is UBigInt UBigIntValue
                ? CompareTo(UBigIntValue)
                : obj is BigInteger BigIntegerValue
                ? CompareTo(BigIntegerValue)
                : obj is Int64 Int64Value
                ? CompareTo(Int64Value)
                : obj is UInt64 UInt64Value
                ? CompareTo(UInt64Value)
                : obj is Int32 Int32Value
                ? CompareTo(Int32Value)
                : obj is UInt32 UInt32Value
                ? CompareTo(UInt32Value)
                : _value.CompareTo(obj);

        #endregion

        #region Equals

        public Boolean Equals(BigInt other) => _value.Equals(((IBigIntInternalValue)other).Value);
        public Boolean Equals(UBigInt other) => _value.Equals(other._value);
        public Boolean Equals(BigInteger other) => _value.Equals(other);
        public Boolean Equals(Int64 other) => _value.Equals(other);
        public Boolean Equals(UInt64 other) => _value.Equals(other);
        public Boolean Equals(Int32 other) => _value.Equals(other);
        public Boolean Equals(UInt32 other) => _value.Equals(other);

        public override Boolean Equals([NotNullWhen(true)] Object? obj)
            => obj is null
                ? false
                : obj is BigInt BigIntValue
                ? Equals(BigIntValue)
                : obj is UBigInt UBigIntValue
                ? Equals(UBigIntValue)
                : obj is BigInteger BigIntegerValue
                ? Equals(BigIntegerValue)
                : obj is Int64 Int64Value
                ? Equals(Int64Value)
                : obj is UInt64 UInt64Value
                ? Equals(UInt64Value)
                : obj is Int32 Int32Value
                ? Equals(Int32Value)
                : obj is UInt32 UInt32Value
                ? Equals(UInt32Value)
                : _value.Equals(obj);

        #endregion

        public override Int32 GetHashCode() => _value.GetHashCode();

        #region ToString

        public String ToString(String? format) => _value.ToString(format, null);
        public String ToString(IFormatProvider? formatProvider) => _value.ToString(null, formatProvider);
        public String ToString(String? format, IFormatProvider? formatProvider) => _value.ToString(format, formatProvider);
        public override String ToString() => _value.ToString();

        #endregion

        public static UBigInt operator +(UBigInt left, UBigInt right) => new(left._value + right._value);

        #region operator -

        public static UBigInt operator -(UBigInt left, UBigInt right) => new(left._value - right._value);
        public static UInt64 operator -(UInt64 left, UBigInt right) => (UInt64)(left - right._value);
        public static UInt32 operator -(UInt32 left, UBigInt right) => (UInt32)(left - right._value);

        #endregion

        public static UBigInt operator *(UBigInt left, UBigInt right) => new(left._value * right._value);

        #region operator /

        public static UBigInt operator /(UBigInt left, UBigInt right) => new(left._value / right._value);
        public static UBigInt operator /(UBigInt left, UInt64 right) => new(left._value / right);
        public static UBigInt operator /(UBigInt left, UInt32 right) => new(left._value / right);
        public static UInt64 operator /(UInt64 left, UBigInt right) => (UInt64)(left / right._value);
        public static UInt32 operator /(UInt32 left, UBigInt right) => (UInt32)(left / right._value);

        #endregion

        #region operator %

        public static UBigInt operator %(UBigInt left, UBigInt right) => new(left._value % right._value);
        public static UInt64 operator %(UBigInt left, UInt64 right) => (UInt64)(left._value % right);
        public static UInt32 operator %(UBigInt left, UInt32 right) => (UInt32)(left._value % right);
        public static UInt64 operator %(UInt64 left, UBigInt right) => (UInt64)(left % right._value);
        public static UInt32 operator %(UInt32 left, UBigInt right) => (UInt32)(left % right._value);

        #endregion

        public static UBigInt operator ++(UBigInt value) => new(value._value + 1);
        public static UBigInt operator --(UBigInt value) => new(value._value - 1);
        public static UBigInt operator +(UBigInt value) => value;
        public static BigInt operator -(UBigInt value) => new(-value._value);

        public static UBigInt operator <<(UBigInt value, Int32 shift) => new(value._value << shift);
        public static UBigInt operator >>(UBigInt value, Int32 shift) => new(value._value >> shift);

        #region operator &

        public static UBigInt operator &(UBigInt left, UBigInt right) => new(left._value & right._value);
        public static UInt64 operator &(UBigInt left, UInt64 right) => (UInt64)(left._value & right);
        public static UInt32 operator &(UBigInt left, UInt32 right) => (UInt32)(left._value & right);
        public static UInt64 operator &(UInt64 left, UBigInt right) => (UInt64)(left & right._value);
        public static UInt32 operator &(UInt32 left, UBigInt right) => (UInt32)(left & right._value);

        #endregion

        public static UBigInt operator |(UBigInt left, UBigInt right) => new(left._value | right._value);

        public static UBigInt operator ^(UBigInt left, UBigInt right) => new(left._value ^ right._value);

        #region oeprator ==

        public static Boolean operator ==(UBigInt left, UBigInt right) => left.Equals(right);
        public static Boolean operator ==(UBigInt left, BigInteger right) => left.Equals(right);
        public static Boolean operator ==(UBigInt left, Int64 right) => left.Equals(right);
        public static Boolean operator ==(UBigInt left, UInt64 right) => left.Equals(right);
        public static Boolean operator ==(UBigInt left, Int32 right) => left.Equals(right);
        public static Boolean operator ==(UBigInt left, UInt32 right) => left.Equals(right);
        public static Boolean operator ==(BigInteger left, UBigInt right) => right.Equals(left);
        public static Boolean operator ==(Int64 left, UBigInt right) => right.Equals(left);
        public static Boolean operator ==(UInt64 left, UBigInt right) => right.Equals(left);
        public static Boolean operator ==(Int32 left, UBigInt right) => right.Equals(left);
        public static Boolean operator ==(UInt32 left, UBigInt right) => right.Equals(left);

        #endregion

        #region oeprator !=

        public static Boolean operator !=(UBigInt left, UBigInt right) => !left.Equals(right);
        public static Boolean operator !=(UBigInt left, BigInteger right) => !left.Equals(right);
        public static Boolean operator !=(UBigInt left, Int64 right) => !left.Equals(right);
        public static Boolean operator !=(UBigInt left, UInt64 right) => !left.Equals(right);
        public static Boolean operator !=(UBigInt left, Int32 right) => !left.Equals(right);
        public static Boolean operator !=(UBigInt left, UInt32 right) => !left.Equals(right);
        public static Boolean operator !=(BigInteger left, UBigInt right) => !right.Equals(left);
        public static Boolean operator !=(Int64 left, UBigInt right) => !right.Equals(left);
        public static Boolean operator !=(UInt64 left, UBigInt right) => !right.Equals(left);
        public static Boolean operator !=(Int32 left, UBigInt right) => !right.Equals(left);
        public static Boolean operator !=(UInt32 left, UBigInt right) => !right.Equals(left);

        #endregion

        #region oeprator >

        public static Boolean operator >(UBigInt left, UBigInt right) => left.CompareTo(right) > 0;
        public static Boolean operator >(UBigInt left, BigInteger right) => left.CompareTo(right) > 0;
        public static Boolean operator >(UBigInt left, Int64 right) => left.CompareTo(right) > 0;
        public static Boolean operator >(UBigInt left, UInt64 right) => left.CompareTo(right) > 0;
        public static Boolean operator >(UBigInt left, Int32 right) => left.CompareTo(right) > 0;
        public static Boolean operator >(UBigInt left, UInt32 right) => left.CompareTo(right) > 0;
        public static Boolean operator >(BigInteger left, UBigInt right) => right.CompareTo(left) < 0;
        public static Boolean operator >(Int64 left, UBigInt right) => right.CompareTo(left) < 0;
        public static Boolean operator >(UInt64 left, UBigInt right) => right.CompareTo(left) < 0;
        public static Boolean operator >(Int32 left, UBigInt right) => right.CompareTo(left) < 0;
        public static Boolean operator >(UInt32 left, UBigInt right) => right.CompareTo(left) < 0;

        #endregion

        #region oeprator >=

        public static Boolean operator >=(UBigInt left, UBigInt right) => left.CompareTo(right) >= 0;
        public static Boolean operator >=(UBigInt left, BigInteger right) => left.CompareTo(right) >= 0;
        public static Boolean operator >=(UBigInt left, Int64 right) => left.CompareTo(right) >= 0;
        public static Boolean operator >=(UBigInt left, UInt64 right) => left.CompareTo(right) >= 0;
        public static Boolean operator >=(UBigInt left, Int32 right) => left.CompareTo(right) >= 0;
        public static Boolean operator >=(UBigInt left, UInt32 right) => left.CompareTo(right) >= 0;
        public static Boolean operator >=(BigInteger left, UBigInt right) => right.CompareTo(left) <= 0;
        public static Boolean operator >=(Int64 left, UBigInt right) => right.CompareTo(left) <= 0;
        public static Boolean operator >=(UInt64 left, UBigInt right) => right.CompareTo(left) <= 0;
        public static Boolean operator >=(Int32 left, UBigInt right) => right.CompareTo(left) <= 0;
        public static Boolean operator >=(UInt32 left, UBigInt right) => right.CompareTo(left) <= 0;

        #endregion

        #region oeprator <

        public static Boolean operator <(UBigInt left, UBigInt right) => left.CompareTo(right) < 0;
        public static Boolean operator <(UBigInt left, BigInteger right) => left.CompareTo(right) < 0;
        public static Boolean operator <(UBigInt left, Int64 right) => left.CompareTo(right) < 0;
        public static Boolean operator <(UBigInt left, UInt64 right) => left.CompareTo(right) < 0;
        public static Boolean operator <(UBigInt left, Int32 right) => left.CompareTo(right) < 0;
        public static Boolean operator <(UBigInt left, UInt32 right) => left.CompareTo(right) < 0;
        public static Boolean operator <(BigInteger left, UBigInt right) => right.CompareTo(left) > 0;
        public static Boolean operator <(Int64 left, UBigInt right) => right.CompareTo(left) > 0;
        public static Boolean operator <(UInt64 left, UBigInt right) => right.CompareTo(left) > 0;
        public static Boolean operator <(Int32 left, UBigInt right) => right.CompareTo(left) > 0;
        public static Boolean operator <(UInt32 left, UBigInt right) => right.CompareTo(left) > 0;

        #endregion

        #region oeprator <=

        public static Boolean operator <=(UBigInt left, UBigInt right) => left.CompareTo(right) <= 0;
        public static Boolean operator <=(UBigInt left, BigInteger right) => left.CompareTo(right) <= 0;
        public static Boolean operator <=(UBigInt left, Int64 right) => left.CompareTo(right) <= 0;
        public static Boolean operator <=(UBigInt left, UInt64 right) => left.CompareTo(right) <= 0;
        public static Boolean operator <=(UBigInt left, Int32 right) => left.CompareTo(right) <= 0;
        public static Boolean operator <=(UBigInt left, UInt32 right) => left.CompareTo(right) <= 0;
        public static Boolean operator <=(BigInteger left, UBigInt right) => right.CompareTo(left) >= 0;
        public static Boolean operator <=(Int64 left, UBigInt right) => right.CompareTo(left) >= 0;
        public static Boolean operator <=(UInt64 left, UBigInt right) => right.CompareTo(left) >= 0;
        public static Boolean operator <=(Int32 left, UBigInt right) => right.CompareTo(left) >= 0;
        public static Boolean operator <=(UInt32 left, UBigInt right) => right.CompareTo(left) >= 0;

        #endregion

        #region operator explicit

        public static explicit operator SByte(UBigInt value) => (SByte)value._value;
        public static explicit operator Byte(UBigInt value) => (Byte)value._value;
        public static explicit operator Int16(UBigInt value) => (Int16)value._value;
        public static explicit operator UInt16(UBigInt value) => (UInt16)value._value;
        public static explicit operator Int32(UBigInt value) => (Int32)value._value;
        public static explicit operator UInt32(UBigInt value) => (UInt32)value._value;
        public static explicit operator Int64(UBigInt value) => (Int64)value._value;
        public static explicit operator UInt64(UBigInt value) => (UInt64)value._value;
        public static explicit operator BigInteger(UBigInt value) => value._value;
        public static explicit operator Single(UBigInt value) => (Single)value._value;
        public static explicit operator Double(UBigInt value) => (Double)value._value;
        public static explicit operator Decimal(UBigInt value) => (Decimal)value._value;
        public static explicit operator UBigInt(Single value) => new(value);
        public static explicit operator UBigInt(Double value) => new(value);
        public static explicit operator UBigInt(Decimal value) => new(value);

        #endregion

        #region operator implicit

        public static implicit operator UBigInt(Byte value) => new(value);
        public static implicit operator UBigInt(UInt16 value) => new(value);
        public static implicit operator UBigInt(UInt32 value) => new(value);
        public static implicit operator UBigInt(UInt64 value) => new(value);

        #endregion
    }
}
