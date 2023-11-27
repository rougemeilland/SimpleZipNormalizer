using System;

namespace Utility
{
    /// <summary>
    /// バイトストリームから各種の値を読み込むクラスです。
    /// </summary>
    public class ByteArrayReader
    {
        private readonly ReadOnlyMemory<Byte> _sourceArray;
        private Int32 _currentIndex;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="sourceArray">
        /// 読み込み元のバイト列です。
        /// </param>
        public ByteArrayReader(ReadOnlyMemory<Byte> sourceArray)
        {
            _sourceArray = sourceArray;
            _currentIndex = 0;
        }

        /// <summary>
        /// ストリームが空であるかどうかの値を取得します。
        /// </summary>
        /// <value>
        /// 空である場合は true、そうではない場合は false です。
        /// </value>
        public Boolean IsEmpty => _currentIndex >= _sourceArray.Length;

        /// <summary>
        /// 1 バイトだけストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public Byte ReadByte()
        {
            const Int32 valueLength = sizeof(Byte);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Span[_currentIndex];
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// 指定された長さのバイト列をストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public ReadOnlyMemory<Byte> ReadBytes(Int32 length)
        {
            if (_currentIndex + length > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = new Byte[length];
            _sourceArray.Slice(_currentIndex, value.Length).CopyTo(value);
            _currentIndex += length;
            return value;
        }

        /// <summary>
        /// バイト列をストリームから読み込みます。
        /// </summary>
        /// <param name="buffer">
        /// バイト列を読み込むためのバッファーです。
        /// </param>
        /// <remarks>
        /// このメソッドは <paramref name="buffer"/> の長さだけのデータをストリームから読み込みます。
        /// </remarks>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public void ReadBytes(Span<Byte> buffer)
        {
            if (_currentIndex + buffer.Length > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            _sourceArray.Slice(_currentIndex, buffer.Length).Span.CopyTo(buffer);
            _currentIndex += buffer.Length;
        }

        /// <summary>
        /// ストリームに残されたすべてのデータを読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        public ReadOnlyMemory<Byte> ReadAllBytes()
        {
            var value = new Byte[_sourceArray.Length - _currentIndex];
            _sourceArray[_currentIndex..].CopyTo(value);
            _currentIndex = _sourceArray.Length;
            return value;
        }

        /// <summary>
        /// <see cref="Int16"/> の値をリトルエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public Int16 ReadInt16LE()
        {
            const Int32 valueLength = sizeof(Int16);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToInt16LE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="UInt16"/> の値をリトルエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public UInt16 ReadUInt16LE()
        {
            const Int32 valueLength = sizeof(UInt16);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToUInt16LE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="Int32"/> の値をリトルエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public Int32 ReadInt32LE()
        {
            const Int32 valueLength = sizeof(Int32);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToInt32LE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="UInt32"/> の値をリトルエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public UInt32 ReadUInt32LE()
        {
            const Int32 valueLength = sizeof(UInt32);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToUInt32LE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="Int64"/> の値をリトルエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public Int64 ReadInt64LE()
        {
            const Int32 valueLength = sizeof(Int64);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToInt64LE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="UInt64"/> の値をリトルエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public UInt64 ReadUInt64LE()
        {
            const Int32 valueLength = sizeof(UInt64);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToUInt64LE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="Single"/> の値をリトルエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public Single ReadSingleLE()
        {
            const Int32 valueLength = sizeof(Single);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToSingleLE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="Double"/> の値をリトルエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public Double ReadDoubleLE()
        {
            const Int32 valueLength = sizeof(Double);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToDoubleLE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="Decimal"/> の値をリトルエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public Decimal ReadDecimalLE()
        {
            const Int32 valueLength = sizeof(Decimal);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToDecimalLE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="Int16"/> の値をビッグエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public Int16 ReadInt16BE()
        {
            const Int32 valueLength = sizeof(Int16);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToInt16BE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="UInt16"/> の値をビッグエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public UInt16 ReadUInt16BE()
        {
            const Int32 valueLength = sizeof(UInt16);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToUInt16BE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="Int32"/> の値をビッグエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public Int32 ReadInt32BE()
        {
            const Int32 valueLength = sizeof(Int32);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToInt32BE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="UInt32"/> の値をビッグエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public UInt32 ReadUInt32BE()
        {
            const Int32 valueLength = sizeof(UInt32);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToUInt32BE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="Int64"/> の値をビッグエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public Int64 ReadInt64BE()
        {
            const Int32 valueLength = sizeof(Int64);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToInt64BE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="UInt64"/> の値をビッグエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public UInt64 ReadUInt64BE()
        {
            const Int32 valueLength = sizeof(UInt64);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToUInt64BE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="Single"/> の値をビッグエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public Single ReadSingleBE()
        {
            const Int32 valueLength = sizeof(Single);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToSingleBE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="Double"/> の値をビッグエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public Double ReadDoubleBE()
        {
            const Int32 valueLength = sizeof(Double);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToDoubleBE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }

        /// <summary>
        /// <see cref="Decimal"/> の値をビッグエンディアン形式でストリームから読み込みます。
        /// </summary>
        /// <returns>
        /// 読み込んだデータです。
        /// </returns>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// ストリームに残っているデータの長さが不足しています。
        /// </exception>
        public Decimal ReadDecimalBE()
        {
            const Int32 valueLength = sizeof(Decimal);
            if (checked(_currentIndex + valueLength) > _sourceArray.Length)
                throw new UnexpectedEndOfBufferException();
            var value = _sourceArray.Slice(_currentIndex, valueLength).ToDecimalBE();
            checked
            {
                _currentIndex += valueLength;
            }

            return value;
        }
    }
}
