using System;
using System.Collections;
using System.Collections.Generic;

namespace Utility
{
    /// <summary>
    /// 符号なし 32ビット整数を添え字として使用する配列のクラスです。
    /// </summary>
    /// <typeparam name="ELEMENT_T">
    /// 配列の要素の型です。
    /// </typeparam>
    public class BigArray<ELEMENT_T>
        : IIndexer<UInt32, ELEMENT_T>, IReadOnlyIndexer<UInt32, ELEMENT_T>, IEnumerable<ELEMENT_T>
    {
        const Int32 _SUB_ATTAY_SIZE = (Int32)((Int32.MaxValue + 1UL) / 2);
        const Int32 _SUB_ARRAY_SIZE_BITS = 30;
        const UInt32 _SUB_ARRAY_SIZE_MASK = (1U << 30) - 1;
        private ELEMENT_T[][] _array;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="length">
        /// 配列の長さを示す <see cref="UInt32"/> 値です。既定値は 0 です。
        /// </param>
        public BigArray(UInt32 length = 0)
        {
#if DEBUG
            const UInt32 _TEST_VALUE = 0x12345678U;
            if (_TEST_VALUE / _SUB_ATTAY_SIZE != (_TEST_VALUE >> _SUB_ARRAY_SIZE_BITS))
                throw new Exception();
            if (_TEST_VALUE % _SUB_ATTAY_SIZE != (_TEST_VALUE & _SUB_ARRAY_SIZE_MASK))
                throw new Exception();
#endif
            Length = length;
            _array = new ELEMENT_T[(Int32)(((UInt64)length + _SUB_ATTAY_SIZE - 1) >> _SUB_ARRAY_SIZE_BITS)][];
            for (var index = 0; index < _array.Length; ++index)
            {
                var subArray = new ELEMENT_T[(Int32)length.Minimum((UInt32)_SUB_ATTAY_SIZE)];
                _array[index] = subArray;
                length -= (UInt32)subArray.Length;
            }
        }

        private BigArray(ELEMENT_T[] array)
        {
            _array = new[] { array };
        }

        /// <summary>
        /// 配列の要素を取得または設定します。
        /// </summary>
        /// <param name="index">
        /// 配列の添え字を示す <see cref="UInt32"/> 値です。
        /// </param>
        /// <returns>
        /// 配列の値を示す <typeparamref name="ELEMENT_T"/> オブジェクトです。
        /// </returns>
        public ELEMENT_T this[UInt32 index]
        {
            get => _array[(Int32)(index >> _SUB_ARRAY_SIZE_BITS)][(Int32)(index & _SUB_ARRAY_SIZE_MASK)];
            set => _array[(Int32)(index >> _SUB_ARRAY_SIZE_BITS)][(Int32)(index & _SUB_ARRAY_SIZE_MASK)] = value;
        }

        /// <summary>
        /// 配列の長さを取得します。
        /// </summary>
        /// <value>
        /// 配列の長さを示す <see cref="UInt32"/> 値です。
        /// </value>
        public UInt32 Length { get; private set; }

        /// <summary>
        /// 配列の長さを変更します。
        /// </summary>
        /// <param name="length">
        /// 新たな配列の長さを示す <see cref="UInt32"/> です。
        /// </param>
        public void Resize(UInt32 length)
        {
#if DEBUG
            checked
#endif
            {
                if (length > Length)
                {
                    // 要素数が増える場合
                    if (_array.Length <= 0)
                    {
                        var sizeOfNewArray = (Int32)(((UInt64)length + _SUB_ATTAY_SIZE - 1) >> _SUB_ARRAY_SIZE_BITS);
                        var count = length;
                        if (sizeOfNewArray > 0)
                            Array.Resize(ref _array, sizeOfNewArray);
                        for (var index = 0; count > 0; ++index)
                        {
                            var size = (Int32)count.Minimum((UInt32)_SUB_ATTAY_SIZE);
                            _array[index] = new ELEMENT_T[size];
                            count -= (UInt32)size;
                        }
                    }
                    else
                    {
                        var sizeOfNewArray = (Int32)(((UInt64)length + _SUB_ATTAY_SIZE - 1) >> _SUB_ARRAY_SIZE_BITS);
                        var count = length - (UInt32)((_array.Length - 1) << _SUB_ARRAY_SIZE_BITS);
                        var sizeOfCurrentArray = _array.Length;
                        if (sizeOfNewArray > sizeOfCurrentArray)
                            Array.Resize(ref _array, sizeOfNewArray);
                        for (var index = sizeOfCurrentArray - 1; count > 0; ++index)
                        {
                            var size = (Int32)count.Minimum((UInt32)_SUB_ATTAY_SIZE);
                            if (_array[index] is null)
                                _array[index] = new ELEMENT_T[size];
                            else
                                Array.Resize(ref _array[index], size);
                            count -= (UInt32)size;
                        }
                    }
                }
                else if (length < Length)
                {
                    // 要素数が減る場合
                    if (length <= 0)
                    {
                        Array.Resize(ref _array, 0);
                    }
                    else
                    {
                        var sizeOfNewArray = (Int32)(((UInt64)length + _SUB_ATTAY_SIZE - 1) >> _SUB_ARRAY_SIZE_BITS);
                        var lastSubArraySizeOfNewArray = (Int32)(length - ((UInt32)(sizeOfNewArray - 1) << _SUB_ARRAY_SIZE_BITS));
                        var sizeOfCurrentArray = _array.Length;
                        if (sizeOfNewArray < sizeOfCurrentArray)
                            Array.Resize(ref _array, sizeOfNewArray);
                        if (lastSubArraySizeOfNewArray > 0)
                            Array.Resize(ref _array[^1], lastSubArraySizeOfNewArray);
                    }
                }
                else
                {
                }

                Length = length;
#if DEBUG
                var totalLength = 0UL;
                for (var index = 0; index < _array.Length; ++index)
                {
                    totalLength += (UInt32)_array[index].Length;
                }

                if (totalLength != length)
                    throw new Exception();
#endif
            }
        }

        /// <summary>
        /// この配列の列挙子を取得します。
        /// </summary>
        /// <returns>
        /// この配列の列挙子である <see cref="IEnumerable{T}"/> オブジェクトです。
        /// </returns>
        public IEnumerator<ELEMENT_T> GetEnumerator()
        {
            for (var index1 = 0; index1 < _array.Length; ++index1)
            {
                var subArray = _array[index1];
                for (var index2 = 0; index2 < subArray.Length; ++index2)
                    yield return subArray[index2];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator BigArray<ELEMENT_T>(ELEMENT_T[] array) => new(array);

        public static implicit operator ELEMENT_T[](BigArray<ELEMENT_T> array)
        {
            if (array._array.Length < 1)
                return Array.Empty<ELEMENT_T>();
            else if (array._array.Length == 1)
                return array._array[0];
            else
                throw new OutOfMemoryException();
        }
    }
}
