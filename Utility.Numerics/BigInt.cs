using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace Utility.Numerics
{
    public readonly struct BigInt
        : IComparable, IComparable<BigInt>, IComparable<UBigInt>, IComparable<BigInteger>, IComparable<Int64>, IComparable<UInt64>, IComparable<Int32>, IComparable<UInt32>, IEquatable<BigInt>, IEquatable<UBigInt>, IEquatable<BigInteger>, IEquatable<Int64>, IEquatable<UInt64>, IEquatable<Int32>, IEquatable<UInt32>, IFormattable, IBigIntInternalValue
    {
        private readonly BigInteger _value;

        #region constructor

        static BigInt()
        {
            One = new BigInt(BigInteger.One);
            Zero = new BigInt(BigInteger.Zero);
            MinusOne = new BigInt(new BigInteger(-1));
        }

        public BigInt()
            : this(BigInteger.Zero)
        {
        }

        public BigInt(Int32 value)
            : this(new BigInteger(value))
        {
        }

        public BigInt(UInt32 value)
            : this(new BigInteger(value))
        {
        }

        public BigInt(Int64 value)
            : this(new BigInteger(value))
        {
        }

        public BigInt(UInt64 value)
            : this(new BigInteger(value))
        {
        }

        public BigInt(BigInteger value)
        {
            _value = value;
        }

        public BigInt(Single value)
            : this(new BigInteger(value))
        {
        }

        public BigInt(Double value)
            : this(new BigInteger(value))
        {
        }

        public BigInt(Decimal value)
            : this(new BigInteger(value))
        {
        }

        public BigInt(ReadOnlyMemory<Byte> value, Boolean isBigEndian = false)
            : this(new BigInteger(value.Span, false, isBigEndian))
        {
        }

        public BigInt(ReadOnlySpan<Byte> value, Boolean isBigEndian = false)
            : this(new BigInteger(value, false, isBigEndian))
        {
        }

        #endregion

        #region properties

        public static BigInt Zero { get; }
        public static BigInt One { get; }
        public static BigInt MinusOne { get; }
        BigInteger IBigIntInternalValue.Value => _value;

        #endregion

        public BigInt Add(BigInt other) => new(_value + other._value);
        public BigInt Subtract(BigInt other) => new(_value - other._value);
        public BigInt Multiply(BigInt other) => new(_value * other._value);
        public BigInt Divide(BigInt other) => new(_value / other._value);

        #region Remainder

        public BigInt Remainder(BigInt other) => new(_value % other._value);
        public Int64 Remainder(Int64 other) => (Int64)(_value % other);
        public Int32 Remainder(Int32 other) => (Int32)(_value % other);

        #endregion

        #region DivRem

        public (BigInt Quotient, BigInt Remainder) DivRem(BigInt divisor)
        {
            var quotient = BigInteger.DivRem(_value, divisor._value, out var remainder);
            return (new BigInt(quotient), new BigInt(remainder));
        }

        public (BigInt Quotient, Int64 Remainder) DivRem(Int64 divisor)
        {
            var quotient = BigInteger.DivRem(_value, divisor, out var remainder);
            return (new BigInt(quotient), checked((Int64)remainder));
        }

        public (BigInt Quotient, Int32 Remainder) DivRem(Int32 divisor)
        {
            var quotient = BigInteger.DivRem(_value, divisor, out var remainder);
            return (new BigInt(quotient), checked((Int32)remainder));
        }

        #endregion

        public BigInt Xor(BigInt other) => new(_value ^ other._value);
        public BigInt BitwiseAnd(BigInt other) => new(_value & other._value);
        public BigInt BitwiseOr(BigInt other) => new(_value | other._value);
        public BigInt LeftShift(Int32 shiftCount) => new(_value << shiftCount);
        public BigInt RightShift(Int32 shiftCount) => new(_value >> shiftCount);
        public BigInt Decrement() => new(_value - 1);
        public BigInt Increment() => new(_value + 1);
        public BigInt Negate() => new(BigInteger.Negate(_value));
        public BigInt Plus() => this;
        public BigInt OnesComplement() => new(~_value);
        public Int64 GetBitLength() => _value.GetBitLength();
        public Int32 GetByteCount() => _value.GetByteCount(false);
        public BigInt GreatestCommonDivisor(BigInt other) => new(BigInteger.GreatestCommonDivisor(_value, other._value));
        public BigInt Pow(Int32 exponent) => new(BigInteger.Pow(_value, exponent));
        public BigInt ModPow(BigInt exponent, BigInt modulus) => new(BigInteger.ModPow(_value, exponent._value, modulus._value));
        public Double Log() => BigInteger.Log(_value);
        public Double Log(Double baseValue) => BigInteger.Log(_value, baseValue);
        public Double Log10() => BigInteger.Log10(_value);

        #region Parse

        public static BigInt Parse(String value) => new(BigInteger.Parse(value));
        public static BigInt Parse(String value, NumberStyles style) => new(BigInteger.Parse(value, style));
        public static BigInt Parse(String value, IFormatProvider? provider) => new(BigInteger.Parse(value, provider));
        public static BigInt Parse(String value, NumberStyles style, IFormatProvider? provider) => new(BigInteger.Parse(value, style, provider));

        #endregion

        #region TryParse

        public static Boolean TryParse([NotNullWhen(true)] String? value, NumberStyles style, IFormatProvider? provider, out BigInt result)
        {
            if (!BigInteger.TryParse(value, style, provider, out BigInteger bigIntegerValue))
            {
                result = Zero;
                return false;
            }

            result = new BigInt(bigIntegerValue);
            return true;
        }

        public static Boolean TryParse([NotNullWhen(true)] String? value, out BigInt result)
        {
            if (!BigInteger.TryParse(value, out BigInteger bigIntegerValue))
            {
                result = Zero;
                return false;
            }

            result = new BigInt(bigIntegerValue);
            return true;
        }

        #endregion

        #region ToByteArray

        public Byte[] ToByteArray(Boolean isBigEndian = false) => _value.ToByteArray(false, isBigEndian);

        #endregion

        #region CompareTo

        public Int32 CompareTo(BigInt other) => _value.CompareTo(other._value);
        public Int32 CompareTo(UBigInt other) => _value.CompareTo(((IBigIntInternalValue)other).Value);
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

        public Boolean Equals(BigInt other) => _value.Equals(other._value);
        public Boolean Equals(UBigInt other) => _value.Equals(((IBigIntInternalValue)other).Value);
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

        public static BigInt operator +(BigInt left, BigInt right) => new(left._value + right._value);
        public static BigInt operator -(BigInt left, BigInt right) => new(left._value - right._value);
        public static BigInt operator *(BigInt left, BigInt right) => new(left._value * right._value);

        #region operator /

        public static BigInt operator /(BigInt left, BigInt right) => new(left._value / right._value);
        public static Int64 operator /(Int32 left, BigInt right) => (Int64)(left / right._value);

        #endregion

        #region operator %

        public static BigInt operator %(BigInt left, BigInt right) => new(left._value % right._value);
        public static Int64 operator %(BigInt left, Int64 right) => (Int64)(left._value % right);
        public static Int32 operator %(BigInt left, Int32 right) => (Int32)(left._value % right);
        public static Int64 operator %(Int64 left, BigInt right) => (Int64)(left % right._value);
        public static Int32 operator %(Int32 left, BigInt right) => (Int32)(left % right._value);

        #endregion

        public static BigInt operator ++(BigInt value) => new(value._value + 1);
        public static BigInt operator --(BigInt value) => new(value._value - 1);
        public static BigInt operator +(BigInt value) => value;
        public static BigInt operator -(BigInt value) => new(-value._value);
        public static BigInt operator <<(BigInt value, Int32 shift) => new(value._value << shift);
        public static BigInt operator >>(BigInt value, Int32 shift) => new(value._value >> shift);
        public static BigInt operator &(BigInt left, BigInt right) => new(left._value & right._value);
        public static BigInt operator |(BigInt left, BigInt right) => new(left._value | right._value);
        public static BigInt operator ^(BigInt left, BigInt right) => new(left._value ^ right._value);

        #region oeprator ==

        public static Boolean operator ==(BigInt left, BigInt right) => left.Equals(right);
        public static Boolean operator ==(BigInt left, UBigInt right) => left.Equals(right);
        public static Boolean operator ==(BigInt left, BigInteger right) => left.Equals(right);
        public static Boolean operator ==(BigInt left, Int64 right) => left.Equals(right);
        public static Boolean operator ==(BigInt left, UInt64 right) => left.Equals(right);
        public static Boolean operator ==(BigInt left, Int32 right) => left.Equals(right);
        public static Boolean operator ==(BigInt left, UInt32 right) => left.Equals(right);
        public static Boolean operator ==(UBigInt left, BigInt right) => right.Equals(left);
        public static Boolean operator ==(BigInteger left, BigInt right) => right.Equals(left);
        public static Boolean operator ==(Int64 left, BigInt right) => right.Equals(left);
        public static Boolean operator ==(UInt64 left, BigInt right) => right.Equals(left);
        public static Boolean operator ==(Int32 left, BigInt right) => right.Equals(left);
        public static Boolean operator ==(UInt32 left, BigInt right) => right.Equals(left);

        #endregion

        #region oeprator !=

        public static Boolean operator !=(BigInt left, BigInt right) => !left.Equals(right);
        public static Boolean operator !=(BigInt left, UBigInt right) => !left.Equals(right);
        public static Boolean operator !=(BigInt left, BigInteger right) => !left.Equals(right);
        public static Boolean operator !=(BigInt left, Int64 right) => !left.Equals(right);
        public static Boolean operator !=(BigInt left, UInt64 right) => !left.Equals(right);
        public static Boolean operator !=(BigInt left, Int32 right) => !left.Equals(right);
        public static Boolean operator !=(BigInt left, UInt32 right) => !left.Equals(right);
        public static Boolean operator !=(UBigInt left, BigInt right) => !right.Equals(left);
        public static Boolean operator !=(BigInteger left, BigInt right) => !right.Equals(left);
        public static Boolean operator !=(Int64 left, BigInt right) => !right.Equals(left);
        public static Boolean operator !=(UInt64 left, BigInt right) => !right.Equals(left);
        public static Boolean operator !=(Int32 left, BigInt right) => !right.Equals(left);
        public static Boolean operator !=(UInt32 left, BigInt right) => !right.Equals(left);

        #endregion

        #region oeprator >

        public static Boolean operator >(BigInt left, BigInt right) => left.CompareTo(right) > 0;
        public static Boolean operator >(BigInt left, UBigInt right) => left.CompareTo(right) > 0;
        public static Boolean operator >(BigInt left, BigInteger right) => left.CompareTo(right) > 0;
        public static Boolean operator >(BigInt left, Int64 right) => left.CompareTo(right) > 0;
        public static Boolean operator >(BigInt left, UInt64 right) => left.CompareTo(right) > 0;
        public static Boolean operator >(BigInt left, Int32 right) => left.CompareTo(right) > 0;
        public static Boolean operator >(BigInt left, UInt32 right) => left.CompareTo(right) > 0;
        public static Boolean operator >(UBigInt left, BigInt right) => right.CompareTo(left) < 0;
        public static Boolean operator >(BigInteger left, BigInt right) => right.CompareTo(left) < 0;
        public static Boolean operator >(Int64 left, BigInt right) => right.CompareTo(left) < 0;
        public static Boolean operator >(UInt64 left, BigInt right) => right.CompareTo(left) < 0;
        public static Boolean operator >(Int32 left, BigInt right) => right.CompareTo(left) < 0;
        public static Boolean operator >(UInt32 left, BigInt right) => right.CompareTo(left) < 0;

        #endregion

        #region oeprator >=

        public static Boolean operator >=(BigInt left, BigInt right) => left.CompareTo(right) >= 0;
        public static Boolean operator >=(BigInt left, UBigInt right) => left.CompareTo(right) >= 0;
        public static Boolean operator >=(BigInt left, BigInteger right) => left.CompareTo(right) >= 0;
        public static Boolean operator >=(BigInt left, Int64 right) => left.CompareTo(right) >= 0;
        public static Boolean operator >=(BigInt left, UInt64 right) => left.CompareTo(right) >= 0;
        public static Boolean operator >=(BigInt left, Int32 right) => left.CompareTo(right) >= 0;
        public static Boolean operator >=(BigInt left, UInt32 right) => left.CompareTo(right) >= 0;
        public static Boolean operator >=(UBigInt left, BigInt right) => right.CompareTo(left) <= 0;
        public static Boolean operator >=(BigInteger left, BigInt right) => right.CompareTo(left) <= 0;
        public static Boolean operator >=(Int64 left, BigInt right) => right.CompareTo(left) <= 0;
        public static Boolean operator >=(UInt64 left, BigInt right) => right.CompareTo(left) <= 0;
        public static Boolean operator >=(Int32 left, BigInt right) => right.CompareTo(left) <= 0;
        public static Boolean operator >=(UInt32 left, BigInt right) => right.CompareTo(left) <= 0;

        #endregion

        #region oeprator <

        public static Boolean operator <(BigInt left, BigInt right) => left.CompareTo(right) < 0;
        public static Boolean operator <(BigInt left, UBigInt right) => left.CompareTo(right) < 0;
        public static Boolean operator <(BigInt left, BigInteger right) => left.CompareTo(right) < 0;
        public static Boolean operator <(BigInt left, Int64 right) => left.CompareTo(right) < 0;
        public static Boolean operator <(BigInt left, UInt64 right) => left.CompareTo(right) < 0;
        public static Boolean operator <(BigInt left, Int32 right) => left.CompareTo(right) < 0;
        public static Boolean operator <(BigInt left, UInt32 right) => left.CompareTo(right) < 0;
        public static Boolean operator <(UBigInt left, BigInt right) => right.CompareTo(left) > 0;
        public static Boolean operator <(BigInteger left, BigInt right) => right.CompareTo(left) > 0;
        public static Boolean operator <(Int64 left, BigInt right) => right.CompareTo(left) > 0;
        public static Boolean operator <(UInt64 left, BigInt right) => right.CompareTo(left) > 0;
        public static Boolean operator <(Int32 left, BigInt right) => right.CompareTo(left) > 0;
        public static Boolean operator <(UInt32 left, BigInt right) => right.CompareTo(left) > 0;

        #endregion

        #region oeprator <=

        public static Boolean operator <=(BigInt left, BigInt right) => left.CompareTo(right) <= 0;
        public static Boolean operator <=(BigInt left, UBigInt right) => left.CompareTo(right) <= 0;
        public static Boolean operator <=(BigInt left, BigInteger right) => left.CompareTo(right) <= 0;
        public static Boolean operator <=(BigInt left, Int64 right) => left.CompareTo(right) <= 0;
        public static Boolean operator <=(BigInt left, UInt64 right) => left.CompareTo(right) <= 0;
        public static Boolean operator <=(BigInt left, Int32 right) => left.CompareTo(right) <= 0;
        public static Boolean operator <=(BigInt left, UInt32 right) => left.CompareTo(right) <= 0;
        public static Boolean operator <=(UBigInt left, BigInt right) => right.CompareTo(left) >= 0;
        public static Boolean operator <=(BigInteger left, BigInt right) => right.CompareTo(left) >= 0;
        public static Boolean operator <=(Int64 left, BigInt right) => right.CompareTo(left) >= 0;
        public static Boolean operator <=(UInt64 left, BigInt right) => right.CompareTo(left) >= 0;
        public static Boolean operator <=(Int32 left, BigInt right) => right.CompareTo(left) >= 0;
        public static Boolean operator <=(UInt32 left, BigInt right) => right.CompareTo(left) >= 0;

        #endregion

        #region operator explicit

        public static explicit operator SByte(BigInt value) => (SByte)value._value;
        public static explicit operator Byte(BigInt value) => (Byte)value._value;
        public static explicit operator Int16(BigInt value) => (Int16)value._value;
        public static explicit operator UInt16(BigInt value) => (UInt16)value._value;
        public static explicit operator Int32(BigInt value) => (Int32)value._value;
        public static explicit operator UInt32(BigInt value) => (UInt32)value._value;
        public static explicit operator Int64(BigInt value) => (Int64)value._value;
        public static explicit operator UInt64(BigInt value) => (UInt64)value._value;
        public static explicit operator BigInteger(BigInt value) => value._value;
        public static explicit operator Single(BigInt value) => (Single)value._value;
        public static explicit operator Double(BigInt value) => (Double)value._value;
        public static explicit operator Decimal(BigInt value) => (Decimal)value._value;
        public static explicit operator UBigInt(BigInt value) => new(value._value);
        public static explicit operator BigInt(Single value) => new(value);
        public static explicit operator BigInt(Double value) => new(value);
        public static explicit operator BigInt(Decimal value) => new(value);

        #endregion

        #region operator implicit

        public static implicit operator BigInt(SByte value) => new(value);
        public static implicit operator BigInt(Int16 value) => new(value);
        public static implicit operator BigInt(Int32 value) => new(value);
        public static implicit operator BigInt(Int64 value) => new(value);
        public static implicit operator BigInt(BigInteger value) => new(value);
        public static implicit operator BigInt(UBigInt value) => new(((IBigIntInternalValue)value).Value);

        #endregion
    }
}
