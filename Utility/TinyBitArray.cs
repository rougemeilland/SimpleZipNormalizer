using System;
using System.Collections;
using System.Collections.Generic;

namespace Utility
{
    /// <summary>
    /// <see cref="Boolean"/> で表現されたビットを要素とする配列のクラスです。
    /// 配列内のランダムアクセスや、ビット集合を整数とみなした相互変換もサポートされています。
    /// </summary>
    /// <remarks>
    /// このクラスは比較的少ないビット数の集合を扱う際にパフォーマンスが向上するように設計されています。
    /// <see cref="RecommendedMaxLength"/> を超える大きさのビットを保持するとパフォーマンスが低下することがありますので注意してください。
    /// </remarks>
    public readonly struct TinyBitArray
        : IEnumerable<Boolean>, IEquatable<TinyBitArray>, ICloneable<TinyBitArray>, IInternalBitArray
    {
        public const Int32 RecommendedMaxLength = _BIT_LENGTH_OF_UINT64;

        private const Int32 _BIT_LENGTH_OF_BYTE = InternalBitQueue.BIT_LENGTH_OF_BYTE;
        private const Int32 _BIT_LENGTH_OF_UINT16 = InternalBitQueue.BIT_LENGTH_OF_UINT16;
        private const Int32 _BIT_LENGTH_OF_UINT32 = InternalBitQueue.BIT_LENGTH_OF_UINT32;
        private const Int32 _BIT_LENGTH_OF_UINT64 = InternalBitQueue.BIT_LENGTH_OF_UINT64;

        private readonly InternalBitQueue _bitArray;

        public TinyBitArray()
            : this(new InternalBitQueue())
        {
        }

        public TinyBitArray(ReadOnlySpan<Boolean> bitPattern)
            : this(new InternalBitQueue(bitPattern))
        {
        }

        public TinyBitArray(String bitPattern)
            : this(new InternalBitQueue(bitPattern ?? throw new ArgumentNullException(nameof(bitPattern))))
        {
        }

        internal TinyBitArray(InternalBitQueue bitAarray)
        {
            _bitArray = bitAarray;
        }

        public static TinyBitArray FromBoolean(Boolean value)
            => new(InternalBitQueue.FromBoolean(value));

        public static TinyBitArray FromByte(Byte value, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
            => new(InternalBitQueue.FromInteger(value, _BIT_LENGTH_OF_BYTE, bitPackingDirection));

        public static TinyBitArray FromByte(Byte value, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (!bitCount.IsBetween(1, _BIT_LENGTH_OF_BYTE))
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            return new TinyBitArray(InternalBitQueue.FromInteger(value, bitCount, bitPackingDirection));
        }

        public static TinyBitArray FromUInt16(UInt16 value, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
            => new(InternalBitQueue.FromInteger(value, _BIT_LENGTH_OF_UINT16, bitPackingDirection));

        public static TinyBitArray FromUInt16(UInt16 value, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (!bitCount.IsBetween(1, _BIT_LENGTH_OF_UINT16))
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            return new TinyBitArray(InternalBitQueue.FromInteger(value, bitCount, bitPackingDirection));
        }

        public static TinyBitArray FromUInt32(UInt32 value, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
            => new(InternalBitQueue.FromInteger(value, _BIT_LENGTH_OF_UINT32, bitPackingDirection));

        public static TinyBitArray FromUInt32(UInt32 value, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (!bitCount.IsBetween(1, _BIT_LENGTH_OF_UINT32))
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            return new TinyBitArray(InternalBitQueue.FromInteger(value, bitCount, bitPackingDirection));
        }

        public static TinyBitArray FromUInt64(UInt64 value, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
            => new(InternalBitQueue.FromInteger(value, _BIT_LENGTH_OF_UINT64, bitPackingDirection));

        public static TinyBitArray FromUInt64(UInt64 value, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (!bitCount.IsBetween(1, _BIT_LENGTH_OF_UINT64))
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            return new TinyBitArray(InternalBitQueue.FromInteger(value, bitCount, bitPackingDirection));
        }

        public Boolean ToBoolean()
        {
            if (_bitArray.Length < 1)
                throw new InvalidOperationException();

            if (_bitArray.Length > 1)
                throw new OverflowException();

            return _bitArray.ToBoolean();
        }

        public Byte ToByte(BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (_bitArray.Length < 1)
                throw new InvalidOperationException();
            if (_bitArray.Length > _BIT_LENGTH_OF_BYTE)
                throw new OverflowException();

            return (Byte)_bitArray.ToInteger(_BIT_LENGTH_OF_BYTE.Minimum(_bitArray.Length), bitPackingDirection);
        }

        public UInt16 ToUInt16(BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (_bitArray.Length < 1)
                throw new InvalidOperationException();
            if (_bitArray.Length > _BIT_LENGTH_OF_UINT16)
                throw new OverflowException();

            return (UInt16)_bitArray.ToInteger(_BIT_LENGTH_OF_UINT16.Minimum(_bitArray.Length), bitPackingDirection);
        }

        public UInt32 ToUInt32(BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (_bitArray.Length < 1)
                throw new InvalidOperationException();
            if (_bitArray.Length > _BIT_LENGTH_OF_UINT32)
                throw new OverflowException();

            return (UInt32)_bitArray.ToInteger(_BIT_LENGTH_OF_UINT32.Minimum(_bitArray.Length), bitPackingDirection);
        }

        public UInt64 ToUInt64(BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (_bitArray.Length < 1)
                throw new InvalidOperationException();
            if (_bitArray.Length > _BIT_LENGTH_OF_UINT64)
                throw new OverflowException();

            return _bitArray.ToInteger(_BIT_LENGTH_OF_UINT64.Minimum(_bitArray.Length), bitPackingDirection);
        }

        public TinyBitArray Concat(Boolean value)
        {
            var bitArray = _bitArray.Clone();
            bitArray.Enqueue(value);
            return new TinyBitArray(bitArray);
        }

        public TinyBitArray Concat(Byte value, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            var bitArray = _bitArray.Clone();
            bitArray.Enqueue(value, _BIT_LENGTH_OF_BYTE, bitPackingDirection);
            return new TinyBitArray(bitArray);
        }

        public TinyBitArray Concat(Byte value, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (bitCount < 1)
                throw new ArgumentOutOfRangeException(nameof(bitCount));
            if (bitCount > _BIT_LENGTH_OF_BYTE)
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            var bitArray = _bitArray.Clone();
            bitArray.Enqueue(value, bitCount, bitPackingDirection);
            return new TinyBitArray(bitArray);
        }

        public TinyBitArray Concat(UInt16 value, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
            => ConcatInterger(value, _BIT_LENGTH_OF_UINT16, bitPackingDirection);

        public TinyBitArray Concat(UInt16 value, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (bitCount < 1)
                throw new ArgumentOutOfRangeException(nameof(bitCount));
            if (bitCount > _BIT_LENGTH_OF_UINT16)
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            return ConcatInterger(value, bitCount, bitPackingDirection);
        }

        public TinyBitArray Concat(UInt32 value, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
            => ConcatInterger(value, _BIT_LENGTH_OF_UINT32, bitPackingDirection);

        public TinyBitArray Concat(UInt32 value, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (bitCount < 1)
                throw new ArgumentOutOfRangeException(nameof(bitCount));
            if (bitCount > _BIT_LENGTH_OF_UINT32)
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            return ConcatInterger(value, bitCount, bitPackingDirection);
        }

        public TinyBitArray Concat(UInt64 value, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
            => ConcatInterger(value, _BIT_LENGTH_OF_UINT64, bitPackingDirection);

        public TinyBitArray Concat(UInt64 value, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (bitCount < 1)
                throw new ArgumentOutOfRangeException(nameof(bitCount));
            if (bitCount > _BIT_LENGTH_OF_UINT64)
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            return ConcatInterger(value, bitCount, bitPackingDirection);
        }

        public TinyBitArray Concat(TinyBitArray other)
        {
            var newBitArray = _bitArray.Clone();
            newBitArray.Enqueue(other._bitArray);
            return new TinyBitArray(newBitArray);
        }

        public (TinyBitArray FirstHalf, TinyBitArray SecondHalf) Divide(Int32 count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            else if (count == 0)
            {
                return (new TinyBitArray(), Clone());
            }
            else if (count < _bitArray.Length)
            {
                var newBitArray = _bitArray.Clone();
                var result = newBitArray.DequeueBitQueue(count);
                return (new TinyBitArray(result), new TinyBitArray(newBitArray));
            }
            else
            {
                if (count != _bitArray.Length)
                    throw new ArgumentOutOfRangeException(nameof(count));

                return (Clone(), new TinyBitArray());
            }
        }

        public String ToString(String? format) => _bitArray.ToString(format);

        public static TinyBitArray operator +(TinyBitArray x, TinyBitArray y) => x.Concat(y);

        public Boolean this[Int32 index] => _bitArray[index];
        public Boolean this[UInt32 index] => this[checked((Int32)index)];
        public Int32 Length => _bitArray.Length;

        public TinyBitArray Clone() => new(_bitArray.Clone());
        public Boolean Equals(TinyBitArray other) => _bitArray.Equals(other._bitArray);
        public override Boolean Equals(Object? obj) => obj is not null && GetType() == obj.GetType() && Equals((TinyBitArray)obj);
        public override Int32 GetHashCode() => _bitArray.GetHashCode();
        public override String ToString() => ToString("G");
        public IEnumerator<Boolean> GetEnumerator() => _bitArray.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        InternalBitQueue IInternalBitArray.BitArray => _bitArray;
        public static Boolean operator ==(TinyBitArray left, TinyBitArray right) => left.Equals(right);
        public static Boolean operator !=(TinyBitArray left, TinyBitArray right) => !(left == right);

        private TinyBitArray ConcatInterger(UInt64 value, Int32 bitCount, BitPackingDirection bitPackingDirection)
        {
#if DEBUG
            if (bitCount < 1)
                throw new Exception();
            if (bitCount > _BIT_LENGTH_OF_UINT64)
                throw new Exception();
#endif
            var bitArray = _bitArray.Clone();
            bitArray.Enqueue(value, bitCount, bitPackingDirection);
            return new TinyBitArray(bitArray);
        }
    }
}
