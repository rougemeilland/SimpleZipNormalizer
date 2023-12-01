using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Utility
{
    public static class ArrayExtensions
    {
        // ジェネリックメソッドにおいて、typeof() による型分岐のコストは JIT の最適化によりほぼゼロになるらしい。
        // 出典: https://qiita.com/aka-nse/items/2f45f056262d2d5c6df7

        private const Int32 _THRESHOLD_ARRAY_EQUAL_BY_LONG_POINTER = 32;
        private const Int32 _THRESHOLD_COPY_MEMORY_BY_LONG_POINTER = 14;

        private readonly static Boolean _is64bitProcess;
        private readonly static Int32 _alignment;
        private readonly static Int32 _alignmentMask;

        static ArrayExtensions()
        {
            _is64bitProcess = Environment.Is64BitProcess;
            _alignment = _is64bitProcess ? sizeof(UInt64) : sizeof(UInt32);
            _alignmentMask = _alignment - 1;
        }

        #region GetOffsetAndLength

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Boolean IsOk, Int32 Offset, Int32 Length) GetOffsetAndLength<ELEMENT_T>(this ELEMENT_T[] source, Range range)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            try
            {
                var (offset, count) = range.GetOffsetAndLength(source.Length);
                return (true, offset, count);
            }
            catch (ArgumentOutOfRangeException)
            {
                return (false, 0, 0);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Boolean IsOk, Int32 Offset, Int32 Length) GetOffsetAndLength<ELEMENT_T>(this Span<ELEMENT_T> source, Range range)
        {
            try
            {
                var (offset, count) = range.GetOffsetAndLength(source.Length);
                return (true, offset, count);
            }
            catch (ArgumentOutOfRangeException)
            {
                return (false, 0, 0);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (Boolean IsOk, Int32 Offset, Int32 Length) GetOffsetAndLength<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> source, Range range)
        {
            try
            {
                var (offset, count) = range.GetOffsetAndLength(source.Length);
                return (true, offset, count);
            }
            catch (ArgumentOutOfRangeException)
            {
                return (false, 0, 0);
            }
        }

        #endregion

        #region AsReadOnly

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<ELEMENT_T> AsReadOnly<ELEMENT_T>(this ELEMENT_T[] source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return new ReadOnlyMemory<ELEMENT_T>(source);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<ELEMENT_T> AsReadOnly<ELEMENT_T>(this ELEMENT_T[] source, Int32 offset, Int32 count)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > source.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(source)}.");

            return new ReadOnlyMemory<ELEMENT_T>(source, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<ELEMENT_T> AsReadOnly<ELEMENT_T>(this ELEMENT_T[] sourceArray, UInt32 offset, UInt32 length)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (checked(offset + length) > (UInt32)sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(length)}) is not within the {nameof(sourceArray)}.");

            return new ReadOnlyMemory<ELEMENT_T>(sourceArray, (Int32)offset, (Int32)length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<ELEMENT_T> AsReadOnly<ELEMENT_T>(this Span<ELEMENT_T> sourceArray) => sourceArray;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<ELEMENT_T> AsReadOnly<ELEMENT_T>(this Memory<ELEMENT_T> sourceArray) => sourceArray;

        #endregion

        #region AsMemory

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<ELEMENT_T> AsMemory<ELEMENT_T>(this ELEMENT_T[] sourceArray, UInt32 offset)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (offset > (UInt32)sourceArray.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return new Memory<ELEMENT_T>(sourceArray, (Int32)offset, (Int32)(sourceArray.Length - offset));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<ELEMENT_T> AsMemory<ELEMENT_T>(this ELEMENT_T[] sourceArray, Int32 offset, Int32 length)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (checked(offset + length) > (UInt32)sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(length)}) is not within the {nameof(sourceArray)}.");

            return new Memory<ELEMENT_T>(sourceArray, offset, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<ELEMENT_T> AsMemory<ELEMENT_T>(this ELEMENT_T[] sourceArray, UInt32 offset, UInt32 length)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (checked(offset + length) > (UInt32)sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(length)}) is not within the {nameof(sourceArray)}.");

            return new Memory<ELEMENT_T>(sourceArray, (Int32)offset, (Int32)length);
        }

        #endregion

        #region AsSpan

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<ELEMENT_T> AsSpan<ELEMENT_T>(this ELEMENT_T[] sourceArray, UInt32 offset)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (offset > (UInt32)sourceArray.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return new Span<ELEMENT_T>(sourceArray, checked((Int32)offset), checked((Int32)((UInt32)sourceArray.Length - offset)));
        }

        public static Span<ELEMENT_T> AsSpan<ELEMENT_T>(this ELEMENT_T[] sourceArray, UInt32 offset, UInt32 count)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (checked(offset + count) > sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(sourceArray)}.");

            return new Span<ELEMENT_T>(sourceArray, checked((Int32)offset), checked((Int32)count));
        }

        #endregion

        #region AsReadOnlyMemory

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<ELEMENT_T> AsReadOnlyMemory<ELEMENT_T>(this ELEMENT_T[] sourceArray)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));

            return new ReadOnlyMemory<ELEMENT_T>(sourceArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<ELEMENT_T> AsReadOnlyMemory<ELEMENT_T>(this ELEMENT_T[] sourceArray, Int32 offset)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (!offset.IsBetween(0, sourceArray.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            return new ReadOnlyMemory<ELEMENT_T>(sourceArray, offset, sourceArray.Length - offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<ELEMENT_T> AsReadOnlyMemory<ELEMENT_T>(this ELEMENT_T[] sourceArray, UInt32 offset)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (offset > (UInt32)sourceArray.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return new ReadOnlyMemory<ELEMENT_T>(sourceArray, (Int32)offset, (Int32)(sourceArray.Length - offset));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<ELEMENT_T> AsReadOnlyMemory<ELEMENT_T>(this ELEMENT_T[] sourceArray, Int32 offset, Int32 length)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (checked(offset + length) > (UInt32)sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(length)}) is not within the {nameof(sourceArray)}.");

            return new ReadOnlyMemory<ELEMENT_T>(sourceArray, offset, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<ELEMENT_T> AsReadOnlyMemory<ELEMENT_T>(this ELEMENT_T[] sourceArray, UInt32 offset, UInt32 length)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (checked(offset + length) > (UInt32)sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(length)}) is not within the {nameof(sourceArray)}.");

            return new ReadOnlyMemory<ELEMENT_T>(sourceArray, (Int32)offset, (Int32)length);
        }

        #endregion

        #region AsReadOnlySpan

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<ELEMENT_T> AsReadOnlySpan<ELEMENT_T>(this ELEMENT_T[] sourceArray)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));

            return (ReadOnlySpan<ELEMENT_T>)sourceArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<ELEMENT_T> AsReadOnlySpan<ELEMENT_T>(this ELEMENT_T[] sourceArray, Int32 offset)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (!offset.IsBetween(0, sourceArray.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            return new ReadOnlySpan<ELEMENT_T>(sourceArray, offset, sourceArray.Length - offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<ELEMENT_T> AsReadOnlySpan<ELEMENT_T>(this ELEMENT_T[] sourceArray, UInt32 offset)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (offset > (UInt32)sourceArray.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return new Span<ELEMENT_T>(sourceArray, (Int32)offset, sourceArray.Length - (Int32)offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<ELEMENT_T> AsReadOnlySpan<ELEMENT_T>(this ELEMENT_T[] sourceArray, Range range)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            var (isOk, offset, count) = sourceArray.GetOffsetAndLength(range);
            if (!isOk)
                throw new ArgumentOutOfRangeException(nameof(range));

            return new ReadOnlySpan<ELEMENT_T>(sourceArray, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<ELEMENT_T> AsReadOnlySpan<ELEMENT_T>(this ELEMENT_T[] sourceArray, Int32 offset, Int32 count)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(sourceArray)}.");

            return new ReadOnlySpan<ELEMENT_T>(sourceArray, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<ELEMENT_T> AsReadOnlySpan<ELEMENT_T>(this ELEMENT_T[] sourceArray, UInt32 offset, UInt32 count)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (checked(offset + count) > (UInt32)sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(sourceArray)}.");

            return new Span<ELEMENT_T>(sourceArray, checked((Int32)offset), checked((Int32)count));
        }

        #endregion

        #region Slice

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<ELEMENT_T> Slice<ELEMENT_T>(this ELEMENT_T[] sourceArray, Int32 offset)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (!offset.IsBetween(0, sourceArray.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            return new Memory<ELEMENT_T>(sourceArray, offset, sourceArray.Length - offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<ELEMENT_T> Slice<ELEMENT_T>(this ELEMENT_T[] sourceArray, UInt32 offset)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (offset > (UInt32)sourceArray.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return new Memory<ELEMENT_T>(sourceArray, (Int32)offset, (Int32)(sourceArray.Length - offset));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<ELEMENT_T> Slice<ELEMENT_T>(this ELEMENT_T[] sourceArray, Range range)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            Int32 offset;
            Int32 length;
            try
            {
                (offset, length) = range.GetOffsetAndLength(sourceArray.Length);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException(nameof(range));
            }

            return new Memory<ELEMENT_T>(sourceArray, offset, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<ELEMENT_T> Slice<ELEMENT_T>(this ELEMENT_T[] sourceArray, Int32 offset, Int32 length)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (checked(offset + length) > (UInt32)sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(length)}) is not within the {nameof(sourceArray)}.");

            return new Memory<ELEMENT_T>(sourceArray, offset, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<ELEMENT_T> Slice<ELEMENT_T>(this ELEMENT_T[] sourceArray, UInt32 offset, UInt32 length)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (checked(offset + length) > (UInt32)sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(length)}) is not within the {nameof(sourceArray)}.");

            return new Memory<ELEMENT_T>(sourceArray, (Int32)offset, (Int32)length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<ELEMENT_T> Slice<ELEMENT_T>(this Span<ELEMENT_T> sourceArray, UInt32 offset)
            => sourceArray[(Int32)offset..];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<ELEMENT_T> Slice<ELEMENT_T>(this Span<ELEMENT_T> sourceArray, UInt32 offset, UInt32 length)
            => sourceArray.Slice(checked((Int32)offset), checked((Int32)length));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<ELEMENT_T> Slice<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> sourceArray, UInt32 offset)
            => sourceArray[(Int32)offset..];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<ELEMENT_T> Slice<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> sourceArray, UInt32 offset, UInt32 length)
            => sourceArray.Slice(checked((Int32)offset), checked((Int32)length));

        #endregion

        #region GetSequence

        public static IEnumerable<ELEMENT_T> GetSequence<ELEMENT_T>(this Memory<ELEMENT_T> source)
        {
            for (var index = 0; index < source.Length; ++index)
                yield return source.Span[index];
        }

        public static IEnumerable<ELEMENT_T> GetSequence<ELEMENT_T>(this ReadOnlyMemory<ELEMENT_T> source)
        {
            for (var index = 0; index < source.Length; ++index)
                yield return source.Span[index];
        }

        #endregion

        #region GetPointer

        public static ArrayPointer<ELEMENT_T> GetPointer<ELEMENT_T>(this ELEMENT_T[] sourceArray, Int32 initialIndex = 0)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (!initialIndex.IsBetween(0, sourceArray.Length))
                throw new ArgumentOutOfRangeException(nameof(initialIndex));

            return new ArrayPointer<ELEMENT_T>(sourceArray, initialIndex);
        }

        public static ArrayPointer<ELEMENT_T> GetPointer<ELEMENT_T>(this ELEMENT_T[] sourceArray, UInt32 initialIndex)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (initialIndex > sourceArray.Length)
                throw new ArgumentOutOfRangeException(nameof(initialIndex));

            return new ArrayPointer<ELEMENT_T>(sourceArray, (Int32)initialIndex);
        }

        #endregion

        #region QuickSort

        public static ELEMENT_T[] QuickSort<ELEMENT_T>(this ELEMENT_T[] sourceArray)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));

            InternalQuickSort(sourceArray, 0, sourceArray.Length);
            return sourceArray;
        }

        public static ELEMENT_T[] QuickSort<ELEMENT_T, KEY_T>(this ELEMENT_T[] sourceArray, Func<ELEMENT_T, KEY_T> keySekecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (keySekecter is null)
                throw new ArgumentNullException(nameof(keySekecter));

            InternalQuickSort(sourceArray, 0, sourceArray.Length, keySekecter);
            return sourceArray;
        }

        public static ELEMENT_T[] QuickSort<ELEMENT_T>(this ELEMENT_T[] sourceArray, IComparer<ELEMENT_T> comparer)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            InternalQuickSort(sourceArray, 0, sourceArray.Length, comparer);
            return sourceArray;
        }

        public static ELEMENT_T[] QuickSort<ELEMENT_T, KEY_T>(this ELEMENT_T[] sourceArray, Func<ELEMENT_T, KEY_T> keySekecter, IComparer<KEY_T> keyComparer)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (keySekecter is null)
                throw new ArgumentNullException(nameof(keySekecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            InternalQuickSort(sourceArray, 0, sourceArray.Length, keySekecter, keyComparer);
            return sourceArray;
        }

        public static ELEMENT_T[] QuickSort<ELEMENT_T>(this ELEMENT_T[] sourceArray, Range range)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            var (isOk, offset, count) = sourceArray.GetOffsetAndLength(range);
            if (!isOk)
                throw new ArgumentOutOfRangeException(nameof(range));

            InternalQuickSort(sourceArray, offset, count);
            return sourceArray;
        }

        public static ELEMENT_T[] QuickSort<ELEMENT_T, KEY_T>(this ELEMENT_T[] sourceArray, Range range, Func<ELEMENT_T, KEY_T> keySekecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (keySekecter is null)
                throw new ArgumentNullException(nameof(keySekecter));
            var (isOk, offset, count) = sourceArray.GetOffsetAndLength(range);
            if (!isOk)
                throw new ArgumentOutOfRangeException(nameof(range));

            InternalQuickSort(sourceArray, offset, count, keySekecter);
            return sourceArray;
        }

        public static ELEMENT_T[] QuickSort<ELEMENT_T>(this ELEMENT_T[] sourceArray, Range range, IComparer<ELEMENT_T> comparer)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));
            var (isOk, offset, count) = sourceArray.GetOffsetAndLength(range);
            if (!isOk)
                throw new ArgumentOutOfRangeException(nameof(range));

            InternalQuickSort(sourceArray, offset, count, comparer);
            return sourceArray;
        }

        public static ELEMENT_T[] QuickSort<ELEMENT_T, KEY_T>(this ELEMENT_T[] sourceArray, Range range, Func<ELEMENT_T, KEY_T> keySekecter, IComparer<KEY_T> keyComparer)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (keySekecter is null)
                throw new ArgumentNullException(nameof(keySekecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));
            var (isOk, offset, count) = sourceArray.GetOffsetAndLength(range);
            if (!isOk)
                throw new ArgumentOutOfRangeException(nameof(range));

            InternalQuickSort(sourceArray, offset, count, keySekecter, keyComparer);
            return sourceArray;
        }

        public static ELEMENT_T[] QuickSort<ELEMENT_T>(this ELEMENT_T[] sourceArray, Int32 offset, Int32 count)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(sourceArray)}.");

            InternalQuickSort(sourceArray, offset, count);
            return sourceArray;
        }

        public static ELEMENT_T[] QuickSort<ELEMENT_T, KEY_T>(this ELEMENT_T[] sourceArray, Int32 offset, Int32 count, Func<ELEMENT_T, KEY_T> keySekecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(sourceArray)}.");
            if (keySekecter is null)
                throw new ArgumentNullException(nameof(keySekecter));

            InternalQuickSort(sourceArray, offset, count, keySekecter);
            return sourceArray;
        }

        public static ELEMENT_T[] QuickSort<ELEMENT_T>(this ELEMENT_T[] sourceArray, Int32 offset, Int32 count, IComparer<ELEMENT_T> comparer)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(sourceArray)}.");
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            InternalQuickSort(sourceArray, offset, count, comparer);
            return sourceArray;
        }

        public static ELEMENT_T[] QuickSort<ELEMENT_T, KEY_T>(this ELEMENT_T[] sourceArray, Int32 offset, Int32 count, Func<ELEMENT_T, KEY_T> keySekecter, IComparer<KEY_T> keyComparer)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(sourceArray)}.");
            if (keySekecter is null)
                throw new ArgumentNullException(nameof(keySekecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            InternalQuickSort(sourceArray, offset, count, keySekecter, keyComparer);
            return sourceArray;
        }

        public static ELEMENT_T[] QuickSort<ELEMENT_T>(this ELEMENT_T[] sourceArray, UInt32 offset, UInt32 count)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (checked(offset + count) > (UInt32)sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(sourceArray)}.");

            InternalQuickSort(sourceArray, checked((Int32)offset), checked((Int32)count));
            return sourceArray;
        }

        public static ELEMENT_T[] QuickSort<ELEMENT_T, KEY_T>(this ELEMENT_T[] sourceArray, UInt32 offset, UInt32 count, Func<ELEMENT_T, KEY_T> keySekecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (checked(offset + count) > (UInt32)sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(sourceArray)}.");
            if (keySekecter is null)
                throw new ArgumentNullException(nameof(keySekecter));

            InternalQuickSort(sourceArray, checked((Int32)offset), checked((Int32)count), keySekecter);
            return sourceArray;
        }

        public static ELEMENT_T[] QuickSort<ELEMENT_T>(this ELEMENT_T[] sourceArray, UInt32 offset, UInt32 count, IComparer<ELEMENT_T> comparer)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (checked(offset + count) > (UInt32)sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(sourceArray)}.");
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            InternalQuickSort(sourceArray, (Int32)offset, checked((Int32)count), comparer);
            return sourceArray;
        }

        public static ELEMENT_T[] QuickSort<ELEMENT_T, KEY_T>(this ELEMENT_T[] sourceArray, UInt32 offset, UInt32 count, Func<ELEMENT_T, KEY_T> keySekecter, IComparer<KEY_T> keyComparer)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (checked(offset + count) > (UInt32)sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(sourceArray)}.");
            if (keySekecter is null)
                throw new ArgumentNullException(nameof(keySekecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            InternalQuickSort(sourceArray, checked((Int32)offset), checked((Int32)offset), keySekecter, keyComparer);
            return sourceArray;
        }

        public static Span<ELEMENT_T> QuickSort<ELEMENT_T>(this Span<ELEMENT_T> sourceArray)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            InternalQuickSort(sourceArray);
            return sourceArray;
        }

        public static Span<ELEMENT_T> QuickSort<ELEMENT_T, KEY_T>(this Span<ELEMENT_T> sourceArray, Func<ELEMENT_T, KEY_T> keySekecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (keySekecter is null)
                throw new ArgumentNullException(nameof(keySekecter));

            InternalQuickSort(sourceArray, keySekecter);
            return sourceArray;
        }

        public static Span<ELEMENT_T> QuickSort<ELEMENT_T>(this Span<ELEMENT_T> sourceArray, IComparer<ELEMENT_T> comparer)
        {
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            InternalQuickSort(sourceArray, comparer);
            return sourceArray;
        }

        public static Span<ELEMENT_T> QuickSort<ELEMENT_T, KEY_T>(this Span<ELEMENT_T> sourceArray, Func<ELEMENT_T, KEY_T> keySekecter, IComparer<KEY_T> keyComparer)
        {
            if (keySekecter is null)
                throw new ArgumentNullException(nameof(keySekecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            InternalQuickSort(sourceArray, keySekecter, keyComparer);
            return sourceArray;
        }

        #endregion

        #region SequenceEqual

        public static Boolean SequenceEqual<ELEMENT_T>(this ELEMENT_T[] array1, ELEMENT_T[] array2)
            where ELEMENT_T : IEquatable<ELEMENT_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));

            return
                InternalSequenceEqual(
                    array1,
                    0,
                    array1.Length,
                    array2,
                    0,
                    array2.Length);
        }

        public static Boolean SequenceEqual<ELEMENT_T>(this ELEMENT_T[] array1, ELEMENT_T[] array2, IEqualityComparer<ELEMENT_T> equalityComparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (equalityComparer is null)
                throw new ArgumentNullException(nameof(equalityComparer));

            return
                InternalSequenceEqual(
                    array1,
                    0,
                    array1.Length,
                    array2,
                    0,
                    array2.Length,
                    equalityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, ELEMENT_T[] array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return
                InternalSequenceEqual(
                    array1,
                    0,
                    array1.Length,
                    array2,
                    0,
                    array2.Length,
                    keySelecter);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, ELEMENT_T[] array2, Func<ELEMENT_T, KEY_T> keySelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));

            return
                InternalSequenceEqual(
                    array1,
                    0,
                    array1.Length,
                    array2,
                    0,
                    array2.Length,
                    keySelecter,
                    keyEqualityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T>(this ELEMENT_T[] array1, Int32 array1Offset, ELEMENT_T[] array2, Int32 array2Offset, Int32 count)
            where ELEMENT_T : IEquatable<ELEMENT_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array1Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(array1Offset));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (array2Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(array2Offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(array1Offset + count) > array1.Length)
                throw new ArgumentException($"The specified range ({nameof(array1Offset)} and {nameof(count)}) is not within the {nameof(array1)}.");
            if (checked(array2Offset + count) > array2.Length)
                throw new ArgumentException($"The specified range ({nameof(array2Offset)} and {nameof(count)}) is not within the {nameof(array2)}.");

            return
                InternalSequenceEqual(
                    array1,
                    array1Offset,
                    count,
                    array2,
                    array2Offset,
                    count);
        }

        public static Boolean SequenceEqual<ELEMENT_T>(this ELEMENT_T[] array1, Int32 array1Offset, ELEMENT_T[] array2, Int32 array2Offset, Int32 count, IEqualityComparer<ELEMENT_T> equalityComparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array1Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(array1Offset));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (array2Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(array2Offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (equalityComparer is null)
                throw new ArgumentNullException(nameof(equalityComparer));
            if (checked(array1Offset + count) > array1.Length)
                throw new ArgumentException($"The specified range ({nameof(array1Offset)} and {nameof(count)}) is not within the {nameof(array1)}.");
            if (checked(array2Offset + count) > array2.Length)
                throw new ArgumentException($"The specified range ({nameof(array2Offset)} and {nameof(count)}) is not within the {nameof(array2)}.");

            return
                InternalSequenceEqual(
                    array1,
                    array1Offset,
                    count,
                    array2,
                    array2Offset,
                    count,
                    equalityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, Int32 array1Offset, ELEMENT_T[] array2, Int32 array2Offset, Int32 count, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array1Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(array1Offset));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (array2Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(array2Offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (checked(array1Offset + count) > array1.Length)
                throw new ArgumentException($"The specified range ({nameof(array1Offset)} and {nameof(count)}) is not within the {nameof(array1)}.");
            if (checked(array2Offset + count) > array2.Length)
                throw new ArgumentException($"The specified range ({nameof(array2Offset)} and {nameof(count)}) is not within the {nameof(array2)}.");

            return
                InternalSequenceEqual(
                    array1,
                    array1Offset,
                    count,
                    array2,
                    array2Offset,
                    count,
                    keySelecter);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, Int32 array1Offset, ELEMENT_T[] array2, Int32 array2Offset, Int32 count, Func<ELEMENT_T, KEY_T> keySelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array1Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(array1Offset));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (array2Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(array2Offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));
            if (checked(array1Offset + count) > array1.Length)
                throw new ArgumentException($"The specified range ({nameof(array1Offset)} and {nameof(count)}) is not within the {nameof(array1)}.");
            if (checked(array2Offset + count) > array2.Length)
                throw new ArgumentException($"The specified range ({nameof(array2Offset)} and {nameof(count)}) is not within the {nameof(array2)}.");

            return
                InternalSequenceEqual(
                    array1,
                    array1Offset,
                    count,
                    array2,
                    array2Offset,
                    count,
                    keySelecter,
                    keyEqualityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T>(this ELEMENT_T[] array1, UInt32 array1Offset, ELEMENT_T[] array2, UInt32 array2Offset, UInt32 count)
            where ELEMENT_T : IEquatable<ELEMENT_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (checked(array1Offset + count) > (UInt32)array1.Length)
                throw new ArgumentException($"The specified range ({nameof(array1Offset)} and {nameof(count)}) is not within the {nameof(array1)}.");
            if (checked(array2Offset + count) > (UInt32)array2.Length)
                throw new ArgumentException($"The specified range ({nameof(array2Offset)} and {nameof(count)}) is not within the {nameof(array2)}.");

            return
                InternalSequenceEqual(
                    array1,
                    (Int32)array1Offset,
                    (Int32)count,
                    array2,
                    (Int32)array2Offset,
                    (Int32)count);
        }

        public static Boolean SequenceEqual<ELEMENT_T>(this ELEMENT_T[] array1, UInt32 array1Offset, ELEMENT_T[] array2, UInt32 array2Offset, UInt32 count, IEqualityComparer<ELEMENT_T> equalityComparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (equalityComparer is null)
                throw new ArgumentNullException(nameof(equalityComparer));
            if (checked(array1Offset + count) > (UInt32)array1.Length)
                throw new ArgumentException($"The specified range ({nameof(array1Offset)} and {nameof(count)}) is not within the {nameof(array1)}.");
            if (checked(array2Offset + count) > (UInt32)array2.Length)
                throw new ArgumentException($"The specified range ({nameof(array2Offset)} and {nameof(count)}) is not within the {nameof(array2)}.");

            return
                InternalSequenceEqual(
                    array1,
                    (Int32)array1Offset,
                    (Int32)count,
                    array2,
                    (Int32)array2Offset,
                    (Int32)count,
                    equalityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, UInt32 array1Offset, ELEMENT_T[] array2, UInt32 array2Offset, UInt32 count, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (checked(array1Offset + count) > (UInt32)array1.Length)
                throw new ArgumentException($"The specified range ({nameof(array1Offset)} and {nameof(count)}) is not within the {nameof(array1)}.");
            if (checked(array2Offset + count) > (UInt32)array2.Length)
                throw new ArgumentException($"The specified range ({nameof(array2Offset)} and {nameof(count)}) is not within the {nameof(array2)}.");

            return
                InternalSequenceEqual(
                    array1,
                    (Int32)array1Offset,
                    (Int32)count,
                    array2,
                    (Int32)array2Offset,
                    (Int32)count,
                    keySelecter);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, UInt32 array1Offset, ELEMENT_T[] array2, UInt32 array2Offset, UInt32 count, Func<ELEMENT_T, KEY_T> keySelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));
            if (checked(array1Offset + count) > (UInt32)array1.Length)
                throw new ArgumentException($"The specified range ({nameof(array1Offset)} and {nameof(count)}) is not within the {nameof(array1)}.");
            if (checked(array2Offset + count) > (UInt32)array2.Length)
                throw new ArgumentException($"The specified range ({nameof(array2Offset)} and {nameof(count)}) is not within the {nameof(array2)}.");

            return
                InternalSequenceEqual(
                    array1,
                    (Int32)array1Offset,
                    (Int32)count,
                    array2,
                    (Int32)array2Offset,
                    (Int32)count,
                    keySelecter,
                    keyEqualityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T>(this ELEMENT_T[] array1, Span<ELEMENT_T> array2, IEqualityComparer<ELEMENT_T> equalityComparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (equalityComparer is null)
                throw new ArgumentNullException(nameof(equalityComparer));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, equalityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, Span<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, Span<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter, keyEqualityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T>(this ELEMENT_T[] array1, ReadOnlySpan<ELEMENT_T> array2)
            where ELEMENT_T : IEquatable<ELEMENT_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, array2);
        }

        public static Boolean SequenceEqual<ELEMENT_T>(this ELEMENT_T[] array1, ReadOnlySpan<ELEMENT_T> array2, IEqualityComparer<ELEMENT_T> equalityComparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (equalityComparer is null)
                throw new ArgumentNullException(nameof(equalityComparer));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, array2, equalityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, ReadOnlySpan<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, array2, keySelecter);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, ReadOnlySpan<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, array2, keySelecter, keyEqualityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T>(this Span<ELEMENT_T> array1, ELEMENT_T[] array2)
            where ELEMENT_T : IEquatable<ELEMENT_T>
        {
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2);
        }

        public static Boolean SequenceEqual<ELEMENT_T>(this Span<ELEMENT_T> array1, ELEMENT_T[] array2, IEqualityComparer<ELEMENT_T> equalityComparer)
        {
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (equalityComparer is null)
                throw new ArgumentNullException(nameof(equalityComparer));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, equalityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this Span<ELEMENT_T> array1, ELEMENT_T[] array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this Span<ELEMENT_T> array1, ELEMENT_T[] array2, Func<ELEMENT_T, KEY_T> keySelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
        {
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter, keyEqualityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T>(this Span<ELEMENT_T> array1, Span<ELEMENT_T> array2, IEqualityComparer<ELEMENT_T> equalityComparer)
        {
            if (equalityComparer is null)
                throw new ArgumentNullException(nameof(equalityComparer));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, equalityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this Span<ELEMENT_T> array1, Span<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this Span<ELEMENT_T> array1, Span<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter, keyEqualityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T>(this Span<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, IEqualityComparer<ELEMENT_T> equalityComparer)
        {
            if (equalityComparer is null)
                throw new ArgumentNullException(nameof(equalityComparer));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, array2, equalityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this Span<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, array2, keySelecter);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this Span<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));

            return InternalSequenceEqual((ReadOnlySpan<ELEMENT_T>)array1, array2, keySelecter, keyEqualityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> array1, ELEMENT_T[] array2)
            where ELEMENT_T : IEquatable<ELEMENT_T>
        {
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));

            return InternalSequenceEqual(array1, (ReadOnlySpan<ELEMENT_T>)array2);
        }

        public static Boolean SequenceEqual<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> array1, ELEMENT_T[] array2, IEqualityComparer<ELEMENT_T> equalityComparer)
        {
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (equalityComparer is null)
                throw new ArgumentNullException(nameof(equalityComparer));

            return InternalSequenceEqual(array1, (ReadOnlySpan<ELEMENT_T>)array2, equalityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this ReadOnlySpan<ELEMENT_T> array1, ELEMENT_T[] array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return InternalSequenceEqual(array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this ReadOnlySpan<ELEMENT_T> array1, ELEMENT_T[] array2, Func<ELEMENT_T, KEY_T> keySelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
        {
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));

            return InternalSequenceEqual(array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter, keyEqualityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> array1, Span<ELEMENT_T> array2, IEqualityComparer<ELEMENT_T> equalityComparer)
        {
            if (equalityComparer is null)
                throw new ArgumentNullException(nameof(equalityComparer));

            return InternalSequenceEqual(array1, (ReadOnlySpan<ELEMENT_T>)array2, equalityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this ReadOnlySpan<ELEMENT_T> array1, Span<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return InternalSequenceEqual(array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this ReadOnlySpan<ELEMENT_T> array1, Span<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));

            return InternalSequenceEqual(array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter, keyEqualityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, IEqualityComparer<ELEMENT_T> equalityComparer)
        {
            if (equalityComparer is null)
                throw new ArgumentNullException(nameof(equalityComparer));

            return InternalSequenceEqual(array1, array2, equalityComparer);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return InternalSequenceEqual(array1, array2, keySelecter);
        }

        public static Boolean SequenceEqual<ELEMENT_T, KEY_T>(this ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));

            return InternalSequenceEqual(array1, array2, keySelecter, keyEqualityComparer);
        }

        #endregion

        #region SequenceCompare

        public static Int32 SequenceCompare<ELEMENT_T>(this ELEMENT_T[] array1, ELEMENT_T[] array2)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));

            return
                InternalSequenceCompare(
                    array1,
                    0,
                    array1.Length,
                    array2,
                    0,
                    array2.Length);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this ELEMENT_T[] array1, ELEMENT_T[] array2, IComparer<ELEMENT_T> comparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            return
                InternalSequenceCompare(
                    array1,
                    0,
                    array1.Length,
                    array2,
                    0,
                    array2.Length,
                    comparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, ELEMENT_T[] array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return
                InternalSequenceCompare(
                    array1,
                    0,
                    array1.Length,
                    array2,
                    0,
                    array2.Length,
                    keySelecter);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, ELEMENT_T[] array2, Func<ELEMENT_T, KEY_T> keySelecter, IComparer<KEY_T> keyComparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            return
                InternalSequenceCompare(
                    array1,
                    0,
                    array1.Length,
                    array2,
                    0,
                    array2.Length,
                    keySelecter,
                    keyComparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this ELEMENT_T[] array1, Range array1Range, ELEMENT_T[] array2, Range array2Range)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            var (isOk1, array1Offset, array1Count) = array1.GetOffsetAndLength(array1Range);
            if (!isOk1)
                throw new ArgumentOutOfRangeException(nameof(array1Range));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            var (isOk2, array2Offset, array2Count) = array1.GetOffsetAndLength(array2Range);
            if (!isOk2)
                throw new ArgumentOutOfRangeException(nameof(array2Range));

            return
                InternalSequenceCompare(
                    array1,
                    array1Offset,
                    array1Count,
                    array2,
                    array2Offset,
                    array2Count);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this ELEMENT_T[] array1, Range array1Range, ELEMENT_T[] array2, Range array2Range, IComparer<ELEMENT_T> comparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            var (isOk1, array1Offset, array1Count) = array1.GetOffsetAndLength(array1Range);
            if (!isOk1)
                throw new ArgumentOutOfRangeException(nameof(array1Range));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            var (isOk2, array2Offset, array2Count) = array1.GetOffsetAndLength(array2Range);
            if (!isOk2)
                throw new ArgumentOutOfRangeException(nameof(array2Range));
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            return
                InternalSequenceCompare(
                    array1,
                    array1Offset,
                    array1Count,
                    array2,
                    array2Offset,
                    array2Count,
                    comparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, Range array1Range, ELEMENT_T[] array2, Range array2Range, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            var (isOk1, array1Offset, array1Count) = array1.GetOffsetAndLength(array1Range);
            if (!isOk1)
                throw new ArgumentOutOfRangeException(nameof(array1Range));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            var (isOk2, array2Offset, array2Count) = array1.GetOffsetAndLength(array2Range);
            if (!isOk2)
                throw new ArgumentOutOfRangeException(nameof(array2Range));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return
                InternalSequenceCompare(
                    array1,
                    array1Offset,
                    array1Count,
                    array2,
                    array2Offset,
                    array2Count,
                    keySelecter);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, Range array1Range, ELEMENT_T[] array2, Range array2Range, Func<ELEMENT_T, KEY_T> keySelecter, IComparer<KEY_T> keyComparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            var (isOk1, array1Offset, array1Count) = array1.GetOffsetAndLength(array1Range);
            if (!isOk1)
                throw new ArgumentOutOfRangeException(nameof(array1Range));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            var (isOk2, array2Offset, array2Count) = array1.GetOffsetAndLength(array2Range);
            if (!isOk2)
                throw new ArgumentOutOfRangeException(nameof(array2Range));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            return
                InternalSequenceCompare(
                    array1,
                    array1Offset,
                    array1Count,
                    array2,
                    array2Offset,
                    array2Count,
                    keySelecter,
                    keyComparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this ELEMENT_T[] array1, Int32 array1Offset, Int32 array1Count, ELEMENT_T[] array2, Int32 array2Offset, Int32 array2Count)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array1Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(array1Offset));
            if (array1Count < 0)
                throw new ArgumentOutOfRangeException(nameof(array1Count));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (array2Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(array2Offset));
            if (array2Count < 0)
                throw new ArgumentOutOfRangeException(nameof(array2Count));
            if (checked(array1Offset + array1Count) > array1.Length)
                throw new ArgumentException($"The specified range ({nameof(array1Offset)} and {nameof(array1Count)}) is not within the {nameof(array1)}.");
            if (checked(array2Offset + array2Count) > array2.Length)
                throw new ArgumentException($"The specified range ({nameof(array2Offset)} and {nameof(array2Count)}) is not within the {nameof(array2)}.");

            return
                InternalSequenceCompare(
                    array1,
                    array1Offset,
                    array1Count,
                    array2,
                    array2Offset,
                    array2Count);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this ELEMENT_T[] array1, Int32 array1Offset, Int32 array1Count, ELEMENT_T[] array2, Int32 array2Offset, Int32 array2Count, IComparer<ELEMENT_T> comparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array1Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(array1Offset));
            if (array1Count < 0)
                throw new ArgumentOutOfRangeException(nameof(array1Count));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (array2Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(array2Offset));
            if (array2Count < 0)
                throw new ArgumentOutOfRangeException(nameof(array2Count));
            if (checked(array1Offset + array1Count) > array1.Length)
                throw new ArgumentException($"The specified range ({nameof(array1Offset)} and {nameof(array1Count)}) is not within the {nameof(array1)}.");
            if (checked(array2Offset + array2Count) > array2.Length)
                throw new ArgumentException($"The specified range ({nameof(array2Offset)} and {nameof(array2Count)}) is not within the {nameof(array2)}.");
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            return
                InternalSequenceCompare(
                    array1,
                    array1Offset,
                    array1Count,
                    array2,
                    array2Offset,
                    array2Count,
                    comparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, Int32 array1Offset, Int32 array1Count, ELEMENT_T[] array2, Int32 array2Offset, Int32 array2Count, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array1Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(array1Offset));
            if (array1Count < 0)
                throw new ArgumentOutOfRangeException(nameof(array1Count));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (array2Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(array2Offset));
            if (array2Count < 0)
                throw new ArgumentOutOfRangeException(nameof(array2Count));
            if (checked(array1Offset + array1Count) > array1.Length)
                throw new ArgumentException($"The specified range ({nameof(array1Offset)} and {nameof(array1Count)}) is not within the {nameof(array1)}.");
            if (checked(array2Offset + array2Count) > array2.Length)
                throw new ArgumentException($"The specified range ({nameof(array2Offset)} and {nameof(array2Count)}) is not within the {nameof(array2)}.");
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return
                InternalSequenceCompare(
                    array1,
                    array1Offset,
                    array1Count,
                    array2,
                    array2Offset,
                    array2Count,
                    keySelecter);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, Int32 array1Offset, Int32 array1Count, ELEMENT_T[] array2, Int32 array2Offset, Int32 array2Count, Func<ELEMENT_T, KEY_T> keySelecter, IComparer<KEY_T> keyComparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array1Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(array1Offset));
            if (array1Count < 0)
                throw new ArgumentOutOfRangeException(nameof(array1Count));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (array2Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(array2Offset));
            if (array2Count < 0)
                throw new ArgumentOutOfRangeException(nameof(array2Count));
            if (checked(array1Offset + array1Count) > array1.Length)
                throw new ArgumentException($"The specified range ({nameof(array1Offset)} and {nameof(array1Count)}) is not within the {nameof(array1)}.");
            if (checked(array2Offset + array2Count) > array2.Length)
                throw new ArgumentException($"The specified range ({nameof(array2Offset)} and {nameof(array2Count)}) is not within the {nameof(array2)}.");
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            return
                InternalSequenceCompare(
                    array1,
                    array1Offset,
                    array1Count,
                    array2,
                    array2Offset,
                    array2Count,
                    keySelecter,
                    keyComparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this ELEMENT_T[] array1, UInt32 array1Offset, UInt32 array1Count, ELEMENT_T[] array2, UInt32 array2Offset, UInt32 array2Count)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (checked(array1Offset + array1Count) > (UInt32)array1.Length)
                throw new ArgumentException($"The specified range ({nameof(array1Offset)} and {nameof(array1Count)}) is not within the {nameof(array1)}.");
            if (checked(array2Offset + array2Count) > (UInt32)array2.Length)
                throw new ArgumentException($"The specified range ({nameof(array2Offset)} and {nameof(array2Count)}) is not within the {nameof(array2)}.");

            return
                InternalSequenceCompare(
                    array1,
                    (Int32)array1Offset,
                    (Int32)array1Count,
                    array2,
                    (Int32)array2Offset,
                    (Int32)array2Count);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this ELEMENT_T[] array1, UInt32 array1Offset, UInt32 array1Count, ELEMENT_T[] array2, UInt32 array2Offset, UInt32 array2Count, IComparer<ELEMENT_T> comparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (checked(array1Offset + array1Count) > (UInt32)array1.Length)
                throw new ArgumentException($"The specified range ({nameof(array1Offset)} and {nameof(array1Count)}) is not within the {nameof(array1)}.");
            if (checked(array2Offset + array2Count) > (UInt32)array2.Length)
                throw new ArgumentException($"The specified range ({nameof(array2Offset)} and {nameof(array2Count)}) is not within the {nameof(array2)}.");
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            return
                InternalSequenceCompare(
                    array1,
                    (Int32)array1Offset,
                    (Int32)array1Count,
                    array2,
                    (Int32)array2Offset,
                    (Int32)array2Count,
                    comparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, UInt32 array1Offset, UInt32 array1Count, ELEMENT_T[] array2, UInt32 array2Offset, UInt32 array2Count, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (checked(array1Offset + array1Count) > (UInt32)array1.Length)
                throw new ArgumentException($"The specified range ({nameof(array1Offset)} and {nameof(array1Count)}) is not within the {nameof(array1)}.");
            if (checked(array2Offset + array2Count) > (UInt32)array2.Length)
                throw new ArgumentException($"The specified range ({nameof(array2Offset)} and {nameof(array2Count)}) is not within the {nameof(array2)}.");
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return
                InternalSequenceCompare(
                    array1,
                    (Int32)array1Offset,
                    (Int32)array1Count,
                    array2,
                    (Int32)array2Offset,
                    (Int32)array2Count,
                    keySelecter);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, UInt32 array1Offset, UInt32 array1Count, ELEMENT_T[] array2, UInt32 array2Offset, UInt32 array2Count, Func<ELEMENT_T, KEY_T> keySelecter, IComparer<KEY_T> keyComparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (checked(array1Offset + array1Count) > (UInt32)array1.Length)
                throw new ArgumentException($"The specified range ({nameof(array1Offset)} and {nameof(array1Count)}) is not within the {nameof(array1)}.");
            if (checked(array2Offset + array2Count) > (UInt32)array2.Length)
                throw new ArgumentException($"The specified range ({nameof(array2Offset)} and {nameof(array2Count)}) is not within the {nameof(array2)}.");
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            return
                InternalSequenceCompare(
                    array1,
                    (Int32)array1Offset,
                    (Int32)array1Count,
                    array2,
                    (Int32)array2Offset,
                    (Int32)array2Count,
                    keySelecter,
                    keyComparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this ELEMENT_T[] array1, Span<ELEMENT_T> array2)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this ELEMENT_T[] array1, Span<ELEMENT_T> array2, IComparer<ELEMENT_T> comparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, comparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, Span<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, Span<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter, IComparer<KEY_T> keyComparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter, keyComparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this ELEMENT_T[] array1, ReadOnlySpan<ELEMENT_T> array2)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, array2);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this ELEMENT_T[] array1, ReadOnlySpan<ELEMENT_T> array2, IComparer<ELEMENT_T> comparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, array2, comparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, ReadOnlySpan<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, array2, keySelecter);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ELEMENT_T[] array1, ReadOnlySpan<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter, IComparer<KEY_T> keyComparer)
        {
            if (array1 is null)
                throw new ArgumentNullException(nameof(array1));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, array2, keySelecter, keyComparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this Span<ELEMENT_T> array1, ELEMENT_T[] array2)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this Span<ELEMENT_T> array1, ELEMENT_T[] array2, IComparer<ELEMENT_T> comparer)
        {
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, comparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this Span<ELEMENT_T> array1, ELEMENT_T[] array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this Span<ELEMENT_T> array1, ELEMENT_T[] array2, Func<ELEMENT_T, KEY_T> keySelecter, IComparer<KEY_T> keyComparer)
        {
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter, keyComparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this Span<ELEMENT_T> array1, Span<ELEMENT_T> array2)
            where ELEMENT_T : IComparable<ELEMENT_T>
            => InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2);

        public static Int32 SequenceCompare<ELEMENT_T>(this Span<ELEMENT_T> array1, Span<ELEMENT_T> array2, IComparer<ELEMENT_T> comparer)
        {
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, comparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this Span<ELEMENT_T> array1, Span<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this Span<ELEMENT_T> array1, Span<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter, IComparer<KEY_T> keyComparer)
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter, keyComparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this Span<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2)
            where ELEMENT_T : IComparable<ELEMENT_T>
            => InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, array2);

        public static Int32 SequenceCompare<ELEMENT_T>(this Span<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, IComparer<ELEMENT_T> comparer)
        {
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, array2, comparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this Span<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, array2, keySelecter);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this Span<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter, IComparer<KEY_T> keyComparer)
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            return InternalSequenceCompare((ReadOnlySpan<ELEMENT_T>)array1, array2, keySelecter, keyComparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> array1, ELEMENT_T[] array2)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));

            return InternalSequenceCompare(array1, (ReadOnlySpan<ELEMENT_T>)array2);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> array1, ELEMENT_T[] array2, IComparer<ELEMENT_T> comparer)
        {
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            return InternalSequenceCompare(array1, (ReadOnlySpan<ELEMENT_T>)array2, comparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ReadOnlySpan<ELEMENT_T> array1, ELEMENT_T[] array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return InternalSequenceCompare(array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ReadOnlySpan<ELEMENT_T> array1, ELEMENT_T[] array2, Func<ELEMENT_T, KEY_T> keySelecter, IComparer<KEY_T> keyComparer)
        {
            if (array2 is null)
                throw new ArgumentNullException(nameof(array2));
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            return InternalSequenceCompare(array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter, keyComparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> array1, Span<ELEMENT_T> array2)
            where ELEMENT_T : IComparable<ELEMENT_T>
            => InternalSequenceCompare(array1, (ReadOnlySpan<ELEMENT_T>)array2);

        public static Int32 SequenceCompare<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> array1, Span<ELEMENT_T> array2, IComparer<ELEMENT_T> comparer)
        {
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            return InternalSequenceCompare(array1, (ReadOnlySpan<ELEMENT_T>)array2, comparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ReadOnlySpan<ELEMENT_T> array1, Span<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return InternalSequenceCompare(array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ReadOnlySpan<ELEMENT_T> array1, Span<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter, IComparer<KEY_T> keyComparer)
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            return InternalSequenceCompare(array1, (ReadOnlySpan<ELEMENT_T>)array2, keySelecter, keyComparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2)
            where ELEMENT_T : IComparable<ELEMENT_T>
            => InternalSequenceCompare(array1, array2);

        public static Int32 SequenceCompare<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, IComparer<ELEMENT_T> comparer)
        {
            if (comparer is null)
                throw new ArgumentNullException(nameof(comparer));

            return InternalSequenceCompare(array1, array2, comparer);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return InternalSequenceCompare(array1, array2, keySelecter);
        }

        public static Int32 SequenceCompare<ELEMENT_T, KEY_T>(this ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter, IComparer<KEY_T> keyComparer)
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            return InternalSequenceCompare(array1, array2, keySelecter, keyComparer);
        }

        #endregion

        #region Duplicate

        public static ELEMENT_T[] Duplicate<ELEMENT_T>(this ELEMENT_T[] sourceArray)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));

            var buffer = new ELEMENT_T[sourceArray.Length];
            sourceArray.CopyTo(buffer, 0);
            return buffer;
        }

        public static Memory<ELEMENT_T> Duplicate<ELEMENT_T>(this Memory<ELEMENT_T> sourceArray)
        {
            var buffer = new ELEMENT_T[sourceArray.Length];
            sourceArray.Span.CopyTo(buffer);
            return buffer;
        }

        public static ReadOnlyMemory<ELEMENT_T> Duplicate<ELEMENT_T>(this ReadOnlyMemory<ELEMENT_T> source)
        {
            var buffer = new ELEMENT_T[source.Length];
            source.Span.CopyTo(buffer);
            return buffer;
        }

        public static Span<ELEMENT_T> Duplicate<ELEMENT_T>(this Span<ELEMENT_T> sourceArray)
        {
            var buffer = new ELEMENT_T[sourceArray.Length];
            sourceArray.CopyTo(buffer);
            return buffer;
        }

        public static ReadOnlySpan<ELEMENT_T> Duplicate<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> source)
        {
            var buffer = new ELEMENT_T[source.Length];
            source.CopyTo(buffer);
            return buffer;
        }

        #endregion

        #region ClearArray

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearArray<ELEMENT_T>(this ELEMENT_T[] buffer)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            Array.Clear(buffer, 0, buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearArray<ELEMENT_T>(this ELEMENT_T[] buffer, Int32 offset)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (!offset.IsBetween(0, buffer.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            Array.Clear(buffer, offset, buffer.Length - offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearArray<ELEMENT_T>(this ELEMENT_T[] buffer, UInt32 offset)
            => buffer.ClearArray(checked((Int32)offset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearArray<ELEMENT_T>(this ELEMENT_T[] buffer, Range range)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            var (isOk, offset, count) = buffer.GetOffsetAndLength(range);
            if (!isOk)
                throw new ArgumentOutOfRangeException(nameof(range));

            Array.Clear(buffer, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearArray<ELEMENT_T>(this ELEMENT_T[] buffer, Int32 offset, Int32 count)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            Array.Clear(buffer, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearArray<ELEMENT_T>(this ELEMENT_T[] buffer, UInt32 offset, UInt32 count)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            buffer.ClearArray(checked((Int32)offset), checked((Int32)count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearArray<ELEMENT_T>(this Span<ELEMENT_T> buffer) => buffer.Clear();

        #endregion

        #region FillArray

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillArray<ELEMENT_T>(this ELEMENT_T[] buffer, ELEMENT_T value)
            where ELEMENT_T : struct // もし ELEMENT_T が参照型だと同じ参照がすべての要素にコピーされバグの原因となりやすいため、値型に限定する
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            Array.Fill(buffer, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillArray<ELEMENT_T>(this ELEMENT_T[] buffer, ELEMENT_T value, Int32 offset)
            where ELEMENT_T : struct // もし ELEMENT_T が参照型だと同じ参照がすべての要素にコピーされバグの原因となりやすいため、値型に限定する
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (!offset.IsBetween(0, buffer.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            Array.Fill(buffer, value, offset, buffer.Length - offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillArray<ELEMENT_T>(this ELEMENT_T[] buffer, ELEMENT_T value, UInt32 offset)
            where ELEMENT_T : struct // もし ELEMENT_T が参照型だと同じ参照がすべての要素にコピーされバグの原因となりやすいため、値型に限定する
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            buffer.FillArray(value, checked((Int32)offset));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillArray<ELEMENT_T>(this ELEMENT_T[] buffer, ELEMENT_T value, Range range)
            where ELEMENT_T : struct // もし ELEMENT_T が参照型だと同じ参照がすべての要素にコピーされバグの原因となりやすいため、値型に限定する
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            var (isOk, offset, count) = buffer.GetOffsetAndLength(range);
            if (!isOk)
                throw new ArgumentOutOfRangeException(nameof(range));

            Array.Fill(buffer, value, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillArray<ELEMENT_T>(this ELEMENT_T[] buffer, ELEMENT_T value, Int32 offset, Int32 count)
            where ELEMENT_T : struct // もし ELEMENT_T が参照型だと同じ参照がすべての要素にコピーされバグの原因となりやすいため、値型に限定する
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            Array.Fill(buffer, value, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillArray<ELEMENT_T>(this ELEMENT_T[] buffer, ELEMENT_T value, UInt32 offset, UInt32 count)
            where ELEMENT_T : struct // もし ELEMENT_T が参照型だと同じ参照がすべての要素にコピーされバグの原因となりやすいため、値型に限定する
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            buffer.FillArray(value, checked((Int32)offset), checked((Int32)count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillArray<ELEMENT_T>(this Span<ELEMENT_T> buffer, ELEMENT_T value)
            where ELEMENT_T : struct // もし ELEMENT_T が参照型だと同じ参照がすべての要素にコピーされバグの原因となりやすいため、値型に限定する
            => buffer.Fill(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillArray<ELEMENT_T>(this ELEMENT_T[] buffer, Func<Int32, ELEMENT_T> valueGetter)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (valueGetter is null)
                throw new ArgumentNullException(nameof(valueGetter));

            var count = buffer.Length;
            for (var index = 0; index < count; ++index)
                buffer[index] = valueGetter(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillArray<ELEMENT_T>(this ELEMENT_T[] buffer, Func<Int32, ELEMENT_T> valueGetter, Int32 offset)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (valueGetter is null)
                throw new ArgumentNullException(nameof(valueGetter));
            if (!offset.IsBetween(0, buffer.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            var count = buffer.Length - offset;
            for (var index = 0; index < count; ++index)
                buffer[offset + index] = valueGetter(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillArray<ELEMENT_T>(this ELEMENT_T[] buffer, Func<Int32, ELEMENT_T> valueGetter, Range range)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (valueGetter is null)
                throw new ArgumentNullException(nameof(valueGetter));
            var (isOk, offset, count) = buffer.GetOffsetAndLength(range);
            if (!isOk)
                throw new ArgumentOutOfRangeException(nameof(range));

            for (var index = 0; index < count; ++index)
                buffer[offset + index] = valueGetter(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillArray<ELEMENT_T>(this ELEMENT_T[] buffer, Func<Int32, ELEMENT_T> valueGetter, Int32 offset, Int32 count)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (valueGetter is null)
                throw new ArgumentNullException(nameof(valueGetter));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            for (var index = 0; index < count; ++index)
                buffer[offset + index] = valueGetter(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillArray<ELEMENT_T>(this ELEMENT_T[] buffer, Func<UInt32, ELEMENT_T> valueGetter, UInt32 offset)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (valueGetter is null)
                throw new ArgumentNullException(nameof(valueGetter));
            if (offset > (UInt32)buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            var count = (UInt32)buffer.Length - offset;
            for (var index = 0U; index < count; ++index)
                buffer[offset + index] = valueGetter(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillArray<ELEMENT_T>(this ELEMENT_T[] buffer, Func<UInt32, ELEMENT_T> valueGetter, UInt32 offset, UInt32 count)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (valueGetter is null)
                throw new ArgumentNullException(nameof(valueGetter));
            if (checked(offset + count) > (UInt32)buffer.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(buffer)}.");

            for (var index = 0U; index < count; ++index)
                buffer[offset + index] = valueGetter(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillArray<ELEMENT_T>(this Span<ELEMENT_T> buffer, Func<Int32, ELEMENT_T> valueGetter)
        {
            if (valueGetter is null)
                throw new ArgumentNullException(nameof(valueGetter));

            var count = buffer.Length;
            for (var index = 0; index < count; ++index)
                buffer[index] = valueGetter(index);
        }

        #endregion

        #region CopyTo

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<ELEMENT_T>(this ELEMENT_T[] sourceArray, ELEMENT_T[] destinationArray, UInt32 destinationArrayOffset)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (destinationArray is null)
                throw new ArgumentNullException(nameof(destinationArray));
            if (checked(destinationArrayOffset + (UInt32)sourceArray.Length) > (UInt32)destinationArray.Length)
                throw new ArgumentException("There is not enough space for the copy destination.");

            sourceArray.CopyTo(destinationArray, (Int32)destinationArrayOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<ELEMENT_T>(this ELEMENT_T[] sourceArray, Int32 sourceArrayOffset, ELEMENT_T[] destinationArray, Int32 destinationArrayOffset, Int32 count)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (sourceArrayOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(sourceArrayOffset));
            if (destinationArray is null)
                throw new ArgumentNullException(nameof(destinationArray));
            if (destinationArrayOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(destinationArrayOffset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(sourceArrayOffset + count) > sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(sourceArrayOffset)} and {nameof(count)}) is not within the {nameof(sourceArray)}.");
            if (checked(destinationArrayOffset + count) > destinationArray.Length)
                throw new ArgumentException($"The specified range ({nameof(destinationArrayOffset)} and {nameof(count)}) is not within the {nameof(destinationArray)}.");

            Array.Copy(sourceArray, sourceArrayOffset, destinationArray, destinationArrayOffset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<ELEMENT_T>(this ELEMENT_T[] sourceArray, UInt32 sourceArrayOffset, ELEMENT_T[] destinationArray, UInt32 destinationArrayOffset, UInt32 count)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (destinationArray is null)
                throw new ArgumentNullException(nameof(destinationArray));
            if (checked(sourceArrayOffset + count) > (UInt32)sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(sourceArrayOffset)} and {nameof(count)}) is not within the {nameof(sourceArray)}.");
            if (checked(destinationArrayOffset + count) > (UInt32)destinationArray.Length)
                throw new ArgumentException($"The specified range ({nameof(destinationArrayOffset)} and {nameof(count)}) is not within the {nameof(destinationArray)}.");

            Array.Copy(sourceArray, (Int32)sourceArrayOffset, destinationArray, (Int32)destinationArrayOffset, (Int32)count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<ELEMENT_T>(this ELEMENT_T[] sourceArray, Span<ELEMENT_T> destinationArray)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));

            ((Span<ELEMENT_T>)sourceArray).CopyTo(destinationArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<ELEMENT_T>(this Span<ELEMENT_T> sourceArray, ELEMENT_T[] destinationArray)
        {
            if (destinationArray is null)
                throw new ArgumentNullException(nameof(destinationArray));

            sourceArray.CopyTo((Span<ELEMENT_T>)destinationArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> sourceArray, ELEMENT_T[] destinationArray)
            => sourceArray.CopyTo((Span<ELEMENT_T>)destinationArray);

        #endregion

        #region CopyMemoryTo

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyMemoryTo<ELEMENT_T>(this ELEMENT_T[] sourceArray, ELEMENT_T[] destinationArray)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (destinationArray is null)
                throw new ArgumentNullException(nameof(destinationArray));
            if (sourceArray.Length > destinationArray.Length)
                throw new ArgumentException("There is not enough space for the copy destination.");

            InternalCopyMemory(sourceArray, 0, destinationArray, 0, sourceArray.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyMemoryTo<ELEMENT_T>(this ELEMENT_T[] sourceArray, ELEMENT_T[] destinationArray, Int32 destinationArrayOffset)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (destinationArray is null)
                throw new ArgumentNullException(nameof(destinationArray));
            if (destinationArrayOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(destinationArrayOffset));
            if (checked(destinationArrayOffset + sourceArray.Length) > destinationArray.Length)
                throw new ArgumentException("There is not enough space for the copy destination.");

            InternalCopyMemory(sourceArray, 0, destinationArray, destinationArrayOffset, sourceArray.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyMemoryTo<ELEMENT_T>(this ELEMENT_T[] sourceArray, ELEMENT_T[] destinationArray, UInt32 destinationArrayOffset)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (destinationArray is null)
                throw new ArgumentNullException(nameof(destinationArray));
            if (checked(destinationArrayOffset + (UInt32)sourceArray.Length) > (UInt32)destinationArray.Length)
                throw new ArgumentException("There is not enough space for the copy destination.");

            InternalCopyMemory(sourceArray, 0, destinationArray, (Int32)destinationArrayOffset, sourceArray.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyMemoryTo<ELEMENT_T>(this ELEMENT_T[] sourceArray, Int32 sourceArrayOffset, ELEMENT_T[] destinationArray, Int32 destinationArrayOffset, Int32 count)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (sourceArrayOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(sourceArrayOffset));
            if (destinationArray is null)
                throw new ArgumentNullException(nameof(destinationArray));
            if (destinationArrayOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(destinationArrayOffset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(sourceArrayOffset + count) > sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(sourceArrayOffset)} and {nameof(count)}) is not within the {nameof(sourceArray)}.");
            if (checked(destinationArrayOffset + count) > destinationArray.Length)
                throw new ArgumentException($"The specified range ({nameof(destinationArrayOffset)} and {nameof(count)}) is not within the {nameof(destinationArray)}.");

            InternalCopyMemory(sourceArray, sourceArrayOffset, destinationArray, destinationArrayOffset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyMemoryTo<ELEMENT_T>(this ELEMENT_T[] sourceArray, UInt32 sourceArrayOffset, ELEMENT_T[] destinationArray, UInt32 destinationArrayOffset, UInt32 count)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (destinationArray is null)
                throw new ArgumentNullException(nameof(destinationArray));
            if (checked(sourceArrayOffset + count) > sourceArray.Length)
                throw new ArgumentException($"The specified range ({nameof(sourceArrayOffset)} and {nameof(count)}) is not within the {nameof(sourceArray)}.");
            if (checked(destinationArrayOffset + count) > destinationArray.Length)
                throw new ArgumentException($"The specified range ({nameof(destinationArrayOffset)} and {nameof(count)}) is not within the {nameof(destinationArray)}.");

            InternalCopyMemory(sourceArray, (Int32)sourceArrayOffset, destinationArray, (Int32)destinationArrayOffset, (Int32)count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyMemoryTo<ELEMENT_T>(this ELEMENT_T[] sourceArray, Span<ELEMENT_T> destinationArray)
        {
            if (sourceArray is null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (destinationArray.Length < sourceArray.Length)
                throw new ArgumentException($"{nameof(destinationArray)} is shorter than {nameof(sourceArray)}");

            InternalCopyMemory((ReadOnlySpan<ELEMENT_T>)sourceArray, destinationArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyMemoryTo<ELEMENT_T>(this Span<ELEMENT_T> sourceArray, ELEMENT_T[] destinationArray)
        {
            if (destinationArray is null)
                throw new ArgumentNullException(nameof(destinationArray));
            if (destinationArray.Length < sourceArray.Length)
                throw new ArgumentException($"{nameof(destinationArray)} is shorter than {nameof(sourceArray)}");

            InternalCopyMemory((ReadOnlySpan<ELEMENT_T>)sourceArray, (Span<ELEMENT_T>)destinationArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyMemoryTo<ELEMENT_T>(this Span<ELEMENT_T> sourceArray, Span<ELEMENT_T> destinationArray)
        {
            if (destinationArray.Length < sourceArray.Length)
                throw new ArgumentException($"{nameof(destinationArray)} is shorter than {nameof(sourceArray)}");

            InternalCopyMemory((ReadOnlySpan<ELEMENT_T>)sourceArray, destinationArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyMemoryTo<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> sourceArray, ELEMENT_T[] destinationArray)
        {
            if (destinationArray.Length < sourceArray.Length)
                throw new ArgumentException($"{nameof(destinationArray)} is shorter than {nameof(sourceArray)}");

            InternalCopyMemory(sourceArray, (Span<ELEMENT_T>)destinationArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyMemoryTo<ELEMENT_T>(this ReadOnlySpan<ELEMENT_T> sourceArray, Span<ELEMENT_T> destinationArray)
        {
            if (destinationArray.Length < sourceArray.Length)
                throw new ArgumentException($"{nameof(destinationArray)} is shorter than {nameof(sourceArray)}");

            InternalCopyMemory(sourceArray, destinationArray);
        }

        #endregion

        #region ReverseArray

        /// <summary>
        /// 与えられた配列の要素を逆順に並べ替えます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// 配列の要素の型です。
        /// </typeparam>
        /// <param name="source">
        /// 並び替える配列です。
        /// </param>
        /// <returns>
        /// 並び替えられた配列です。この配列は <paramref name="source"/> と同じ参照です。
        /// </returns>
        /// <remarks>
        /// このメソッドは<paramref name="source"/> で与えられた配列の内容を変更します。
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> が nullです。
        /// </exception>
        public static ELEMENT_T[] ReverseArray<ELEMENT_T>(this ELEMENT_T[] source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            InternalReverseArray(source, 0, source.Length);
            return source;
        }

        /// <summary>
        /// 与えられた配列の指定された範囲の要素を逆順に並べ替えます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// 配列の要素の型です。
        /// </typeparam>
        /// <param name="source">
        /// 並び替える配列です。
        /// </param>
        /// <param name="offset">
        /// 並び替える範囲の開始位置です。
        /// </param>
        /// <param name="count">
        /// 並び替える範囲の長さです。
        /// </param>
        /// <returns>
        /// 並び替えられた配列です。この配列は <paramref name="source"/> と同じ参照です。
        /// </returns>
        /// <remarks>
        /// このメソッドは<paramref name="source"/> で与えられた配列の内容を変更します。
        /// </remarks>
        /// <paramref name="source"/> が nullです。
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> または <paramref name="count"/> が負の値です。
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="offset"/> および <paramref name="count"/> で指定された範囲が <paramref name="source"/> の範囲外です。
        /// </exception>
        public static ELEMENT_T[] ReverseArray<ELEMENT_T>(this ELEMENT_T[] source, Int32 offset, Int32 count)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (checked(offset + count) > source.Length)
                throw new ArgumentException($"The specified range ({nameof(offset)} and {nameof(count)}) is not within the {nameof(source)}.");

            InternalReverseArray(source, offset, count);
            return source;
        }

        /// <summary>
        /// 与えられた配列の要素を逆順に並べ替えます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// 配列の要素の型です。
        /// </typeparam>
        /// <param name="source">
        /// 並び替える配列です。
        /// </param>
        /// <returns>
        /// 並び替えられた配列です。この配列は <paramref name="source"/> と同じ参照です。
        /// </returns>
        /// <remarks>
        /// このメソッドは<paramref name="source"/> で与えられた配列の内容を変更します。
        /// </remarks>
        public static Memory<ELEMENT_T> ReverseArray<ELEMENT_T>(this Memory<ELEMENT_T> source)
        {
            InternalReverseArray(source.Span);
            return source;
        }

        /// <summary>
        /// 与えられた配列の要素を逆順に並べ替えます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// 配列の要素の型です。
        /// </typeparam>
        /// <param name="source">
        /// 並び替える配列です。
        /// </param>
        /// <returns>
        /// 並び替えられた配列です。この配列は <paramref name="source"/> と同じ参照です。
        /// </returns>
        /// <remarks>
        /// このメソッドは<paramref name="source"/> で与えられた配列の内容を変更します。
        /// </remarks>
        public static Span<ELEMENT_T> ReverseArray<ELEMENT_T>(this Span<ELEMENT_T> source)
        {
            InternalReverseArray(source);
            return source;
        }

        #endregion

        #region ToDictionary

        public static IDictionary<KEY_T, ELEMENT_T> ToDictionary<ELEMENT_T, KEY_T>(this ELEMENT_T[] source, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return ((ReadOnlySpan<ELEMENT_T>)source).ToDictionary(keySelecter);
        }

        public static IDictionary<KEY_T, ELEMENT_T> ToDictionary<ELEMENT_T, KEY_T>(this ELEMENT_T[] source, Func<ELEMENT_T, KEY_T> keySelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
            where KEY_T : IEquatable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));

            return ((ReadOnlySpan<ELEMENT_T>)source).ToDictionary(keySelecter, keyEqualityComparer);
        }

        public static IDictionary<KEY_T, VALUE_T> ToDictionary<ELEMENT_T, KEY_T, VALUE_T>(this ELEMENT_T[] source, Func<ELEMENT_T, KEY_T> keySelecter, Func<ELEMENT_T, VALUE_T> valueSelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (valueSelecter is null)
                throw new ArgumentNullException(nameof(valueSelecter));

            return ((ReadOnlySpan<ELEMENT_T>)source).ToDictionary(keySelecter, valueSelecter);
        }

        public static IDictionary<KEY_T, VALUE_T> ToDictionary<ELEMENT_T, KEY_T, VALUE_T>(this ELEMENT_T[] source, Func<ELEMENT_T, KEY_T> keySelecter, Func<ELEMENT_T, VALUE_T> valueSelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
            where KEY_T : IEquatable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (valueSelecter is null)
                throw new ArgumentNullException(nameof(valueSelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));

            return ((ReadOnlySpan<ELEMENT_T>)source).ToDictionary(keySelecter, valueSelecter, keyEqualityComparer);
        }

        public static IDictionary<KEY_T, ELEMENT_T> ToDictionary<ELEMENT_T, KEY_T>(this Span<ELEMENT_T> source, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            return ((ReadOnlySpan<ELEMENT_T>)source).ToDictionary(keySelecter);
        }

        public static IDictionary<KEY_T, ELEMENT_T> ToDictionary<ELEMENT_T, KEY_T>(this Span<ELEMENT_T> source, Func<ELEMENT_T, KEY_T> keySelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
            where KEY_T : IEquatable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));

            return ((ReadOnlySpan<ELEMENT_T>)source).ToDictionary(keySelecter, keyEqualityComparer);
        }

        public static IDictionary<KEY_T, VALUE_T> ToDictionary<ELEMENT_T, KEY_T, VALUE_T>(this Span<ELEMENT_T> source, Func<ELEMENT_T, KEY_T> keySelecter, Func<ELEMENT_T, VALUE_T> valueSelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (valueSelecter is null)
                throw new ArgumentNullException(nameof(valueSelecter));

            return ((ReadOnlySpan<ELEMENT_T>)source).ToDictionary(keySelecter, valueSelecter);
        }

        public static IDictionary<KEY_T, VALUE_T> ToDictionary<ELEMENT_T, KEY_T, VALUE_T>(this Span<ELEMENT_T> source, Func<ELEMENT_T, KEY_T> keySelecter, Func<ELEMENT_T, VALUE_T> valueSelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
            where KEY_T : IEquatable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (valueSelecter is null)
                throw new ArgumentNullException(nameof(valueSelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));

            return ((ReadOnlySpan<ELEMENT_T>)source).ToDictionary(keySelecter, valueSelecter, keyEqualityComparer);
        }

        public static IDictionary<KEY_T, ELEMENT_T> ToDictionary<ELEMENT_T, KEY_T>(this ReadOnlySpan<ELEMENT_T> source, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            var dictionary = new Dictionary<KEY_T, ELEMENT_T>();
            foreach (var element in source)
                dictionary.Add(keySelecter(element), element);
            return dictionary;
        }

        public static IDictionary<KEY_T, ELEMENT_T> ToDictionary<ELEMENT_T, KEY_T>(this ReadOnlySpan<ELEMENT_T> source, Func<ELEMENT_T, KEY_T> keySelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
            where KEY_T : IEquatable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));

            var dictionary = new Dictionary<KEY_T, ELEMENT_T>(keyEqualityComparer);
            foreach (var element in source)
                dictionary.Add(keySelecter(element), element);
            return dictionary;
        }

        public static IDictionary<KEY_T, VALUE_T> ToDictionary<ELEMENT_T, KEY_T, VALUE_T>(this ReadOnlySpan<ELEMENT_T> source, Func<ELEMENT_T, KEY_T> keySelecter, Func<ELEMENT_T, VALUE_T> valueSelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (valueSelecter is null)
                throw new ArgumentNullException(nameof(valueSelecter));

            var dictionary = new Dictionary<KEY_T, VALUE_T>();
            foreach (var element in source)
                dictionary.Add(keySelecter(element), valueSelecter(element));
            return dictionary;
        }

        public static IDictionary<KEY_T, VALUE_T> ToDictionary<ELEMENT_T, KEY_T, VALUE_T>(this ReadOnlySpan<ELEMENT_T> source, Func<ELEMENT_T, KEY_T> keySelecter, Func<ELEMENT_T, VALUE_T> valueSelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
            where KEY_T : IEquatable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (valueSelecter is null)
                throw new ArgumentNullException(nameof(valueSelecter));
            if (keyEqualityComparer is null)
                throw new ArgumentNullException(nameof(keyEqualityComparer));

            var dictionary = new Dictionary<KEY_T, VALUE_T>(keyEqualityComparer);
            foreach (var element in source)
                dictionary.Add(keySelecter(element), valueSelecter(element));
            return dictionary;
        }

        #endregion

        #region InternalQuickSort

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void InternalQuickSort<ELEMENT_T>(ELEMENT_T[] source, Int32 offset, Int32 count)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            if (source.Length < 2)
                return;

            switch (Type.GetTypeCode(typeof(ELEMENT_T)))
            {
                case TypeCode.Boolean:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref source[offset]), count);
                    break;
                case TypeCode.Char:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref source[offset]), count);
                    break;
                case TypeCode.SByte:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref source[offset]), count);
                    break;
                case TypeCode.Byte:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref source[offset]), count);
                    break;
                case TypeCode.Int16:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref source[offset]), count);
                    break;
                case TypeCode.UInt16:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref source[offset]), count);
                    break;
                case TypeCode.Int32:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref source[offset]), count);
                    break;
                case TypeCode.UInt32:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref source[offset]), count);
                    break;
                case TypeCode.Int64:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref source[offset]), count);
                    break;
                case TypeCode.UInt64:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref source[offset]), count);
                    break;
                case TypeCode.Single:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref source[offset]), count);
                    break;
                case TypeCode.Double:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref source[offset]), count);
                    break;
                case TypeCode.Decimal:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref source[offset]), count);
                    break;
                default:
                    InternalQuickSortManaged(source, offset, offset + count - 1);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void InternalQuickSort<ELEMENT_T, KEY_T>(ELEMENT_T[] source, Int32 offset, Int32 count, Func<ELEMENT_T, KEY_T> keySelector)
            where KEY_T : IComparable<KEY_T>
        {
            if (source.Length < 2)
                return;

            switch (Type.GetTypeCode(typeof(ELEMENT_T)))
            {
                case TypeCode.Boolean:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Boolean, KEY_T>>(ref keySelector));
                    break;
                case TypeCode.Char:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Char, KEY_T>>(ref keySelector));
                    break;
                case TypeCode.SByte:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<SByte, KEY_T>>(ref keySelector));
                    break;
                case TypeCode.Byte:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Byte, KEY_T>>(ref keySelector));
                    break;
                case TypeCode.Int16:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Int16, KEY_T>>(ref keySelector));
                    break;
                case TypeCode.UInt16:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<UInt16, KEY_T>>(ref keySelector));
                    break;
                case TypeCode.Int32:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Int32, KEY_T>>(ref keySelector));
                    break;
                case TypeCode.UInt32:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<UInt32, KEY_T>>(ref keySelector));
                    break;
                case TypeCode.Int64:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Int64, KEY_T>>(ref keySelector));
                    break;
                case TypeCode.UInt64:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<UInt64, KEY_T>>(ref keySelector));
                    break;
                case TypeCode.Single:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Single, KEY_T>>(ref keySelector));
                    break;
                case TypeCode.Double:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Double, KEY_T>>(ref keySelector));
                    break;
                case TypeCode.Decimal:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Decimal, KEY_T>>(ref keySelector));
                    break;
                default:
                    InternalQuickSortManaged(source, offset, offset + count - 1, keySelector);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void InternalQuickSort<ELEMENT_T>(ELEMENT_T[] source, Int32 offset, Int32 count, IComparer<ELEMENT_T> comparer)
        {
            if (source.Length < 2)
                return;

            switch (Type.GetTypeCode(typeof(ELEMENT_T)))
            {
                case TypeCode.Boolean:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref source[offset]), count, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Boolean>>(ref comparer));
                    break;
                case TypeCode.Char:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref source[offset]), count, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Char>>(ref comparer));
                    break;
                case TypeCode.SByte:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref source[offset]), count, Unsafe.As<IComparer<ELEMENT_T>, IComparer<SByte>>(ref comparer));
                    break;
                case TypeCode.Byte:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref source[offset]), count, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Byte>>(ref comparer));
                    break;
                case TypeCode.Int16:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref source[offset]), count, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Int16>>(ref comparer));
                    break;
                case TypeCode.UInt16:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref source[offset]), count, Unsafe.As<IComparer<ELEMENT_T>, IComparer<UInt16>>(ref comparer));
                    break;
                case TypeCode.Int32:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref source[offset]), count, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Int32>>(ref comparer));
                    break;
                case TypeCode.UInt32:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref source[offset]), count, Unsafe.As<IComparer<ELEMENT_T>, IComparer<UInt32>>(ref comparer));
                    break;
                case TypeCode.Int64:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref source[offset]), count, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Int64>>(ref comparer));
                    break;
                case TypeCode.UInt64:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref source[offset]), count, Unsafe.As<IComparer<ELEMENT_T>, IComparer<UInt64>>(ref comparer));
                    break;
                case TypeCode.Single:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref source[offset]), count, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Single>>(ref comparer));
                    break;
                case TypeCode.Double:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref source[offset]), count, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Double>>(ref comparer));
                    break;
                case TypeCode.Decimal:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref source[offset]), count, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Decimal>>(ref comparer));
                    break;
                default:
                    InternalQuickSortManaged(source, offset, offset + count - 1, comparer);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void InternalQuickSort<ELEMENT_T, KEY_T>(ELEMENT_T[] source, Int32 offset, Int32 count, Func<ELEMENT_T, KEY_T> keySelector, IComparer<KEY_T> keyComparer)
        {
            if (source.Length < 2)
                return;

            switch (Type.GetTypeCode(typeof(ELEMENT_T)))
            {
                case TypeCode.Boolean:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Boolean, KEY_T>>(ref keySelector), keyComparer);
                    break;
                case TypeCode.Char:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Char, KEY_T>>(ref keySelector), keyComparer);
                    break;
                case TypeCode.SByte:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<SByte, KEY_T>>(ref keySelector), keyComparer);
                    break;
                case TypeCode.Byte:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Byte, KEY_T>>(ref keySelector), keyComparer);
                    break;
                case TypeCode.Int16:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Int16, KEY_T>>(ref keySelector), keyComparer);
                    break;
                case TypeCode.UInt16:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<UInt16, KEY_T>>(ref keySelector), keyComparer);
                    break;
                case TypeCode.Int32:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Int32, KEY_T>>(ref keySelector), keyComparer);
                    break;
                case TypeCode.UInt32:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<UInt32, KEY_T>>(ref keySelector), keyComparer);
                    break;
                case TypeCode.Int64:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Int64, KEY_T>>(ref keySelector), keyComparer);
                    break;
                case TypeCode.UInt64:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<UInt64, KEY_T>>(ref keySelector), keyComparer);
                    break;
                case TypeCode.Single:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Single, KEY_T>>(ref keySelector), keyComparer);
                    break;
                case TypeCode.Double:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Double, KEY_T>>(ref keySelector), keyComparer);
                    break;
                case TypeCode.Decimal:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref source[offset]), count, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Decimal, KEY_T>>(ref keySelector), keyComparer);
                    break;
                default:
                    InternalQuickSortManaged(source, offset, offset + count - 1, keySelector, keyComparer);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void InternalQuickSort<ELEMENT_T>(Span<ELEMENT_T> source)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            if (source.Length < 2)
                return;

            switch (Type.GetTypeCode(typeof(ELEMENT_T)))
            {
                case TypeCode.Boolean:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.Char:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.SByte:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.Byte:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.Int16:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.UInt16:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.Int32:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.UInt32:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.Int64:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.UInt64:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.Single:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.Double:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.Decimal:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref source.GetPinnableReference()), source.Length);
                    break;
                default:
                    InternalQuickSortManaged(source, 0, source.Length - 1);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void InternalQuickSort<ELEMENT_T, KEY_T>(Span<ELEMENT_T> source, Func<ELEMENT_T, KEY_T> keySekecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (source.Length < 2)
                return;

            switch (Type.GetTypeCode(typeof(ELEMENT_T)))
            {
                case TypeCode.Boolean:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Boolean, KEY_T>>(ref keySekecter));
                    break;
                case TypeCode.Char:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Char, KEY_T>>(ref keySekecter));
                    break;
                case TypeCode.SByte:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<SByte, KEY_T>>(ref keySekecter));
                    break;
                case TypeCode.Byte:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Byte, KEY_T>>(ref keySekecter));
                    break;
                case TypeCode.Int16:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Int16, KEY_T>>(ref keySekecter));
                    break;
                case TypeCode.UInt16:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<UInt16, KEY_T>>(ref keySekecter));
                    break;
                case TypeCode.Int32:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Int32, KEY_T>>(ref keySekecter));
                    break;
                case TypeCode.UInt32:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<UInt32, KEY_T>>(ref keySekecter));
                    break;
                case TypeCode.Int64:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Int64, KEY_T>>(ref keySekecter));
                    break;
                case TypeCode.UInt64:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<UInt64, KEY_T>>(ref keySekecter));
                    break;
                case TypeCode.Single:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Single, KEY_T>>(ref keySekecter));
                    break;
                case TypeCode.Double:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Double, KEY_T>>(ref keySekecter));
                    break;
                case TypeCode.Decimal:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Decimal, KEY_T>>(ref keySekecter));
                    break;
                default:
                    InternalQuickSortManaged(source, 0, source.Length - 1, keySekecter);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void InternalQuickSort<ELEMENT_T>(Span<ELEMENT_T> source, IComparer<ELEMENT_T> comparer)
        {
            if (source.Length < 2)
                return;

            switch (Type.GetTypeCode(typeof(ELEMENT_T)))
            {
                case TypeCode.Boolean:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref source.GetPinnableReference()), source.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Boolean>>(ref comparer));
                    break;
                case TypeCode.Char:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref source.GetPinnableReference()), source.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Char>>(ref comparer));
                    break;
                case TypeCode.SByte:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref source.GetPinnableReference()), source.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<SByte>>(ref comparer));
                    break;
                case TypeCode.Byte:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref source.GetPinnableReference()), source.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Byte>>(ref comparer));
                    break;
                case TypeCode.Int16:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref source.GetPinnableReference()), source.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Int16>>(ref comparer));
                    break;
                case TypeCode.UInt16:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref source.GetPinnableReference()), source.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<UInt16>>(ref comparer));
                    break;
                case TypeCode.Int32:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref source.GetPinnableReference()), source.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Int32>>(ref comparer));
                    break;
                case TypeCode.UInt32:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref source.GetPinnableReference()), source.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<UInt32>>(ref comparer));
                    break;
                case TypeCode.Int64:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref source.GetPinnableReference()), source.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Int64>>(ref comparer));
                    break;
                case TypeCode.UInt64:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref source.GetPinnableReference()), source.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<UInt64>>(ref comparer));
                    break;
                case TypeCode.Single:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref source.GetPinnableReference()), source.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Single>>(ref comparer));
                    break;
                case TypeCode.Double:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref source.GetPinnableReference()), source.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Double>>(ref comparer));
                    break;
                case TypeCode.Decimal:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref source.GetPinnableReference()), source.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Decimal>>(ref comparer));
                    break;
                default:
                    InternalQuickSortManaged(source, 0, source.Length - 1, comparer);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void InternalQuickSort<ELEMENT_T, KEY_T>(Span<ELEMENT_T> source, Func<ELEMENT_T, KEY_T> keySekecter, IComparer<KEY_T> keyComparer)
        {
            if (source.Length < 2)
                return;

            switch (Type.GetTypeCode(typeof(ELEMENT_T)))
            {
                case TypeCode.Boolean:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Boolean, KEY_T>>(ref keySekecter), keyComparer);
                    break;
                case TypeCode.Char:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Char, KEY_T>>(ref keySekecter), keyComparer);
                    break;
                case TypeCode.SByte:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<SByte, KEY_T>>(ref keySekecter), keyComparer);
                    break;
                case TypeCode.Byte:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Byte, KEY_T>>(ref keySekecter), keyComparer);
                    break;
                case TypeCode.Int16:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Int16, KEY_T>>(ref keySekecter), keyComparer);
                    break;
                case TypeCode.UInt16:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<UInt16, KEY_T>>(ref keySekecter), keyComparer);
                    break;
                case TypeCode.Int32:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Int32, KEY_T>>(ref keySekecter), keyComparer);
                    break;
                case TypeCode.UInt32:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<UInt32, KEY_T>>(ref keySekecter), keyComparer);
                    break;
                case TypeCode.Int64:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Int64, KEY_T>>(ref keySekecter), keyComparer);
                    break;
                case TypeCode.UInt64:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<UInt64, KEY_T>>(ref keySekecter), keyComparer);
                    break;
                case TypeCode.Single:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Single, KEY_T>>(ref keySekecter), keyComparer);
                    break;
                case TypeCode.Double:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Double, KEY_T>>(ref keySekecter), keyComparer);
                    break;
                case TypeCode.Decimal:
                    InternalQuickSortUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref source.GetPinnableReference()), source.Length, Unsafe.As<Func<ELEMENT_T, KEY_T>, Func<Decimal, KEY_T>>(ref keySekecter), keyComparer);
                    break;
                default:
                    InternalQuickSortManaged(source, 0, source.Length - 1, keySekecter, keyComparer);
                    break;
            }
        }

        #endregion

        #region InternalQuickSortManaged

        ///<summary>
        ///A quicksort method that allows duplicate keys.
        ///</summary>
        /// <remarks>
        /// See also <seealso href="https://kankinkon.hatenadiary.org/entry/20120202/1328133196">kanmo's blog</seealso>. 
        /// </remarks>
        private static void InternalQuickSortManaged<ELEMENT_T>(ELEMENT_T[] source, Int32 startIndex, Int32 endIndex)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
#if DEBUG
#if false
            System.Diagnostics.Debug.WriteLine($"Enter QuickSort({startIndex}, {endIndex}) {endIndex - startIndex + 1} bytes, ");
            System.Diagnostics.Debug.Indent();
#endif

            try
            {
#endif
                if (endIndex <= startIndex)
                    return;
                if (endIndex - startIndex == 1)
                {
                    if (source[startIndex].CompareTo(source[endIndex]) > 0)
                        (source[startIndex], source[endIndex]) = (source[endIndex], source[startIndex]);
                    return;
                }

                // もしキー値が重複していないと仮定すれば、 3 点のキー値の中間値を pivotKey として採用することによりよりよい分割が望めるが、
                // この QuickSort メソッドでは重複キーを許容するので、source[startIndex] のキー値を pivotKey とする。
#if true
                // 配列の最初の要素のキー値が pivotKey なので、後述の配列レイアウトに従って、lowerBoundary および endOfPivotKeys を +1 しておく。
                var pivotKey = source[startIndex];
                var lowerBoundary = startIndex + 1;
                var upperBoundary = endIndex;
                var endOfPivotKeys = startIndex + 1;
#else
                var pivotKey = SelectPivotKey(keySelector(source[startIndex]), keySelector(source[endIndex]), keySelector(source[(startIndex + endIndex) / 2]), keyComparer);
                var lowerBoundary = startIndex;
                var upperBoundary = endIndex;
                var endOfPivotKeys = startIndex;
#endif

                // この時点での配列のレイアウトは以下の通り
                // region-w を如何に縮小するかがこのループの目的である
                //
                // region-a) [startIndex, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 1)
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                // region-w) [lowerBoundary, upperBoundary] : pivotKey との大小関係が不明なキー値を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                while (lowerBoundary <= upperBoundary)
                {
                    // source[lowerBoundary] に pivotKey より大きいキーが見つかるまで lowerBoundary を増やし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = source[lowerBoundary].CompareTo(pivotKey);
                        if (c > 0)
                        {
                            // source[lowerBoundary] > pivotKey である場合
#if DEBUG
                            Assert(source[lowerBoundary].CompareTo(pivotKey) > 0);
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif
                            // pivotKey より大きいキー値を持つ要素が見つかったので、ループを終える
                            break;
                        }

                        // source[lowerBoundary] <= pivotKey である場合
#if DEBUG
                        Assert(source[lowerBoundary].CompareTo(pivotKey) <= 0);
#endif
                        if (c == 0)
                        {
                            // source[lowerBoundary] == pivotKey である場合
#if DEBUG
                            Assert(source[lowerBoundary].CompareTo(pivotKey) == 0);
#endif
                            // region-a に lowerBoundary にある要素を追加する
                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // region-b は空ではない、つまり source[endOfPivotKeys] < pivotKey であるはずなので、source[lowerBoundary] と要素を交換する。
                                (source[endOfPivotKeys], source[lowerBoundary]) = (source[lowerBoundary], source[endOfPivotKeys]);
                            }
                            else
                            {
                                // region-b が空である場合

                                // endOfPivotKeys == lowerBoundary であるはずなので、要素の交換は不要。
#if DEBUG
                                Assert(endOfPivotKeys == lowerBoundary);
#endif
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;
                        }

                        // region-b の終端位置をインクリメントする
                        ++lowerBoundary;
#if DEBUG
                        AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif
                    }

#if DEBUG
                    AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif

                    // この時点で lowerBoundary > upperBoundary || source[lowerBoundary] > pivotKey && source[endOfPivotKeys] != pivotKey
                    Assert(lowerBoundary > upperBoundary || source[lowerBoundary].CompareTo(pivotKey) > 0 && source[endOfPivotKeys].CompareTo(pivotKey) != 0);

                    // source[upperBoundary] に pivotKey より小さいまたは等しいキー値を持つ要素が見つかるまで upperBoundary を減らし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = source[upperBoundary].CompareTo(pivotKey);
                        if (c == 0)
                        {
                            // source[upperBoundary] == pivotKey である場合

                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // 以下の 3 つの事実が判明しているので、3 つの要素をそれぞれ入れ替える。
                                // 1) source[upperBoundary] == pivotKey
                                // 2) source[lowerBoundary] > pivotKey (前の while ループの結果より)
                                // 3) source[endOfPivotKeys] < pivotKey (regon-b が空ではないことより)
#if DEBUG
                                Assert(source[upperBoundary].CompareTo(pivotKey) == 0 && source[lowerBoundary].CompareTo(pivotKey) > 0 && source[endOfPivotKeys].CompareTo(pivotKey) < 0);
#endif
                                var t = source[endOfPivotKeys];
                                source[endOfPivotKeys] = source[upperBoundary];
                                source[upperBoundary] = source[lowerBoundary];
                                source[lowerBoundary] = t;
                            }
                            else
                            {
                                // region-b が空である場合

                                // 以下の 3 つの事実が判明しているので、2 つの要素を入れ替える。
                                // 1) source[upperBoundary] == pivotKey
                                // 2) source[lowerBoundary] > pivotKey (前の while ループの結果より)
                                // 3) endOfPivotKeys == lowerBoundary (regon-b が空ではあることより)
#if DEBUG
                                Assert(source[upperBoundary].CompareTo(pivotKey) == 0 && source[lowerBoundary].CompareTo(pivotKey) > 0 && endOfPivotKeys == lowerBoundary);
#endif
                                (source[endOfPivotKeys], source[upperBoundary]) = (source[upperBoundary], source[endOfPivotKeys]);
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;

                            // region -b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
                            Assert(endOfPivotKeys <= lowerBoundary);
#endif
                            // pivotKey と等しいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else if (c < 0)
                        {
                            // source[upperBoundary] < pivotKey である場合

                            // 前の while ループの結果より、region-b の末尾の要素のキー値が pivotKey より小さい (source[lowerBoundary] > pivotKey) ことが判明しているので、
                            // region-b の終端と要素を入れ替える
                            (source[upperBoundary], source[lowerBoundary]) = (source[lowerBoundary], source[upperBoundary]);

                            // region-b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            Assert(endOfPivotKeys <= lowerBoundary);
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif
                            // pivotKey より小さいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else
                        {
                            // source[upperBoundary] > pivotKey である場合

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif
                        }
                    }
#if DEBUG
                    AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif
                }

                // この時点で region-w のサイズは 0 であり、lowerBoundary == upperBoundary + 1 のはずである。
#if DEBUG
                Assert(lowerBoundary == upperBoundary + 1);
#endif

                // この時点での配列のレイアウトは以下の通り。
                //
                // region-a) [startIndex, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif

                // 配列を [region-b] [region-a] [region-c] の順に並び替えるために、region-b の終端の一部または全部を region-a と入れ替える。

                // 入れ替える長さを求める (region-a の長さと region-b の長さの最小値)
                var lengthToExchange = (endOfPivotKeys - startIndex).Minimum(lowerBoundary - endOfPivotKeys);

                // 入れ替える片方の開始位置 (region-a の先端位置)
                var exStartIndex = startIndex;

                // 入れ替えるもう片方の開始位置 (region-b の終端位置)
                var exEndIndex = upperBoundary;

                // 入れ替える値がなくなるまで繰り返す
                while (exStartIndex < exEndIndex)
                {
                    // 値を入れ替える
                    (source[exStartIndex], source[exEndIndex]) = (source[exEndIndex], source[exStartIndex]);

                    // 入れ替える値の位置を変更する
                    ++exStartIndex;
                    --exEndIndex;
                }

                // この時点で、配列の並びは以下の通り
                // region-b) [startIndex, startIndex + upperBoundary - endOfPivotKeys] : x < pivotKey であるキー値 x を持つ要素の集合
                // region-a) [startIndex + lowerBoundary - endOfPivotKeys, upperBoundary] : x == pivotKey であるキー値 x を持つ要素の集合
                // region-c) [lowerBoundary, endIndex]: x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                for (var index = startIndex; index <= startIndex + upperBoundary - endOfPivotKeys; ++index)
                    Assert(source[index].CompareTo(pivotKey) < 0);
                for (var index = startIndex + lowerBoundary - endOfPivotKeys; index <= upperBoundary; ++index)
                    Assert(source[index].CompareTo(pivotKey) == 0);
                for (var index = lowerBoundary; index <= endIndex; ++index)
                    Assert(source[index].CompareTo(pivotKey) > 0);
#endif

                // region-b の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortManaged(source, startIndex, upperBoundary - endOfPivotKeys + startIndex);

                // region-c の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortManaged(source, lowerBoundary, endIndex);
#if DEBUG
            }
            finally
            {
                AssertSortResult<ELEMENT_T>(source, startIndex, endIndex);
#if false
                System.Diagnostics.Debug.Unindent();
                System.Diagnostics.Debug.WriteLine($"Leave QuickSort({startIndex}, {endIndex}) {endIndex - startIndex + 1} bytes");
#endif
            }
#endif
        }

        ///<summary>
        ///A quicksort method that allows duplicate keys.
        ///</summary>
        /// <remarks>
        /// See also <seealso href="https://kankinkon.hatenadiary.org/entry/20120202/1328133196">kanmo's blog</seealso>. 
        /// </remarks>
        private static void InternalQuickSortManaged<ELEMENT_T, KEY_T>(ELEMENT_T[] source, Int32 startIndex, Int32 endIndex, Func<ELEMENT_T, KEY_T> keySelector)
            where KEY_T : IComparable<KEY_T>
        {
#if DEBUG
#if false
            System.Diagnostics.Debug.WriteLine($"Enter QuickSort({startIndex}, {endIndex}) {endIndex - startIndex + 1} bytes, ");
            System.Diagnostics.Debug.Indent();
#endif

            try
            {
#endif
                if (endIndex <= startIndex)
                    return;
                if (endIndex - startIndex == 1)
                {
                    if (keySelector(source[startIndex]).CompareTo(keySelector(source[endIndex])) > 0)
                        (source[startIndex], source[endIndex]) = (source[endIndex], source[startIndex]);
                    return;
                }

                // もしキー値が重複していないと仮定すれば、 3 点のキー値の中間値を pivotKey として採用することによりよりよい分割が望めるが、
                // この QuickSort メソッドでは重複キーを許容するので、source[startIndex] のキー値を pivotKey とする。
#if true
                // 配列の最初の要素のキー値が pivotKey なので、後述の配列レイアウトに従って、lowerBoundary および endOfPivotKeys を +1 しておく。
                var pivotKey = keySelector(source[startIndex]);
                var lowerBoundary = startIndex + 1;
                var upperBoundary = endIndex;
                var endOfPivotKeys = startIndex + 1;
#else
                var pivotKey = SelectPivotKey(keySelector(source[startIndex]), keySelector(source[endIndex]), keySelector(source[(startIndex + endIndex) / 2]), keyComparer);
                var lowerBoundary = startIndex;
                var upperBoundary = endIndex;
                var endOfPivotKeys = startIndex;
#endif

                // この時点での配列のレイアウトは以下の通り
                // region-w を如何に縮小するかがこのループの目的である
                //
                // region-a) [startIndex, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 1)
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                // region-w) [lowerBoundary, upperBoundary] : pivotKey との大小関係が不明なキー値を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                while (lowerBoundary <= upperBoundary)
                {
                    // source[lowerBoundary] に pivotKey より大きいキーが見つかるまで lowerBoundary を増やし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = keySelector(source[lowerBoundary]).CompareTo(pivotKey);
                        if (c > 0)
                        {
                            // source[lowerBoundary] > pivotKey である場合
#if DEBUG
                            Assert(keySelector(source[lowerBoundary]).CompareTo(pivotKey) > 0);
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif
                            // pivotKey より大きいキー値を持つ要素が見つかったので、ループを終える
                            break;
                        }

                        // source[lowerBoundary] <= pivotKey である場合
#if DEBUG
                        Assert(keySelector(source[lowerBoundary]).CompareTo(pivotKey) <= 0);
#endif
                        if (c == 0)
                        {
                            // source[lowerBoundary] == pivotKey である場合
#if DEBUG
                            Assert(keySelector(source[lowerBoundary]).CompareTo(pivotKey) == 0);
#endif
                            // region-a に lowerBoundary にある要素を追加する
                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // region-b は空ではない、つまり source[endOfPivotKeys] < pivotKey であるはずなので、source[lowerBoundary] と要素を交換する。
                                (source[endOfPivotKeys], source[lowerBoundary]) = (source[lowerBoundary], source[endOfPivotKeys]);
                            }
                            else
                            {
                                // region-b が空である場合

                                // endOfPivotKeys == lowerBoundary であるはずなので、要素の交換は不要。
#if DEBUG
                                Assert(endOfPivotKeys == lowerBoundary);
#endif
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;
                        }

                        // region-b の終端位置をインクリメントする
                        ++lowerBoundary;
#if DEBUG
                        AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif
                    }

#if DEBUG
                    AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif

                    // この時点で lowerBoundary > upperBoundary || source[lowerBoundary] > pivotKey && source[endOfPivotKeys] != pivotKey
                    Assert(lowerBoundary > upperBoundary || keySelector(source[lowerBoundary]).CompareTo(pivotKey) > 0 && keySelector(source[endOfPivotKeys]).CompareTo(pivotKey) != 0);

                    // source[upperBoundary] に pivotKey より小さいまたは等しいキー値を持つ要素が見つかるまで upperBoundary を減らし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = keySelector(source[upperBoundary]).CompareTo(pivotKey);
                        if (c == 0)
                        {
                            // source[upperBoundary] == pivotKey である場合

                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // 以下の 3 つの事実が判明しているので、3 つの要素をそれぞれ入れ替える。
                                // 1) source[upperBoundary] == pivotKey
                                // 2) source[lowerBoundary] > pivotKey (前の while ループの結果より)
                                // 3) source[endOfPivotKeys] < pivotKey (regon-b が空ではないことより)
#if DEBUG
                                Assert(keySelector(source[upperBoundary]).CompareTo(pivotKey) == 0 && keySelector(source[lowerBoundary]).CompareTo(pivotKey) > 0 && keySelector(source[endOfPivotKeys]).CompareTo(pivotKey) < 0);
#endif
                                var t = source[endOfPivotKeys];
                                source[endOfPivotKeys] = source[upperBoundary];
                                source[upperBoundary] = source[lowerBoundary];
                                source[lowerBoundary] = t;
                            }
                            else
                            {
                                // region-b が空である場合

                                // 以下の 3 つの事実が判明しているので、2 つの要素を入れ替える。
                                // 1) source[upperBoundary] == pivotKey
                                // 2) source[lowerBoundary] > pivotKey (前の while ループの結果より)
                                // 3) endOfPivotKeys == lowerBoundary (regon-b が空ではあることより)
#if DEBUG
                                Assert(keySelector(source[upperBoundary]).CompareTo(pivotKey) == 0 && keySelector(source[lowerBoundary]).CompareTo(pivotKey) > 0 && endOfPivotKeys == lowerBoundary);
#endif
                                (source[endOfPivotKeys], source[upperBoundary]) = (source[upperBoundary], source[endOfPivotKeys]);
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;

                            // region -b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
                            Assert(endOfPivotKeys <= lowerBoundary);
#endif
                            // pivotKey と等しいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else if (c < 0)
                        {
                            // source[upperBoundary] < pivotKey である場合

                            // 前の while ループの結果より、region-b の末尾の要素のキー値が pivotKey より小さい (source[lowerBoundary] > pivotKey) ことが判明しているので、
                            // region-b の終端と要素を入れ替える
                            (source[upperBoundary], source[lowerBoundary]) = (source[lowerBoundary], source[upperBoundary]);

                            // region-b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            Assert(endOfPivotKeys <= lowerBoundary);
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif
                            // pivotKey より小さいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else
                        {
                            // source[upperBoundary] > pivotKey である場合

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif
                        }
                    }
#if DEBUG
                    AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif
                }

                // この時点で region-w のサイズは 0 であり、lowerBoundary == upperBoundary + 1 のはずである。
#if DEBUG
                Assert(lowerBoundary == upperBoundary + 1);
#endif

                // この時点での配列のレイアウトは以下の通り。
                //
                // region-a) [startIndex, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif

                // 配列を [region-b] [region-a] [region-c] の順に並び替えるために、region-b の終端の一部または全部を region-a と入れ替える。

                // 入れ替える長さを求める (region-a の長さと region-b の長さの最小値)
                var lengthToExchange = (endOfPivotKeys - startIndex).Minimum(lowerBoundary - endOfPivotKeys);

                // 入れ替える片方の開始位置 (region-a の先端位置)
                var exStartIndex = startIndex;

                // 入れ替えるもう片方の開始位置 (region-b の終端位置)
                var exEndIndex = upperBoundary;

                // 入れ替える値がなくなるまで繰り返す
                while (exStartIndex < exEndIndex)
                {
                    // 値を入れ替える
                    (source[exStartIndex], source[exEndIndex]) = (source[exEndIndex], source[exStartIndex]);

                    // 入れ替える値の位置を変更する
                    ++exStartIndex;
                    --exEndIndex;
                }

                // この時点で、配列の並びは以下の通り
                // region-b) [startIndex, startIndex + upperBoundary - endOfPivotKeys] : x < pivotKey であるキー値 x を持つ要素の集合
                // region-a) [startIndex + lowerBoundary - endOfPivotKeys, upperBoundary] : x == pivotKey であるキー値 x を持つ要素の集合
                // region-c) [lowerBoundary, endIndex]: x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                for (var index = startIndex; index <= startIndex + upperBoundary - endOfPivotKeys; ++index)
                    Assert(keySelector(source[index]).CompareTo(pivotKey) < 0);
                for (var index = startIndex + lowerBoundary - endOfPivotKeys; index <= upperBoundary; ++index)
                    Assert(keySelector(source[index]).CompareTo(pivotKey) == 0);
                for (var index = lowerBoundary; index <= endIndex; ++index)
                    Assert(keySelector(source[index]).CompareTo(pivotKey) > 0);
#endif

                // region-b の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortManaged(source, startIndex, upperBoundary - endOfPivotKeys + startIndex, keySelector);

                // region-c の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortManaged(source, lowerBoundary, endIndex, keySelector);
#if DEBUG
            }
            finally
            {
                AssertSortResult(source, startIndex, endIndex, keySelector);
#if false
                System.Diagnostics.Debug.Unindent();
                System.Diagnostics.Debug.WriteLine($"Leave QuickSort({startIndex}, {endIndex}) {endIndex - startIndex + 1} bytes");
#endif
            }
#endif
        }

        ///<summary>
        ///A quicksort method that allows duplicate keys.
        ///</summary>
        /// <remarks>
        /// See also <seealso href="https://kankinkon.hatenadiary.org/entry/20120202/1328133196">kanmo's blog</seealso>. 
        /// </remarks>
        private static void InternalQuickSortManaged<ELEMENT_T>(ELEMENT_T[] source, Int32 startIndex, Int32 endIndex, IComparer<ELEMENT_T> keyComparer)
        {
#if DEBUG
#if false
            System.Diagnostics.Debug.WriteLine($"Enter QuickSort({startIndex}, {endIndex}) {endIndex - startIndex + 1} bytes, ");
            System.Diagnostics.Debug.Indent();
#endif

            try
            {
#endif
                if (endIndex <= startIndex)
                    return;
                if (endIndex - startIndex == 1)
                {
                    if (keyComparer.Compare(source[startIndex], source[endIndex]) > 0)
                        (source[startIndex], source[endIndex]) = (source[endIndex], source[startIndex]);
                    return;
                }

                // もしキー値が重複していないと仮定すれば、 3 点のキー値の中間値を pivotKey として採用することによりよりよい分割が望めるが、
                // この QuickSort メソッドでは重複キーを許容するので、source[startIndex] のキー値を pivotKey とする。
#if true
                // 配列の最初の要素のキー値が pivotKey なので、後述の配列レイアウトに従って、lowerBoundary および endOfPivotKeys を +1 しておく。
                var pivotKey = source[startIndex];
                var lowerBoundary = startIndex + 1;
                var upperBoundary = endIndex;
                var endOfPivotKeys = startIndex + 1;
#else
                var pivotKey = SelectPivotKey(keySelector(source[startIndex]), keySelector(source[endIndex]), keySelector(source[(startIndex + endIndex) / 2]), keyComparer);
                var lowerBoundary = startIndex;
                var upperBoundary = endIndex;
                var endOfPivotKeys = startIndex;
#endif

                // この時点での配列のレイアウトは以下の通り
                // region-w を如何に縮小するかがこのループの目的である
                //
                // region-a) [startIndex, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 1)
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                // region-w) [lowerBoundary, upperBoundary] : pivotKey との大小関係が不明なキー値を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                while (lowerBoundary <= upperBoundary)
                {
                    // source[lowerBoundary] に pivotKey より大きいキーが見つかるまで lowerBoundary を増やし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = keyComparer.Compare(source[lowerBoundary], pivotKey);
                        if (c > 0)
                        {
                            // source[lowerBoundary] > pivotKey である場合
#if DEBUG
                            Assert(keyComparer.Compare(source[lowerBoundary], pivotKey) > 0);
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keyComparer);
#endif
                            // pivotKey より大きいキー値を持つ要素が見つかったので、ループを終える
                            break;
                        }

                        // source[lowerBoundary] <= pivotKey である場合
#if DEBUG
                        Assert(keyComparer.Compare(source[lowerBoundary], pivotKey) <= 0);
#endif
                        if (c == 0)
                        {
                            // source[lowerBoundary] == pivotKey である場合
#if DEBUG
                            Assert(keyComparer.Compare(source[lowerBoundary], pivotKey) == 0);
#endif
                            // region-a に lowerBoundary にある要素を追加する
                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // region-b は空ではない、つまり source[endOfPivotKeys] < pivotKey であるはずなので、source[lowerBoundary] と要素を交換する。
                                (source[endOfPivotKeys], source[lowerBoundary]) = (source[lowerBoundary], source[endOfPivotKeys]);
                            }
                            else
                            {
                                // region-b が空である場合

                                // endOfPivotKeys == lowerBoundary であるはずなので、要素の交換は不要。
#if DEBUG
                                Assert(endOfPivotKeys == lowerBoundary);
#endif
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;
                        }

                        // region-b の終端位置をインクリメントする
                        ++lowerBoundary;
#if DEBUG
                        AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keyComparer);
#endif
                    }

#if DEBUG
                    AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keyComparer);
#endif

                    // この時点で lowerBoundary > upperBoundary || source[lowerBoundary] > pivotKey && source[endOfPivotKeys] != pivotKey
                    Assert(lowerBoundary > upperBoundary || keyComparer.Compare(source[lowerBoundary], pivotKey) > 0 && keyComparer.Compare(source[endOfPivotKeys], pivotKey) != 0);

                    // source[upperBoundary] に pivotKey より小さいまたは等しいキー値を持つ要素が見つかるまで upperBoundary を減らし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = keyComparer.Compare(source[upperBoundary], pivotKey);
                        if (c == 0)
                        {
                            // source[upperBoundary] == pivotKey である場合

                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // 以下の 3 つの事実が判明しているので、3 つの要素をそれぞれ入れ替える。
                                // 1) source[upperBoundary] == pivotKey
                                // 2) source[lowerBoundary] > pivotKey (前の while ループの結果より)
                                // 3) source[endOfPivotKeys] < pivotKey (regon-b が空ではないことより)
#if DEBUG
                                Assert(keyComparer.Compare(source[upperBoundary], pivotKey) == 0 && keyComparer.Compare(source[lowerBoundary], pivotKey) > 0 && keyComparer.Compare(source[endOfPivotKeys], pivotKey) < 0);
#endif
                                var t = source[endOfPivotKeys];
                                source[endOfPivotKeys] = source[upperBoundary];
                                source[upperBoundary] = source[lowerBoundary];
                                source[lowerBoundary] = t;
                            }
                            else
                            {
                                // region-b が空である場合

                                // 以下の 3 つの事実が判明しているので、2 つの要素を入れ替える。
                                // 1) source[upperBoundary] == pivotKey
                                // 2) source[lowerBoundary] > pivotKey (前の while ループの結果より)
                                // 3) endOfPivotKeys == lowerBoundary (regon-b が空ではあることより)
#if DEBUG
                                Assert(keyComparer.Compare(source[upperBoundary], pivotKey) == 0 && keyComparer.Compare(source[lowerBoundary], pivotKey) > 0 && endOfPivotKeys == lowerBoundary);
#endif
                                (source[endOfPivotKeys], source[upperBoundary]) = (source[upperBoundary], source[endOfPivotKeys]);
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;

                            // region -b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keyComparer);
                            Assert(endOfPivotKeys <= lowerBoundary);
#endif
                            // pivotKey と等しいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else if (c < 0)
                        {
                            // source[upperBoundary] < pivotKey である場合

                            // 前の while ループの結果より、region-b の末尾の要素のキー値が pivotKey より小さい (source[lowerBoundary] > pivotKey) ことが判明しているので、
                            // region-b の終端と要素を入れ替える
                            (source[upperBoundary], source[lowerBoundary]) = (source[lowerBoundary], source[upperBoundary]);

                            // region-b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            Assert(endOfPivotKeys <= lowerBoundary);
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keyComparer);
#endif
                            // pivotKey より小さいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else
                        {
                            // source[upperBoundary] > pivotKey である場合

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keyComparer);
#endif
                        }
                    }
#if DEBUG
                    AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keyComparer);
#endif
                }

                // この時点で region-w のサイズは 0 であり、lowerBoundary == upperBoundary + 1 のはずである。
#if DEBUG
                Assert(lowerBoundary == upperBoundary + 1);
#endif

                // この時点での配列のレイアウトは以下の通り。
                //
                // region-a) [startIndex, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keyComparer);
#endif

                // 配列を [region-b] [region-a] [region-c] の順に並び替えるために、region-b の終端の一部または全部を region-a と入れ替える。

                // 入れ替える長さを求める (region-a の長さと region-b の長さの最小値)
                var lengthToExchange = (endOfPivotKeys - startIndex).Minimum(lowerBoundary - endOfPivotKeys);

                // 入れ替える片方の開始位置 (region-a の先端位置)
                var exStartIndex = startIndex;

                // 入れ替えるもう片方の開始位置 (region-b の終端位置)
                var exEndIndex = upperBoundary;

                // 入れ替える値がなくなるまで繰り返す
                while (exStartIndex < exEndIndex)
                {
                    // 値を入れ替える
                    (source[exStartIndex], source[exEndIndex]) = (source[exEndIndex], source[exStartIndex]);

                    // 入れ替える値の位置を変更する
                    ++exStartIndex;
                    --exEndIndex;
                }

                // この時点で、配列の並びは以下の通り
                // region-b) [startIndex, startIndex + upperBoundary - endOfPivotKeys] : x < pivotKey であるキー値 x を持つ要素の集合
                // region-a) [startIndex + lowerBoundary - endOfPivotKeys, upperBoundary] : x == pivotKey であるキー値 x を持つ要素の集合
                // region-c) [lowerBoundary, endIndex]: x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                for (var index = startIndex; index <= startIndex + upperBoundary - endOfPivotKeys; ++index)
                    Assert(keyComparer.Compare(source[index], pivotKey) < 0);
                for (var index = startIndex + lowerBoundary - endOfPivotKeys; index <= upperBoundary; ++index)
                    Assert(keyComparer.Compare(source[index], pivotKey) == 0);
                for (var index = lowerBoundary; index <= endIndex; ++index)
                    Assert(keyComparer.Compare(source[index], pivotKey) > 0);
#endif

                // region-b の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortManaged(source, startIndex, upperBoundary - endOfPivotKeys + startIndex, keyComparer);

                // region-c の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortManaged(source, lowerBoundary, endIndex, keyComparer);
#if DEBUG
            }
            finally
            {
                AssertSortResult(source, startIndex, endIndex, keyComparer);
#if false
                System.Diagnostics.Debug.Unindent();
                System.Diagnostics.Debug.WriteLine($"Leave QuickSort({startIndex}, {endIndex}) {endIndex - startIndex + 1} bytes");
#endif
            }
#endif
        }

        ///<summary>
        ///A quicksort method that allows duplicate keys.
        ///</summary>
        /// <remarks>
        /// See also <seealso href="https://kankinkon.hatenadiary.org/entry/20120202/1328133196">kanmo's blog</seealso>. 
        /// </remarks>
        private static void InternalQuickSortManaged<ELEMENT_T, KEY_T>(ELEMENT_T[] source, Int32 startIndex, Int32 endIndex, Func<ELEMENT_T, KEY_T> keySelector, IComparer<KEY_T> keyComparer)

        {
#if DEBUG
#if false
            System.Diagnostics.Debug.WriteLine($"Enter QuickSort({startIndex}, {endIndex}) {endIndex - startIndex + 1} bytes, ");
            System.Diagnostics.Debug.Indent();
#endif

            try
            {
#endif
                if (endIndex <= startIndex)
                    return;
                if (endIndex - startIndex == 1)
                {
                    if (keyComparer.Compare(keySelector(source[startIndex]), keySelector(source[endIndex])) > 0)
                        (source[startIndex], source[endIndex]) = (source[endIndex], source[startIndex]);
                    return;
                }

                // もしキー値が重複していないと仮定すれば、 3 点のキー値の中間値を pivotKey として採用することによりよりよい分割が望めるが、
                // この QuickSort メソッドでは重複キーを許容するので、source[startIndex] のキー値を pivotKey とする。
#if true
                // 配列の最初の要素のキー値が pivotKey なので、後述の配列レイアウトに従って、lowerBoundary および endOfPivotKeys を +1 しておく。
                var pivotKey = keySelector(source[startIndex]);
                var lowerBoundary = startIndex + 1;
                var upperBoundary = endIndex;
                var endOfPivotKeys = startIndex + 1;
#else
                var pivotKey = SelectPivotKey(keySelector(source[startIndex]), keySelector(source[endIndex]), keySelector(source[(startIndex + endIndex) / 2]), keyComparer);
                var lowerBoundary = startIndex;
                var upperBoundary = endIndex;
                var endOfPivotKeys = startIndex;
#endif

                // この時点での配列のレイアウトは以下の通り
                // region-w を如何に縮小するかがこのループの目的である
                //
                // region-a) [startIndex, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 1)
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                // region-w) [lowerBoundary, upperBoundary] : pivotKey との大小関係が不明なキー値を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                while (lowerBoundary <= upperBoundary)
                {
                    // source[lowerBoundary] に pivotKey より大きいキーが見つかるまで lowerBoundary を増やし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = keyComparer.Compare(keySelector(source[lowerBoundary]), pivotKey);
                        if (c > 0)
                        {
                            // source[lowerBoundary] > pivotKey である場合
#if DEBUG
                            Assert(keyComparer.Compare(keySelector(source[lowerBoundary]), pivotKey) > 0);
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif
                            // pivotKey より大きいキー値を持つ要素が見つかったので、ループを終える
                            break;
                        }

                        // source[lowerBoundary] <= pivotKey である場合
#if DEBUG
                        Assert(keyComparer.Compare(keySelector(source[lowerBoundary]), pivotKey) <= 0);
#endif
                        if (c == 0)
                        {
                            // source[lowerBoundary] == pivotKey である場合
#if DEBUG
                            Assert(keyComparer.Compare(keySelector(source[lowerBoundary]), pivotKey) == 0);
#endif
                            // region-a に lowerBoundary にある要素を追加する
                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // region-b は空ではない、つまり source[endOfPivotKeys] < pivotKey であるはずなので、source[lowerBoundary] と要素を交換する。
                                (source[endOfPivotKeys], source[lowerBoundary]) = (source[lowerBoundary], source[endOfPivotKeys]);
                            }
                            else
                            {
                                // region-b が空である場合

                                // endOfPivotKeys == lowerBoundary であるはずなので、要素の交換は不要。
#if DEBUG
                                Assert(endOfPivotKeys == lowerBoundary);
#endif
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;
                        }

                        // region-b の終端位置をインクリメントする
                        ++lowerBoundary;
#if DEBUG
                        AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif
                    }

#if DEBUG
                    AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif

                    // この時点で lowerBoundary > upperBoundary || source[lowerBoundary] > pivotKey && source[endOfPivotKeys] != pivotKey
                    Assert(lowerBoundary > upperBoundary || keyComparer.Compare(keySelector(source[lowerBoundary]), pivotKey) > 0 && keyComparer.Compare(keySelector(source[endOfPivotKeys]), pivotKey) != 0);

                    // source[upperBoundary] に pivotKey より小さいまたは等しいキー値を持つ要素が見つかるまで upperBoundary を減らし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = keyComparer.Compare(keySelector(source[upperBoundary]), pivotKey);
                        if (c == 0)
                        {
                            // source[upperBoundary] == pivotKey である場合

                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // 以下の 3 つの事実が判明しているので、3 つの要素をそれぞれ入れ替える。
                                // 1) source[upperBoundary] == pivotKey
                                // 2) source[lowerBoundary] > pivotKey (前の while ループの結果より)
                                // 3) source[endOfPivotKeys] < pivotKey (regon-b が空ではないことより)
#if DEBUG
                                Assert(keyComparer.Compare(keySelector(source[upperBoundary]), pivotKey) == 0 && keyComparer.Compare(keySelector(source[lowerBoundary]), pivotKey) > 0 && keyComparer.Compare(keySelector(source[endOfPivotKeys]), pivotKey) < 0);
#endif
                                var t = source[endOfPivotKeys];
                                source[endOfPivotKeys] = source[upperBoundary];
                                source[upperBoundary] = source[lowerBoundary];
                                source[lowerBoundary] = t;
                            }
                            else
                            {
                                // region-b が空である場合

                                // 以下の 3 つの事実が判明しているので、2 つの要素を入れ替える。
                                // 1) source[upperBoundary] == pivotKey
                                // 2) source[lowerBoundary] > pivotKey (前の while ループの結果より)
                                // 3) endOfPivotKeys == lowerBoundary (regon-b が空ではあることより)
#if DEBUG
                                Assert(keyComparer.Compare(keySelector(source[upperBoundary]), pivotKey) == 0 && keyComparer.Compare(keySelector(source[lowerBoundary]), pivotKey) > 0 && endOfPivotKeys == lowerBoundary);
#endif
                                (source[endOfPivotKeys], source[upperBoundary]) = (source[upperBoundary], source[endOfPivotKeys]);
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;

                            // region -b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
                            Assert(endOfPivotKeys <= lowerBoundary);
#endif
                            // pivotKey と等しいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else if (c < 0)
                        {
                            // source[upperBoundary] < pivotKey である場合

                            // 前の while ループの結果より、region-b の末尾の要素のキー値が pivotKey より小さい (source[lowerBoundary] > pivotKey) ことが判明しているので、
                            // region-b の終端と要素を入れ替える
                            (source[upperBoundary], source[lowerBoundary]) = (source[lowerBoundary], source[upperBoundary]);

                            // region-b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            Assert(endOfPivotKeys <= lowerBoundary);
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif
                            // pivotKey より小さいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else
                        {
                            // source[upperBoundary] > pivotKey である場合

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif
                        }
                    }
#if DEBUG
                    AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif
                }

                // この時点で region-w のサイズは 0 であり、lowerBoundary == upperBoundary + 1 のはずである。
#if DEBUG
                Assert(lowerBoundary == upperBoundary + 1);
#endif

                // この時点での配列のレイアウトは以下の通り。
                //
                // region-a) [startIndex, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif

                // 配列を [region-b] [region-a] [region-c] の順に並び替えるために、region-b の終端の一部または全部を region-a と入れ替える。

                // 入れ替える長さを求める (region-a の長さと region-b の長さの最小値)
                var lengthToExchange = (endOfPivotKeys - startIndex).Minimum(lowerBoundary - endOfPivotKeys);

                // 入れ替える片方の開始位置 (region-a の先端位置)
                var exStartIndex = startIndex;

                // 入れ替えるもう片方の開始位置 (region-b の終端位置)
                var exEndIndex = upperBoundary;

                // 入れ替える値がなくなるまで繰り返す
                while (exStartIndex < exEndIndex)
                {
                    // 値を入れ替える
                    (source[exStartIndex], source[exEndIndex]) = (source[exEndIndex], source[exStartIndex]);

                    // 入れ替える値の位置を変更する
                    ++exStartIndex;
                    --exEndIndex;
                }

                // この時点で、配列の並びは以下の通り
                // region-b) [startIndex, startIndex + upperBoundary - endOfPivotKeys] : x < pivotKey であるキー値 x を持つ要素の集合
                // region-a) [startIndex + lowerBoundary - endOfPivotKeys, upperBoundary] : x == pivotKey であるキー値 x を持つ要素の集合
                // region-c) [lowerBoundary, endIndex]: x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                for (var index = startIndex; index <= startIndex + upperBoundary - endOfPivotKeys; ++index)
                    Assert(keyComparer.Compare(keySelector(source[index]), pivotKey) < 0);
                for (var index = startIndex + lowerBoundary - endOfPivotKeys; index <= upperBoundary; ++index)
                    Assert(keyComparer.Compare(keySelector(source[index]), pivotKey) == 0);
                for (var index = lowerBoundary; index <= endIndex; ++index)
                    Assert(keyComparer.Compare(keySelector(source[index]), pivotKey) > 0);
#endif

                // region-b の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortManaged(source, startIndex, upperBoundary - endOfPivotKeys + startIndex, keySelector, keyComparer);

                // region-c の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortManaged(source, lowerBoundary, endIndex, keySelector, keyComparer);
#if DEBUG
            }
            finally
            {
                AssertSortResult(source, startIndex, endIndex, keySelector, keyComparer);
#if false
                System.Diagnostics.Debug.Unindent();
                System.Diagnostics.Debug.WriteLine($"Leave QuickSort({startIndex}, {endIndex}) {endIndex - startIndex + 1} bytes");
#endif
            }
#endif
        }

        ///<summary>
        ///A quicksort method that allows duplicate keys.
        ///</summary>
        /// <remarks>
        /// See also <seealso href="https://kankinkon.hatenadiary.org/entry/20120202/1328133196">kanmo's blog</seealso>. 
        /// </remarks>
        private static void InternalQuickSortManaged<ELEMENT_T>(Span<ELEMENT_T> source, Int32 startIndex, Int32 endIndex)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
#if DEBUG
#if false
            System.Diagnostics.Debug.WriteLine($"Enter QuickSort({startIndex}, {endIndex}) {endIndex - startIndex + 1} bytes, ");
            System.Diagnostics.Debug.Indent();
#endif

            try
            {
#endif
                if (endIndex <= startIndex)
                    return;
                if (endIndex - startIndex == 1)
                {
                    if (source[startIndex].CompareTo(source[endIndex]) > 0)
                        (source[startIndex], source[endIndex]) = (source[endIndex], source[startIndex]);
                    return;
                }

                // もしキー値が重複していないと仮定すれば、 3 点のキー値の中間値を pivotKey として採用することによりよりよい分割が望めるが、
                // この QuickSort メソッドでは重複キーを許容するので、source[startIndex] のキー値を pivotKey とする。
#if true
                // 配列の最初の要素のキー値が pivotKey なので、後述の配列レイアウトに従って、lowerBoundary および endOfPivotKeys を +1 しておく。
                var pivotKey = source[startIndex];
                var lowerBoundary = startIndex + 1;
                var upperBoundary = endIndex;
                var endOfPivotKeys = startIndex + 1;
#else
                var pivotKey = SelectPivotKey(keySelector(source[startIndex]), keySelector(source[endIndex]), keySelector(source[(startIndex + endIndex) / 2]), keyComparer);
                var lowerBoundary = startIndex;
                var upperBoundary = endIndex;
                var endOfPivotKeys = startIndex;
#endif

                // この時点での配列のレイアウトは以下の通り
                // region-w を如何に縮小するかがこのループの目的である
                //
                // region-a) [startIndex, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 1)
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                // region-w) [lowerBoundary, upperBoundary] : pivotKey との大小関係が不明なキー値を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                while (lowerBoundary <= upperBoundary)
                {
                    // source[lowerBoundary] に pivotKey より大きいキーが見つかるまで lowerBoundary を増やし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = source[lowerBoundary].CompareTo(pivotKey);
                        if (c > 0)
                        {
                            // source[lowerBoundary] > pivotKey である場合
#if DEBUG
                            Assert(source[lowerBoundary].CompareTo(pivotKey) > 0);
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif
                            // pivotKey より大きいキー値を持つ要素が見つかったので、ループを終える
                            break;
                        }

                        // source[lowerBoundary] <= pivotKey である場合
#if DEBUG
                        Assert(source[lowerBoundary].CompareTo(pivotKey) <= 0);
#endif
                        if (c == 0)
                        {
                            // source[lowerBoundary] == pivotKey である場合
#if DEBUG
                            Assert(source[lowerBoundary].CompareTo(pivotKey) == 0);
#endif
                            // region-a に lowerBoundary にある要素を追加する
                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // region-b は空ではない、つまり source[endOfPivotKeys] < pivotKey であるはずなので、source[lowerBoundary] と要素を交換する。
                                (source[endOfPivotKeys], source[lowerBoundary]) = (source[lowerBoundary], source[endOfPivotKeys]);
                            }
                            else
                            {
                                // region-b が空である場合

                                // endOfPivotKeys == lowerBoundary であるはずなので、要素の交換は不要。
#if DEBUG
                                Assert(endOfPivotKeys == lowerBoundary);
#endif
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;
                        }

                        // region-b の終端位置をインクリメントする
                        ++lowerBoundary;
#if DEBUG
                        AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif
                    }

#if DEBUG
                    AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif

                    // この時点で lowerBoundary > upperBoundary || source[lowerBoundary] > pivotKey && source[endOfPivotKeys] != pivotKey
                    Assert(lowerBoundary > upperBoundary || source[lowerBoundary].CompareTo(pivotKey) > 0 && source[endOfPivotKeys].CompareTo(pivotKey) != 0);

                    // source[upperBoundary] に pivotKey より小さいまたは等しいキー値を持つ要素が見つかるまで upperBoundary を減らし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = source[upperBoundary].CompareTo(pivotKey);
                        if (c == 0)
                        {
                            // source[upperBoundary] == pivotKey である場合

                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // 以下の 3 つの事実が判明しているので、3 つの要素をそれぞれ入れ替える。
                                // 1) source[upperBoundary] == pivotKey
                                // 2) source[lowerBoundary] > pivotKey (前の while ループの結果より)
                                // 3) source[endOfPivotKeys] < pivotKey (regon-b が空ではないことより)
#if DEBUG
                                Assert(source[upperBoundary].CompareTo(pivotKey) == 0 && source[lowerBoundary].CompareTo(pivotKey) > 0 && source[endOfPivotKeys].CompareTo(pivotKey) < 0);
#endif
                                var t = source[endOfPivotKeys];
                                source[endOfPivotKeys] = source[upperBoundary];
                                source[upperBoundary] = source[lowerBoundary];
                                source[lowerBoundary] = t;
                            }
                            else
                            {
                                // region-b が空である場合

                                // 以下の 3 つの事実が判明しているので、2 つの要素を入れ替える。
                                // 1) source[upperBoundary] == pivotKey
                                // 2) source[lowerBoundary] > pivotKey (前の while ループの結果より)
                                // 3) endOfPivotKeys == lowerBoundary (regon-b が空ではあることより)
#if DEBUG
                                Assert(source[upperBoundary].CompareTo(pivotKey) == 0 && source[lowerBoundary].CompareTo(pivotKey) > 0 && endOfPivotKeys == lowerBoundary);
#endif
                                (source[endOfPivotKeys], source[upperBoundary]) = (source[upperBoundary], source[endOfPivotKeys]);
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;

                            // region -b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
                            Assert(endOfPivotKeys <= lowerBoundary);
#endif
                            // pivotKey と等しいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else if (c < 0)
                        {
                            // source[upperBoundary] < pivotKey である場合

                            // 前の while ループの結果より、region-b の末尾の要素のキー値が pivotKey より小さい (source[lowerBoundary] > pivotKey) ことが判明しているので、
                            // region-b の終端と要素を入れ替える
                            (source[upperBoundary], source[lowerBoundary]) = (source[lowerBoundary], source[upperBoundary]);

                            // region-b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            Assert(endOfPivotKeys <= lowerBoundary);
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif
                            // pivotKey より小さいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else
                        {
                            // source[upperBoundary] > pivotKey である場合

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif
                        }
                    }
#if DEBUG
                    AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif
                }

                // この時点で region-w のサイズは 0 であり、lowerBoundary == upperBoundary + 1 のはずである。
#if DEBUG
                Assert(lowerBoundary == upperBoundary + 1);
#endif

                // この時点での配列のレイアウトは以下の通り。
                //
                // region-a) [startIndex, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif

                // 配列を [region-b] [region-a] [region-c] の順に並び替えるために、region-b の終端の一部または全部を region-a と入れ替える。

                // 入れ替える長さを求める (region-a の長さと region-b の長さの最小値)
                var lengthToExchange = (endOfPivotKeys - startIndex).Minimum(lowerBoundary - endOfPivotKeys);

                // 入れ替える片方の開始位置 (region-a の先端位置)
                var exStartIndex = startIndex;

                // 入れ替えるもう片方の開始位置 (region-b の終端位置)
                var exEndIndex = upperBoundary;

                // 入れ替える値がなくなるまで繰り返す
                while (exStartIndex < exEndIndex)
                {
                    // 値を入れ替える
                    (source[exStartIndex], source[exEndIndex]) = (source[exEndIndex], source[exStartIndex]);

                    // 入れ替える値の位置を変更する
                    ++exStartIndex;
                    --exEndIndex;
                }

                // この時点で、配列の並びは以下の通り
                // region-b) [startIndex, startIndex + upperBoundary - endOfPivotKeys] : x < pivotKey であるキー値 x を持つ要素の集合
                // region-a) [startIndex + lowerBoundary - endOfPivotKeys, upperBoundary] : x == pivotKey であるキー値 x を持つ要素の集合
                // region-c) [lowerBoundary, endIndex]: x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                for (var index = startIndex; index <= startIndex + upperBoundary - endOfPivotKeys; ++index)
                    Assert(source[index].CompareTo(pivotKey) < 0);
                for (var index = startIndex + lowerBoundary - endOfPivotKeys; index <= upperBoundary; ++index)
                    Assert(source[index].CompareTo(pivotKey) == 0);
                for (var index = lowerBoundary; index <= endIndex; ++index)
                    Assert(source[index].CompareTo(pivotKey) > 0);
#endif

                // region-b の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortManaged(source, startIndex, upperBoundary - endOfPivotKeys + startIndex);

                // region-c の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortManaged(source, lowerBoundary, endIndex);
#if DEBUG
            }
            finally
            {
                AssertSortResult<ELEMENT_T>(source, startIndex, endIndex);
#if false
                System.Diagnostics.Debug.Unindent();
                System.Diagnostics.Debug.WriteLine($"Leave QuickSort({startIndex}, {endIndex}) {endIndex - startIndex + 1} bytes");
#endif
            }
#endif
        }

        ///<summary>
        ///A quicksort method that allows duplicate keys.
        ///</summary>
        /// <remarks>
        /// See also <seealso href="https://kankinkon.hatenadiary.org/entry/20120202/1328133196">kanmo's blog</seealso>. 
        /// </remarks>
        private static void InternalQuickSortManaged<ELEMENT_T, KEY_T>(Span<ELEMENT_T> source, Int32 startIndex, Int32 endIndex, Func<ELEMENT_T, KEY_T> keySelector)
            where KEY_T : IComparable<KEY_T>
        {
#if DEBUG
#if false
            System.Diagnostics.Debug.WriteLine($"Enter QuickSort({startIndex}, {endIndex}) {endIndex - startIndex + 1} bytes, ");
            System.Diagnostics.Debug.Indent();
#endif

            try
            {
#endif
                if (endIndex <= startIndex)
                    return;
                if (endIndex - startIndex == 1)
                {
                    if (keySelector(source[startIndex]).CompareTo(keySelector(source[endIndex])) > 0)
                        (source[startIndex], source[endIndex]) = (source[endIndex], source[startIndex]);
                    return;
                }

                // もしキー値が重複していないと仮定すれば、 3 点のキー値の中間値を pivotKey として採用することによりよりよい分割が望めるが、
                // この QuickSort メソッドでは重複キーを許容するので、source[startIndex] のキー値を pivotKey とする。
#if true
                // 配列の最初の要素のキー値が pivotKey なので、後述の配列レイアウトに従って、lowerBoundary および endOfPivotKeys を +1 しておく。
                var pivotKey = keySelector(source[startIndex]);
                var lowerBoundary = startIndex + 1;
                var upperBoundary = endIndex;
                var endOfPivotKeys = startIndex + 1;
#else
                var pivotKey = SelectPivotKey(keySelector(source[startIndex]), keySelector(source[endIndex]), keySelector(source[(startIndex + endIndex) / 2]), keyComparer);
                var lowerBoundary = startIndex;
                var upperBoundary = endIndex;
                var endOfPivotKeys = startIndex;
#endif

                // この時点での配列のレイアウトは以下の通り
                // region-w を如何に縮小するかがこのループの目的である
                //
                // region-a) [startIndex, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 1)
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                // region-w) [lowerBoundary, upperBoundary] : pivotKey との大小関係が不明なキー値を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                while (lowerBoundary <= upperBoundary)
                {
                    // source[lowerBoundary] に pivotKey より大きいキーが見つかるまで lowerBoundary を増やし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = keySelector(source[lowerBoundary]).CompareTo(pivotKey);
                        if (c > 0)
                        {
                            // source[lowerBoundary] > pivotKey である場合
#if DEBUG
                            Assert(keySelector(source[lowerBoundary]).CompareTo(pivotKey) > 0);
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif
                            // pivotKey より大きいキー値を持つ要素が見つかったので、ループを終える
                            break;
                        }

                        // source[lowerBoundary] <= pivotKey である場合
#if DEBUG
                        Assert(keySelector(source[lowerBoundary]).CompareTo(pivotKey) <= 0);
#endif
                        if (c == 0)
                        {
                            // source[lowerBoundary] == pivotKey である場合
#if DEBUG
                            Assert(keySelector(source[lowerBoundary]).CompareTo(pivotKey) == 0);
#endif
                            // region-a に lowerBoundary にある要素を追加する
                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // region-b は空ではない、つまり source[endOfPivotKeys] < pivotKey であるはずなので、source[lowerBoundary] と要素を交換する。
                                (source[endOfPivotKeys], source[lowerBoundary]) = (source[lowerBoundary], source[endOfPivotKeys]);
                            }
                            else
                            {
                                // region-b が空である場合

                                // endOfPivotKeys == lowerBoundary であるはずなので、要素の交換は不要。
#if DEBUG
                                Assert(endOfPivotKeys == lowerBoundary);
#endif
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;
                        }

                        // region-b の終端位置をインクリメントする
                        ++lowerBoundary;
#if DEBUG
                        AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif
                    }

#if DEBUG
                    AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif

                    // この時点で lowerBoundary > upperBoundary || source[lowerBoundary] > pivotKey && source[endOfPivotKeys] != pivotKey
                    Assert(lowerBoundary > upperBoundary || keySelector(source[lowerBoundary]).CompareTo(pivotKey) > 0 && keySelector(source[endOfPivotKeys]).CompareTo(pivotKey) != 0);

                    // source[upperBoundary] に pivotKey より小さいまたは等しいキー値を持つ要素が見つかるまで upperBoundary を減らし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = keySelector(source[upperBoundary]).CompareTo(pivotKey);
                        if (c == 0)
                        {
                            // source[upperBoundary] == pivotKey である場合

                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // 以下の 3 つの事実が判明しているので、3 つの要素をそれぞれ入れ替える。
                                // 1) source[upperBoundary] == pivotKey
                                // 2) source[lowerBoundary] > pivotKey (前の while ループの結果より)
                                // 3) source[endOfPivotKeys] < pivotKey (regon-b が空ではないことより)
#if DEBUG
                                Assert(keySelector(source[upperBoundary]).CompareTo(pivotKey) == 0 && keySelector(source[lowerBoundary]).CompareTo(pivotKey) > 0 && keySelector(source[endOfPivotKeys]).CompareTo(pivotKey) < 0);
#endif
                                var t = source[endOfPivotKeys];
                                source[endOfPivotKeys] = source[upperBoundary];
                                source[upperBoundary] = source[lowerBoundary];
                                source[lowerBoundary] = t;
                            }
                            else
                            {
                                // region-b が空である場合

                                // 以下の 3 つの事実が判明しているので、2 つの要素を入れ替える。
                                // 1) source[upperBoundary] == pivotKey
                                // 2) source[lowerBoundary] > pivotKey (前の while ループの結果より)
                                // 3) endOfPivotKeys == lowerBoundary (regon-b が空ではあることより)
#if DEBUG
                                Assert(keySelector(source[upperBoundary]).CompareTo(pivotKey) == 0 && keySelector(source[lowerBoundary]).CompareTo(pivotKey) > 0 && endOfPivotKeys == lowerBoundary);
#endif
                                (source[endOfPivotKeys], source[upperBoundary]) = (source[upperBoundary], source[endOfPivotKeys]);
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;

                            // region -b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
                            Assert(endOfPivotKeys <= lowerBoundary);
#endif
                            // pivotKey と等しいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else if (c < 0)
                        {
                            // source[upperBoundary] < pivotKey である場合

                            // 前の while ループの結果より、region-b の末尾の要素のキー値が pivotKey より小さい (source[lowerBoundary] > pivotKey) ことが判明しているので、
                            // region-b の終端と要素を入れ替える
                            (source[upperBoundary], source[lowerBoundary]) = (source[lowerBoundary], source[upperBoundary]);

                            // region-b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            Assert(endOfPivotKeys <= lowerBoundary);
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif
                            // pivotKey より小さいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else
                        {
                            // source[upperBoundary] > pivotKey である場合

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif
                        }
                    }
#if DEBUG
                    AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif
                }

                // この時点で region-w のサイズは 0 であり、lowerBoundary == upperBoundary + 1 のはずである。
#if DEBUG
                Assert(lowerBoundary == upperBoundary + 1);
#endif

                // この時点での配列のレイアウトは以下の通り。
                //
                // region-a) [startIndex, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif

                // 配列を [region-b] [region-a] [region-c] の順に並び替えるために、region-b の終端の一部または全部を region-a と入れ替える。

                // 入れ替える長さを求める (region-a の長さと region-b の長さの最小値)
                var lengthToExchange = (endOfPivotKeys - startIndex).Minimum(lowerBoundary - endOfPivotKeys);

                // 入れ替える片方の開始位置 (region-a の先端位置)
                var exStartIndex = startIndex;

                // 入れ替えるもう片方の開始位置 (region-b の終端位置)
                var exEndIndex = upperBoundary;

                // 入れ替える値がなくなるまで繰り返す
                while (exStartIndex < exEndIndex)
                {
                    // 値を入れ替える
                    (source[exStartIndex], source[exEndIndex]) = (source[exEndIndex], source[exStartIndex]);

                    // 入れ替える値の位置を変更する
                    ++exStartIndex;
                    --exEndIndex;
                }

                // この時点で、配列の並びは以下の通り
                // region-b) [startIndex, startIndex + upperBoundary - endOfPivotKeys] : x < pivotKey であるキー値 x を持つ要素の集合
                // region-a) [startIndex + lowerBoundary - endOfPivotKeys, upperBoundary] : x == pivotKey であるキー値 x を持つ要素の集合
                // region-c) [lowerBoundary, endIndex]: x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                for (var index = startIndex; index <= startIndex + upperBoundary - endOfPivotKeys; ++index)
                    Assert(keySelector(source[index]).CompareTo(pivotKey) < 0);
                for (var index = startIndex + lowerBoundary - endOfPivotKeys; index <= upperBoundary; ++index)
                    Assert(keySelector(source[index]).CompareTo(pivotKey) == 0);
                for (var index = lowerBoundary; index <= endIndex; ++index)
                    Assert(keySelector(source[index]).CompareTo(pivotKey) > 0);
#endif

                // region-b の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortManaged(source, startIndex, upperBoundary - endOfPivotKeys + startIndex, keySelector);

                // region-c の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortManaged(source, lowerBoundary, endIndex, keySelector);
#if DEBUG
            }
            finally
            {
                AssertSortResult(source, startIndex, endIndex, keySelector);
#if false
                System.Diagnostics.Debug.Unindent();
                System.Diagnostics.Debug.WriteLine($"Leave QuickSort({startIndex}, {endIndex}) {endIndex - startIndex + 1} bytes");
#endif
            }
#endif
        }

        ///<summary>
        ///A quicksort method that allows duplicate keys.
        ///</summary>
        /// <remarks>
        /// See also <seealso href="https://kankinkon.hatenadiary.org/entry/20120202/1328133196">kanmo's blog</seealso>. 
        /// </remarks>
        private static void InternalQuickSortManaged<ELEMENT_T>(Span<ELEMENT_T> source, Int32 startIndex, Int32 endIndex, IComparer<ELEMENT_T> keyComparer)
        {
#if DEBUG
#if false
            System.Diagnostics.Debug.WriteLine($"Enter QuickSort({startIndex}, {endIndex}) {endIndex - startIndex + 1} bytes, ");
            System.Diagnostics.Debug.Indent();
#endif

            try
            {
#endif
                if (endIndex <= startIndex)
                    return;
                if (endIndex - startIndex == 1)
                {
                    if (keyComparer.Compare(source[startIndex], source[endIndex]) > 0)
                        (source[startIndex], source[endIndex]) = (source[endIndex], source[startIndex]);
                    return;
                }

                // もしキー値が重複していないと仮定すれば、 3 点のキー値の中間値を pivotKey として採用することによりよりよい分割が望めるが、
                // この QuickSort メソッドでは重複キーを許容するので、source[startIndex] のキー値を pivotKey とする。
#if true
                // 配列の最初の要素のキー値が pivotKey なので、後述の配列レイアウトに従って、lowerBoundary および endOfPivotKeys を +1 しておく。
                var pivotKey = source[startIndex];
                var lowerBoundary = startIndex + 1;
                var upperBoundary = endIndex;
                var endOfPivotKeys = startIndex + 1;
#else
                var pivotKey = SelectPivotKey(keySelector(source[startIndex]), keySelector(source[endIndex]), keySelector(source[(startIndex + endIndex) / 2]), keyComparer);
                var lowerBoundary = startIndex;
                var upperBoundary = endIndex;
                var endOfPivotKeys = startIndex;
#endif

                // この時点での配列のレイアウトは以下の通り
                // region-w を如何に縮小するかがこのループの目的である
                //
                // region-a) [startIndex, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 1)
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                // region-w) [lowerBoundary, upperBoundary] : pivotKey との大小関係が不明なキー値を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                while (lowerBoundary <= upperBoundary)
                {
                    // source[lowerBoundary] に pivotKey より大きいキーが見つかるまで lowerBoundary を増やし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = keyComparer.Compare(source[lowerBoundary], pivotKey);
                        if (c > 0)
                        {
                            // source[lowerBoundary] > pivotKey である場合
#if DEBUG
                            Assert(keyComparer.Compare(source[lowerBoundary], pivotKey) > 0);
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keyComparer);
#endif
                            // pivotKey より大きいキー値を持つ要素が見つかったので、ループを終える
                            break;
                        }

                        // source[lowerBoundary] <= pivotKey である場合
#if DEBUG
                        Assert(keyComparer.Compare(source[lowerBoundary], pivotKey) <= 0);
#endif
                        if (c == 0)
                        {
                            // source[lowerBoundary] == pivotKey である場合
#if DEBUG
                            Assert(keyComparer.Compare(source[lowerBoundary], pivotKey) == 0);
#endif
                            // region-a に lowerBoundary にある要素を追加する
                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // region-b は空ではない、つまり source[endOfPivotKeys] < pivotKey であるはずなので、source[lowerBoundary] と要素を交換する。
                                (source[endOfPivotKeys], source[lowerBoundary]) = (source[lowerBoundary], source[endOfPivotKeys]);
                            }
                            else
                            {
                                // region-b が空である場合

                                // endOfPivotKeys == lowerBoundary であるはずなので、要素の交換は不要。
#if DEBUG
                                Assert(endOfPivotKeys == lowerBoundary);
#endif
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;
                        }

                        // region-b の終端位置をインクリメントする
                        ++lowerBoundary;
#if DEBUG
                        AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keyComparer);
#endif
                    }

#if DEBUG
                    AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keyComparer);
#endif

                    // この時点で lowerBoundary > upperBoundary || source[lowerBoundary] > pivotKey && source[endOfPivotKeys] != pivotKey
                    Assert(lowerBoundary > upperBoundary || keyComparer.Compare(source[lowerBoundary], pivotKey) > 0 && keyComparer.Compare(source[endOfPivotKeys], pivotKey) != 0);

                    // source[upperBoundary] に pivotKey より小さいまたは等しいキー値を持つ要素が見つかるまで upperBoundary を減らし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = keyComparer.Compare(source[upperBoundary], pivotKey);
                        if (c == 0)
                        {
                            // source[upperBoundary] == pivotKey である場合

                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // 以下の 3 つの事実が判明しているので、3 つの要素をそれぞれ入れ替える。
                                // 1) source[upperBoundary] == pivotKey
                                // 2) source[lowerBoundary] > pivotKey (前の while ループの結果より)
                                // 3) source[endOfPivotKeys] < pivotKey (regon-b が空ではないことより)
#if DEBUG
                                Assert(keyComparer.Compare(source[upperBoundary], pivotKey) == 0 && keyComparer.Compare(source[lowerBoundary], pivotKey) > 0 && keyComparer.Compare(source[endOfPivotKeys], pivotKey) < 0);
#endif
                                var t = source[endOfPivotKeys];
                                source[endOfPivotKeys] = source[upperBoundary];
                                source[upperBoundary] = source[lowerBoundary];
                                source[lowerBoundary] = t;
                            }
                            else
                            {
                                // region-b が空である場合

                                // 以下の 3 つの事実が判明しているので、2 つの要素を入れ替える。
                                // 1) source[upperBoundary] == pivotKey
                                // 2) source[lowerBoundary] > pivotKey (前の while ループの結果より)
                                // 3) endOfPivotKeys == lowerBoundary (regon-b が空ではあることより)
#if DEBUG
                                Assert(keyComparer.Compare(source[upperBoundary], pivotKey) == 0 && keyComparer.Compare(source[lowerBoundary], pivotKey) > 0 && endOfPivotKeys == lowerBoundary);
#endif
                                (source[endOfPivotKeys], source[upperBoundary]) = (source[upperBoundary], source[endOfPivotKeys]);
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;

                            // region -b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keyComparer);
                            Assert(endOfPivotKeys <= lowerBoundary);
#endif
                            // pivotKey と等しいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else if (c < 0)
                        {
                            // source[upperBoundary] < pivotKey である場合

                            // 前の while ループの結果より、region-b の末尾の要素のキー値が pivotKey より小さい (source[lowerBoundary] > pivotKey) ことが判明しているので、
                            // region-b の終端と要素を入れ替える
                            (source[upperBoundary], source[lowerBoundary]) = (source[lowerBoundary], source[upperBoundary]);

                            // region-b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            Assert(endOfPivotKeys <= lowerBoundary);
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keyComparer);
#endif
                            // pivotKey より小さいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else
                        {
                            // source[upperBoundary] > pivotKey である場合

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keyComparer);
#endif
                        }
                    }
#if DEBUG
                    AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keyComparer);
#endif
                }

                // この時点で region-w のサイズは 0 であり、lowerBoundary == upperBoundary + 1 のはずである。
#if DEBUG
                Assert(lowerBoundary == upperBoundary + 1);
#endif

                // この時点での配列のレイアウトは以下の通り。
                //
                // region-a) [startIndex, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keyComparer);
#endif

                // 配列を [region-b] [region-a] [region-c] の順に並び替えるために、region-b の終端の一部または全部を region-a と入れ替える。

                // 入れ替える長さを求める (region-a の長さと region-b の長さの最小値)
                var lengthToExchange = (endOfPivotKeys - startIndex).Minimum(lowerBoundary - endOfPivotKeys);

                // 入れ替える片方の開始位置 (region-a の先端位置)
                var exStartIndex = startIndex;

                // 入れ替えるもう片方の開始位置 (region-b の終端位置)
                var exEndIndex = upperBoundary;

                // 入れ替える値がなくなるまで繰り返す
                while (exStartIndex < exEndIndex)
                {
                    // 値を入れ替える
                    (source[exStartIndex], source[exEndIndex]) = (source[exEndIndex], source[exStartIndex]);

                    // 入れ替える値の位置を変更する
                    ++exStartIndex;
                    --exEndIndex;
                }

                // この時点で、配列の並びは以下の通り
                // region-b) [startIndex, startIndex + upperBoundary - endOfPivotKeys] : x < pivotKey であるキー値 x を持つ要素の集合
                // region-a) [startIndex + lowerBoundary - endOfPivotKeys, upperBoundary] : x == pivotKey であるキー値 x を持つ要素の集合
                // region-c) [lowerBoundary, endIndex]: x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                for (var index = startIndex; index <= startIndex + upperBoundary - endOfPivotKeys; ++index)
                    Assert(keyComparer.Compare(source[index], pivotKey) < 0);
                for (var index = startIndex + lowerBoundary - endOfPivotKeys; index <= upperBoundary; ++index)
                    Assert(keyComparer.Compare(source[index], pivotKey) == 0);
                for (var index = lowerBoundary; index <= endIndex; ++index)
                    Assert(keyComparer.Compare(source[index], pivotKey) > 0);
#endif

                // region-b の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortManaged(source, startIndex, upperBoundary - endOfPivotKeys + startIndex, keyComparer);

                // region-c の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortManaged(source, lowerBoundary, endIndex, keyComparer);
#if DEBUG
            }
            finally
            {
                AssertSortResult(source, startIndex, endIndex, keyComparer);
#if false
                System.Diagnostics.Debug.Unindent();
                System.Diagnostics.Debug.WriteLine($"Leave QuickSort({startIndex}, {endIndex}) {endIndex - startIndex + 1} bytes");
#endif
            }
#endif
        }

        ///<summary>
        ///A quicksort method that allows duplicate keys.
        ///</summary>
        /// <remarks>
        /// See also <seealso href="https://kankinkon.hatenadiary.org/entry/20120202/1328133196">kanmo's blog</seealso>. 
        /// </remarks>
        private static void InternalQuickSortManaged<ELEMENT_T, KEY_T>(Span<ELEMENT_T> source, Int32 startIndex, Int32 endIndex, Func<ELEMENT_T, KEY_T> keySelector, IComparer<KEY_T> keyComparer)

        {
#if DEBUG
#if false
            System.Diagnostics.Debug.WriteLine($"Enter QuickSort({startIndex}, {endIndex}) {endIndex - startIndex + 1} bytes, ");
            System.Diagnostics.Debug.Indent();
#endif

            try
            {
#endif
                if (endIndex <= startIndex)
                    return;
                if (endIndex - startIndex == 1)
                {
                    if (keyComparer.Compare(keySelector(source[startIndex]), keySelector(source[endIndex])) > 0)
                        (source[startIndex], source[endIndex]) = (source[endIndex], source[startIndex]);
                    return;
                }

                // もしキー値が重複していないと仮定すれば、 3 点のキー値の中間値を pivotKey として採用することによりよりよい分割が望めるが、
                // この QuickSort メソッドでは重複キーを許容するので、source[startIndex] のキー値を pivotKey とする。
#if true
                // 配列の最初の要素のキー値が pivotKey なので、後述の配列レイアウトに従って、lowerBoundary および endOfPivotKeys を +1 しておく。
                var pivotKey = keySelector(source[startIndex]);
                var lowerBoundary = startIndex + 1;
                var upperBoundary = endIndex;
                var endOfPivotKeys = startIndex + 1;
#else
                var pivotKey = SelectPivotKey(keySelector(source[startIndex]), keySelector(source[endIndex]), keySelector(source[(startIndex + endIndex) / 2]), keyComparer);
                var lowerBoundary = startIndex;
                var upperBoundary = endIndex;
                var endOfPivotKeys = startIndex;
#endif

                // この時点での配列のレイアウトは以下の通り
                // region-w を如何に縮小するかがこのループの目的である
                //
                // region-a) [startIndex, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 1)
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                // region-w) [lowerBoundary, upperBoundary] : pivotKey との大小関係が不明なキー値を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                while (lowerBoundary <= upperBoundary)
                {
                    // source[lowerBoundary] に pivotKey より大きいキーが見つかるまで lowerBoundary を増やし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = keyComparer.Compare(keySelector(source[lowerBoundary]), pivotKey);
                        if (c > 0)
                        {
                            // source[lowerBoundary] > pivotKey である場合
#if DEBUG
                            Assert(keyComparer.Compare(keySelector(source[lowerBoundary]), pivotKey) > 0);
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif
                            // pivotKey より大きいキー値を持つ要素が見つかったので、ループを終える
                            break;
                        }

                        // source[lowerBoundary] <= pivotKey である場合
#if DEBUG
                        Assert(keyComparer.Compare(keySelector(source[lowerBoundary]), pivotKey) <= 0);
#endif
                        if (c == 0)
                        {
                            // source[lowerBoundary] == pivotKey である場合
#if DEBUG
                            Assert(keyComparer.Compare(keySelector(source[lowerBoundary]), pivotKey) == 0);
#endif
                            // region-a に lowerBoundary にある要素を追加する
                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // region-b は空ではない、つまり source[endOfPivotKeys] < pivotKey であるはずなので、source[lowerBoundary] と要素を交換する。
                                (source[endOfPivotKeys], source[lowerBoundary]) = (source[lowerBoundary], source[endOfPivotKeys]);
                            }
                            else
                            {
                                // region-b が空である場合

                                // endOfPivotKeys == lowerBoundary であるはずなので、要素の交換は不要。
#if DEBUG
                                Assert(endOfPivotKeys == lowerBoundary);
#endif
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;
                        }

                        // region-b の終端位置をインクリメントする
                        ++lowerBoundary;
#if DEBUG
                        AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif
                    }

#if DEBUG
                    AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif

                    // この時点で lowerBoundary > upperBoundary || source[lowerBoundary] > pivotKey && source[endOfPivotKeys] != pivotKey
                    Assert(lowerBoundary > upperBoundary || keyComparer.Compare(keySelector(source[lowerBoundary]), pivotKey) > 0 && keyComparer.Compare(keySelector(source[endOfPivotKeys]), pivotKey) != 0);

                    // source[upperBoundary] に pivotKey より小さいまたは等しいキー値を持つ要素が見つかるまで upperBoundary を減らし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = keyComparer.Compare(keySelector(source[upperBoundary]), pivotKey);
                        if (c == 0)
                        {
                            // source[upperBoundary] == pivotKey である場合

                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // 以下の 3 つの事実が判明しているので、3 つの要素をそれぞれ入れ替える。
                                // 1) source[upperBoundary] == pivotKey
                                // 2) source[lowerBoundary] > pivotKey (前の while ループの結果より)
                                // 3) source[endOfPivotKeys] < pivotKey (regon-b が空ではないことより)
#if DEBUG
                                Assert(keyComparer.Compare(keySelector(source[upperBoundary]), pivotKey) == 0 && keyComparer.Compare(keySelector(source[lowerBoundary]), pivotKey) > 0 && keyComparer.Compare(keySelector(source[endOfPivotKeys]), pivotKey) < 0);
#endif
                                var t = source[endOfPivotKeys];
                                source[endOfPivotKeys] = source[upperBoundary];
                                source[upperBoundary] = source[lowerBoundary];
                                source[lowerBoundary] = t;
                            }
                            else
                            {
                                // region-b が空である場合

                                // 以下の 3 つの事実が判明しているので、2 つの要素を入れ替える。
                                // 1) source[upperBoundary] == pivotKey
                                // 2) source[lowerBoundary] > pivotKey (前の while ループの結果より)
                                // 3) endOfPivotKeys == lowerBoundary (regon-b が空ではあることより)
#if DEBUG
                                Assert(keyComparer.Compare(keySelector(source[upperBoundary]), pivotKey) == 0 && keyComparer.Compare(keySelector(source[lowerBoundary]), pivotKey) > 0 && endOfPivotKeys == lowerBoundary);
#endif
                                (source[endOfPivotKeys], source[upperBoundary]) = (source[upperBoundary], source[endOfPivotKeys]);
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;

                            // region -b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
                            Assert(endOfPivotKeys <= lowerBoundary);
#endif
                            // pivotKey と等しいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else if (c < 0)
                        {
                            // source[upperBoundary] < pivotKey である場合

                            // 前の while ループの結果より、region-b の末尾の要素のキー値が pivotKey より小さい (source[lowerBoundary] > pivotKey) ことが判明しているので、
                            // region-b の終端と要素を入れ替える
                            (source[upperBoundary], source[lowerBoundary]) = (source[lowerBoundary], source[upperBoundary]);

                            // region-b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            Assert(endOfPivotKeys <= lowerBoundary);
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif
                            // pivotKey より小さいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else
                        {
                            // source[upperBoundary] > pivotKey である場合

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif
                        }
                    }
#if DEBUG
                    AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif
                }

                // この時点で region-w のサイズは 0 であり、lowerBoundary == upperBoundary + 1 のはずである。
#if DEBUG
                Assert(lowerBoundary == upperBoundary + 1);
#endif

                // この時点での配列のレイアウトは以下の通り。
                //
                // region-a) [startIndex, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                AssertQuickSortState(source, startIndex, endIndex, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif

                // 配列を [region-b] [region-a] [region-c] の順に並び替えるために、region-b の終端の一部または全部を region-a と入れ替える。

                // 入れ替える長さを求める (region-a の長さと region-b の長さの最小値)
                var lengthToExchange = (endOfPivotKeys - startIndex).Minimum(lowerBoundary - endOfPivotKeys);

                // 入れ替える片方の開始位置 (region-a の先端位置)
                var exStartIndex = startIndex;

                // 入れ替えるもう片方の開始位置 (region-b の終端位置)
                var exEndIndex = upperBoundary;

                // 入れ替える値がなくなるまで繰り返す
                while (exStartIndex < exEndIndex)
                {
                    // 値を入れ替える
                    (source[exStartIndex], source[exEndIndex]) = (source[exEndIndex], source[exStartIndex]);

                    // 入れ替える値の位置を変更する
                    ++exStartIndex;
                    --exEndIndex;
                }

                // この時点で、配列の並びは以下の通り
                // region-b) [startIndex, startIndex + upperBoundary - endOfPivotKeys] : x < pivotKey であるキー値 x を持つ要素の集合
                // region-a) [startIndex + lowerBoundary - endOfPivotKeys, upperBoundary] : x == pivotKey であるキー値 x を持つ要素の集合
                // region-c) [lowerBoundary, endIndex]: x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                for (var index = startIndex; index <= startIndex + upperBoundary - endOfPivotKeys; ++index)
                    Assert(keyComparer.Compare(keySelector(source[index]), pivotKey) < 0);
                for (var index = startIndex + lowerBoundary - endOfPivotKeys; index <= upperBoundary; ++index)
                    Assert(keyComparer.Compare(keySelector(source[index]), pivotKey) == 0);
                for (var index = lowerBoundary; index <= endIndex; ++index)
                    Assert(keyComparer.Compare(keySelector(source[index]), pivotKey) > 0);
#endif

                // region-b の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortManaged(source, startIndex, upperBoundary - endOfPivotKeys + startIndex, keySelector, keyComparer);

                // region-c の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortManaged(source, lowerBoundary, endIndex, keySelector, keyComparer);
#if DEBUG
            }
            finally
            {
                AssertSortResult(source, startIndex, endIndex, keySelector, keyComparer);
#if false
                System.Diagnostics.Debug.Unindent();
                System.Diagnostics.Debug.WriteLine($"Leave QuickSort({startIndex}, {endIndex}) {endIndex - startIndex + 1} bytes");
#endif
            }
#endif
        }

#if DEBUG
        private static void AssertSortResult<ELEMENT_T>(ReadOnlySpan<ELEMENT_T> source, Int32 startIndex, Int32 endIndex)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            for (var index = startIndex; index < endIndex - 1; ++index)
                Assert(source[index].CompareTo(source[index + 1]) <= 0);
        }

        private static void AssertSortResult<ELEMENT_T, KEY_T>(ReadOnlySpan<ELEMENT_T> source, Int32 startIndex, Int32 endIndex, Func<ELEMENT_T, KEY_T> keySelector)
            where KEY_T : IComparable<KEY_T>
        {
            for (var index = startIndex; index < endIndex - 1; ++index)
                Assert(keySelector(source[index]).CompareTo(keySelector(source[index + 1])) <= 0);
        }

        private static void AssertSortResult<ELEMENT_T>(ReadOnlySpan<ELEMENT_T> source, Int32 startIndex, Int32 endIndex, IComparer<ELEMENT_T> keyComparer)
        {
            for (var index = startIndex; index < endIndex - 1; ++index)
                Assert(keyComparer.Compare(source[index], source[index + 1]) <= 0);
        }

        private static void AssertSortResult<ELEMENT_T, KEY_T>(ReadOnlySpan<ELEMENT_T> source, Int32 startIndex, Int32 endIndex, Func<ELEMENT_T, KEY_T> keySelector, IComparer<KEY_T> keyComparer)
        {
            for (var index = startIndex; index < endIndex - 1; ++index)
                Assert(keyComparer.Compare(keySelector(source[index]), keySelector(source[index + 1])) <= 0);
        }

        private static void AssertQuickSortState<ELEMENT_T>(ELEMENT_T[] source, Int32 startIndex, Int32 endIndex, ELEMENT_T pivotKey, Int32 lowerBoundary, Int32 upperBoundary, Int32 endOfPivotKeys)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            for (var index = startIndex; index < endOfPivotKeys; ++index)
                Assert(source[index].CompareTo(pivotKey) == 0);
            for (var index = endOfPivotKeys; index < lowerBoundary; ++index)
                Assert(source[index].CompareTo(pivotKey) < 0);
            for (var index = upperBoundary + 1; index <= endIndex; ++index)
                Assert(source[index].CompareTo(pivotKey) > 0);
        }

        private static void AssertQuickSortState<ELEMENT_T, KEY_T>(ELEMENT_T[] source, Int32 startIndex, Int32 endIndex, KEY_T pivotKey, Int32 lowerBoundary, Int32 upperBoundary, Int32 endOfPivotKeys, Func<ELEMENT_T, KEY_T> keySelector)
            where KEY_T : IComparable<KEY_T>
        {
            for (var index = startIndex; index < endOfPivotKeys; ++index)
                Assert(keySelector(source[index]).CompareTo(pivotKey) == 0);
            for (var index = endOfPivotKeys; index < lowerBoundary; ++index)
                Assert(keySelector(source[index]).CompareTo(pivotKey) < 0);
            for (var index = upperBoundary + 1; index <= endIndex; ++index)
                Assert(keySelector(source[index]).CompareTo(pivotKey) > 0);
        }

        private static void AssertQuickSortState<ELEMENT_T>(ELEMENT_T[] source, Int32 startIndex, Int32 endIndex, ELEMENT_T pivotKey, Int32 lowerBoundary, Int32 upperBoundary, Int32 endOfPivotKeys, IComparer<ELEMENT_T> comparer)
        {
            for (var index = startIndex; index < endOfPivotKeys; ++index)
                Assert(comparer.Compare(source[index], pivotKey) == 0);
            for (var index = endOfPivotKeys; index < lowerBoundary; ++index)
                Assert(comparer.Compare(source[index], pivotKey) < 0);
            for (var index = upperBoundary + 1; index <= endIndex; ++index)
                Assert(comparer.Compare(source[index], pivotKey) > 0);
        }
        private static void AssertQuickSortState<ELEMENT_T, KEY_T>(ELEMENT_T[] source, Int32 startIndex, Int32 endIndex, KEY_T pivotKey, Int32 lowerBoundary, Int32 upperBoundary, Int32 endOfPivotKeys, Func<ELEMENT_T, KEY_T> keySelector, IComparer<KEY_T> keyComparer)
        {
            for (var index = startIndex; index < endOfPivotKeys; ++index)
                Assert(keyComparer.Compare(keySelector(source[index]), pivotKey) == 0);
            for (var index = endOfPivotKeys; index < lowerBoundary; ++index)
                Assert(keyComparer.Compare(keySelector(source[index]), pivotKey) < 0);
            for (var index = upperBoundary + 1; index <= endIndex; ++index)
                Assert(keyComparer.Compare(keySelector(source[index]), pivotKey) > 0);
        }

        private static void AssertQuickSortState<ELEMENT_T>(ReadOnlySpan<ELEMENT_T> source, Int32 startIndex, Int32 endIndex, ELEMENT_T pivotKey, Int32 lowerBoundary, Int32 upperBoundary, Int32 endOfPivotKeys)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            for (var index = startIndex; index < endOfPivotKeys; ++index)
                Assert(source[index].CompareTo(pivotKey) == 0);
            for (var index = endOfPivotKeys; index < lowerBoundary; ++index)
                Assert(source[index].CompareTo(pivotKey) < 0);
            for (var index = upperBoundary + 1; index <= endIndex; ++index)
                Assert(source[index].CompareTo(pivotKey) > 0);
        }

        private static void AssertQuickSortState<ELEMENT_T, KEY_T>(ReadOnlySpan<ELEMENT_T> source, Int32 startIndex, Int32 endIndex, KEY_T pivotKey, Int32 lowerBoundary, Int32 upperBoundary, Int32 endOfPivotKeys, Func<ELEMENT_T, KEY_T> keySelector)
            where KEY_T : IComparable<KEY_T>
        {
            for (var index = startIndex; index < endOfPivotKeys; ++index)
                Assert(keySelector(source[index]).CompareTo(pivotKey) == 0);
            for (var index = endOfPivotKeys; index < lowerBoundary; ++index)
                Assert(keySelector(source[index]).CompareTo(pivotKey) < 0);
            for (var index = upperBoundary + 1; index <= endIndex; ++index)
                Assert(keySelector(source[index]).CompareTo(pivotKey) > 0);
        }

        private static void AssertQuickSortState<ELEMENT_T>(ReadOnlySpan<ELEMENT_T> source, Int32 startIndex, Int32 endIndex, ELEMENT_T pivotKey, Int32 lowerBoundary, Int32 upperBoundary, Int32 endOfPivotKeys, IComparer<ELEMENT_T> comparer)
        {
            for (var index = startIndex; index < endOfPivotKeys; ++index)
                Assert(comparer.Compare(source[index], pivotKey) == 0);
            for (var index = endOfPivotKeys; index < lowerBoundary; ++index)
                Assert(comparer.Compare(source[index], pivotKey) < 0);
            for (var index = upperBoundary + 1; index <= endIndex; ++index)
                Assert(comparer.Compare(source[index], pivotKey) > 0);
        }
        private static void AssertQuickSortState<ELEMENT_T, KEY_T>(ReadOnlySpan<ELEMENT_T> source, Int32 startIndex, Int32 endIndex, KEY_T pivotKey, Int32 lowerBoundary, Int32 upperBoundary, Int32 endOfPivotKeys, Func<ELEMENT_T, KEY_T> keySelector, IComparer<KEY_T> keyComparer)
        {
            for (var index = startIndex; index < endOfPivotKeys; ++index)
                Assert(keyComparer.Compare(keySelector(source[index]), pivotKey) == 0);
            for (var index = endOfPivotKeys; index < lowerBoundary; ++index)
                Assert(keyComparer.Compare(keySelector(source[index]), pivotKey) < 0);
            for (var index = upperBoundary + 1; index <= endIndex; ++index)
                Assert(keyComparer.Compare(keySelector(source[index]), pivotKey) > 0);
        }

#endif

        #endregion

        #region InternalQuickSortUnmanaged

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void InternalQuickSortUnmanaged<ELEMENT_T>(ref ELEMENT_T source, Int32 count)
            where ELEMENT_T : unmanaged, IComparable<ELEMENT_T>
        {
            // count が 1 以下の場合はソート不要。
            // 特に、source に渡されるはずの配列のサイズが 0 の場合、source は null 参照となるので、このチェックは必要。
            if (count <= 1)
                return;

            fixed (ELEMENT_T* startPointer = &source)
            {
                InternalQuickSortUnmanaged(startPointer, startPointer + count - 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void InternalQuickSortUnmanaged<ELEMENT_T, KEY_T>(ref ELEMENT_T source, Int32 count, Func<ELEMENT_T, KEY_T> keySelector)
            where ELEMENT_T : unmanaged
            where KEY_T : IComparable<KEY_T>
        {
            // count が 1 以下の場合はソート不要。
            // 特に、source に渡されるはずの配列のサイズが 0 の場合、source は null 参照となるので、このチェックは必要。
            if (count <= 1)
                return;

            fixed (ELEMENT_T* startPointer = &source)
            {
                InternalQuickSortUnmanaged(startPointer, startPointer + count - 1, keySelector);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void InternalQuickSortUnmanaged<ELEMENT_T>(ref ELEMENT_T source, Int32 count, IComparer<ELEMENT_T> comparer)
            where ELEMENT_T : unmanaged
        {
            // count が 1 以下の場合はソート不要。
            // 特に、source に渡されるはずの配列のサイズが 0 の場合、source は null 参照となるので、このチェックは必要。
            if (count <= 1)
                return;

            fixed (ELEMENT_T* startPointer = &source)
            {
                InternalQuickSortUnmanaged(startPointer, startPointer + count - 1, comparer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void InternalQuickSortUnmanaged<ELEMENT_T, KEY_T>(ref ELEMENT_T source, Int32 count, Func<ELEMENT_T, KEY_T> keySelector, IComparer<KEY_T> keyComparer)
            where ELEMENT_T : unmanaged
        {
            // count が 1 以下の場合はソート不要。
            // 特に、source に渡されるはずの配列のサイズが 0 の場合、source は null 参照となるので、このチェックは必要。
            if (count <= 1)
                return;

            fixed (ELEMENT_T* startPointer = &source)
            {
                InternalQuickSortUnmanaged(startPointer, startPointer + count - 1, keySelector, keyComparer);
            }
        }

        private static unsafe void InternalQuickSortUnmanaged<ELEMENT_T>(ELEMENT_T* startPointer, ELEMENT_T* endPointer)
            where ELEMENT_T : unmanaged, IComparable<ELEMENT_T>
        {
#if DEBUG
#if false
            System.Diagnostics.Debug.WriteLine($"Enter QuickSort(0x{(UInt64)startPointer:x16}, 0x{(UInt64)endPointer:x16}) {endPointer - startPointer + 1} bytes.");
            System.Diagnostics.Debug.Indent();
#endif

            try
            {
#endif
                if (endPointer <= startPointer)
                    return;
                if (endPointer - startPointer == 1)
                {
                    if (startPointer->CompareTo(*endPointer) > 0)
                        (*startPointer, *endPointer) = (*endPointer, *startPointer);
                    return;
                }

                // もしキー値が重複していないと仮定すれば、 3 点のキー値の中間値を pivotKey として採用することによりよりよい分割が望めるが、
                // この QuickSort メソッドでは重複キーを許容するので、*startPointer のキー値を pivotKey とする。
#if true
                // 配列の最初の要素のキー値が pivotKey なので、後述の配列レイアウトに従って、lowerBoundary および endOfPivotKeys を +1 しておく。
                var pivotKey = *startPointer;
                var lowerBoundary = startPointer + 1;
                var upperBoundary = endPointer;
                var endOfPivotKeys = startPointer + 1;
#else
                var pivotKey = SelectPivotKey(*startPointer, *endPointer, startPointer[(endPointer - startPointer + 1) / 2], keyComparer);
                var lowerBoundary = startPointer;
                var upperBoundary = endPointer;
                var endOfPivotKeys = startPointer;
#endif

                // この時点での配列のレイアウトは以下の通り
                // region-w を如何に縮小するかがこのループの目的である
                //
                // region-a) [startPointer, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 1)
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                // region-w) [lowerBoundary, upperBoundary] : pivotKey との大小関係が不明なキー値を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                while (lowerBoundary <= upperBoundary)
                {
                    // *lowerBoundary に pivotKey より大きいキーが見つかるまで lowerBoundary を増やし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = lowerBoundary->CompareTo(pivotKey);
                        if (c > 0)
                        {
                            // *lowerBoundary > pivotKey である場合
#if DEBUG
                            Assert(lowerBoundary->CompareTo(pivotKey) > 0);
                            AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif
                            // pivotKey より大きいキー値を持つ要素が見つかったので、ループを終える
                            break;
                        }

                        // *lowerBoundary <= pivotKey である場合
#if DEBUG
                        Assert(lowerBoundary->CompareTo(pivotKey) <= 0);
#endif
                        if (c == 0)
                        {
                            // *lowerBoundary == pivotKey である場合
#if DEBUG
                            Assert(lowerBoundary->CompareTo(pivotKey) == 0);
#endif
                            // region-a に lowerBoundary にある要素を追加する
                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // region-b は空ではない、つまり *endOfPivotKeys < pivotKey であるはずなので、*lowerBoundary と要素を交換する。
                                (*endOfPivotKeys, *lowerBoundary) = (*lowerBoundary, *endOfPivotKeys);
                            }
                            else
                            {
                                // region-b が空である場合

                                // endOfPivotKeys == lowerBoundary であるはずなので、要素の交換は不要。
#if DEBUG
                                Assert(endOfPivotKeys == lowerBoundary);
#endif
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;
                        }

                        // region-b の終端位置をインクリメントする
                        ++lowerBoundary;
#if DEBUG
                        AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif
                    }

#if DEBUG
                    AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif

                    // この時点で lowerBoundary > upperBoundary || *lowerBoundary > pivotKey && *endOfPivotKeys != pivotKey
                    Assert(lowerBoundary > upperBoundary || lowerBoundary->CompareTo(pivotKey) > 0 && endOfPivotKeys->CompareTo(pivotKey) != 0);

                    // *upperBoundary に pivotKey より小さいまたは等しいキー値を持つ要素が見つかるまで upperBoundary を減らし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = upperBoundary->CompareTo(pivotKey);
                        if (c == 0)
                        {
                            // *upperBoundary == pivotKey である場合

                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // 以下の 3 つの事実が判明しているので、3 つの要素をそれぞれ入れ替える。
                                // 1) *upperBoundary == pivotKey
                                // 2) *lowerBoundary > pivotKey (前の while ループの結果より)
                                // 3) *endOfPivotKeys < pivotKey (regon-b が空ではないことより)
#if DEBUG
                                Assert(upperBoundary->CompareTo(pivotKey) == 0 && lowerBoundary->CompareTo(pivotKey) > 0 && endOfPivotKeys->CompareTo(pivotKey) < 0);
#endif
                                var t = *endOfPivotKeys;
                                *endOfPivotKeys = *upperBoundary;
                                *upperBoundary = *lowerBoundary;
                                *lowerBoundary = t;
                            }
                            else
                            {
                                // region-b が空である場合

                                // 以下の 3 つの事実が判明しているので、2 つの要素を入れ替える。
                                // 1) *upperBoundary == pivotKey
                                // 2) *lowerBoundary > pivotKey (前の while ループの結果より)
                                // 3) endOfPivotKeys == lowerBoundary (regon-b が空ではあることより)
#if DEBUG
                                Assert(upperBoundary->CompareTo(pivotKey) == 0 && lowerBoundary->CompareTo(pivotKey) > 0 && endOfPivotKeys == lowerBoundary);
#endif
                                (*endOfPivotKeys, *upperBoundary) = (*upperBoundary, *endOfPivotKeys);
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;

                            // region -b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
                            Assert(endOfPivotKeys <= lowerBoundary);
#endif
                            // pivotKey と等しいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else if (c < 0)
                        {
                            // *upperBoundary < pivotKey である場合

                            // 前の while ループの結果より、region-b の末尾の要素のキー値が pivotKey より小さい (*lowerBoundary > pivotKey) ことが判明しているので、
                            // region-b の終端と要素を入れ替える
                            (*upperBoundary, *lowerBoundary) = (*lowerBoundary, *upperBoundary);

                            // region-b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            Assert(endOfPivotKeys <= lowerBoundary);
                            AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif
                            // pivotKey より小さいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else
                        {
                            // *upperBoundary > pivotKey である場合

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif
                        }
                    }
#if DEBUG
                    AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif
                }

                // この時点で region-w のサイズは 0 であり、lowerBoundary == upperBoundary + 1 のはずである。
#if DEBUG
                Assert(lowerBoundary == upperBoundary + 1);
#endif

                // この時点での配列のレイアウトは以下の通り。
                //
                // region-a) [startPointer, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys);
#endif

                // 配列を [region-b] [region-a] [region-c] の順に並び替えるために、region-b の終端の一部または全部を region-a と入れ替える。

                // 入れ替える長さを求める (region-a の長さと region-b の長さの最小値)
                var lengthToExchange = (endOfPivotKeys - startPointer).Minimum(lowerBoundary - endOfPivotKeys);

                // 入れ替える片方の開始位置 (region-a の先端位置)
                var exStartIndex = startPointer;

                // 入れ替えるもう片方の開始位置 (region-b の終端位置)
                var exEndIndex = upperBoundary;

                // 入れ替える値がなくなるまで繰り返す
                while (exStartIndex < exEndIndex)
                {
                    // 値を入れ替える
                    (*exStartIndex, *exEndIndex) = (*exEndIndex, *exStartIndex);

                    // 入れ替える値の位置を変更する
                    ++exStartIndex;
                    --exEndIndex;
                }

                // この時点で、配列の並びは以下の通り
                // region-b) [startPointer, startPointer + upperBoundary - endOfPivotKeys] : x < pivotKey であるキー値 x を持つ要素の集合
                // region-a) [startPointer + lowerBoundary - endOfPivotKeys, upperBoundary] : x == pivotKey であるキー値 x を持つ要素の集合
                // region-c) [lowerBoundary, endIndex]: x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                for (var p = startPointer; p <= startPointer + (upperBoundary - endOfPivotKeys); ++p)
                    Assert(p->CompareTo(pivotKey) < 0);
                for (var p = startPointer + (lowerBoundary - endOfPivotKeys); p <= upperBoundary; ++p)
                    Assert(p->CompareTo(pivotKey) == 0);
                for (var p = lowerBoundary; p <= endPointer; ++p)
                    Assert(p->CompareTo(pivotKey) > 0);
#endif

                // region-b の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortUnmanaged(startPointer, upperBoundary - endOfPivotKeys + startPointer);

                // region-c の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortUnmanaged(lowerBoundary, endPointer);
#if DEBUG
            }
            finally
            {
                AssertSortResult(startPointer, endPointer);
#if false
                System.Diagnostics.Debug.Unindent();
                System.Diagnostics.Debug.WriteLine($"Leave QuickSort(0x{(UInt64)startPointer:x16}, 0x{(UInt64)endPointer:x16}) {endPointer - startPointer + 1} bytes.");
#endif
            }
#endif
        }

        private static unsafe void InternalQuickSortUnmanaged<ELEMENT_T, KEY_T>(ELEMENT_T* startPointer, ELEMENT_T* endPointer, Func<ELEMENT_T, KEY_T> keySelector)
            where ELEMENT_T : unmanaged
            where KEY_T : IComparable<KEY_T>
        {
#if DEBUG
#if false
            System.Diagnostics.Debug.WriteLine($"Enter QuickSort(0x{(UInt64)startPointer:x16}, 0x{(UInt64)endPointer:x16}) {endPointer - startPointer + 1} bytes.");
            System.Diagnostics.Debug.Indent();
#endif

            try
            {
#endif
                if (endPointer <= startPointer)
                    return;
                if (endPointer - startPointer == 1)
                {
                    if (keySelector(*startPointer).CompareTo(keySelector(*endPointer)) > 0)
                        (*startPointer, *endPointer) = (*endPointer, *startPointer);
                    return;
                }

                // もしキー値が重複していないと仮定すれば、 3 点のキー値の中間値を pivotKey として採用することによりよりよい分割が望めるが、
                // この QuickSort メソッドでは重複キーを許容するので、*startPointer のキー値を pivotKey とする。
#if true
                // 配列の最初の要素のキー値が pivotKey なので、後述の配列レイアウトに従って、lowerBoundary および endOfPivotKeys を +1 しておく。
                var pivotKey = keySelector(*startPointer);
                var lowerBoundary = startPointer + 1;
                var upperBoundary = endPointer;
                var endOfPivotKeys = startPointer + 1;
#else
                var pivotKey = SelectPivotKey(keySelector(*startPointer), keySelector(*endPointer), keySelector(startPointer[(endPointer - startPointer + 1) / 2]), keyComparer);
                var lowerBoundary = startPointer;
                var upperBoundary = endPointer;
                var endOfPivotKeys = startPointer;
#endif

                // この時点での配列のレイアウトは以下の通り
                // region-w を如何に縮小するかがこのループの目的である
                //
                // region-a) [startPointer, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 1)
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                // region-w) [lowerBoundary, upperBoundary] : pivotKey との大小関係が不明なキー値を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                while (lowerBoundary <= upperBoundary)
                {
                    // *lowerBoundary に pivotKey より大きいキーが見つかるまで lowerBoundary を増やし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = keySelector(*lowerBoundary).CompareTo(pivotKey);
                        if (c > 0)
                        {
                            // *lowerBoundary > pivotKey である場合
#if DEBUG
                            Assert(keySelector(*lowerBoundary).CompareTo(pivotKey) > 0);
                            AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif
                            // pivotKey より大きいキー値を持つ要素が見つかったので、ループを終える
                            break;
                        }

                        // *lowerBoundary <= pivotKey である場合
#if DEBUG
                        Assert(keySelector(*lowerBoundary).CompareTo(pivotKey) <= 0);
#endif
                        if (c == 0)
                        {
                            // *lowerBoundary == pivotKey である場合
#if DEBUG
                            Assert(keySelector(*lowerBoundary).CompareTo(pivotKey) == 0);
#endif
                            // region-a に lowerBoundary にある要素を追加する
                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // region-b は空ではない、つまり *endOfPivotKeys < pivotKey であるはずなので、*lowerBoundary と要素を交換する。
                                (*endOfPivotKeys, *lowerBoundary) = (*lowerBoundary, *endOfPivotKeys);
                            }
                            else
                            {
                                // region-b が空である場合

                                // endOfPivotKeys == lowerBoundary であるはずなので、要素の交換は不要。
#if DEBUG
                                Assert(endOfPivotKeys == lowerBoundary);
#endif
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;
                        }

                        // region-b の終端位置をインクリメントする
                        ++lowerBoundary;
#if DEBUG
                        AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif
                    }

#if DEBUG
                    AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif

                    // この時点で lowerBoundary > upperBoundary || *lowerBoundary > pivotKey && *endOfPivotKeys != pivotKey
                    Assert(lowerBoundary > upperBoundary || keySelector(*lowerBoundary).CompareTo(pivotKey) > 0 && keySelector(*endOfPivotKeys).CompareTo(pivotKey) != 0);

                    // *upperBoundary に pivotKey より小さいまたは等しいキー値を持つ要素が見つかるまで upperBoundary を減らし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = keySelector(*upperBoundary).CompareTo(pivotKey);
                        if (c == 0)
                        {
                            // *upperBoundary == pivotKey である場合

                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // 以下の 3 つの事実が判明しているので、3 つの要素をそれぞれ入れ替える。
                                // 1) *upperBoundary == pivotKey
                                // 2) *lowerBoundary > pivotKey (前の while ループの結果より)
                                // 3) *endOfPivotKeys < pivotKey (regon-b が空ではないことより)
#if DEBUG
                                Assert(keySelector(*upperBoundary).CompareTo(pivotKey) == 0 && keySelector(*lowerBoundary).CompareTo(pivotKey) > 0 && keySelector(*endOfPivotKeys).CompareTo(pivotKey) < 0);
#endif
                                var t = *endOfPivotKeys;
                                *endOfPivotKeys = *upperBoundary;
                                *upperBoundary = *lowerBoundary;
                                *lowerBoundary = t;
                            }
                            else
                            {
                                // region-b が空である場合

                                // 以下の 3 つの事実が判明しているので、2 つの要素を入れ替える。
                                // 1) *upperBoundary == pivotKey
                                // 2) *lowerBoundary > pivotKey (前の while ループの結果より)
                                // 3) endOfPivotKeys == lowerBoundary (regon-b が空ではあることより)
#if DEBUG
                                Assert(keySelector(*upperBoundary).CompareTo(pivotKey) == 0 && keySelector(*lowerBoundary).CompareTo(pivotKey) > 0 && endOfPivotKeys == lowerBoundary);
#endif
                                (*endOfPivotKeys, *upperBoundary) = (*upperBoundary, *endOfPivotKeys);
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;

                            // region -b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
                            Assert(endOfPivotKeys <= lowerBoundary);
#endif
                            // pivotKey と等しいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else if (c < 0)
                        {
                            // *upperBoundary < pivotKey である場合

                            // 前の while ループの結果より、region-b の末尾の要素のキー値が pivotKey より小さい (*lowerBoundary > pivotKey) ことが判明しているので、
                            // region-b の終端と要素を入れ替える
                            (*upperBoundary, *lowerBoundary) = (*lowerBoundary, *upperBoundary);

                            // region-b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            Assert(endOfPivotKeys <= lowerBoundary);
                            AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif
                            // pivotKey より小さいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else
                        {
                            // *upperBoundary > pivotKey である場合

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif
                        }
                    }
#if DEBUG
                    AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif
                }

                // この時点で region-w のサイズは 0 であり、lowerBoundary == upperBoundary + 1 のはずである。
#if DEBUG
                Assert(lowerBoundary == upperBoundary + 1);
#endif

                // この時点での配列のレイアウトは以下の通り。
                //
                // region-a) [startPointer, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector);
#endif

                // 配列を [region-b] [region-a] [region-c] の順に並び替えるために、region-b の終端の一部または全部を region-a と入れ替える。

                // 入れ替える長さを求める (region-a の長さと region-b の長さの最小値)
                var lengthToExchange = (endOfPivotKeys - startPointer).Minimum(lowerBoundary - endOfPivotKeys);

                // 入れ替える片方の開始位置 (region-a の先端位置)
                var exStartIndex = startPointer;

                // 入れ替えるもう片方の開始位置 (region-b の終端位置)
                var exEndIndex = upperBoundary;

                // 入れ替える値がなくなるまで繰り返す
                while (exStartIndex < exEndIndex)
                {
                    // 値を入れ替える
                    (*exStartIndex, *exEndIndex) = (*exEndIndex, *exStartIndex);

                    // 入れ替える値の位置を変更する
                    ++exStartIndex;
                    --exEndIndex;
                }

                // この時点で、配列の並びは以下の通り
                // region-b) [startPointer, startPointer + upperBoundary - endOfPivotKeys] : x < pivotKey であるキー値 x を持つ要素の集合
                // region-a) [startPointer + lowerBoundary - endOfPivotKeys, upperBoundary] : x == pivotKey であるキー値 x を持つ要素の集合
                // region-c) [lowerBoundary, endIndex]: x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                for (var p = startPointer; p <= startPointer + (upperBoundary - endOfPivotKeys); ++p)
                    Assert(keySelector(*p).CompareTo(pivotKey) < 0);
                for (var p = startPointer + (lowerBoundary - endOfPivotKeys); p <= upperBoundary; ++p)
                    Assert(keySelector(*p).CompareTo(pivotKey) == 0);
                for (var p = lowerBoundary; p <= endPointer; ++p)
                    Assert(keySelector(*p).CompareTo(pivotKey) > 0);
#endif

                // region-b の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortUnmanaged(startPointer, upperBoundary - endOfPivotKeys + startPointer, keySelector);

                // region-c の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortUnmanaged(lowerBoundary, endPointer, keySelector);
#if DEBUG
            }
            finally
            {
                AssertSortResult(startPointer, endPointer, keySelector);
#if false
                System.Diagnostics.Debug.Unindent();
                System.Diagnostics.Debug.WriteLine($"Leave QuickSort(0x{(UInt64)startPointer:x16}, 0x{(UInt64)endPointer:x16}) {endPointer - startPointer + 1} bytes.");
#endif
            }
#endif
        }

        private static unsafe void InternalQuickSortUnmanaged<ELEMENT_T>(ELEMENT_T* startPointer, ELEMENT_T* endPointer, IComparer<ELEMENT_T> comparer)
            where ELEMENT_T : unmanaged
        {
#if DEBUG
#if false
            System.Diagnostics.Debug.WriteLine($"Enter QuickSort(0x{(UInt64)startPointer:x16}, 0x{(UInt64)endPointer:x16}) {endPointer - startPointer + 1} bytes.");
            System.Diagnostics.Debug.Indent();
#endif

            try
            {
#endif
                if (endPointer <= startPointer)
                    return;
                if (endPointer - startPointer == 1)
                {
                    if (comparer.Compare(*startPointer, *endPointer) > 0)
                        (*startPointer, *endPointer) = (*endPointer, *startPointer);
                    return;
                }

                // もしキー値が重複していないと仮定すれば、 3 点のキー値の中間値を pivotKey として採用することによりよりよい分割が望めるが、
                // この QuickSort メソッドでは重複キーを許容するので、*startPointer のキー値を pivotKey とする。
#if true
                // 配列の最初の要素のキー値が pivotKey なので、後述の配列レイアウトに従って、lowerBoundary および endOfPivotKeys を +1 しておく。
                var pivotKey = *startPointer;
                var lowerBoundary = startPointer + 1;
                var upperBoundary = endPointer;
                var endOfPivotKeys = startPointer + 1;
#else
                var pivotKey = SelectPivotKey(*startPointer, *endPointer, startPointer[(endPointer - startPointer + 1) / 2], keyComparer);
                var lowerBoundary = startPointer;
                var upperBoundary = endPointer;
                var endOfPivotKeys = startPointer;
#endif

                // この時点での配列のレイアウトは以下の通り
                // region-w を如何に縮小するかがこのループの目的である
                //
                // region-a) [startPointer, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 1)
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                // region-w) [lowerBoundary, upperBoundary] : pivotKey との大小関係が不明なキー値を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                while (lowerBoundary <= upperBoundary)
                {
                    // *lowerBoundary に pivotKey より大きいキーが見つかるまで lowerBoundary を増やし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = comparer.Compare(*lowerBoundary, pivotKey);
                        if (c > 0)
                        {
                            // *lowerBoundary > pivotKey である場合
#if DEBUG
                            Assert(comparer.Compare(*lowerBoundary, pivotKey) > 0);
                            AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, comparer);
#endif
                            // pivotKey より大きいキー値を持つ要素が見つかったので、ループを終える
                            break;
                        }

                        // *lowerBoundary <= pivotKey である場合
#if DEBUG
                        Assert(comparer.Compare(*lowerBoundary, pivotKey) <= 0);
#endif
                        if (c == 0)
                        {
                            // *lowerBoundary == pivotKey である場合
#if DEBUG
                            Assert(comparer.Compare(*lowerBoundary, pivotKey) == 0);
#endif
                            // region-a に lowerBoundary にある要素を追加する
                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // region-b は空ではない、つまり *endOfPivotKeys < pivotKey であるはずなので、*lowerBoundary と要素を交換する。
                                (*endOfPivotKeys, *lowerBoundary) = (*lowerBoundary, *endOfPivotKeys);
                            }
                            else
                            {
                                // region-b が空である場合

                                // endOfPivotKeys == lowerBoundary であるはずなので、要素の交換は不要。
#if DEBUG
                                Assert(endOfPivotKeys == lowerBoundary);
#endif
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;
                        }

                        // region-b の終端位置をインクリメントする
                        ++lowerBoundary;
#if DEBUG
                        AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, comparer);
#endif
                    }

#if DEBUG
                    AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, comparer);
#endif

                    // この時点で lowerBoundary > upperBoundary || *lowerBoundary > pivotKey && *endOfPivotKeys != pivotKey
                    Assert(lowerBoundary > upperBoundary || comparer.Compare(*lowerBoundary, pivotKey) > 0 && comparer.Compare(*endOfPivotKeys, pivotKey) != 0);

                    // *upperBoundary に pivotKey より小さいまたは等しいキー値を持つ要素が見つかるまで upperBoundary を減らし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = comparer.Compare(*upperBoundary, pivotKey);
                        if (c == 0)
                        {
                            // *upperBoundary == pivotKey である場合

                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // 以下の 3 つの事実が判明しているので、3 つの要素をそれぞれ入れ替える。
                                // 1) *upperBoundary == pivotKey
                                // 2) *lowerBoundary > pivotKey (前の while ループの結果より)
                                // 3) *endOfPivotKeys < pivotKey (regon-b が空ではないことより)
#if DEBUG
                                Assert(comparer.Compare(*upperBoundary, pivotKey) == 0 && comparer.Compare(*lowerBoundary, pivotKey) > 0 && comparer.Compare(*endOfPivotKeys, pivotKey) < 0);
#endif
                                var t = *endOfPivotKeys;
                                *endOfPivotKeys = *upperBoundary;
                                *upperBoundary = *lowerBoundary;
                                *lowerBoundary = t;
                            }
                            else
                            {
                                // region-b が空である場合

                                // 以下の 3 つの事実が判明しているので、2 つの要素を入れ替える。
                                // 1) *upperBoundary == pivotKey
                                // 2) *lowerBoundary > pivotKey (前の while ループの結果より)
                                // 3) endOfPivotKeys == lowerBoundary (regon-b が空ではあることより)
#if DEBUG
                                Assert(comparer.Compare(*upperBoundary, pivotKey) == 0 && comparer.Compare(*lowerBoundary, pivotKey) > 0 && endOfPivotKeys == lowerBoundary);
#endif
                                (*endOfPivotKeys, *upperBoundary) = (*upperBoundary, *endOfPivotKeys);
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;

                            // region -b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, comparer);
                            Assert(endOfPivotKeys <= lowerBoundary);
#endif
                            // pivotKey と等しいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else if (c < 0)
                        {
                            // *upperBoundary < pivotKey である場合

                            // 前の while ループの結果より、region-b の末尾の要素のキー値が pivotKey より小さい (*lowerBoundary > pivotKey) ことが判明しているので、
                            // region-b の終端と要素を入れ替える
                            (*upperBoundary, *lowerBoundary) = (*lowerBoundary, *upperBoundary);

                            // region-b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            Assert(endOfPivotKeys <= lowerBoundary);
                            AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, comparer);
#endif
                            // pivotKey より小さいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else
                        {
                            // *upperBoundary > pivotKey である場合

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, comparer);
#endif
                        }
                    }
#if DEBUG
                    AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, comparer);
#endif
                }

                // この時点で region-w のサイズは 0 であり、lowerBoundary == upperBoundary + 1 のはずである。
#if DEBUG
                Assert(lowerBoundary == upperBoundary + 1);
#endif

                // この時点での配列のレイアウトは以下の通り。
                //
                // region-a) [startPointer, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, comparer);
#endif

                // 配列を [region-b] [region-a] [region-c] の順に並び替えるために、region-b の終端の一部または全部を region-a と入れ替える。

                // 入れ替える長さを求める (region-a の長さと region-b の長さの最小値)
                var lengthToExchange = (endOfPivotKeys - startPointer).Minimum(lowerBoundary - endOfPivotKeys);

                // 入れ替える片方の開始位置 (region-a の先端位置)
                var exStartIndex = startPointer;

                // 入れ替えるもう片方の開始位置 (region-b の終端位置)
                var exEndIndex = upperBoundary;

                // 入れ替える値がなくなるまで繰り返す
                while (exStartIndex < exEndIndex)
                {
                    // 値を入れ替える
                    (*exStartIndex, *exEndIndex) = (*exEndIndex, *exStartIndex);

                    // 入れ替える値の位置を変更する
                    ++exStartIndex;
                    --exEndIndex;
                }

                // この時点で、配列の並びは以下の通り
                // region-b) [startPointer, startPointer + upperBoundary - endOfPivotKeys] : x < pivotKey であるキー値 x を持つ要素の集合
                // region-a) [startPointer + lowerBoundary - endOfPivotKeys, upperBoundary] : x == pivotKey であるキー値 x を持つ要素の集合
                // region-c) [lowerBoundary, endIndex]: x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                for (var p = startPointer; p <= startPointer + (upperBoundary - endOfPivotKeys); ++p)
                    Assert(comparer.Compare(*p, pivotKey) < 0);
                for (var p = startPointer + (lowerBoundary - endOfPivotKeys); p <= upperBoundary; ++p)
                    Assert(comparer.Compare(*p, pivotKey) == 0);
                for (var p = lowerBoundary; p <= endPointer; ++p)
                    Assert(comparer.Compare(*p, pivotKey) > 0);
#endif

                // region-b の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortUnmanaged(startPointer, upperBoundary - endOfPivotKeys + startPointer, comparer);

                // region-c の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortUnmanaged(lowerBoundary, endPointer, comparer);
#if DEBUG
            }
            finally
            {
                AssertSortResult(startPointer, endPointer, comparer);
#if false
                System.Diagnostics.Debug.Unindent();
                System.Diagnostics.Debug.WriteLine($"Leave QuickSort(0x{(UInt64)startPointer:x16}, 0x{(UInt64)endPointer:x16}) {endPointer - startPointer + 1} bytes.");
#endif
            }
#endif
        }

        ///<summary>
        ///A quicksort method that allows duplicate keys.
        ///</summary>
        /// <remarks>
        /// See also <seealso href="https://kankinkon.hatenadiary.org/entry/20120202/1328133196">kanmo's blog</seealso>. 
        /// </remarks>
        private static unsafe void InternalQuickSortUnmanaged<ELEMENT_T, KEY_T>(ELEMENT_T* startPointer, ELEMENT_T* endPointer, Func<ELEMENT_T, KEY_T> keySelector, IComparer<KEY_T> keyComparer)
            where ELEMENT_T : unmanaged
        {
#if DEBUG
#if false
            System.Diagnostics.Debug.WriteLine($"Enter QuickSort(0x{(UInt64)startPointer:x16}, 0x{(UInt64)endPointer:x16}) {endPointer - startPointer + 1} bytes.");
            System.Diagnostics.Debug.Indent();
#endif

            try
            {
#endif
                if (endPointer <= startPointer)
                    return;
                if (endPointer - startPointer == 1)
                {
                    if (keyComparer.Compare(keySelector(*startPointer), keySelector(*endPointer)) > 0)
                        (*startPointer, *endPointer) = (*endPointer, *startPointer);
                    return;
                }

                // もしキー値が重複していないと仮定すれば、 3 点のキー値の中間値を pivotKey として採用することによりよりよい分割が望めるが、
                // この QuickSort メソッドでは重複キーを許容するので、*startPointer のキー値を pivotKey とする。
#if true
                // 配列の最初の要素のキー値が pivotKey なので、後述の配列レイアウトに従って、lowerBoundary および endOfPivotKeys を +1 しておく。
                var pivotKey = keySelector(*startPointer);
                var lowerBoundary = startPointer + 1;
                var upperBoundary = endPointer;
                var endOfPivotKeys = startPointer + 1;
#else
                var pivotKey = SelectPivotKey(keySelector(*startPointer), keySelector(*endPointer), keySelector(startPointer[(endPointer - startPointer + 1) / 2]), keyComparer);
                var lowerBoundary = startPointer;
                var upperBoundary = endPointer;
                var endOfPivotKeys = startPointer;
#endif

                // この時点での配列のレイアウトは以下の通り
                // region-w を如何に縮小するかがこのループの目的である
                //
                // region-a) [startPointer, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 1)
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                // region-w) [lowerBoundary, upperBoundary] : pivotKey との大小関係が不明なキー値を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合 (初期の長さは 0)
                while (lowerBoundary <= upperBoundary)
                {
                    // *lowerBoundary に pivotKey より大きいキーが見つかるまで lowerBoundary を増やし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = keyComparer.Compare(keySelector(*lowerBoundary), pivotKey);
                        if (c > 0)
                        {
                            // *lowerBoundary > pivotKey である場合
#if DEBUG
                            Assert(keyComparer.Compare(keySelector(*lowerBoundary), pivotKey) > 0);
                            AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif
                            // pivotKey より大きいキー値を持つ要素が見つかったので、ループを終える
                            break;
                        }

                        // *lowerBoundary <= pivotKey である場合
#if DEBUG
                        Assert(keyComparer.Compare(keySelector(*lowerBoundary), pivotKey) <= 0);
#endif
                        if (c == 0)
                        {
                            // *lowerBoundary == pivotKey である場合
#if DEBUG
                            Assert(keyComparer.Compare(keySelector(*lowerBoundary), pivotKey) == 0);
#endif
                            // region-a に lowerBoundary にある要素を追加する
                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // region-b は空ではない、つまり *endOfPivotKeys < pivotKey であるはずなので、*lowerBoundary と要素を交換する。
                                (*endOfPivotKeys, *lowerBoundary) = (*lowerBoundary, *endOfPivotKeys);
                            }
                            else
                            {
                                // region-b が空である場合

                                // endOfPivotKeys == lowerBoundary であるはずなので、要素の交換は不要。
#if DEBUG
                                Assert(endOfPivotKeys == lowerBoundary);
#endif
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;
                        }

                        // region-b の終端位置をインクリメントする
                        ++lowerBoundary;
#if DEBUG
                        AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif
                    }

#if DEBUG
                    AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif

                    // この時点で lowerBoundary > upperBoundary || *lowerBoundary > pivotKey && *endOfPivotKeys != pivotKey
                    Assert(lowerBoundary > upperBoundary || keyComparer.Compare(keySelector(*lowerBoundary), pivotKey) > 0 && keyComparer.Compare(keySelector(*endOfPivotKeys), pivotKey) != 0);

                    // *upperBoundary に pivotKey より小さいまたは等しいキー値を持つ要素が見つかるまで upperBoundary を減らし続ける。
                    while (lowerBoundary <= upperBoundary)
                    {
                        var c = keyComparer.Compare(keySelector(*upperBoundary), pivotKey);
                        if (c == 0)
                        {
                            // *upperBoundary == pivotKey である場合

                            if (endOfPivotKeys < lowerBoundary)
                            {
                                // region-b が空ではない場合

                                // 以下の 3 つの事実が判明しているので、3 つの要素をそれぞれ入れ替える。
                                // 1) *upperBoundary == pivotKey
                                // 2) *lowerBoundary > pivotKey (前の while ループの結果より)
                                // 3) *endOfPivotKeys < pivotKey (regon-b が空ではないことより)
#if DEBUG
                                Assert(keyComparer.Compare(keySelector(*upperBoundary), pivotKey) == 0 && keyComparer.Compare(keySelector(*lowerBoundary), pivotKey) > 0 && keyComparer.Compare(keySelector(*endOfPivotKeys), pivotKey) < 0);
#endif
                                var t = *endOfPivotKeys;
                                *endOfPivotKeys = *upperBoundary;
                                *upperBoundary = *lowerBoundary;
                                *lowerBoundary = t;
                            }
                            else
                            {
                                // region-b が空である場合

                                // 以下の 3 つの事実が判明しているので、2 つの要素を入れ替える。
                                // 1) *upperBoundary == pivotKey
                                // 2) *lowerBoundary > pivotKey (前の while ループの結果より)
                                // 3) endOfPivotKeys == lowerBoundary (regon-b が空ではあることより)
#if DEBUG
                                Assert(keyComparer.Compare(keySelector(*upperBoundary), pivotKey) == 0 && keyComparer.Compare(keySelector(*lowerBoundary), pivotKey) > 0 && endOfPivotKeys == lowerBoundary);
#endif
                                (*endOfPivotKeys, *upperBoundary) = (*upperBoundary, *endOfPivotKeys);
                            }

                            // region-a の終端位置をインクリメントする
                            ++endOfPivotKeys;

                            // region -b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
                            Assert(endOfPivotKeys <= lowerBoundary);
#endif
                            // pivotKey と等しいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else if (c < 0)
                        {
                            // *upperBoundary < pivotKey である場合

                            // 前の while ループの結果より、region-b の末尾の要素のキー値が pivotKey より小さい (*lowerBoundary > pivotKey) ことが判明しているので、
                            // region-b の終端と要素を入れ替える
                            (*upperBoundary, *lowerBoundary) = (*lowerBoundary, *upperBoundary);

                            // region-b の終端位置をインクリメントする
                            ++lowerBoundary;

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            Assert(endOfPivotKeys <= lowerBoundary);
                            AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif
                            // pivotKey より小さいキー値を持つ要素が見つかったので、ループを終える。
                            break;
                        }
                        else
                        {
                            // *upperBoundary > pivotKey である場合

                            // region-c の先端位置をデクリメントする
                            --upperBoundary;
#if DEBUG
                            AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif
                        }
                    }
#if DEBUG
                    AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif
                }

                // この時点で region-w のサイズは 0 であり、lowerBoundary == upperBoundary + 1 のはずである。
#if DEBUG
                Assert(lowerBoundary == upperBoundary + 1);
#endif

                // この時点での配列のレイアウトは以下の通り。
                //
                // region-a) [startPointer, endOfPivotKeys) : x == pivotKey であるキー値 x を持つ要素の集合
                // region-b) [endOfPivotKeys, lowerBoundary) : x < pivotKey であるキー値 x を持つ要素の集合
                // region-c) (upperBoundary, endIndex] : x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                AssertQuickSortState(startPointer, endPointer, pivotKey, lowerBoundary, upperBoundary, endOfPivotKeys, keySelector, keyComparer);
#endif

                // 配列を [region-b] [region-a] [region-c] の順に並び替えるために、region-b の終端の一部または全部を region-a と入れ替える。

                // 入れ替える長さを求める (region-a の長さと region-b の長さの最小値)
                var lengthToExchange = (endOfPivotKeys - startPointer).Minimum(lowerBoundary - endOfPivotKeys);

                // 入れ替える片方の開始位置 (region-a の先端位置)
                var exStartIndex = startPointer;

                // 入れ替えるもう片方の開始位置 (region-b の終端位置)
                var exEndIndex = upperBoundary;

                // 入れ替える値がなくなるまで繰り返す
                while (exStartIndex < exEndIndex)
                {
                    // 値を入れ替える
                    (*exStartIndex, *exEndIndex) = (*exEndIndex, *exStartIndex);

                    // 入れ替える値の位置を変更する
                    ++exStartIndex;
                    --exEndIndex;
                }

                // この時点で、配列の並びは以下の通り
                // region-b) [startPointer, startPointer + upperBoundary - endOfPivotKeys] : x < pivotKey であるキー値 x を持つ要素の集合
                // region-a) [startPointer + lowerBoundary - endOfPivotKeys, upperBoundary] : x == pivotKey であるキー値 x を持つ要素の集合
                // region-c) [lowerBoundary, endIndex]: x > pivotKey であるキー値 x を持つ要素の集合
                // ※ただし lowerBoundary == upperBoundary + 1

#if DEBUG
                for (var p = startPointer; p <= startPointer + (upperBoundary - endOfPivotKeys); ++p)
                    Assert(keyComparer.Compare(keySelector(*p), pivotKey) < 0);
                for (var p = startPointer + (lowerBoundary - endOfPivotKeys); p <= upperBoundary; ++p)
                    Assert(keyComparer.Compare(keySelector(*p), pivotKey) == 0);
                for (var p = lowerBoundary; p <= endPointer; ++p)
                    Assert(keyComparer.Compare(keySelector(*p), pivotKey) > 0);
#endif

                // region-b の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortUnmanaged(startPointer, upperBoundary - endOfPivotKeys + startPointer, keySelector, keyComparer);

                // region-c の内部を並び替えるために、再帰的に QuickSort を呼び出す
                InternalQuickSortUnmanaged(lowerBoundary, endPointer, keySelector, keyComparer);
#if DEBUG
            }
            finally
            {
                AssertSortResult(startPointer, endPointer, keySelector, keyComparer);
#if false
                System.Diagnostics.Debug.Unindent();
                System.Diagnostics.Debug.WriteLine($"Leave QuickSort(0x{(UInt64)startPointer:x16}, 0x{(UInt64)endPointer:x16}) {endPointer - startPointer + 1} bytes.");
#endif
            }
#endif
        }

#if DEBUG
        private static unsafe void AssertSortResult<ELEMENT_T>(ELEMENT_T* startPointer, ELEMENT_T* endPointer)
            where ELEMENT_T : unmanaged, IComparable<ELEMENT_T>
        {
            for (var p = startPointer; p < endPointer; ++p)
                Assert(p->CompareTo(p[1]) <= 0);
        }

        private static unsafe void AssertSortResult<ELEMENT_T, KEY_T>(ELEMENT_T* startPointer, ELEMENT_T* endPointer, Func<ELEMENT_T, KEY_T> keySelector)
            where ELEMENT_T : unmanaged
            where KEY_T : IComparable<KEY_T>
        {
            for (var p = startPointer; p < endPointer; ++p)
                Assert(keySelector(*p).CompareTo(keySelector(p[1])) <= 0);
        }

        private static unsafe void AssertSortResult<ELEMENT_T>(ELEMENT_T* startPointer, ELEMENT_T* endPointer, IComparer<ELEMENT_T> keyComparer)
            where ELEMENT_T : unmanaged
        {
            for (var p = startPointer; p < endPointer; ++p)
                Assert(keyComparer.Compare(*p, p[1]) <= 0);
        }

        private static unsafe void AssertSortResult<ELEMENT_T, KEY_T>(ELEMENT_T* startPointer, ELEMENT_T* endPointer, Func<ELEMENT_T, KEY_T> keySelector, IComparer<KEY_T> keyComparer)
            where ELEMENT_T : unmanaged
        {
            for (var p = startPointer; p < endPointer - 1; ++p)
                Assert(keyComparer.Compare(keySelector(*p), keySelector(p[1])) <= 0);
        }

        private static unsafe void AssertQuickSortState<ELEMENT_T>(ELEMENT_T* startPointer, ELEMENT_T* endPointer, ELEMENT_T pivotKey, ELEMENT_T* lowerBoundary, ELEMENT_T* upperBoundary, ELEMENT_T* endOfPivotKeys)
            where ELEMENT_T : unmanaged, IComparable<ELEMENT_T>
        {
            for (var p = startPointer; p < endOfPivotKeys; ++p)
                Assert(p->CompareTo(pivotKey) == 0);
            for (var p = endOfPivotKeys; p < lowerBoundary; ++p)
                Assert(p->CompareTo(pivotKey) < 0);
            for (var p = upperBoundary + 1; p <= endPointer; ++p)
                Assert(p->CompareTo(pivotKey) > 0);
        }

        private static unsafe void AssertQuickSortState<ELEMENT_T, KEY_T>(ELEMENT_T* startPointer, ELEMENT_T* endPointer, KEY_T pivotKey, ELEMENT_T* lowerBoundary, ELEMENT_T* upperBoundary, ELEMENT_T* endOfPivotKeys, Func<ELEMENT_T, KEY_T> keySelector)
            where ELEMENT_T : unmanaged
            where KEY_T : IComparable<KEY_T>
        {
            for (var p = startPointer; p < endOfPivotKeys; ++p)
                Assert(keySelector(*p).CompareTo(pivotKey) == 0);
            for (var p = endOfPivotKeys; p < lowerBoundary; ++p)
                Assert(keySelector(*p).CompareTo(pivotKey) < 0);
            for (var p = upperBoundary + 1; p <= endPointer; ++p)
                Assert(keySelector(*p).CompareTo(pivotKey) > 0);
        }

        private static unsafe void AssertQuickSortState<ELEMENT_T>(ELEMENT_T* startPointer, ELEMENT_T* endPointer, ELEMENT_T pivotKey, ELEMENT_T* lowerBoundary, ELEMENT_T* upperBoundary, ELEMENT_T* endOfPivotKeys, IComparer<ELEMENT_T> comparer)
            where ELEMENT_T : unmanaged
        {
            for (var p = startPointer; p < endOfPivotKeys; ++p)
                Assert(comparer.Compare(*p, pivotKey) == 0);
            for (var p = endOfPivotKeys; p < lowerBoundary; ++p)
                Assert(comparer.Compare(*p, pivotKey) < 0);
            for (var p = upperBoundary + 1; p <= endPointer; ++p)
                Assert(comparer.Compare(*p, pivotKey) > 0);
        }
        private static unsafe void AssertQuickSortState<ELEMENT_T, KEY_T>(ELEMENT_T* startPointer, ELEMENT_T* endPointer, KEY_T pivotKey, ELEMENT_T* lowerBoundary, ELEMENT_T* upperBoundary, ELEMENT_T* endOfPivotKeys, Func<ELEMENT_T, KEY_T> keySelector, IComparer<KEY_T> keyComparer)
            where ELEMENT_T : unmanaged
        {
            for (var p = startPointer; p < endOfPivotKeys; ++p)
                Assert(keyComparer.Compare(keySelector(*p), pivotKey) == 0);
            for (var p = endOfPivotKeys; p < lowerBoundary; ++p)
                Assert(keyComparer.Compare(keySelector(*p), pivotKey) < 0);
            for (var p = upperBoundary + 1; p <= endPointer; ++p)
                Assert(keyComparer.Compare(keySelector(*p), pivotKey) > 0);
        }
#endif

        #endregion

        #region InternalSequenceEqual

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static Boolean InternalSequenceEqual<ELEMENT_T>(ELEMENT_T[] array1, Int32 array1Offset, Int32 array1Length, ELEMENT_T[] array2, Int32 array2Offset, Int32 array2Length)
            where ELEMENT_T : IEquatable<ELEMENT_T>
            => Type.GetTypeCode(typeof(ELEMENT_T)) switch
            {
                TypeCode.Boolean => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Boolean>(ref array2[array2Offset]), array2Length),
                TypeCode.Char => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Char>(ref array2[array2Offset]), array2Length),
                TypeCode.SByte => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, SByte>(ref array2[array2Offset]), array2Length),
                TypeCode.Byte => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Byte>(ref array2[array2Offset]), array2Length),
                TypeCode.Int16 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Int16>(ref array2[array2Offset]), array2Length),
                TypeCode.UInt16 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, UInt16>(ref array2[array2Offset]), array2Length),
                TypeCode.Int32 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Int32>(ref array2[array2Offset]), array2Length),
                TypeCode.UInt32 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, UInt32>(ref array2[array2Offset]), array2Length),
                TypeCode.Int64 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Int64>(ref array2[array2Offset]), array2Length),
                TypeCode.UInt64 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, UInt64>(ref array2[array2Offset]), array2Length),
                TypeCode.Single => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Single>(ref array2[array2Offset]), array2Length),
                TypeCode.Double => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Double>(ref array2[array2Offset]), array2Length),
                TypeCode.Decimal => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Decimal>(ref array2[array2Offset]), array2Length),
                _ => InternalSequenceEqualManaged(array1, array1Offset, array1Length, array2, array2Offset, array2Length),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static Boolean InternalSequenceEqual<ELEMENT_T>(ELEMENT_T[] array1, Int32 array1Offset, Int32 array1Length, ELEMENT_T[] array2, Int32 array2Offset, Int32 array2Length, IEqualityComparer<ELEMENT_T> equalityComparer)
            => Type.GetTypeCode(typeof(ELEMENT_T)) switch
            {
                TypeCode.Boolean => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Boolean>(ref array2[array2Offset]), array2Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Boolean>>(ref equalityComparer)),
                TypeCode.Char => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Char>(ref array2[array2Offset]), array2Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Char>>(ref equalityComparer)),
                TypeCode.SByte => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, SByte>(ref array2[array2Offset]), array2Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<SByte>>(ref equalityComparer)),
                TypeCode.Byte => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Byte>(ref array2[array2Offset]), array2Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Byte>>(ref equalityComparer)),
                TypeCode.Int16 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Int16>(ref array2[array2Offset]), array2Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Int16>>(ref equalityComparer)),
                TypeCode.UInt16 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, UInt16>(ref array2[array2Offset]), array2Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<UInt16>>(ref equalityComparer)),
                TypeCode.Int32 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Int32>(ref array2[array2Offset]), array2Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Int32>>(ref equalityComparer)),
                TypeCode.UInt32 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, UInt32>(ref array2[array2Offset]), array2Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<UInt32>>(ref equalityComparer)),
                TypeCode.Int64 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Int64>(ref array2[array2Offset]), array2Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Int64>>(ref equalityComparer)),
                TypeCode.UInt64 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, UInt64>(ref array2[array2Offset]), array2Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<UInt64>>(ref equalityComparer)),
                TypeCode.Single => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Single>(ref array2[array2Offset]), array2Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Single>>(ref equalityComparer)),
                TypeCode.Double => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Double>(ref array2[array2Offset]), array2Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Double>>(ref equalityComparer)),
                TypeCode.Decimal => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Decimal>(ref array2[array2Offset]), array2Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Decimal>>(ref equalityComparer)),
                _ => InternalSequenceEqualManaged(array1, array1Offset, array1Length, array2, array2Offset, array2Length, equalityComparer),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Boolean InternalSequenceEqual<ELEMENT_T, KEY_T>(ELEMENT_T[] array1, Int32 array1Offset, Int32 array1Length, ELEMENT_T[] array2, Int32 array2Offset, Int32 array2Length, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (array1Length != array2Length)
                return false;
            for (var index = 0; index < array1Length; index++)
            {
                var key1 = keySelecter(array1[array1Offset + index]);
                var key2 = keySelecter(array2[array2Offset + index]);
                if (!DefaultEqual(key1, key2))
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Boolean InternalSequenceEqual<ELEMENT_T, KEY_T>(ELEMENT_T[] array1, Int32 array1Offset, Int32 array1Length, ELEMENT_T[] array2, Int32 array2Offset, Int32 array2Length, Func<ELEMENT_T, KEY_T> keySelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
        {
            if (array1Length != array2Length)
                return false;
            for (var index = 0; index < array1Length; index++)
            {
                if (!keyEqualityComparer.Equals(keySelecter(array1[array1Offset + index]), keySelecter(array2[array2Offset + index])))
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static Boolean InternalSequenceEqual<ELEMENT_T>(ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2)
            where ELEMENT_T : IEquatable<ELEMENT_T>
            => Type.GetTypeCode(typeof(ELEMENT_T)) switch
            {
                TypeCode.Boolean => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Boolean>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.Char => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Char>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.SByte => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, SByte>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.Byte => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Byte>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.Int16 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Int16>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.UInt16 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, UInt16>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.Int32 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Int32>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.UInt32 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, UInt32>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.Int64 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Int64>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.UInt64 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, UInt64>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.Single => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Single>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.Double => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Double>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.Decimal => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Decimal>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                _ => InternalSequenceEqualManaged(array1, array2),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static Boolean InternalSequenceEqual<ELEMENT_T>(ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, IEqualityComparer<ELEMENT_T> equalityComparer)
            => Type.GetTypeCode(typeof(ELEMENT_T)) switch
            {
                TypeCode.Boolean => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Boolean>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Boolean>>(ref equalityComparer)),
                TypeCode.Char => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Char>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Char>>(ref equalityComparer)),
                TypeCode.SByte => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, SByte>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<SByte>>(ref equalityComparer)),
                TypeCode.Byte => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Byte>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Byte>>(ref equalityComparer)),
                TypeCode.Int16 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Int16>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Int16>>(ref equalityComparer)),
                TypeCode.UInt16 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, UInt16>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<UInt16>>(ref equalityComparer)),
                TypeCode.Int32 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Int32>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Int32>>(ref equalityComparer)),
                TypeCode.UInt32 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, UInt32>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<UInt32>>(ref equalityComparer)),
                TypeCode.Int64 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Int64>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Int64>>(ref equalityComparer)),
                TypeCode.UInt64 => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, UInt64>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<UInt64>>(ref equalityComparer)),
                TypeCode.Single => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Single>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Single>>(ref equalityComparer)),
                TypeCode.Double => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Double>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Double>>(ref equalityComparer)),
                TypeCode.Decimal => InternalSequenceEqualUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Decimal>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IEqualityComparer<ELEMENT_T>, IEqualityComparer<Decimal>>(ref equalityComparer)),
                _ => InternalSequenceEqualManaged(array1, array2, equalityComparer),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Boolean InternalSequenceEqual<ELEMENT_T, KEY_T>(ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IEquatable<KEY_T>
        {
            if (array1.Length != array2.Length)
                return false;

            var count = array1.Length;
            for (var index = 0; index < count; index++)
            {
                var key1 = keySelecter(array1[index]);
                var key2 = keySelecter(array2[index]);
                if (!DefaultEqual(key1, key2))
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Boolean InternalSequenceEqual<ELEMENT_T, KEY_T>(ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter, IEqualityComparer<KEY_T> keyEqualityComparer)
        {
            if (array1.Length != array2.Length)
                return false;

            var count = array1.Length;
            for (var index = 0; index < count; index++)
            {
                if (!keyEqualityComparer.Equals(keySelecter(array1[index]), keySelecter(array2[index])))
                    return false;
            }

            return true;
        }

        #endregion

        #region InternalSequenceEqualManaged

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Boolean InternalSequenceEqualManaged<ELEMENT_T>(ELEMENT_T[] array1, Int32 array1Offset, Int32 array1Length, ELEMENT_T[] array2, Int32 array2Offset, Int32 array2Length)
            where ELEMENT_T : IEquatable<ELEMENT_T>
        {
            if (array1Length != array2Length)
                return false;

            for (var index = 0; index < array1Length; index++)
            {
                if (!DefaultEqual(array1[array1Offset + index], array2[array2Offset + index]))
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Boolean InternalSequenceEqualManaged<ELEMENT_T>(ELEMENT_T[] array1, Int32 array1Offset, Int32 array1Length, ELEMENT_T[] array2, Int32 array2Offset, Int32 array2Length, IEqualityComparer<ELEMENT_T> equalityComparer)
        {
            if (array1Length != array2Length)
                return false;

            for (var index = 0; index < array1Length; index++)
            {
                if (!equalityComparer.Equals(array1[array1Offset + index], array2[array2Offset + index]))
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Boolean InternalSequenceEqualManaged<ELEMENT_T>(ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2)
            where ELEMENT_T : IEquatable<ELEMENT_T>
        {
            if (array1.Length != array2.Length)
                return false;

            var count = array1.Length;
            for (var index = 0; index < count; index++)
            {
                if (!DefaultEqual(array1[index], array2[index]))
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Boolean InternalSequenceEqualManaged<ELEMENT_T>(ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, IEqualityComparer<ELEMENT_T> equalityComparer)
        {
            if (array1.Length != array2.Length)
                return false;

            var count = array1.Length;
            for (var index = 0; index < count; index++)
            {
                if (!equalityComparer.Equals(array1[index], array2[index]))
                    return false;
            }

            return true;
        }

        #endregion

        #region InternalSequenceEqualUnmanaged

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Boolean InternalSequenceEqualUnmanaged<ELEMENT_T>(ref ELEMENT_T array1, Int32 array1Length, ref ELEMENT_T array2, Int32 array2Length)
            where ELEMENT_T : unmanaged
        {
            if (array1Length != array2Length)
                return false;

            if (array1Length <= 0)
                return true;

            fixed (ELEMENT_T* pointer1 = &array1)
            fixed (ELEMENT_T* pointer2 = &array2)
            {
                if (pointer1 == pointer2)
                    return true;

                return
                    array1Length < _THRESHOLD_ARRAY_EQUAL_BY_LONG_POINTER
                    ? InternalSequenceEqualUnmanagedByByte((Byte*)pointer1, (Byte*)pointer2, array1Length * sizeof(ELEMENT_T))
                    : _is64bitProcess
                    ? InternalSequenceEqualUnmanagedByUInt64((Byte*)pointer1, (Byte*)pointer2, array1Length * sizeof(ELEMENT_T))
                    : InternalSequenceEqualUnmanagedByUInt32((Byte*)pointer1, (Byte*)pointer2, array1Length * sizeof(ELEMENT_T));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Boolean InternalSequenceEqualUnmanaged<ELEMENT_T>(ref ELEMENT_T array1, Int32 array1Length, ref ELEMENT_T array2, Int32 array2Length, IEqualityComparer<ELEMENT_T> equalityComparer)
            where ELEMENT_T : unmanaged
        {
            if (array1Length != array2Length)
                return false;

            if (array1Length <= 0)
                return true;

            fixed (ELEMENT_T* buffer1 = &array1)
            fixed (ELEMENT_T* buffer2 = &array2)
            {
                if (buffer1 == buffer2)
                    return true;

                var count = array1Length;
                var pointer1 = buffer1;
                var pointer2 = buffer2;
                while (count-- > 0)
                {
                    if (!equalityComparer.Equals(*pointer1++, *pointer2++))
                        return false;
                }

                return true;
            }
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe Boolean InternalSequenceEqualUnmanagedByUInt32(Byte* pointer1, Byte* pointer2, Int32 count)
        {
            const Int32 alignmentMask = sizeof(UInt32) - 1;

            // 先行してバイト単位で比較する長さを計算する
            {
                var offset = (Int32)pointer1 & alignmentMask;
                var preCount = (-offset & alignmentMask).Minimum(count);
                var __count = preCount;
                while (__count-- > 0)
                {
                    if (*pointer1++ != *pointer2++)
                        return false;
                }

                count -= preCount;
            }

            // この時点で pointer1 が sizeof(UInt32) バイトバウンダリ、または count == 0 のはず。
#if DEBUG
            Assert((UInt32)pointer1 % sizeof(UInt32) == 0 || count == 0);
#endif

            var longPointer1 = (UInt32*)pointer1;
            var longpointer2 = (UInt32*)pointer2;

            while (count >= 8 * sizeof(UInt32))
            {
                if (longPointer1[0] != longpointer2[0]
                    || longPointer1[1] != longpointer2[1]
                    || longPointer1[2] != longpointer2[2]
                    || longPointer1[3] != longpointer2[3]
                    || longPointer1[4] != longpointer2[4]
                    || longPointer1[5] != longpointer2[5]
                    || longPointer1[6] != longpointer2[6]
                    || longPointer1[7] != longpointer2[7])
                {
                    return false;
                }

                count -= 8 * sizeof(UInt32);
                longPointer1 += 8;
                longpointer2 += 8;
            }
#if DEBUG
            Assert((count & ~((1 << 5) - 1)) == 0);
#endif
            if ((count & (1 << 4)) != 0)
            {
                if (longPointer1[0] != longpointer2[0]
                    || longPointer1[1] != longpointer2[1]
                    || longPointer1[2] != longpointer2[2]
                    || longPointer1[3] != longpointer2[3])
                {
                    return false;
                }
#if DEBUG
                count &= ~(1 << 4);
#endif
                longPointer1 += 4;
                longpointer2 += 4;
            }

            if ((count & (1 << 3)) != 0)
            {
                if (longPointer1[0] != longpointer2[0]
                    || longPointer1[1] != longpointer2[1])
                {
                    return false;
                }
#if DEBUG
                count &= ~(1 << 3);
#endif
                longPointer1 += 2;
                longpointer2 += 2;
            }

            if ((count & (1 << 2)) != 0)
            {
                if (*longPointer1 != *longpointer2)
                    return false;
#if DEBUG
                count &= ~(1 << 2);
#endif
                ++longPointer1;
                ++longpointer2;
            }

            pointer1 = (Byte*)longPointer1;
            pointer2 = (Byte*)longpointer2;
            if ((count & (1 << 1)) != 0)
            {
                if (*(UInt16*)pointer1 != *(UInt16*)pointer2)
                    return false;
#if DEBUG
                count &= ~(1 << 1);
#endif
                pointer1 += sizeof(UInt16);
                pointer2 += sizeof(UInt16);
            }

            if ((count & (1 << 0)) != 0)
            {
                if (*pointer1 != *pointer2)
                    return false;
#if DEBUG
                count &= ~(1 << 0);
#endif
                ++pointer1;
                ++pointer2;
            }

            // この時点で count は 0 のはず
#if DEBUG
            Assert(count == 0);
#endif

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe Boolean InternalSequenceEqualUnmanagedByUInt64(Byte* pointer1, Byte* pointer2, Int32 count)
        {
            const Int32 alignmentMask = sizeof(UInt64) - 1;

            // 先行してバイト単位で比較する長さを計算する
            {
                var offset = (Int32)pointer1 & alignmentMask;
                var preCount = (-offset & alignmentMask).Minimum(count);
                var __count = preCount;
                while (__count-- > 0)
                {
                    if (*pointer1++ != *pointer2++)
                        return false;
                }

                count -= preCount;
            }

            // この時点で pointer1 が sizeof(UInt64) バイトバウンダリ、または count == 0 のはず。
#if DEBUG
            Assert((UInt64)pointer1 % sizeof(UInt64) == 0 || count == 0);
#endif

            var longPointer1 = (UInt64*)pointer1;
            var longPointer2 = (UInt64*)pointer2;

            while (count >= 8 * sizeof(UInt64))
            {
                if (longPointer1[0] != longPointer2[0]
                    || longPointer1[1] != longPointer2[1]
                    || longPointer1[2] != longPointer2[2]
                    || longPointer1[3] != longPointer2[3]
                    || longPointer1[4] != longPointer2[4]
                    || longPointer1[5] != longPointer2[5]
                    || longPointer1[6] != longPointer2[6]
                    || longPointer1[7] != longPointer2[7])
                {
                    return false;
                }

                count -= 8 * sizeof(UInt64);
                longPointer1 += 8;
                longPointer2 += 8;
            }
#if DEBUG
            Assert((count & ~((1 << 6) - 1)) == 0);
#endif
            if ((count & (1 << 5)) != 0)
            {
                if (longPointer1[0] != longPointer2[0]
                    || longPointer1[1] != longPointer2[1]
                    || longPointer1[2] != longPointer2[2]
                    || longPointer1[3] != longPointer2[3])
                {
                    return false;
                }
#if DEBUG
                count &= ~(1 << 5);
#endif
                longPointer1 += 4;
                longPointer2 += 4;
            }

            if ((count & (1 << 4)) != 0)
            {
                if (longPointer1[0] != longPointer2[0]
                    || longPointer1[1] != longPointer2[1])
                {
                    return false;
                }
#if DEBUG
                count &= ~(1 << 4);
#endif
                longPointer1 += 2;
                longPointer2 += 2;
            }

            if ((count & (1 << 3)) != 0)
            {
                if (*longPointer1 != *longPointer2)
                    return false;
#if DEBUG
                count &= ~(1 << 3);
#endif
                ++longPointer1;
                ++longPointer2;
            }

            pointer1 = (Byte*)longPointer1;
            pointer2 = (Byte*)longPointer2;
            if ((count & (1 << 2)) != 0)
            {
                if (*(UInt32*)pointer1 != *(UInt32*)pointer2)
                    return false;
#if DEBUG
                count &= ~(1 << 2);
#endif
                pointer1 += sizeof(UInt32);
                pointer2 += sizeof(UInt32);
            }

            if ((count & (1 << 1)) != 0)
            {
                if (*(UInt16*)pointer1 != *(UInt16*)pointer2)
                    return false;
#if DEBUG
                count &= ~(1 << 1);
#endif
                pointer1 += sizeof(UInt16);
                pointer2 += sizeof(UInt16);
            }

            if ((count & (1 << 0)) != 0)
            {
                if (*pointer1 != *pointer2)
                    return false;
#if DEBUG
                count &= ~(1 << 0);
#endif
                ++pointer1;
                ++pointer2;
            }

            // この時点で count は 0 のはず
#if DEBUG
            Assert(count == 0);
#endif

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe Boolean InternalSequenceEqualUnmanagedByByte(Byte* pointer1, Byte* pointer2, Int32 count)
        {
#if DEBUG
            Assert(count < _THRESHOLD_ARRAY_EQUAL_BY_LONG_POINTER);
#endif
            while (count >= 8)
            {
                if (pointer1[0] != pointer2[0]
                    || pointer1[1] != pointer2[1]
                    || pointer1[2] != pointer2[2]
                    || pointer1[3] != pointer2[3]
                    || pointer1[4] != pointer2[4]
                    || pointer1[5] != pointer2[5]
                    || pointer1[6] != pointer2[6]
                    || pointer1[7] != pointer2[7])
                {
                    return false;
                }

                count -= 8;
                pointer1 += 8;
                pointer2 += 8;
            }
#if DEBUG
            Assert((count & ~((1 << 3) - 1)) == 0);
#endif
            if ((count & (1 << 2)) != 0)
            {
                if (pointer1[0] != pointer2[0]
                    || pointer1[1] != pointer2[1]
                    || pointer1[2] != pointer2[2]
                    || pointer1[3] != pointer2[3])
                {
                    return false;
                }
#if DEBUG
                count &= ~(1 << 2);
#endif
                pointer1 += 4;
                pointer2 += 4;
            }

            if ((count & (1 << 1)) != 0)
            {
                if (pointer1[0] != pointer2[0]
                    || pointer1[1] != pointer2[1])
                {
                    return false;
                }
#if DEBUG
                count &= ~(1 << 1);
#endif
                pointer1 += 2;
                pointer2 += 2;
            }

            if ((count & (1 << 0)) != 0)
            {
                if (*pointer1 != *pointer2)
                    return false;
#if DEBUG
                count &= ~(1 << 0);
#endif
                ++pointer1;
                ++pointer2;
            }

            // この時点で count は 0 のはず
#if DEBUG
            Assert(count == 0);
#endif

            return true;
        }

        #region InternalSequenceCompare

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static Int32 InternalSequenceCompare<ELEMENT_T>(ELEMENT_T[] array1, Int32 array1Offset, Int32 array1Length, ELEMENT_T[] array2, Int32 array2Offset, Int32 array2Length)
            where ELEMENT_T : IComparable<ELEMENT_T>
            => Type.GetTypeCode(typeof(ELEMENT_T)) switch
            {
                TypeCode.Boolean => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Boolean>(ref array2[array2Offset]), array2Length),
                TypeCode.Char => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Char>(ref array2[array2Offset]), array2Length),
                TypeCode.SByte => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, SByte>(ref array2[array2Offset]), array2Length),
                TypeCode.Byte => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Byte>(ref array2[array2Offset]), array2Length),
                TypeCode.Int16 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Int16>(ref array2[array2Offset]), array2Length),
                TypeCode.UInt16 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, UInt16>(ref array2[array2Offset]), array2Length),
                TypeCode.Int32 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Int32>(ref array2[array2Offset]), array2Length),
                TypeCode.UInt32 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, UInt32>(ref array2[array2Offset]), array2Length),
                TypeCode.Int64 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Int64>(ref array2[array2Offset]), array2Length),
                TypeCode.UInt64 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, UInt64>(ref array2[array2Offset]), array2Length),
                TypeCode.Single => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Single>(ref array2[array2Offset]), array2Length),
                TypeCode.Double => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Double>(ref array2[array2Offset]), array2Length),
                TypeCode.Decimal => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Decimal>(ref array2[array2Offset]), array2Length),
                _ => InternalSequenceCompareManaged(array1, array1Offset, array1Length, array2, array2Offset, array2Length),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static Int32 InternalSequenceCompare<ELEMENT_T>(ELEMENT_T[] array1, Int32 array1Offset, Int32 array1Length, ELEMENT_T[] array2, Int32 array2Offset, Int32 array2Length, IComparer<ELEMENT_T> comparer)
            => Type.GetTypeCode(typeof(ELEMENT_T)) switch
            {
                TypeCode.Boolean => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Boolean>(ref array2[array2Offset]), array2Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Boolean>>(ref comparer)),
                TypeCode.Char => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Char>(ref array2[array2Offset]), array2Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Char>>(ref comparer)),
                TypeCode.SByte => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, SByte>(ref array2[array2Offset]), array2Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<SByte>>(ref comparer)),
                TypeCode.Byte => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Byte>(ref array2[array2Offset]), array2Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Byte>>(ref comparer)),
                TypeCode.Int16 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Int16>(ref array2[array2Offset]), array2Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Int16>>(ref comparer)),
                TypeCode.UInt16 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, UInt16>(ref array2[array2Offset]), array2Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<UInt16>>(ref comparer)),
                TypeCode.Int32 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Int32>(ref array2[array2Offset]), array2Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Int32>>(ref comparer)),
                TypeCode.UInt32 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, UInt32>(ref array2[array2Offset]), array2Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<UInt32>>(ref comparer)),
                TypeCode.Int64 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Int64>(ref array2[array2Offset]), array2Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Int64>>(ref comparer)),
                TypeCode.UInt64 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, UInt64>(ref array2[array2Offset]), array2Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<UInt64>>(ref comparer)),
                TypeCode.Single => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Single>(ref array2[array2Offset]), array2Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Single>>(ref comparer)),
                TypeCode.Double => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Double>(ref array2[array2Offset]), array2Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Double>>(ref comparer)),
                TypeCode.Decimal => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref array1[array1Offset]), array1Length, ref Unsafe.As<ELEMENT_T, Decimal>(ref array2[array2Offset]), array2Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Decimal>>(ref comparer)),
                _ => InternalSequenceCompareManaged(array1, array1Offset, array1Length, array2, array2Offset, array2Length, comparer),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int32 InternalSequenceCompare<ELEMENT_T, KEY_T>(ELEMENT_T[] array1, Int32 array1Offset, Int32 array1Length, ELEMENT_T[] array2, Int32 array2Offset, Int32 array2Length, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IComparable<KEY_T>
        {
            var count = array1Length.Minimum(array2Length);
            for (var index = 0; index < count; index++)
            {
                var c = DefaultCompare(keySelecter(array1[array1Offset + index]), keySelecter(array2[array2Offset + index]));
                if (c != 0)
                    return c;
            }

            return array1Length.CompareTo(array2Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int32 InternalSequenceCompare<ELEMENT_T, KEY_T>(ELEMENT_T[] array1, Int32 array1Offset, Int32 array1Length, ELEMENT_T[] array2, Int32 array2Offset, Int32 array2Length, Func<ELEMENT_T, KEY_T> keySelecter, IComparer<KEY_T> keyComparer)
        {
            var count = array1Length.Minimum(array2Length);
            for (var index = 0; index < count; index++)
            {
                var c = keyComparer.Compare(keySelecter(array1[array1Offset + index]), keySelecter(array2[array2Offset + index]));
                if (c != 0)
                    return c;
            }

            return array1Length.CompareTo(array2Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static Int32 InternalSequenceCompare<ELEMENT_T>(ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2)
            where ELEMENT_T : IComparable<ELEMENT_T>
            => Type.GetTypeCode(typeof(ELEMENT_T)) switch
            {
                TypeCode.Boolean => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Boolean>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.Char => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Char>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.SByte => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, SByte>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.Byte => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Byte>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.Int16 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Int16>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.UInt16 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, UInt16>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.Int32 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Int32>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.UInt32 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, UInt32>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.Int64 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Int64>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.UInt64 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, UInt64>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.Single => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Single>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.Double => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Double>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                TypeCode.Decimal => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Decimal>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length),
                _ => InternalSequenceCompareManaged(array1, array2),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static Int32 InternalSequenceCompare<ELEMENT_T>(ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, IComparer<ELEMENT_T> comparer)
            => Type.GetTypeCode(typeof(ELEMENT_T)) switch
            {
                TypeCode.Boolean => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Boolean>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Boolean>>(ref comparer)),
                TypeCode.Char => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Char>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Char>>(ref comparer)),
                TypeCode.SByte => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, SByte>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<SByte>>(ref comparer)),
                TypeCode.Byte => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Byte>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Byte>>(ref comparer)),
                TypeCode.Int16 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Int16>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Int16>>(ref comparer)),
                TypeCode.UInt16 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, UInt16>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<UInt16>>(ref comparer)),
                TypeCode.Int32 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Int32>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Int32>>(ref comparer)),
                TypeCode.UInt32 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, UInt32>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<UInt32>>(ref comparer)),
                TypeCode.Int64 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Int64>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Int64>>(ref comparer)),
                TypeCode.UInt64 => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, UInt64>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<UInt64>>(ref comparer)),
                TypeCode.Single => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Single>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Single>>(ref comparer)),
                TypeCode.Double => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Double>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Double>>(ref comparer)),
                TypeCode.Decimal => InternalSequenceCompareUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref Unsafe.AsRef(in array1.GetPinnableReference())), array1.Length, ref Unsafe.As<ELEMENT_T, Decimal>(ref Unsafe.AsRef(in array2.GetPinnableReference())), array2.Length, Unsafe.As<IComparer<ELEMENT_T>, IComparer<Decimal>>(ref comparer)),
                _ => InternalSequenceCompareManaged(array1, array2, comparer),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int32 InternalSequenceCompare<ELEMENT_T, KEY_T>(ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter)
            where KEY_T : IComparable<KEY_T>
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));

            var count = array1.Length.Minimum(array2.Length);
            for (var index = 0; index < count; index++)
            {
                var c = DefaultCompare(keySelecter(array1[index]), keySelecter(array2[index]));
                if (c != 0)
                    return c;
            }

            return array1.Length.CompareTo(array2.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int32 InternalSequenceCompare<ELEMENT_T, KEY_T>(ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, Func<ELEMENT_T, KEY_T> keySelecter, IComparer<KEY_T> keyComparer)
        {
            if (keySelecter is null)
                throw new ArgumentNullException(nameof(keySelecter));
            if (keyComparer is null)
                throw new ArgumentNullException(nameof(keyComparer));

            var count = array1.Length.Minimum(array2.Length);
            for (var index = 0; index < count; index++)
            {
                var c = keyComparer.Compare(keySelecter(array1[index]), keySelecter(array2[index]));
                if (c != 0)
                    return c;
            }

            return array1.Length.CompareTo(array2.Length);
        }

        #endregion

        #region InternalSequenceCompareManaged

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int32 InternalSequenceCompareManaged<ELEMENT_T>(ELEMENT_T[] array1, Int32 array1Offset, Int32 array1Length, ELEMENT_T[] array2, Int32 array2Offset, Int32 array2Length)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            var count = array1Length.Minimum(array2Length);
            for (var index = 0; index < count; index++)
            {
                var c = array1[array1Offset + index].CompareTo(array2[array2Offset + index]);
                if (c != 0)
                    return c;
            }

            return array1Length.CompareTo(array2Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int32 InternalSequenceCompareManaged<ELEMENT_T>(ELEMENT_T[] array1, Int32 array1Offset, Int32 array1Length, ELEMENT_T[] array2, Int32 array2Offset, Int32 array2Length, IComparer<ELEMENT_T> comparer)
        {
            var count = array1Length.Minimum(array2Length);
            for (var index = 0; index < count; index++)
            {
                var c = comparer.Compare(array1[array1Offset + index], array2[array2Offset + index]);
                if (c != 0)
                    return c;
            }

            return array1Length.CompareTo(array2Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int32 InternalSequenceCompareManaged<ELEMENT_T>(ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2)
            where ELEMENT_T : IComparable<ELEMENT_T>
        {
            var count = array1.Length.Minimum(array2.Length);
            for (var index = 0; index < count; index++)
            {
                var c = array1[index].CompareTo(array2[index]);
                if (c != 0)
                    return c;
            }

            return array1.Length.CompareTo(array2.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int32 InternalSequenceCompareManaged<ELEMENT_T>(ReadOnlySpan<ELEMENT_T> array1, ReadOnlySpan<ELEMENT_T> array2, IComparer<ELEMENT_T> comparer)
        {
            var count = array1.Length.Minimum(array2.Length);
            for (var index = 0; index < count; index++)
            {
                var c = comparer.Compare(array1[index], array2[index]);
                if (c != 0)
                    return c;
            }

            return array1.Length.CompareTo(array2.Length);
        }

        #endregion

        #region InternalSequenceCompareUnmanaged

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Int32 InternalSequenceCompareUnmanaged<ELEMENT_T>(ref ELEMENT_T array1, Int32 array1Length, ref ELEMENT_T array2, Int32 array2Length)
            where ELEMENT_T : unmanaged, IComparable<ELEMENT_T>
        {
            fixed (ELEMENT_T* buffer1 = &array1)
            fixed (ELEMENT_T* buffer2 = &array2)
            {
                var count = array1Length.Minimum(array2Length);
                var pointer1 = buffer1;
                var pointer2 = buffer2;
                while (count-- > 0)
                {
                    var c = (*pointer1++).CompareTo(*pointer2++);
                    if (c != 0)
                        return c;
                }

                return array1Length.CompareTo(array2Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Int32 InternalSequenceCompareUnmanaged<ELEMENT_T>(ref ELEMENT_T array1, Int32 array1Length, ref ELEMENT_T array2, Int32 array2Length, IComparer<ELEMENT_T> comparer)
            where ELEMENT_T : unmanaged
        {
            fixed (ELEMENT_T* buffer1 = &array1)
            fixed (ELEMENT_T* buffer2 = &array2)
            {
                var count = array1Length.Minimum(array2Length);
                var pointer1 = buffer1;
                var pointer2 = buffer2;
                while (count-- > 0)
                {
                    var c = comparer.Compare(*pointer1++, *pointer2++);
                    if (c != 0)
                        return c;
                }

                return array1Length.CompareTo(array2Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Int32 InternalSequenceCompareUnmanaged<ELEMENT_T>(ReadOnlySpan<ELEMENT_T> array1, Int32 array1Length, ReadOnlySpan<ELEMENT_T> array2, Int32 array2Length)
            where ELEMENT_T : unmanaged, IComparable<ELEMENT_T>
        {
            fixed (ELEMENT_T* buffer1 = array1)
            fixed (ELEMENT_T* buffer2 = array2)
            {
                var count = array1Length.Minimum(array2Length);
                var pointer1 = buffer1;
                var pointer2 = buffer2;
                while (count-- > 0)
                {
                    var c = (*pointer1++).CompareTo(*pointer2++);
                    if (c != 0)
                        return c;
                }

                return array1Length.CompareTo(array2Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Int32 InternalSequenceCompareUnmanaged<ELEMENT_T>(ReadOnlySpan<ELEMENT_T> array1, Int32 array1Length, ReadOnlySpan<ELEMENT_T> array2, Int32 array2Length, IComparer<ELEMENT_T> comparer)
            where ELEMENT_T : unmanaged
        {
            fixed (ELEMENT_T* buffer1 = array1)
            fixed (ELEMENT_T* buffer2 = array2)
            {
                var count = array1Length.Minimum(array2Length);
                var pointer1 = buffer1;
                var pointer2 = buffer2;
                while (count-- > 0)
                {
                    var c = comparer.Compare(*pointer1++, *pointer2++);
                    if (c != 0)
                        return c;
                }

                return array1Length.CompareTo(array2Length);
            }
        }

        #endregion

        #region InternalCopyMemory

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void InternalCopyMemory<ELEMENT_T>(ELEMENT_T[] sourceArray, Int32 sourceArrayOffset, ELEMENT_T[] destinationArray, Int32 destinationArrayOffset, Int32 count)
        {
            switch (Type.GetTypeCode(typeof(ELEMENT_T)))
            {
                case TypeCode.Boolean:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref sourceArray[sourceArrayOffset]), ref Unsafe.As<ELEMENT_T, Boolean>(ref destinationArray[destinationArrayOffset]), count);
                    break;
                case TypeCode.Char:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref sourceArray[sourceArrayOffset]), ref Unsafe.As<ELEMENT_T, Char>(ref destinationArray[destinationArrayOffset]), count);
                    break;
                case TypeCode.SByte:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref sourceArray[sourceArrayOffset]), ref Unsafe.As<ELEMENT_T, SByte>(ref destinationArray[destinationArrayOffset]), count);
                    break;
                case TypeCode.Byte:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref sourceArray[sourceArrayOffset]), ref Unsafe.As<ELEMENT_T, Byte>(ref destinationArray[destinationArrayOffset]), count);
                    break;
                case TypeCode.Int16:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref sourceArray[sourceArrayOffset]), ref Unsafe.As<ELEMENT_T, Int16>(ref destinationArray[destinationArrayOffset]), count);
                    break;
                case TypeCode.UInt16:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref sourceArray[sourceArrayOffset]), ref Unsafe.As<ELEMENT_T, UInt16>(ref destinationArray[destinationArrayOffset]), count);
                    break;
                case TypeCode.Int32:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref sourceArray[sourceArrayOffset]), ref Unsafe.As<ELEMENT_T, Int32>(ref destinationArray[destinationArrayOffset]), count);
                    break;
                case TypeCode.UInt32:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref sourceArray[sourceArrayOffset]), ref Unsafe.As<ELEMENT_T, UInt32>(ref destinationArray[destinationArrayOffset]), count);
                    break;
                case TypeCode.Int64:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref sourceArray[sourceArrayOffset]), ref Unsafe.As<ELEMENT_T, Int64>(ref destinationArray[destinationArrayOffset]), count);
                    break;
                case TypeCode.UInt64:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref sourceArray[sourceArrayOffset]), ref Unsafe.As<ELEMENT_T, UInt64>(ref destinationArray[destinationArrayOffset]), count);
                    break;
                case TypeCode.Single:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref sourceArray[sourceArrayOffset]), ref Unsafe.As<ELEMENT_T, Single>(ref destinationArray[destinationArrayOffset]), count);
                    break;
                case TypeCode.Double:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref sourceArray[sourceArrayOffset]), ref Unsafe.As<ELEMENT_T, Double>(ref destinationArray[destinationArrayOffset]), count);
                    break;
                case TypeCode.Decimal:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref sourceArray[sourceArrayOffset]), ref Unsafe.As<ELEMENT_T, Decimal>(ref destinationArray[destinationArrayOffset]), count);
                    break;
                default:
                    InternalCopyMemoryManaged(sourceArray, ref sourceArrayOffset, destinationArray, ref destinationArrayOffset, count);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void InternalCopyMemory<ELEMENT_T>(ReadOnlySpan<ELEMENT_T> sourceArray, Span<ELEMENT_T> destinationArray)
        {
            switch (Type.GetTypeCode(typeof(ELEMENT_T)))
            {
                case TypeCode.Boolean:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref Unsafe.AsRef(in sourceArray.GetPinnableReference())), ref Unsafe.As<ELEMENT_T, Boolean>(ref destinationArray.GetPinnableReference()), sourceArray.Length);
                    break;
                case TypeCode.Char:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref Unsafe.AsRef(in sourceArray.GetPinnableReference())), ref Unsafe.As<ELEMENT_T, Char>(ref destinationArray.GetPinnableReference()), sourceArray.Length);
                    break;
                case TypeCode.SByte:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref Unsafe.AsRef(in sourceArray.GetPinnableReference())), ref Unsafe.As<ELEMENT_T, SByte>(ref destinationArray.GetPinnableReference()), sourceArray.Length);
                    break;
                case TypeCode.Byte:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref Unsafe.AsRef(in sourceArray.GetPinnableReference())), ref Unsafe.As<ELEMENT_T, Byte>(ref destinationArray.GetPinnableReference()), sourceArray.Length);
                    break;
                case TypeCode.Int16:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref Unsafe.AsRef(in sourceArray.GetPinnableReference())), ref Unsafe.As<ELEMENT_T, Int16>(ref destinationArray.GetPinnableReference()), sourceArray.Length);
                    break;
                case TypeCode.UInt16:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref Unsafe.AsRef(in sourceArray.GetPinnableReference())), ref Unsafe.As<ELEMENT_T, UInt16>(ref destinationArray.GetPinnableReference()), sourceArray.Length);
                    break;
                case TypeCode.Int32:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref Unsafe.AsRef(in sourceArray.GetPinnableReference())), ref Unsafe.As<ELEMENT_T, Int32>(ref destinationArray.GetPinnableReference()), sourceArray.Length);
                    break;
                case TypeCode.UInt32:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref Unsafe.AsRef(in sourceArray.GetPinnableReference())), ref Unsafe.As<ELEMENT_T, UInt32>(ref destinationArray.GetPinnableReference()), sourceArray.Length);
                    break;
                case TypeCode.Int64:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref Unsafe.AsRef(in sourceArray.GetPinnableReference())), ref Unsafe.As<ELEMENT_T, Int64>(ref destinationArray.GetPinnableReference()), sourceArray.Length);
                    break;
                case TypeCode.UInt64:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref Unsafe.AsRef(in sourceArray.GetPinnableReference())), ref Unsafe.As<ELEMENT_T, UInt64>(ref destinationArray.GetPinnableReference()), sourceArray.Length);
                    break;
                case TypeCode.Single:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref Unsafe.AsRef(in sourceArray.GetPinnableReference())), ref Unsafe.As<ELEMENT_T, Single>(ref destinationArray.GetPinnableReference()), sourceArray.Length);
                    break;
                case TypeCode.Double:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref Unsafe.AsRef(in sourceArray.GetPinnableReference())), ref Unsafe.As<ELEMENT_T, Double>(ref destinationArray.GetPinnableReference()), sourceArray.Length);
                    break;
                case TypeCode.Decimal:
                    InternalCopyMemoryUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref Unsafe.AsRef(in sourceArray.GetPinnableReference())), ref Unsafe.As<ELEMENT_T, Decimal>(ref destinationArray.GetPinnableReference()), sourceArray.Length);
                    break;
                default:
                    InternalCopyMemoryManaged(sourceArray, destinationArray);
                    break;
            }
        }

        #endregion

        #region InternalCopyMemoryManaged

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyMemoryManaged<ELEMENT_T>(ELEMENT_T[] sourceArray, ref Int32 sourceArrayOffset, ELEMENT_T[] destinationArray, ref Int32 destinationArrayOffset, Int32 count)
        {
            while (count-- > 0)
                destinationArray[destinationArrayOffset++] = sourceArray[sourceArrayOffset++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalCopyMemoryManaged<ELEMENT_T>(ReadOnlySpan<ELEMENT_T> sourceArray, Span<ELEMENT_T> destinationArray)
        {
            var count = sourceArray.Length;
            var index = 0;
            while (count-- > 0)
            {
                destinationArray[index] = sourceArray[index];
                ++index;
            }
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void InternalCopyMemoryUnmanaged<ELEMENT_T>(ref ELEMENT_T sourceArray, ref ELEMENT_T destinationArray, Int32 count)
            where ELEMENT_T : unmanaged
        {
            if (count <= 0)
                return;

            fixed (ELEMENT_T* sourcePointer = &sourceArray)
            fixed (ELEMENT_T* destinationPointer = &destinationArray)
            {
                if (sourcePointer == destinationPointer)
                    return;

                //
                // Either 'Unsafe.CopyBlock' or 'Unsafe.CopyBlockUnaligned' MUST NOT be called if the sourceArray and destinationArray overlap.
                //
                // The 'cpblk' instruction is used in 'Unsafe.CopyBlock' and 'Unsafe.CopyBlockUnaligned'.
                // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Runtime.CompilerServices.Unsafe/src/System.Runtime.CompilerServices.Unsafe.il
                //
                // The behavior of cpblk is unspecified if the source and destination areas overlap.
                // https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.cpblk?view=net-6.0
                //

                if (destinationPointer + count <= sourcePointer || destinationPointer >= sourcePointer + count)
                {
                    // When sourceArray and destinationArray do not overlap

                    // Here I can safely call 'Unsafe.CopyBlock' or 'Unsafe.CopyBlock'.
                    var aligned = (sizeof(ELEMENT_T) & _alignmentMask) == 0;
                    if (aligned)
                        Unsafe.CopyBlock(destinationPointer, sourcePointer, (UInt32)(count * sizeof(ELEMENT_T) / sizeof(Byte)));
                    else
                        Unsafe.CopyBlockUnaligned(destinationPointer, sourcePointer, (UInt32)(count * sizeof(ELEMENT_T) / sizeof(Byte)));
                }
                else if (count * sizeof(ELEMENT_T) / sizeof(Byte) < _THRESHOLD_COPY_MEMORY_BY_LONG_POINTER)
                {
                    // Since byteCount is small enough, copy every byte.
                    InternalCopyMemoryUnmanagedByByte((Byte*)sourcePointer, (Byte*)destinationPointer, count * sizeof(ELEMENT_T) / sizeof(Byte));
                }
                else if (destinationPointer <= sourcePointer || (Byte*)destinationPointer >= (Byte*)sourcePointer + _alignment)
                {
                    // When sourceArray and destinationArray overlap, but destinationPointer <= sourcePointer or (Byte*)destinationPointer >= (Byte*)sourcePointer + _alignment

                    // Since no undesired overwrite occurs here, copy each UInt64 or UInt32.
                    if (_is64bitProcess)
                    {
                        var byteCount = sizeof(ELEMENT_T) * count;
                        if ((sizeof(ELEMENT_T) & (1 << 0)) != 0 || (byteCount & (1 << 0)) != 0 || ((Int32)sourcePointer & (1 << 0)) != 0 || ((Int32)destinationPointer & (1 << 0)) != 0)
                            InternalCopyMemoryUnmanagedByUInt64((Byte*)sourcePointer, (Byte*)destinationPointer, count * sizeof(ELEMENT_T) / sizeof(Byte));
                        else if ((sizeof(ELEMENT_T) & (1 << 1)) != 0 || (byteCount & (1 << 1)) != 0 || ((Int32)sourcePointer & (1 << 1)) != 0 || ((Int32)destinationPointer & (1 << 1)) != 0)
                            InternalCopyMemoryUnmanagedByUInt64((UInt16*)sourcePointer, (UInt16*)destinationPointer, count * sizeof(ELEMENT_T) / sizeof(UInt16));
                        else if ((sizeof(ELEMENT_T) & (1 << 2)) != 0 || (byteCount & (1 << 2)) != 0 || ((Int32)sourcePointer & (1 << 2)) != 0 || ((Int32)destinationPointer & (1 << 2)) != 0)
                            InternalCopyMemoryUnmanagedByUInt64((UInt32*)sourcePointer, (UInt32*)destinationPointer, count * sizeof(ELEMENT_T) / sizeof(UInt32));
                        else
                            InternalCopyMemoryUnmanagedByUInt64((UInt64*)sourcePointer, (UInt64*)destinationPointer, count * sizeof(ELEMENT_T) / sizeof(UInt64));
                    }
                    else
                    {
                        var byteCount = sizeof(ELEMENT_T) * count;
                        if ((sizeof(ELEMENT_T) & (1 << 0)) != 0 || (byteCount & (1 << 0)) != 0 || ((Int32)sourcePointer & (1 << 0)) != 0 || ((Int32)destinationPointer & (1 << 0)) != 0)
                            InternalCopyMemoryUnmanagedByUInt32((Byte*)sourcePointer, (Byte*)destinationPointer, count * sizeof(ELEMENT_T) / sizeof(Byte));
                        else if ((sizeof(ELEMENT_T) & (1 << 1)) != 0 || (byteCount & (1 << 1)) != 0 || ((Int32)sourcePointer & (1 << 1)) != 0 || ((Int32)destinationPointer & (1 << 1)) != 0)
                            InternalCopyMemoryUnmanagedByUInt32((UInt16*)sourcePointer, (UInt16*)destinationPointer, count * sizeof(ELEMENT_T) / sizeof(UInt16));
                        else
                            InternalCopyMemoryUnmanagedByUInt32((UInt32*)sourcePointer, (UInt32*)destinationPointer, count * sizeof(ELEMENT_T) / sizeof(UInt32));
                    }
                }
                else
                {
                    // When (Byte*)sourcePointer < (Byte*)destinationPointer < (Byte*)sourcePointer + (sizeof(UInt64) or sizeof(Uint32))

                    // Undesirable overwrites may occur here when copying memory.
                    var difference = (Int32)((Byte*)destinationPointer - (Byte*)sourcePointer);
                    if (difference >= sizeof(UInt32) && (((sizeof(ELEMENT_T)) & (sizeof(UInt32) - 1)) == 0 || ((count * sizeof(ELEMENT_T)) & (sizeof(UInt32) - 1)) == 0))
                    {
                        // Since undesired overwriting does not occur here, copy it for each UInt32.
                        InternalCopyMemoryUnmanagedByUInt32((UInt32*)sourcePointer, (UInt32*)destinationPointer, count * sizeof(ELEMENT_T) / sizeof(UInt32));
                    }
                    else if (difference >= sizeof(UInt16) && (((sizeof(ELEMENT_T)) & (sizeof(UInt16) - 1)) == 0 || ((count * sizeof(ELEMENT_T)) & (sizeof(UInt16) - 1)) == 0))
                    {
                        // Since undesired overwriting does not occur here, copy it for each UInt16.
                        InternalCopyMemoryUnmanagedByUInt16((UInt16*)sourcePointer, (UInt16*)destinationPointer, count * sizeof(ELEMENT_T) / sizeof(UInt16));
                    }
                    else
                    {
                        // Here, copying every UInt64 or UInt32 causes an unfavorable overwrite, so copy every byte.
                        InternalCopyMemoryUnmanagedByByte((Byte*)sourcePointer, (Byte*)destinationPointer, count * sizeof(ELEMENT_T) / sizeof(Byte));
                    }
                }
            }
        }

        #region InternalCopyMemoryUnmanagedByUInt64

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe void InternalCopyMemoryUnmanagedByUInt64(UInt64* sourcePointer, UInt64* destinationPointer, Int32 count)
        {
            if (((Int32)sourcePointer & (sizeof(UInt64) - 1)) != 0 || ((Int32)destinationPointer & (sizeof(UInt64) - 1)) != 0)
            {
                // If the sourcePointer or destinationPointer alignment is incorrect (usually not possible)
                InternalCopyMemoryUnmanagedByUInt64((UInt32*)sourcePointer, (UInt32*)destinationPointer, count * sizeof(UInt64) / sizeof(UInt32));
            }
            else
            {
                while (count >= 8)
                {
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    count -= 8;
                }

                if ((count & (1 << 2)) != 0)
                {
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
#if DEBUG
                    count &= ~(1 << 2);
#endif
                }

                if ((count & (1 << 1)) != 0)
                {
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
#if DEBUG
                    count &= ~(1 << 1);
#endif
                }

                if ((count & (1 << 0)) != 0)
                {
                    *destinationPointer++ = *sourcePointer++;
#if DEBUG
                    count &= ~(1 << 0);
#endif
                }
#if DEBUG
                Assert(count == 0);
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe void InternalCopyMemoryUnmanagedByUInt64(UInt32* sourcePointer, UInt32* destinationPointer, Int32 count)
        {
            if (((Int32)sourcePointer & (sizeof(UInt32) - 1)) != 0 || ((Int32)destinationPointer & (sizeof(UInt32) - 1)) != 0)
            {
                // If the sourcePointer or destinationPointer alignment is incorrect (usually not possible)
                InternalCopyMemoryUnmanagedByUInt64((UInt16*)sourcePointer, (UInt16*)destinationPointer, count * sizeof(UInt32) / sizeof(UInt16));
            }
            else
            {
                switch (((-(Int32)destinationPointer & (sizeof(UInt64) - 1)) / sizeof(UInt32)).Minimum(count))
                {
                    case 1:
                        *destinationPointer++ = *sourcePointer++;
                        --count;
                        break;
                    default:
                        break;
                }
#if DEBUG
                Assert((Int32)destinationPointer % sizeof(UInt64) == 0 || count == 0);
#endif
                {
                    var longSourcePointer = (UInt64*)sourcePointer;
                    var longDestinationPointer = (UInt64*)destinationPointer;
                    while (count >= 8 * sizeof(UInt64) / sizeof(UInt32))
                    {
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        count -= 8 * sizeof(UInt64) / sizeof(UInt32);
                    }

                    if ((count & (1 << 3)) != 0)
                    {
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                        count &= ~(1 << 3);
#endif
                    }

                    if ((count & (1 << 2)) != 0)
                    {
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                        count &= ~(1 << 2);
#endif
                    }

                    if ((count & (1 << 1)) != 0)
                    {
                        *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                        count &= ~(1 << 1);
#endif
                    }

                    sourcePointer = (UInt32*)longSourcePointer;
                    destinationPointer = (UInt32*)longDestinationPointer;
                }

                if ((count & (1 << 0)) != 0)
                {
                    *destinationPointer++ = *sourcePointer++;
#if DEBUG
                    count &= ~(1 << 0);
#endif
                }
#if DEBUG
                Assert(count == 0);
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe void InternalCopyMemoryUnmanagedByUInt64(UInt16* sourcePointer, UInt16* destinationPointer, Int32 count)
        {
            if (((Int32)sourcePointer & (sizeof(UInt16) - 1)) != 0 || ((Int32)destinationPointer & (sizeof(UInt16) - 1)) != 0)
            {
                // If the sourcePointer or destinationPointer alignment is incorrect (usually not possible)
                InternalCopyMemoryUnmanagedByUInt64((Byte*)sourcePointer, (Byte*)destinationPointer, count * sizeof(UInt16) / sizeof(Byte));
            }
            else
            {
                switch (((-(Int32)destinationPointer & (sizeof(UInt64) - 1)) / sizeof(UInt16)).Minimum(count))
                {
                    case 1:
                        *destinationPointer++ = *sourcePointer++;
                        --count;
                        break;
                    case 2:
                        *(UInt32*)destinationPointer = *(UInt32*)sourcePointer;
                        sourcePointer += sizeof(UInt32) / sizeof(UInt16);
                        destinationPointer += sizeof(UInt32) / sizeof(UInt16);
                        count -= 2;
                        break;
                    case 3:
                        *destinationPointer++ = *sourcePointer++;
                        *(UInt32*)destinationPointer = *(UInt32*)sourcePointer;
                        sourcePointer += sizeof(UInt32) / sizeof(UInt16);
                        destinationPointer += sizeof(UInt32) / sizeof(UInt16);
                        count -= 3;
                        break;
                    default:
                        break;
                }
#if DEBUG
                Assert((Int32)destinationPointer % sizeof(UInt64) == 0 || count == 0);
#endif
                {
                    var longSourcePointer = (UInt64*)sourcePointer;
                    var longDestinationPointer = (UInt64*)destinationPointer;
                    while (count >= 8 * sizeof(UInt64) / sizeof(UInt16))
                    {
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        count -= 8 * sizeof(UInt64) / sizeof(UInt16);
                    }

                    if ((count & (1 << 4)) != 0)
                    {
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                        count &= ~(1 << 4);
#endif
                    }

                    if ((count & (1 << 3)) != 0)
                    {
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                        count &= ~(1 << 3);
#endif
                    }

                    if ((count & (1 << 2)) != 0)
                    {
                        *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                        count &= ~(1 << 2);
#endif
                    }

                    sourcePointer = (UInt16*)longSourcePointer;
                    destinationPointer = (UInt16*)longDestinationPointer;
                }

                if ((count & (1 << 1)) != 0)
                {
                    *(UInt32*)destinationPointer = *(UInt32*)sourcePointer;
                    destinationPointer += 1 << 1;
                    sourcePointer += 1 << 1;
#if DEBUG
                    count &= ~(1 << 1);
#endif
                }

                if ((count & (1 << 0)) != 0)
                {
                    *destinationPointer++ = *sourcePointer++;
#if DEBUG
                    count &= ~(1 << 0);
#endif
                }
#if DEBUG
                Assert(count == 0);
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe void InternalCopyMemoryUnmanagedByUInt64(Byte* sourcePointer, Byte* destinationPointer, Int32 count)
        {
            switch (((-(Int32)destinationPointer & (sizeof(UInt64) - 1)) / sizeof(Byte)).Minimum(count))
            {
                case 1:
                    *destinationPointer++ = *sourcePointer++;
                    --count;
                    break;
                case 2:
                    *(UInt16*)destinationPointer = *(UInt16*)sourcePointer;
                    sourcePointer += sizeof(UInt16) / sizeof(Byte);
                    destinationPointer += sizeof(UInt16) / sizeof(Byte);
                    count -= 2;
                    break;
                case 3:
                    *destinationPointer++ = *sourcePointer++;
                    *(UInt16*)destinationPointer = *(UInt16*)sourcePointer;
                    sourcePointer += sizeof(UInt16) / sizeof(Byte);
                    destinationPointer += sizeof(UInt16) / sizeof(Byte);
                    count -= 3;
                    break;
                case 4:
                    *(UInt32*)destinationPointer = *(UInt32*)sourcePointer;
                    sourcePointer += sizeof(UInt32) / sizeof(Byte);
                    destinationPointer += sizeof(UInt32) / sizeof(Byte);
                    count -= 4;
                    break;
                case 5:
                    *destinationPointer++ = *sourcePointer++;
                    *(UInt32*)destinationPointer = *(UInt32*)sourcePointer;
                    sourcePointer += sizeof(UInt32) / sizeof(Byte);
                    destinationPointer += sizeof(UInt32) / sizeof(Byte);
                    count -= 5;
                    break;
                case 6:
                    *(UInt16*)destinationPointer = *(UInt16*)sourcePointer;
                    sourcePointer += sizeof(UInt16) / sizeof(Byte);
                    destinationPointer += sizeof(UInt16) / sizeof(Byte);
                    *(UInt32*)destinationPointer = *(UInt32*)sourcePointer;
                    sourcePointer += sizeof(UInt32) / sizeof(Byte);
                    destinationPointer += sizeof(UInt32) / sizeof(Byte);
                    count -= 6;
                    break;
                case 7:
                    *destinationPointer++ = *sourcePointer++;
                    *(UInt16*)destinationPointer = *(UInt16*)sourcePointer;
                    sourcePointer += sizeof(UInt16) / sizeof(Byte);
                    destinationPointer += sizeof(UInt16) / sizeof(Byte);
                    *(UInt32*)destinationPointer = *(UInt32*)sourcePointer;
                    sourcePointer += sizeof(UInt32) / sizeof(Byte);
                    destinationPointer += sizeof(UInt32) / sizeof(Byte);
                    count -= 7;
                    break;
                default:
                    break;
            }
#if DEBUG
            Assert((Int32)destinationPointer % sizeof(UInt64) == 0 || count == 0);
#endif
            {
                var longSourcePointer = (UInt64*)sourcePointer;
                var longDestinationPointer = (UInt64*)destinationPointer;
                while (count >= 8 * sizeof(UInt64) / sizeof(Byte))
                {
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    count -= 8 * sizeof(UInt64) / sizeof(Byte);
                }

                if ((count & (1 << 5)) != 0)
                {
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                    count &= ~(1 << 5);
#endif
                }

                if ((count & (1 << 4)) != 0)
                {
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                    count &= ~(1 << 4);
#endif
                }

                if ((count & (1 << 3)) != 0)
                {
                    *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                    count &= ~(1 << 3);
#endif
                }

                sourcePointer = (Byte*)longSourcePointer;
                destinationPointer = (Byte*)longDestinationPointer;
            }

            if ((count & (1 << 2)) != 0)
            {
                *(UInt32*)destinationPointer = *(UInt32*)sourcePointer;
                destinationPointer += 1 << 2;
                sourcePointer += 1 << 2;
#if DEBUG
                count &= ~(1 << 2);
#endif
            }

            if ((count & (1 << 1)) != 0)
            {
                *(UInt16*)destinationPointer = *(UInt16*)sourcePointer;
                destinationPointer += 1 << 1;
                sourcePointer += 1 << 1;
#if DEBUG
                count &= ~(1 << 1);
#endif
            }

            if ((count & (1 << 0)) != 0)
            {
                *destinationPointer++ = *sourcePointer++;
#if DEBUG
                count &= ~(1 << 0);
#endif
            }
#if DEBUG
            Assert(count == 0);
#endif
        }

        #endregion

        #region InternalCopyMemoryUnmanagedByUInt32

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe void InternalCopyMemoryUnmanagedByUInt32(UInt32* sourcePointer, UInt32* destinationPointer, Int32 count)
        {
            if (((Int32)sourcePointer & (sizeof(UInt32) - 1)) != 0 || ((Int32)destinationPointer & (sizeof(UInt32) - 1)) != 0)
            {
                // If the sourcePointer or destinationPointer alignment is incorrect (usually not possible)
                InternalCopyMemoryUnmanagedByUInt32((UInt16*)sourcePointer, (UInt16*)destinationPointer, count * sizeof(UInt32) / sizeof(UInt16));
            }
            else
            {
                while (count >= 8)
                {
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    count -= 8;
                }

                if ((count & (1 << 2)) != 0)
                {
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
#if DEBUG
                    count &= ~(1 << 2);
#endif
                }

                if ((count & (1 << 1)) != 0)
                {
                    *destinationPointer++ = *sourcePointer++;
                    *destinationPointer++ = *sourcePointer++;
#if DEBUG
                    count &= ~(1 << 1);
#endif
                }

                if ((count & (1 << 0)) != 0)
                {
                    *destinationPointer++ = *sourcePointer++;
#if DEBUG
                    count &= ~(1 << 0);
#endif
                }
#if DEBUG
                Assert(count == 0);
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe void InternalCopyMemoryUnmanagedByUInt32(UInt16* sourcePointer, UInt16* destinationPointer, Int32 count)
        {
            if (((Int32)sourcePointer & (sizeof(UInt16) - 1)) != 0 || ((Int32)destinationPointer & (sizeof(UInt16) - 1)) != 0)
            {
                // If the sourcePointer or destinationPointer alignment is incorrect (usually not possible)
                InternalCopyMemoryUnmanagedByUInt32((Byte*)sourcePointer, (Byte*)destinationPointer, count * sizeof(UInt16) / sizeof(Byte));
            }
            else
            {
                switch (((-(Int32)destinationPointer & (sizeof(UInt32) - 1)) / sizeof(UInt16)).Minimum(count))
                {
                    case 1:
                        *destinationPointer++ = *sourcePointer++;
                        --count;
                        break;
                    default:
                        break;
                }
#if DEBUG
                Assert((Int32)destinationPointer % sizeof(UInt32) == 0 || count == 0);
#endif
                {
                    var longSourcePointer = (UInt32*)sourcePointer;
                    var longDestinationPointer = (UInt32*)destinationPointer;
                    while (count >= 8 * sizeof(UInt32) / sizeof(UInt16))
                    {
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        count -= 8 * sizeof(UInt32) / sizeof(UInt16);
                    }

                    if ((count & (1 << 3)) != 0)
                    {
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                        count &= ~(1 << 3);
#endif
                    }

                    if ((count & (1 << 2)) != 0)
                    {
                        *longDestinationPointer++ = *longSourcePointer++;
                        *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                        count &= ~(1 << 2);
#endif
                    }

                    if ((count & (1 << 1)) != 0)
                    {
                        *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                        count &= ~(1 << 1);
#endif
                    }

                    sourcePointer = (UInt16*)longSourcePointer;
                    destinationPointer = (UInt16*)longDestinationPointer;
                }

                if ((count & (1 << 0)) != 0)
                {
                    *destinationPointer++ = *sourcePointer++;
#if DEBUG
                    count &= ~(1 << 0);
#endif
                }
#if DEBUG
                Assert(count == 0);
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe void InternalCopyMemoryUnmanagedByUInt32(Byte* sourcePointer, Byte* destinationPointer, Int32 byteCount)
        {
            switch (((-(Int32)destinationPointer & (sizeof(UInt32) - 1)) / sizeof(Byte)).Minimum(byteCount))
            {
                case 1:
                    *destinationPointer++ = *sourcePointer++;
                    byteCount -= 1;
                    break;
                case 2:
                    *(UInt16*)destinationPointer = *(UInt16*)sourcePointer;
                    sourcePointer += sizeof(UInt16) / sizeof(Byte);
                    destinationPointer += sizeof(UInt16) / sizeof(Byte);
                    byteCount -= 2;
                    break;
                case 3:
                    *destinationPointer++ = *sourcePointer++;
                    *(UInt16*)destinationPointer = *(UInt16*)sourcePointer;
                    sourcePointer += sizeof(UInt16) / sizeof(Byte);
                    destinationPointer += sizeof(UInt16) / sizeof(Byte);
                    byteCount -= 3;
                    break;
                default:
                    break;
            }
#if DEBUG
            Assert((Int32)destinationPointer % sizeof(UInt32) == 0 || byteCount == 0);
#endif
            {
                var longSourcePointer = (UInt32*)sourcePointer;
                var longDestinationPointer = (UInt32*)destinationPointer;
                while (byteCount >= 8 * sizeof(UInt32) / sizeof(Byte))
                {
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    byteCount -= 8 * sizeof(UInt32) / sizeof(Byte);
                }

                if ((byteCount & (1 << 4)) != 0)
                {
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                    byteCount &= ~(1 << 4);
#endif
                }

                if ((byteCount & (1 << 3)) != 0)
                {
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                    byteCount &= ~(1 << 3);
#endif
                }

                if ((byteCount & (1 << 2)) != 0)
                {
                    *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                    byteCount &= ~(1 << 2);
#endif
                }

                sourcePointer = (Byte*)longSourcePointer;
                destinationPointer = (Byte*)longDestinationPointer;
            }

            if ((byteCount & (1 << 1)) != 0)
            {
                *(UInt16*)destinationPointer = *(UInt16*)sourcePointer;
                destinationPointer += sizeof(UInt16);
                sourcePointer += sizeof(UInt16);
#if DEBUG
                byteCount &= ~(1 << 1);
#endif
            }

            if ((byteCount & (1 << 0)) != 0)
            {
                *destinationPointer++ = *sourcePointer++;
#if DEBUG
                byteCount &= ~(1 << 0);
#endif
            }
#if DEBUG
            Assert(byteCount == 0);
#endif
        }

        #endregion

        #region InternalCopyMemoryUnmanagedByUInt16

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe void InternalCopyMemoryUnmanagedByUInt16(UInt16* sourcePointer, UInt16* destinationPointer, Int32 count)
        {
            while (count >= 8)
            {
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                count -= 8;
            }

            if ((count & (1 << 2)) != 0)
            {
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
#if DEBUG
                count &= ~(1 << 2);
#endif
            }

            if ((count & (1 << 1)) != 0)
            {
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
#if DEBUG
                count &= ~(1 << 1);
#endif
            }

            if ((count & (1 << 0)) != 0)
            {
                *destinationPointer++ = *sourcePointer++;
#if DEBUG
                count &= ~(1 << 0);
#endif
            }
#if DEBUG
            Assert(count == 0);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe void InternalCopyMemoryUnmanagedByUInt16(Byte* sourcePointer, Byte* destinationPointer, Int32 count)
        {
            switch ((((Int32)destinationPointer & (sizeof(UInt16) - 1)) / sizeof(Byte)).Minimum(count))
            {
                case 1:
                    *destinationPointer++ = *sourcePointer++;
                    --count;
                    break;
                default:
                    break;
            }
#if DEBUG
            Assert((Int32)destinationPointer % sizeof(UInt16) == 0 || count == 0);
#endif
            {
                var longSourcePointer = (UInt16*)sourcePointer;
                var longDestinationPointer = (UInt16*)destinationPointer;
                while (count >= 8 * sizeof(UInt16) / sizeof(Byte))
                {
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    count -= 8 * sizeof(UInt16) / sizeof(Byte);
                }

                if ((count & (1 << 3)) != 0)
                {
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                    count &= ~(1 << 3);
#endif
                }

                if ((count & (1 << 2)) != 0)
                {
                    *longDestinationPointer++ = *longSourcePointer++;
                    *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                    count &= ~(1 << 2);
#endif
                }

                if ((count & (1 << 1)) != 0)
                {
                    *longDestinationPointer++ = *longSourcePointer++;
#if DEBUG
                    count &= ~(1 << 1);
#endif
                }

                sourcePointer = (Byte*)longSourcePointer;
                destinationPointer = (Byte*)longDestinationPointer;
            }

            if ((count & (1 << 0)) != 0)
            {
                *destinationPointer++ = *sourcePointer++;
#if DEBUG
                count &= ~(1 << 0);
#endif
            }
#if DEBUG
            Assert(count == 0);
#endif
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static unsafe void InternalCopyMemoryUnmanagedByByte(Byte* sourcePointer, Byte* destinationPointer, Int32 count)
        {
            while (count >= 8)
            {
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                count -= 8;
            }

            if ((count & (1 << 2)) != 0)
            {
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
#if DEBUG
                count &= ~(1 << 2);
#endif
            }

            if ((count & (1 << 1)) != 0)
            {
                *destinationPointer++ = *sourcePointer++;
                *destinationPointer++ = *sourcePointer++;
#if DEBUG
                count &= ~(1 << 1);
#endif
            }

            if ((count & (1 << 0)) != 0)
            {
                *destinationPointer++ = *sourcePointer++;
#if DEBUG
                count &= ~(1 << 0);
#endif
            }
#if DEBUG
            Assert(count == 0);
#endif
        }

        #region InternalReverseArray

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static void InternalReverseArray<ELEMENT_T>(this ELEMENT_T[] source, Int32 offset, Int32 count)
        {
            switch (Type.GetTypeCode(typeof(ELEMENT_T)))
            {
                case TypeCode.Boolean:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref source[offset]), count);
                    break;
                case TypeCode.Char:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref source[offset]), count);
                    break;
                case TypeCode.SByte:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref source[offset]), count);
                    break;
                case TypeCode.Byte:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref source[offset]), count);
                    break;
                case TypeCode.Int16:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref source[offset]), count);
                    break;
                case TypeCode.UInt16:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref source[offset]), count);
                    break;
                case TypeCode.Int32:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref source[offset]), count);
                    break;
                case TypeCode.UInt32:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref source[offset]), count);
                    break;
                case TypeCode.Int64:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref source[offset]), count);
                    break;
                case TypeCode.UInt64:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref source[offset]), count);
                    break;
                case TypeCode.Single:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref source[offset]), count);
                    break;
                case TypeCode.Double:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref source[offset]), count);
                    break;
                case TypeCode.Decimal:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref source[offset]), count);
                    break;
                default:
                    InternalReverseArrayManaged(source, offset, count);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void InternalReverseArray<ELEMENT_T>(Span<ELEMENT_T> source)
        {
            switch (Type.GetTypeCode(typeof(ELEMENT_T)))
            {
                case TypeCode.Boolean:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Boolean>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.Char:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Char>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.SByte:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, SByte>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.Byte:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Byte>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.Int16:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Int16>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.UInt16:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, UInt16>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.Int32:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Int32>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.UInt32:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, UInt32>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.Int64:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Int64>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.UInt64:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, UInt64>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.Single:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Single>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.Double:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Double>(ref source.GetPinnableReference()), source.Length);
                    break;
                case TypeCode.Decimal:
                    InternalReverseArrayUnmanaged(ref Unsafe.As<ELEMENT_T, Decimal>(ref source.GetPinnableReference()), source.Length);
                    break;
                default:
                    InternalReverseArrayManaged(source);
                    break;
            }
        }

        #endregion

        #region InternalReverseArrayManaged

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalReverseArrayManaged<ELEMENT_T>(ELEMENT_T[] source, Int32 offset, Int32 count)
        {
            var index1 = offset;
            var index2 = offset + count - 1;
            while (index2 > index1)
            {
                (source[index2], source[index1]) = (source[index1], source[index2]);
                ++index1;
                --index2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalReverseArrayManaged<ELEMENT_T>(Span<ELEMENT_T> source)
        {
            var index1 = 0;
            var index2 = source.Length - 1;
            while (index2 > index1)
            {
                (source[index2], source[index1]) = (source[index1], source[index2]);
                ++index1;
                --index2;
            }
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void InternalReverseArrayUnmanaged<ELEMENT_T>(ref ELEMENT_T source, Int32 count)
            where ELEMENT_T : unmanaged
        {
            // サイズが 0 の配列が渡されると source は null 参照になるので、count の場合は source を参照してはならない。
            if (count <= 1)
                return;

            fixed (ELEMENT_T* buffer = &source)
            {
                var pointer1 = buffer;
                var pointer2 = buffer + count - 1;
                while (pointer2 > pointer1)
                {
                    (*pointer1, *pointer2) = (*pointer2, *pointer1);
                    ++pointer1;
                    --pointer2;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Boolean DefaultEqual<ELEMENT_T>([AllowNull] ELEMENT_T key1, [AllowNull] ELEMENT_T key2)
            where ELEMENT_T : IEquatable<ELEMENT_T>
            => key1 is null
                ? key2 is null
                : key1.Equals(key2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int32 DefaultCompare<ELEMENT_T>([AllowNull] ELEMENT_T key1, [AllowNull] ELEMENT_T key2)
            where ELEMENT_T : IComparable<ELEMENT_T>
            => key1 is not null
                ? key1.CompareTo(key2)
                : key2 is null
                ? 0
                : -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Assert(Boolean condition)
        {
#if DEBUG
            if (!condition)
                throw new Exception();
#endif
        }
    }
}
