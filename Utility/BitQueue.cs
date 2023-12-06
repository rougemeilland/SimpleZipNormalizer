using System;

namespace Utility
{
    /// <summary>
    /// <see cref="Boolean"/> で表現されたビットを要素とするキューのクラスです。
    /// キュー操作はビット単位だけではなく、整数をビット集合とみなして操作することもできます。
    /// </summary>
    /// <remarks>
    /// このクラスは比較的少ないビット数の集合を扱う際にパフォーマンスが向上するように設計されています。
    /// <see cref="RecommendedMaxCount"/> を超える大きさのビットを保持するとパフォーマンスが低下することがありますので注意してください。
    /// </remarks>
    public class BitQueue
        : ICloneable<BitQueue>
    {
        public const Int32 RecommendedMaxCount = _BIT_LENGTH_OF_UINT64;

        private const Int32 _BIT_LENGTH_OF_BYTE = sizeof(Byte) << 3;
        private const Int32 _BIT_LENGTH_OF_UINT16 = sizeof(UInt16) << 3;
        private const Int32 _BIT_LENGTH_OF_UINT32 = sizeof(UInt32) << 3;
        private const Int32 _BIT_LENGTH_OF_UINT64 = sizeof(UInt64) << 3;

        private readonly InternalBitQueue _bitQueue;

        public BitQueue()
            : this(new InternalBitQueue())
        {
        }

        public BitQueue(ReadOnlySpan<Boolean> bitPattern)
            : this(new InternalBitQueue(bitPattern))
        {
        }

        public BitQueue(String bitPattern)
            : this(new InternalBitQueue(bitPattern ?? throw new ArgumentNullException(nameof(bitPattern))))
        {
        }

        private BitQueue(InternalBitQueue bitQueue)
        {
            _bitQueue = bitQueue;
        }

        public void Enqueue(Boolean value)
            => _bitQueue.Enqueue(value);

        public void Enqueue(Byte value, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
            => _bitQueue.Enqueue(value, _BIT_LENGTH_OF_BYTE, bitPackingDirection);

        public void Enqueue(Byte value, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (!bitCount.IsBetween(1, _BIT_LENGTH_OF_BYTE))
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            _bitQueue.Enqueue(value, bitCount, bitPackingDirection);
        }

        public void Enqueue(UInt16 value, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
            => _bitQueue.Enqueue(value, _BIT_LENGTH_OF_UINT16, bitPackingDirection);

        public void Enqueue(UInt16 value, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (!bitCount.IsBetween(1, _BIT_LENGTH_OF_UINT16))
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            _bitQueue.Enqueue(value, bitCount, bitPackingDirection);
        }

        public void Enqueue(UInt32 value, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
            => _bitQueue.Enqueue(value, _BIT_LENGTH_OF_UINT32, bitPackingDirection);

        public void Enqueue(UInt32 value, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (!bitCount.IsBetween(1, _BIT_LENGTH_OF_UINT32))
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            _bitQueue.Enqueue(value, bitCount, bitPackingDirection);
        }

        public void Enqueue(UInt64 value, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
            => _bitQueue.Enqueue(value, _BIT_LENGTH_OF_UINT64, bitPackingDirection);

        public void Enqueue(UInt64 value, Int32 bitCount, BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (!bitCount.IsBetween(1, _BIT_LENGTH_OF_UINT64))
                throw new ArgumentOutOfRangeException(nameof(bitCount));

            _bitQueue.Enqueue(value, bitCount, bitPackingDirection);
        }

        public void Enqueue(TinyBitArray bitArray)
            => _bitQueue.Enqueue((bitArray as IInternalBitArray).BitArray);

        public Boolean DequeueBoolean()
        {
            if (_bitQueue.Length < 1)
                throw new InvalidOperationException();

            return _bitQueue.DequeueBoolean();
        }

        public Byte DequeueByte(BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (_bitQueue.Length < 1)
                throw new InvalidOperationException();

            return (Byte)_bitQueue.DequeueInteger(_BIT_LENGTH_OF_BYTE.Minimum(_bitQueue.Length), bitPackingDirection);
        }

        public UInt16 DequeueUInt16(BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (_bitQueue.Length < 1)
                throw new InvalidOperationException();

            return (UInt16)_bitQueue.DequeueInteger(_BIT_LENGTH_OF_UINT16.Minimum(_bitQueue.Length), bitPackingDirection);
        }

        public UInt32 DequeueUInt32(BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (_bitQueue.Length < 1)
                throw new InvalidOperationException();

            return (UInt32)_bitQueue.DequeueInteger(_BIT_LENGTH_OF_UINT32.Minimum(_bitQueue.Length), bitPackingDirection);
        }

        public UInt64 DequeueUInt64(BitPackingDirection bitPackingDirection = BitPackingDirection.Default)
        {
            if (_bitQueue.Length < 1)
                throw new InvalidOperationException();

            return _bitQueue.DequeueInteger(_BIT_LENGTH_OF_UINT64.Minimum(_bitQueue.Length), bitPackingDirection);
        }

        public TinyBitArray DequeueBitArray(Int32 bitCount)
        {
            if (_bitQueue.Length < 1)
                throw new InvalidOperationException();

            return new TinyBitArray(_bitQueue.DequeueBitQueue(bitCount.Minimum(_bitQueue.Length)));
        }

        public void Clear() => _bitQueue.Clear();
        public String ToString(String? format) => _bitQueue.ToString(format);
        public Int32 Count => _bitQueue.Length;
        public BitQueue Clone() => new(_bitQueue.Clone());
        public override String ToString() => ToString("G");
    }
}
