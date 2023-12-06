using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Utility
{
    public static class NumericExtensions
    {
        private const Int32 _BIT_LENGTH_OF_BYTE = sizeof(Byte) << 3;
        private const Int32 _BIT_LENGTH_OF_UINT16 = sizeof(UInt16) << 3;
        private const Int32 _BIT_LENGTH_OF_UINT32 = sizeof(UInt32) << 3;
        private const Int32 _BIT_LENGTH_OF_UINT64 = sizeof(UInt64) << 3;

        public static Boolean SignEquals(this Int32 value, Int32 other)
            => value > 0
                ? other > 0
                : value < 0
                ? other < 0
                : other == 0;

        #region AddAsUInt

        /// <summary>
        /// <see cref="UInt64"/> 値と <see cref="Int64"/> 値の和を計算します。
        /// </summary>
        /// <param name="x">
        /// <see cref="UInt64"/> 値です。
        /// </param>
        /// <param name="y">
        /// <see cref="Int64"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="x"/> と <paramref name="y"/> の和を示す <see cref="UInt64"/> 値です。
        /// </returns>
        /// <exception cref="OverflowException">計算結果が<see cref="UInt64"/>で表現できる範囲を超えた場合。</exception>
        /// <remarks>
        /// 計算の際にオーバーフローの検査を行います。
        /// </remarks>
        public static UInt64 AddAsUInt(this UInt64 x, Int64 y)
            => y >= 0
                ? checked(x + (UInt64)y)
                : y != Int64.MinValue
                ? checked(x - (UInt64)(-y))
                : checked(x - Int64.MaxValue - 1UL); // -Int64.MinValue == Int64.MaxValue + 1

        /// <summary>
        /// <see cref="UInt32"/> 値と <see cref="Int32"/> 値の和を計算します。
        /// </summary>
        /// <param name="x">
        /// <see cref="UInt32"/> 値です。
        /// </param>
        /// <param name="y">
        /// <see cref="Int32"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="x"/> と <paramref name="y"/> の和を示す <see cref="UInt32"/> 値です。
        /// </returns>
        /// <exception cref="OverflowException">計算結果が<see cref="UInt32"/>で表現できる範囲を超えた場合。</exception>
        /// <remarks>
        /// 計算の際にオーバーフローの検査を行います。
        /// </remarks>
        public static UInt32 AddAsUInt(this UInt32 x, Int32 y)
            => y >= 0
                ? checked(x + (UInt32)y)
                : y != Int32.MinValue
                ? checked(x - (UInt32)(-y))
                : checked(x - Int32.MaxValue - 1U); // -Int32.MinValue == Int32.MaxValue + 1

        #endregion

        #region SubtractAsUInt

        /// <summary>
        /// <see cref="UInt64"/> 値と <see cref="Int64"/> 値の差を計算します。
        /// </summary>
        /// <param name="x">
        /// <see cref="UInt64"/> 値です。
        /// </param>
        /// <param name="y">
        /// <see cref="Int64"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="x"/> と <paramref name="y"/> の差を示す <see cref="UInt64"/> 値です。
        /// </returns>
        /// <exception cref="OverflowException">計算結果が<see cref="UInt64"/>で表現できる範囲を超えた場合。</exception>
        /// <remarks>
        /// 計算の際にオーバーフローの検査を行います。
        /// </remarks>
        public static UInt64 SubtractAsUInt(this UInt64 x, Int64 y)
            => y >= 0
                ? checked(x - (UInt64)y)
                : y != Int64.MinValue
                ? checked(x + (UInt64)(-y))
                : checked(x + Int64.MaxValue + 1UL); // -Int64.MinValue == Int64.MaxValue + 1

        /// <summary>
        /// <see cref="UInt32"/> 値と <see cref="Int32"/> 値の差を計算します。
        /// </summary>
        /// <param name="x">
        /// <see cref="UInt32"/> 値です。
        /// </param>
        /// <param name="y">
        /// <see cref="Int32"/> 値です。
        /// </param>
        /// <returns>
        /// <paramref name="x"/> と <paramref name="y"/> の差を示す <see cref="UInt32"/> 値です。
        /// </returns>
        /// <exception cref="OverflowException">計算結果が<see cref="UInt32"/>で表現できる範囲を超えた場合。</exception>
        /// <remarks>
        /// 計算の際にオーバーフローの検査を行います。
        /// </remarks>
        public static UInt32 SubtractAsUInt(this UInt32 x, Int32 y)
            => y >= 0
                ? checked(x - (UInt32)y)
                : y != Int32.MinValue
                ? checked(x + (UInt32)(-y))
                : checked(x + Int32.MaxValue + 1U); // -Int32.MinValue == Int32.MaxValue + 1

        #endregion

        #region ReverseBitOrder

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Byte ReverseBitOrder(this Byte value)
        {
            value = (Byte)(((value /* & 0xf0*/) >> 4) | ((value /* & 0x0f*/) << 4));
            value = (Byte)(((value & 0xcc) >> 2) | ((value & 0x33) << 2));
            value = (Byte)(((value & 0xaa) >> 1) | ((value & 0x55) << 1));
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ReverseBitOrder(this UInt16 value)
        {
            value = (UInt16)(((value /* & 0xff00*/) >> 8) | ((value /* & 0x00ff*/) << 8));
            value = (UInt16)(((value & 0xf0f0) >> 4) | ((value & 0x0f0f) << 4));
            value = (UInt16)(((value & 0xcccc) >> 2) | ((value & 0x3333) << 2));
            value = (UInt16)(((value & 0xaaaa) >> 1) | ((value & 0x5555) << 1));
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ReverseBitOrder(this UInt32 value)
        {
            value = ((value /* & 0xffff0000U*/) >> 16) | ((value /* & 0x0000ffffU*/) << 16);
            value = ((value & 0xff00ff00U) >> 08) | ((value & 0x00ff00ffU) << 08);
            value = ((value & 0xf0f0f0f0U) >> 04) | ((value & 0x0f0f0f0fU) << 04);
            value = ((value & 0xccccccccU) >> 02) | ((value & 0x33333333U) << 02);
            value = ((value & 0xaaaaaaaaU) >> 01) | ((value & 0x55555555U) << 01);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ReverseBitOrder(this UInt64 value)
        {
            value = ((value /* & 0xffffffff00000000UL*/) >> 32) | ((value /* & 0x00000000ffffffffUL*/) << 32);
            value = ((value & 0xffff0000ffff0000UL) >> 16) | ((value & 0x0000ffff0000ffffUL) << 16);
            value = ((value & 0xff00ff00ff00ff00UL) >> 08) | ((value & 0x00ff00ff00ff00ffUL) << 08);
            value = ((value & 0xf0f0f0f0f0f0f0f0UL) >> 04) | ((value & 0x0f0f0f0f0f0f0f0fUL) << 04);
            value = ((value & 0xccccccccccccccccUL) >> 02) | ((value & 0x3333333333333333UL) << 02);
            value = ((value & 0xaaaaaaaaaaaaaaaaUL) >> 01) | ((value & 0x5555555555555555UL) << 01);
            return value;
        }

        #endregion

        #region ReverseByteOrder

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ReverseByteOrder(this UInt16 value)
        {
            value = (UInt16)(((value /* & 0xff00*/) >> 8) | ((value /* & 0x00ff*/) << 8));
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ReverseByteOrder(this UInt32 value)
        {
            value = ((value /* & 0xffff0000U*/) >> 16) | ((value /* & 0x0000ffffU*/) << 16);
            value = ((value & 0xff00ff00U) >> 08) | ((value & 0x00ff00ffU) << 08);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ReverseByteOrder(this UInt64 value)
        {
            value = ((value /* & 0xffffffff00000000UL*/) >> 32) | ((value /* & 0x00000000ffffffffUL*/) << 32);
            value = ((value & 0xffff0000ffff0000UL) >> 16) | ((value & 0x0000ffff0000ffffUL) << 16);
            value = ((value & 0xff00ff00ff00ff00UL) >> 08) | ((value & 0x00ff00ff00ff00ffUL) << 08);
            return value;
        }

        #endregion

        #region GetBytesLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesLE(this Int16 value) => ((UInt16)value).GetBytesLE();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesLE(this UInt16 value)
        {
            var buffer = new Byte[sizeof(UInt16)];
            buffer.AsSpan().InternalCopyValueLE(value);
            return buffer.AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesLE(this Int32 value) => ((UInt32)value).GetBytesLE();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesLE(this UInt32 value)
        {
            var buffer = new Byte[sizeof(UInt32)];
            buffer.AsSpan().InternalCopyValueLE(value);
            return buffer.AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesLE(this Int64 value) => ((UInt64)value).GetBytesLE();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesLE(this UInt64 value)
        {
            var buffer = new Byte[sizeof(UInt64)];
            buffer.AsSpan().InternalCopyValueLE(value);
            return buffer.AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesLE(this Single value)
        {
            var buffer = new Byte[sizeof(Single)];
            buffer.AsSpan().InternalCopyValueLE(value);
            return buffer.AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesLE(this Double value)
        {
            var buffer = new Byte[sizeof(Double)];
            buffer.AsSpan().InternalCopyValueLE(value);
            return buffer.AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesLE(this Decimal value)
        {
            var buffer = new Byte[sizeof(Decimal)];
            buffer.AsSpan().InternalCopyValueLE(value);
            return buffer.AsReadOnly();
        }

        #endregion

        #region GetBytesBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesBE(this Int16 value) => ((UInt16)value).GetBytesBE();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesBE(this UInt16 value)
        {
            var buffer = new Byte[sizeof(UInt16)];
            buffer.AsSpan().InternalCopyValueBE(value);
            return buffer.AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesBE(this Int32 value) => ((UInt32)value).GetBytesBE();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesBE(this UInt32 value)
        {
            var buffer = new Byte[sizeof(UInt32)];
            buffer.AsSpan().InternalCopyValueBE(value);
            return buffer.AsReadOnly();
        }

        public static ReadOnlyMemory<Byte> GetBytesBE(this Int64 value) => ((UInt64)value).GetBytesBE();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesBE(this UInt64 value)
        {
            var buffer = new Byte[sizeof(UInt64)];
            buffer.AsSpan().InternalCopyValueBE(value);
            return buffer.AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesBE(this Single value)
        {
            var buffer = new Byte[sizeof(Single)];
            buffer.AsSpan().InternalCopyValueBE(value);
            return buffer.AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesBE(this Double value)
        {
            var buffer = new Byte[sizeof(Double)];
            buffer.AsSpan().InternalCopyValueBE(value);
            return buffer.AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<Byte> GetBytesBE(this Decimal value)
        {
            var buffer = new Byte[sizeof(Decimal)];
            buffer.AsSpan().InternalCopyValueBE(value);
            return buffer.AsReadOnly();
        }

        #endregion

        #region SetValueLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Byte[] buffer, Int32 startIndex, Int16 value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Int16) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(Int16)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueLE(startIndex, (UInt16)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Byte[] buffer, Int32 startIndex, UInt16 value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(UInt16) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(UInt16)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueLE(startIndex, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Byte[] buffer, Int32 startIndex, Int32 value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Int32) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(Int32)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueLE(startIndex, (UInt32)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Byte[] buffer, Int32 startIndex, UInt32 value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(UInt32) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(UInt32)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueLE(startIndex, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Byte[] buffer, Int32 startIndex, Int64 value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Int64) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(Int64)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueLE(startIndex, (UInt64)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Byte[] buffer, Int32 startIndex, UInt64 value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(UInt64) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(UInt64)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueLE(startIndex, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Byte[] buffer, Int32 startIndex, Single value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Single) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(Single)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueLE(startIndex, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Byte[] buffer, Int32 startIndex, Double value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Double) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(Double)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueLE(startIndex, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Byte[] buffer, Int32 startIndex, Decimal value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Decimal) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(Decimal)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueLE(startIndex, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Memory<Byte> buffer, Int16 value)
        {
            if (sizeof(Int16) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueLE((UInt16)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Memory<Byte> buffer, UInt16 value)
        {
            if (sizeof(UInt16) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueLE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Memory<Byte> buffer, Int32 value)
        {
            if (sizeof(Int32) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueLE((UInt32)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Memory<Byte> buffer, UInt32 value)
        {
            if (sizeof(UInt32) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueLE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Memory<Byte> buffer, Int64 value)
        {
            if (sizeof(Int64) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueLE((UInt64)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Memory<Byte> buffer, UInt64 value)
        {
            if (sizeof(UInt64) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueLE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Memory<Byte> buffer, Single value)
        {
            if (sizeof(Single) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueLE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Memory<Byte> buffer, Double value)
        {
            if (sizeof(Double) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueLE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Memory<Byte> buffer, Decimal value)
        {
            if (sizeof(Decimal) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueLE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Span<Byte> buffer, Int16 value)
        {
            if (sizeof(Int16) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueLE((UInt16)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Span<Byte> buffer, UInt16 value)
        {
            if (sizeof(UInt16) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueLE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Span<Byte> buffer, Int32 value)
        {
            if (sizeof(Int32) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueLE((UInt32)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Span<Byte> buffer, UInt32 value)
        {
            if (sizeof(UInt32) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueLE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Span<Byte> buffer, Int64 value)
        {
            if (sizeof(Int64) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueLE((UInt64)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Span<Byte> buffer, UInt64 value)
        {
            if (sizeof(UInt64) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueLE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Span<Byte> buffer, Single value)
        {
            if (sizeof(Single) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueLE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Span<Byte> buffer, Double value)
        {
            if (sizeof(Double) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueLE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueLE(this Span<Byte> buffer, Decimal value)
        {
            if (sizeof(Decimal) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueLE(value);
        }

        #endregion

        #region SetValueBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Byte[] buffer, Int32 startIndex, Int16 value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Int16) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(Int16)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueBE(startIndex, (UInt16)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Byte[] buffer, Int32 startIndex, UInt16 value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(UInt16) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(UInt16)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueBE(startIndex, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Byte[] buffer, Int32 startIndex, Int32 value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Int32) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(Int32)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueBE(startIndex, (UInt32)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Byte[] buffer, Int32 startIndex, UInt32 value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(UInt32) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(UInt32)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueBE(startIndex, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Byte[] buffer, Int32 startIndex, Int64 value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Int64) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(Int64)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueBE(startIndex, (UInt64)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Byte[] buffer, Int32 startIndex, UInt64 value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(UInt64) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(UInt64)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueBE(startIndex, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Byte[] buffer, Int32 startIndex, Single value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Single) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(Single)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueBE(startIndex, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Byte[] buffer, Int32 startIndex, Double value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Double) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(Double)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueBE(startIndex, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Byte[] buffer, Int32 startIndex, Decimal value)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (sizeof(Decimal) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));
            if (checked(startIndex + sizeof(Decimal)) > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            buffer.InternalCopyValueBE(startIndex, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Memory<Byte> buffer, Int16 value)
        {
            if (sizeof(Int16) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueBE((UInt16)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Memory<Byte> buffer, UInt16 value)
        {
            if (sizeof(UInt16) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueBE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Memory<Byte> buffer, Int32 value)
        {
            if (sizeof(Int32) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueBE((UInt32)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Memory<Byte> buffer, UInt32 value)
        {
            if (sizeof(UInt32) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueBE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Memory<Byte> buffer, Int64 value)
        {
            if (sizeof(Int64) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueBE((UInt64)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Memory<Byte> buffer, UInt64 value)
        {
            if (sizeof(UInt64) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueBE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Memory<Byte> buffer, Single value)
        {
            if (sizeof(Single) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueBE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Memory<Byte> buffer, Double value)
        {
            if (sizeof(Double) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueBE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Memory<Byte> buffer, Decimal value)
        {
            if (sizeof(Decimal) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.Span.InternalCopyValueBE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Span<Byte> buffer, Int16 value)
        {
            if (sizeof(Int16) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueBE((UInt16)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Span<Byte> buffer, UInt16 value)
        {
            if (sizeof(UInt16) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueBE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Span<Byte> buffer, Int32 value)
        {
            if (sizeof(Int32) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueBE((UInt32)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Span<Byte> buffer, UInt32 value)
        {
            if (sizeof(UInt32) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueBE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Span<Byte> buffer, Int64 value)
        {
            if (sizeof(Int64) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueBE((UInt64)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Span<Byte> buffer, UInt64 value)
        {
            if (sizeof(UInt64) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueBE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Span<Byte> buffer, Single value)
        {
            if (sizeof(Single) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueBE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Span<Byte> buffer, Double value)
        {
            if (sizeof(Double) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueBE(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValueBE(this Span<Byte> buffer, Decimal value)
        {
            if (sizeof(Decimal) > buffer.Length)
                throw new ArgumentException("Too short array", nameof(buffer));

            buffer.InternalCopyValueBE(value);
        }

        #endregion

        #region DivRem

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Int32 Quotient, Int32 Remainder) DivRem(this Int32 dividend, Int32 divisor) => Math.DivRem(dividend, divisor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (UInt32 Quotient, UInt32 Remainder) DivRem(this UInt32 dividend, UInt32 divisor) => Math.DivRem(dividend, divisor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Int64 Quotient, Int64 Remainder) DivRem(this Int64 dividend, Int64 divisor) => Math.DivRem(dividend, divisor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (UInt64 Quotient, UInt64 Remainder) DivRem(this UInt64 dividend, UInt64 divisor) => Math.DivRem(dividend, divisor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (BigInteger Quotient, Int32 Remainder) DivRem(this BigInteger dividend, Int32 divisor)
        {
            var quotient = BigInteger.DivRem(dividend, divisor, out BigInteger remainder);
            // 少なくとも |remainder| < |diviser| であるので、remainder の Int32 へのキャスト演算によってオーバーフローが発生することはない。
            // (diviser が Int32.MinValue の場合も含む)
            return (quotient, checked((Int32)remainder));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (BigInteger Quotient, Int64 Remainder) DivRem(this BigInteger dividend, Int64 divisor)
        {
            var quotient = BigInteger.DivRem(dividend, divisor, out BigInteger remainder);

            // 少なくとも |remainder| < |diviser| であるので、remainder の Int64 へのキャスト演算によってオーバーフローが発生することはない。
            // (diviser が Int64.MinValue の場合も含む)
            return (quotient, checked((Int64)remainder));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (BigInteger Quotient, BigInteger Remainder) DivRem(this BigInteger dividend, BigInteger divisor)
        {
            var quotient = BigInteger.DivRem(dividend, divisor, out BigInteger remainder);
            return (quotient, remainder);
        }

        #endregion

        #region DivMod

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Int32 Quotient, Int32 Modulo) DivMod(this Int32 dividend, Int32 divisor)
        {
            if (divisor > 0)
            {
                if (dividend >= 0)
                {
                    return Math.DivRem(dividend, divisor);
                }
                else
                {
                    var result = Math.DivRem(dividend, divisor);
                    if (result.Remainder < 0)
                        result = (result.Quotient - 1, result.Remainder + divisor);
                    return result;
                }
            }
            else if (divisor < 0)
            {
                if (dividend >= 0)
                {
                    var result = Math.DivRem(dividend, divisor);
                    if (result.Remainder > 0)
                        result = (result.Quotient + 1, result.Remainder - divisor);
                    return result;
                }
                else
                {
                    var result = Math.DivRem(dividend, divisor);
                    return result;
                }
            }
            else
            {
                throw new DivideByZeroException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (UInt32 Quotient, UInt32 Modulo) DivMod(this UInt32 dividend, UInt32 divisor)
        {
            if (divisor > 0)
            {
                if (dividend >= 0)
                {
                    return Math.DivRem(dividend, divisor);
                }
                else
                {
                    var result = Math.DivRem(dividend, divisor);
                    if (result.Remainder < 0)
                        result = (result.Quotient - 1, result.Remainder + divisor);
                    return result;
                }
            }
            else if (divisor < 0)
            {
                if (dividend >= 0)
                {
                    var result = Math.DivRem(dividend, divisor);
                    if (result.Remainder > 0)
                        result = (result.Quotient + 1, result.Remainder - divisor);
                    return result;
                }
                else
                {
                    return Math.DivRem(dividend, divisor);
                }
            }
            else
            {
                throw new DivideByZeroException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Int64 Quotient, Int64 Modulo) DivMod(this Int64 dividend, Int64 divisor)
        {
            if (divisor > 0)
            {
                if (dividend >= 0)
                {
                    return Math.DivRem(dividend, divisor);
                }
                else
                {
                    var result = Math.DivRem(dividend, divisor);
                    if (result.Remainder < 0)
                        result = (result.Quotient - 1, result.Remainder + divisor);
                    return result;
                }
            }
            else if (divisor < 0)
            {
                if (dividend >= 0)
                {
                    var result = Math.DivRem(dividend, divisor);
                    if (result.Remainder > 0)
                        result = (result.Quotient + 1, result.Remainder - divisor);
                    return result;
                }
                else
                {
                    return Math.DivRem(dividend, divisor);
                }
            }
            else
            {
                throw new DivideByZeroException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (UInt64 Quotient, UInt64 Modulo) DivMod(this UInt64 dividend, UInt64 divisor)
        {
            if (divisor > 0)
            {
                if (dividend >= 0)
                {
                    return Math.DivRem(dividend, divisor);
                }
                else
                {
                    var result = Math.DivRem(dividend, divisor);
                    if (result.Remainder < 0)
                        result = (result.Quotient - 1, result.Remainder + divisor);
                    return result;
                }
            }
            else if (divisor < 0)
            {
                if (dividend >= 0)
                {
                    var result = Math.DivRem(dividend, divisor);
                    if (result.Remainder > 0)
                        result = (result.Quotient + 1, result.Remainder - divisor);
                    return result;
                }
                else
                {
                    return Math.DivRem(dividend, divisor);
                }
            }
            else
            {
                throw new DivideByZeroException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (BigInteger Quotient, Int32 Modulo) DivMod(this BigInteger dividend, Int32 divisor)
        {
            if (divisor > 0)
            {
                if (dividend >= 0)
                {
                    return (BigInteger.DivRem(dividend, divisor, out BigInteger remainder), checked((Int32)remainder));
                }
                else
                {
                    var quotient = BigInteger.DivRem(dividend, divisor, out BigInteger remainder);
                    if (remainder < 0)
                    {
                        --quotient;
                        remainder += divisor;
                    }

                    return (quotient, checked((Int32)remainder));
                }
            }
            else if (divisor < 0)
            {
                if (dividend >= 0)
                {
                    var quotient = BigInteger.DivRem(dividend, divisor, out BigInteger remainder);
                    if (remainder > 0)
                    {
                        ++quotient;
                        remainder -= divisor;
                    }

                    return (quotient, checked((Int32)remainder));
                }
                else
                {
                    return (BigInteger.DivRem(dividend, divisor, out BigInteger remainder), checked((Int32)remainder));
                }
            }
            else
            {
                throw new DivideByZeroException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (BigInteger Quotient, UInt32 Modulo) DivMod(this BigInteger dividend, UInt32 divisor)
        {
            if (divisor > 0)
            {
                if (dividend >= 0)
                {
                    return (BigInteger.DivRem(dividend, divisor, out BigInteger remainder), checked((UInt32)remainder));
                }
                else
                {
                    var quotient = BigInteger.DivRem(dividend, divisor, out BigInteger remainder);
                    if (remainder < 0)
                    {
                        --quotient;
                        remainder += divisor;
                    }

                    return (quotient, checked((UInt32)remainder));
                }
            }
            else if (divisor < 0)
            {
                if (dividend >= 0)
                {
                    var quotient = BigInteger.DivRem(dividend, divisor, out BigInteger remainder);
                    if (remainder > 0)
                    {
                        ++quotient;
                        remainder -= divisor;
                    }

                    return (quotient, checked((UInt32)remainder));
                }
                else
                {
                    return (BigInteger.DivRem(dividend, divisor, out BigInteger remainder), checked((UInt32)remainder));
                }
            }
            else
            {
                throw new DivideByZeroException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (BigInteger Quotient, Int64 Modulo) DivMod(this BigInteger dividend, Int64 divisor)
        {
            if (divisor > 0)
            {
                if (dividend >= 0)
                {
                    return (BigInteger.DivRem(dividend, divisor, out BigInteger remainder), checked((Int64)remainder));
                }
                else
                {
                    var quotient = BigInteger.DivRem(dividend, divisor, out BigInteger remainder);
                    if (remainder < 0)
                    {
                        --quotient;
                        remainder += divisor;
                    }

                    return (quotient, checked((Int64)remainder));
                }
            }
            else if (divisor < 0)
            {
                if (dividend >= 0)
                {
                    var quotient = BigInteger.DivRem(dividend, divisor, out BigInteger remainder);
                    if (remainder > 0)
                    {
                        ++quotient;
                        remainder -= divisor;
                    }

                    return (quotient, checked((Int64)remainder));
                }
                else
                {
                    return (BigInteger.DivRem(dividend, divisor, out BigInteger remainder), checked((Int64)remainder));
                }
            }
            else
            {
                throw new DivideByZeroException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (BigInteger Quotient, UInt64 Modulo) DivMod(this BigInteger dividend, UInt64 divisor)
        {
            if (divisor > 0)
            {
                if (dividend >= 0)
                {
                    return (BigInteger.DivRem(dividend, divisor, out BigInteger remainder), checked((UInt64)remainder));
                }
                else
                {
                    var quotient = BigInteger.DivRem(dividend, divisor, out BigInteger remainder);
                    if (remainder < 0)
                    {
                        --quotient;
                        remainder += divisor;
                    }

                    return (quotient, checked((UInt64)remainder));
                }
            }
            else if (divisor < 0)
            {
                if (dividend >= 0)
                {
                    var quotient = BigInteger.DivRem(dividend, divisor, out BigInteger remainder);
                    if (remainder > 0)
                    {
                        ++quotient;
                        remainder -= divisor;
                    }

                    return (quotient, checked((UInt64)remainder));
                }
                else
                {
                    return (BigInteger.DivRem(dividend, divisor, out BigInteger remainder), checked((UInt64)remainder));
                }
            }
            else
            {
                throw new DivideByZeroException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (BigInteger Quotient, BigInteger Modulo) DivMod(this BigInteger dividend, BigInteger divisor)
        {
            if (divisor > 0)
            {
                if (dividend >= 0)
                {
                    return (BigInteger.DivRem(dividend, divisor, out BigInteger remainder), remainder);
                }
                else
                {
                    var quotient = BigInteger.DivRem(dividend, divisor, out BigInteger remainder);
                    if (remainder < 0)
                    {
                        --quotient;
                        remainder += divisor;
                    }

                    return (quotient, remainder);
                }
            }
            else if (divisor < 0)
            {
                if (dividend >= 0)
                {
                    var quotient = BigInteger.DivRem(dividend, divisor, out BigInteger remainder);
                    if (remainder > 0)
                    {
                        ++quotient;
                        remainder -= divisor;
                    }

                    return (quotient, remainder);
                }
                else
                {
                    return (BigInteger.DivRem(dividend, divisor, out BigInteger remainder), remainder);
                }
            }
            else
            {
                throw new DivideByZeroException();
            }
        }

        #endregion

        #region Modulo

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Modulo(this Int32 dividend, Int32 divisor)
        {
            if (divisor > 0)
            {
                if (dividend >= 0)
                {
                    return dividend % divisor;
                }
                else
                {
                    var modulo = dividend % divisor;
                    if (modulo < 0)
                        modulo += divisor;
                    return modulo;
                }
            }
            else if (divisor < 0)
            {
                if (dividend >= 0)
                {
                    var modulo = dividend % divisor;
                    if (modulo > 0)
                        modulo -= divisor;
                    return modulo;
                }
                else
                {
                    return dividend % divisor;
                }
            }
            else
            {
                throw new DivideByZeroException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Modulo(this UInt32 dividend, UInt32 divisor) => dividend % divisor;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 Modulo(this Int64 dividend, Int64 divisor)
        {
            if (divisor > 0)
            {
                if (dividend >= 0)
                {
                    return dividend % divisor;
                }
                else
                {
                    var modulo = dividend % divisor;
                    if (modulo < 0)
                        modulo += divisor;
                    return modulo;
                }
            }
            else if (divisor < 0)
            {
                if (dividend >= 0)
                {
                    var modulo = dividend % divisor;
                    if (modulo > 0)
                        modulo -= divisor;
                    return modulo;
                }
                else
                {
                    return dividend % divisor;
                }
            }
            else
            {
                throw new DivideByZeroException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 Modulo(this UInt64 dividend, UInt64 divisor) => dividend % divisor;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Modulo(this BigInteger dividend, Int32 divisor)
        {
            if (divisor > 0)
            {
                if (dividend >= 0)
                {
                    var modulo = dividend % divisor;
                    return checked((Int32)modulo);
                }
                else
                {
                    var modulo = dividend % divisor;
                    if (modulo < 0)
                        modulo += divisor;
                    return checked((Int32)modulo);
                }
            }
            else if (divisor < 0)
            {
                if (dividend >= 0)
                {
                    var modulo = dividend % divisor;
                    if (modulo > 0)
                        modulo -= divisor;
                    return checked((Int32)modulo);
                }
                else
                {
                    return checked((Int32)(dividend % divisor));
                }
            }
            else
            {
                throw new DivideByZeroException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 Modulo(this BigInteger dividend, UInt32 divisor)
        {
            if (divisor > 0)
            {
                if (dividend >= 0)
                {
                    return checked((UInt32)(dividend % divisor));
                }
                else
                {
                    var modulo = dividend % divisor;
                    if (modulo < 0)
                        modulo += divisor;
                    return checked((UInt32)modulo);
                }
            }
            else
            {
                throw new DivideByZeroException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 Modulo(this BigInteger dividend, Int64 divisor)
        {
            if (divisor > 0)
            {
                if (dividend >= 0)
                {
                    return checked((Int64)(dividend % divisor));
                }
                else
                {
                    var modulo = dividend % divisor;
                    if (modulo < 0)
                        modulo += divisor;
                    return checked((Int64)modulo);
                }
            }
            else if (divisor < 0)
            {
                if (dividend >= 0)
                {
                    var modulo = dividend % divisor;
                    if (modulo > 0)
                        modulo -= divisor;
                    return checked((Int64)modulo);
                }
                else
                {
                    return checked((Int64)(dividend % divisor));
                }
            }
            else
            {
                throw new DivideByZeroException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 Modulo(this BigInteger dividend, UInt64 divisor)
        {
            if (divisor > 0)
            {
                if (dividend >= 0)
                {
                    return checked((UInt64)(dividend % divisor));
                }
                else
                {
                    var modulo = dividend % divisor;
                    if (modulo < 0)
                        modulo += divisor;
                    return checked((UInt64)modulo);
                }
            }
            else
            {
                throw new DivideByZeroException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BigInteger Modulo(this BigInteger dividend, BigInteger divisor)
        {
            if (divisor > 0)
            {
                if (dividend >= 0)
                {
                    return dividend % divisor;
                }
                else
                {
                    var modulo = dividend % divisor;
                    if (modulo < 0)
                        modulo += divisor;
                    return modulo;
                }
            }
            else if (divisor < 0)
            {
                if (dividend >= 0)
                {
                    var modulo = dividend % divisor;
                    if (modulo > 0)
                        modulo -= divisor;
                    return modulo;
                }
                else
                {
                    return dividend % divisor;
                }
            }
            else
            {
                throw new DivideByZeroException();
            }
        }

        #endregion

        #region ConvertBitOrder

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Byte ConvertBitOrder(this Byte value, Int32 bitCount, BitPackingDirection bitPackingDirection)
        {
            switch (bitPackingDirection)
            {
                case BitPackingDirection.MsbToLsb:
                    value = value.ReverseBitOrder();
                    if (bitCount < _BIT_LENGTH_OF_BYTE)
                        value >>= _BIT_LENGTH_OF_BYTE - bitCount;
                    break;
                case BitPackingDirection.LsbToMsb:
                    break;
                default:
                    throw new ArgumentException($"Unexpected {nameof(BitPackingDirection)} value", nameof(bitPackingDirection));
            }

            if (bitCount < _BIT_LENGTH_OF_BYTE)
                value &= bitCount == _BIT_LENGTH_OF_BYTE ? Byte.MaxValue : (Byte)((1 << bitCount) - 1);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static UInt16 ConvertBitOrder(this UInt16 value, Int32 bitCount, BitPackingDirection bitPackingDirection)
        {
            switch (bitPackingDirection)
            {
                case BitPackingDirection.MsbToLsb:
                    value = value.ReverseBitOrder();
                    if (bitCount < _BIT_LENGTH_OF_UINT16)
                        value >>= _BIT_LENGTH_OF_UINT16 - bitCount;
                    break;
                case BitPackingDirection.LsbToMsb:
                    break;
                default:
                    throw new ArgumentException($"Unexpected {nameof(BitPackingDirection)} value", nameof(bitPackingDirection));
            }

            if (bitCount < _BIT_LENGTH_OF_UINT16)
                value &= (UInt16)((1 << bitCount) - 1);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static UInt32 ConvertBitOrder(this UInt32 value, Int32 bitCount, BitPackingDirection bitPackingDirection)
        {
            switch (bitPackingDirection)
            {
                case BitPackingDirection.MsbToLsb:
                    value = value.ReverseBitOrder();
                    if (bitCount < _BIT_LENGTH_OF_UINT32)
                        value >>= _BIT_LENGTH_OF_UINT32 - bitCount;
                    break;
                case BitPackingDirection.LsbToMsb:
                    break;
                default:
                    throw new ArgumentException($"Unexpected {nameof(BitPackingDirection)} value", nameof(bitPackingDirection));
            }

            if (bitCount < _BIT_LENGTH_OF_UINT32)
                value &= (1U << bitCount) - 1;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static UInt64 ConvertBitOrder(this UInt64 value, Int32 bitCount, BitPackingDirection bitPackingDirection)
        {
            switch (bitPackingDirection)
            {
                case BitPackingDirection.MsbToLsb:
                    value = value.ReverseBitOrder();
                    if (bitCount < _BIT_LENGTH_OF_UINT64)
                        value >>= _BIT_LENGTH_OF_UINT64 - bitCount;
                    break;
                case BitPackingDirection.LsbToMsb:
                    break;
                default:
                    throw new ArgumentException($"Unexpected {nameof(BitPackingDirection)} value", nameof(bitPackingDirection));
            }

            if (bitCount < _BIT_LENGTH_OF_UINT64)
                value &= (1UL << bitCount) - 1;
            return value;
        }

        #endregion

        #region InternalCopyValueLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueLE(this Byte[] buffer, Int32 startIndex, UInt16 value)
        {
            if (sizeof(UInt16) != 2)
                throw new InternalLogicalErrorException();

            unsafe
            {
                fixed (Byte* destinationbuffer = &buffer[startIndex])
                {
                    var destinationPointer = destinationbuffer;
                    *destinationPointer++ = (Byte)(value >> (8 * 0));
                    *destinationPointer++ = (Byte)(value >> (8 * 1));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueLE(this Byte[] buffer, Int32 startIndex, UInt32 value)
        {
            if (sizeof(UInt32) != 4)
                throw new InternalLogicalErrorException();

            unsafe
            {
                fixed (Byte* destinationbuffer = &buffer[startIndex])
                {
                    var destinationPointer = destinationbuffer;
                    *destinationPointer++ = (Byte)(value >> (8 * 0));
                    *destinationPointer++ = (Byte)(value >> (8 * 1));
                    *destinationPointer++ = (Byte)(value >> (8 * 2));
                    *destinationPointer++ = (Byte)(value >> (8 * 3));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueLE(this Byte[] buffer, Int32 startIndex, UInt64 value)
        {
            if (sizeof(UInt64) != 8)
                throw new InternalLogicalErrorException();

            unsafe
            {
                fixed (Byte* destinationbuffer = &buffer[startIndex])
                {
                    var destinationPointer = destinationbuffer;
                    *destinationPointer++ = (Byte)(value >> (8 * 0));
                    *destinationPointer++ = (Byte)(value >> (8 * 1));
                    *destinationPointer++ = (Byte)(value >> (8 * 2));
                    *destinationPointer++ = (Byte)(value >> (8 * 3));
                    *destinationPointer++ = (Byte)(value >> (8 * 4));
                    *destinationPointer++ = (Byte)(value >> (8 * 5));
                    *destinationPointer++ = (Byte)(value >> (8 * 6));
                    *destinationPointer++ = (Byte)(value >> (8 * 7));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueLE(this Byte[] buffer, Int32 startIndex, Single value)
        {
            var bufferSpan = buffer.AsSpan(startIndex, sizeof(Single));
            if (!BitConverter.TryWriteBytes(bufferSpan, value))
                throw new InternalLogicalErrorException();
            if (!BitConverter.IsLittleEndian)
                _ = bufferSpan.ReverseArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueLE(this Byte[] buffer, Int32 startIndex, Double value)
        {
            var bufferSpan = buffer.AsSpan(startIndex, sizeof(Double));
            if (!BitConverter.TryWriteBytes(bufferSpan, value))
                throw new InternalLogicalErrorException();
            if (!BitConverter.IsLittleEndian)
                _ = bufferSpan.ReverseArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueLE(this Byte[] buffer, Int32 startIndex, Decimal value)
        {
            if (sizeof(Decimal) != 16)
                throw new InternalLogicalErrorException();

            const Int32 DECIMAL_BIT_IMAGE_INT32_ARRAY_SIZE = 4;
            Span<Int32> tempBuffer = stackalloc Int32[DECIMAL_BIT_IMAGE_INT32_ARRAY_SIZE];
            _ = Decimal.GetBits(value, tempBuffer);
            unsafe
            {
                fixed (Int32* sourcebuffer = &tempBuffer[0])
                fixed (Byte* destinationbuffer = &buffer[startIndex])
                {
                    Unsafe.CopyBlockUnaligned(destinationbuffer, sourcebuffer, sizeof(Int32) * DECIMAL_BIT_IMAGE_INT32_ARRAY_SIZE);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueLE(this Span<Byte> buffer, UInt16 value)
        {
            if (sizeof(UInt16) != 2)
                throw new InternalLogicalErrorException();

            unsafe
            {
                fixed (Byte* destinationbuffer = &buffer.GetPinnableReference())
                {
                    var destinationPointer = destinationbuffer;
                    *destinationPointer++ = (Byte)(value >> (8 * 0));
                    *destinationPointer++ = (Byte)(value >> (8 * 1));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueLE(this Span<Byte> buffer, UInt32 value)
        {
            if (sizeof(UInt32) != 4)
                throw new InternalLogicalErrorException();

            unsafe
            {
                fixed (Byte* destinationbuffer = &buffer.GetPinnableReference())
                {
                    var destinationPointer = destinationbuffer;
                    *destinationPointer++ = (Byte)(value >> (8 * 0));
                    *destinationPointer++ = (Byte)(value >> (8 * 1));
                    *destinationPointer++ = (Byte)(value >> (8 * 2));
                    *destinationPointer++ = (Byte)(value >> (8 * 3));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueLE(this Span<Byte> buffer, UInt64 value)
        {
            if (sizeof(UInt64) != 8)
                throw new InternalLogicalErrorException();

            unsafe
            {
                fixed (Byte* destinationbuffer = &buffer.GetPinnableReference())
                {
                    var destinationPointer = destinationbuffer;
                    *destinationPointer++ = (Byte)(value >> (8 * 0));
                    *destinationPointer++ = (Byte)(value >> (8 * 1));
                    *destinationPointer++ = (Byte)(value >> (8 * 2));
                    *destinationPointer++ = (Byte)(value >> (8 * 3));
                    *destinationPointer++ = (Byte)(value >> (8 * 4));
                    *destinationPointer++ = (Byte)(value >> (8 * 5));
                    *destinationPointer++ = (Byte)(value >> (8 * 6));
                    *destinationPointer++ = (Byte)(value >> (8 * 7));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueLE(this Span<Byte> buffer, Single value)
        {
            if (!BitConverter.TryWriteBytes(buffer, value))
                throw new InternalLogicalErrorException();
            if (!BitConverter.IsLittleEndian)
                _ = buffer[..sizeof(Single)].ReverseArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueLE(this Span<Byte> buffer, Double value)
        {
            if (!BitConverter.TryWriteBytes(buffer, value))
                throw new InternalLogicalErrorException();
            if (!BitConverter.IsLittleEndian)
                _ = buffer[..sizeof(Double)].ReverseArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueLE(this Span<Byte> buffer, Decimal value)
        {
            if (sizeof(Decimal) != 16)
                throw new InternalLogicalErrorException();

            const Int32 DECIMAL_BIT_IMAGE_INT32_ARRAY_SIZE = 4;
            Span<Int32> tempBuffer = stackalloc Int32[DECIMAL_BIT_IMAGE_INT32_ARRAY_SIZE];
            _ = Decimal.GetBits(value, tempBuffer);
            unsafe
            {
                fixed (Int32* sourcebuffer = &tempBuffer[0])
                fixed (Byte* destinationbuffer = &buffer.GetPinnableReference())
                {
                    Unsafe.CopyBlockUnaligned(destinationbuffer, sourcebuffer, sizeof(Int32) * DECIMAL_BIT_IMAGE_INT32_ARRAY_SIZE);
                }
            }
        }

        #endregion

        #region InternalCopyValueBE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueBE(this Byte[] buffer, Int32 startIndex, UInt16 value)
        {
            if (sizeof(UInt16) != 2)
                throw new InternalLogicalErrorException();

            unsafe
            {
                fixed (Byte* destinationbuffer = &buffer[startIndex])
                {
                    var destinationPointer = destinationbuffer;
                    *destinationPointer++ = (Byte)(value >> (8 * 1));
                    *destinationPointer++ = (Byte)(value >> (8 * 0));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueBE(this Byte[] buffer, Int32 startIndex, UInt32 value)
        {
            if (sizeof(UInt32) != 4)
                throw new InternalLogicalErrorException();

            unsafe
            {
                fixed (Byte* destinationbuffer = &buffer[startIndex])
                {
                    var destinationPointer = destinationbuffer;
                    *destinationPointer++ = (Byte)(value >> (8 * 3));
                    *destinationPointer++ = (Byte)(value >> (8 * 2));
                    *destinationPointer++ = (Byte)(value >> (8 * 1));
                    *destinationPointer++ = (Byte)(value >> (8 * 0));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueBE(this Byte[] buffer, Int32 startIndex, UInt64 value)
        {
            if (sizeof(UInt64) != 8)
                throw new InternalLogicalErrorException();

            unsafe
            {
                fixed (Byte* destinationbuffer = &buffer[startIndex])
                {
                    var destinationPointer = destinationbuffer;
                    *destinationPointer++ = (Byte)(value >> (8 * 7));
                    *destinationPointer++ = (Byte)(value >> (8 * 6));
                    *destinationPointer++ = (Byte)(value >> (8 * 5));
                    *destinationPointer++ = (Byte)(value >> (8 * 4));
                    *destinationPointer++ = (Byte)(value >> (8 * 3));
                    *destinationPointer++ = (Byte)(value >> (8 * 2));
                    *destinationPointer++ = (Byte)(value >> (8 * 1));
                    *destinationPointer++ = (Byte)(value >> (8 * 0));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueBE(this Byte[] buffer, Int32 startIndex, Single value)
        {
            var bufferSpan = buffer.AsSpan(startIndex, sizeof(Single));
            if (!BitConverter.TryWriteBytes(bufferSpan, value))
                throw new InternalLogicalErrorException();
            if (BitConverter.IsLittleEndian)
                _ = bufferSpan.ReverseArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueBE(this Byte[] buffer, Int32 startIndex, Double value)
        {
            var bufferSpan = buffer.AsSpan(startIndex, sizeof(Double));
            if (!BitConverter.TryWriteBytes(bufferSpan, value))
                throw new InternalLogicalErrorException();
            if (BitConverter.IsLittleEndian)
                _ = bufferSpan.ReverseArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueBE(this Byte[] buffer, Int32 startIndex, Decimal value)
        {
            if (sizeof(Decimal) != 16)
                throw new InternalLogicalErrorException();

            const Int32 DECIMAL_BIT_IMAGE_INT32_ARRAY_SIZE = 4;
            Span<Int32> tempBuffer = stackalloc Int32[DECIMAL_BIT_IMAGE_INT32_ARRAY_SIZE];
            _ = Decimal.GetBits(value, tempBuffer);
            unsafe
            {
                fixed (Int32* sourcebuffer = &tempBuffer[DECIMAL_BIT_IMAGE_INT32_ARRAY_SIZE - 1])
                fixed (Byte* destinationbuffer = &buffer[startIndex])
                {
                    var sourcePointer = sourcebuffer;
                    var destinationPointer = destinationbuffer;
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 3));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 2));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 1));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 0));
                    --sourcePointer;
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 3));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 2));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 1));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 0));
                    --sourcePointer;
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 3));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 2));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 1));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 0));
                    --sourcePointer;
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 3));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 2));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 1));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 0));
                    --sourcePointer;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueBE(this Span<Byte> buffer, UInt16 value)
        {
            if (sizeof(UInt16) != 2)
                throw new InternalLogicalErrorException();

            unsafe
            {
                fixed (Byte* destinationbuffer = &buffer.GetPinnableReference())
                {
                    var destinationPointer = destinationbuffer;
                    *destinationPointer++ = (Byte)(value >> (8 * 1));
                    *destinationPointer++ = (Byte)(value >> (8 * 0));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueBE(this Span<Byte> buffer, UInt32 value)
        {
            if (sizeof(UInt32) != 4)
                throw new InternalLogicalErrorException();

            unsafe
            {
                fixed (Byte* destinationbuffer = &buffer.GetPinnableReference())
                {
                    var destinationPointer = destinationbuffer;
                    *destinationPointer++ = (Byte)(value >> (8 * 3));
                    *destinationPointer++ = (Byte)(value >> (8 * 2));
                    *destinationPointer++ = (Byte)(value >> (8 * 1));
                    *destinationPointer++ = (Byte)(value >> (8 * 0));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueBE(this Span<Byte> buffer, UInt64 value)
        {
            if (sizeof(UInt64) != 8)
                throw new InternalLogicalErrorException();

            unsafe
            {
                fixed (Byte* destinationbuffer = &buffer.GetPinnableReference())
                {
                    var destinationPointer = destinationbuffer;
                    *destinationPointer++ = (Byte)(value >> (8 * 7));
                    *destinationPointer++ = (Byte)(value >> (8 * 6));
                    *destinationPointer++ = (Byte)(value >> (8 * 5));
                    *destinationPointer++ = (Byte)(value >> (8 * 4));
                    *destinationPointer++ = (Byte)(value >> (8 * 3));
                    *destinationPointer++ = (Byte)(value >> (8 * 2));
                    *destinationPointer++ = (Byte)(value >> (8 * 1));
                    *destinationPointer++ = (Byte)(value >> (8 * 0));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueBE(this Span<Byte> buffer, Single value)
        {
            if (!BitConverter.TryWriteBytes(buffer, value))
                throw new InternalLogicalErrorException();
            if (BitConverter.IsLittleEndian)
                _ = buffer[..sizeof(Single)].ReverseArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueBE(this Span<Byte> buffer, Double value)
        {
            if (!BitConverter.TryWriteBytes(buffer, value))
                throw new InternalLogicalErrorException();
            if (BitConverter.IsLittleEndian)
                _ = buffer[..sizeof(Double)].ReverseArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyValueBE(this Span<Byte> buffer, Decimal value)
        {
            if (sizeof(Decimal) != 16)
                throw new InternalLogicalErrorException();

            const Int32 DECIMAL_BIT_IMAGE_INT32_ARRAY_SIZE = 4;
            Span<Int32> tempBuffer = stackalloc Int32[DECIMAL_BIT_IMAGE_INT32_ARRAY_SIZE];
            _ = Decimal.GetBits(value, tempBuffer);
            unsafe
            {
                fixed (Int32* sourcebuffer = &tempBuffer[DECIMAL_BIT_IMAGE_INT32_ARRAY_SIZE - 1])
                fixed (Byte* destinationbuffer = &buffer.GetPinnableReference())
                {
                    var sourcePointer = sourcebuffer;
                    var destinationPointer = destinationbuffer;
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 3));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 2));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 1));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 0));
                    --sourcePointer;
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 3));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 2));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 1));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 0));
                    --sourcePointer;
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 3));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 2));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 1));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 0));
                    --sourcePointer;
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 3));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 2));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 1));
                    *destinationPointer++ = (Byte)(*sourcebuffer >> (8 * 0));
                    --sourcePointer;
                }
            }
        }

        #endregion
    }
}
