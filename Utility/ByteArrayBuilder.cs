using System;

namespace Utility
{
    /// <summary>
    /// 各種の値からバイト列を構築するためのバッファのクラスです。
    /// </summary>
    public class ByteArrayBuilder
    {
        private readonly Byte[] _destinationArray;
        private Int32 _currentIndex;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="maximumBufferSize">
        /// バッファの最大サイズです。
        /// </param>
        public ByteArrayBuilder(Int32 maximumBufferSize)
        {
            _destinationArray = new Byte[maximumBufferSize];
            _currentIndex = 0;
        }

        /// <summary>
        /// バッファに <see cref="Byte"/> 型の値を追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendByte(Byte value)
        {
            const Int32 valueLength = sizeof(Byte);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueLE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファにバイト列を追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendBytes(ReadOnlySpan<Byte> value)
        {
            if (_currentIndex + value.Length > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            value.CopyTo(_destinationArray.Slice(_currentIndex).Span);
            _currentIndex += value.Length;
        }

        /// <summary>
        /// バッファに <see cref="Int16"/> 型の値をリトルエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendInt16LE(Int16 value)
        {
            const Int32 valueLength = sizeof(Int16);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueLE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="UInt16"/> 型の値をリトルエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendUInt16LE(UInt16 value)
        {
            const Int32 valueLength = sizeof(UInt16);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueLE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="Int32"/> 型の値をリトルエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendInt32LE(Int32 value)
        {
            const Int32 valueLength = sizeof(Int32);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueLE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="UInt32"/> 型の値をリトルエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendUInt32LE(UInt32 value)
        {
            const Int32 valueLength = sizeof(UInt32);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueLE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="Int64"/> 型の値をリトルエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendInt64LE(Int64 value)
        {
            const Int32 valueLength = sizeof(Int64);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueLE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="UInt64"/> 型の値をリトルエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendUInt64LE(UInt64 value)
        {
            const Int32 valueLength = sizeof(UInt64);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueLE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="Single"/> 型の値をリトルエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendSingleLE(Single value)
        {
            const Int32 valueLength = sizeof(Single);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueLE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="Double"/> 型の値をリトルエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendDoubleLE(Double value)
        {
            const Int32 valueLength = sizeof(Double);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueLE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="Decimal"/> 型の値をリトルエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendDecimalLE(Decimal value)
        {
            const Int32 valueLength = sizeof(Decimal);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueLE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="Int16"/> 型の値をビッグエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendInt16BE(Int16 value)
        {
            const Int32 valueLength = sizeof(Int16);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueBE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="UInt16"/> 型の値をビッグエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendUInt16BE(UInt16 value)
        {
            const Int32 valueLength = sizeof(UInt16);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueBE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="Int32"/> 型の値をビッグエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendInt32BE(Int32 value)
        {
            const Int32 valueLength = sizeof(Int32);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueBE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="UInt32"/> 型の値をビッグエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendUInt32BE(UInt32 value)
        {
            const Int32 valueLength = sizeof(UInt32);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueBE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="Int64"/> 型の値をビッグエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendInt64BE(Int64 value)
        {
            const Int32 valueLength = sizeof(Int64);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueBE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="UInt64"/> 型の値をビッグエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendUInt64BE(UInt64 value)
        {
            const Int32 valueLength = sizeof(UInt64);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueBE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="Single"/> 型の値をビッグエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendSingleBE(Single value)
        {
            const Int32 valueLength = sizeof(Single);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueBE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="Double"/> 型の値をビッグエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendDoubleBE(Double value)
        {
            const Int32 valueLength = sizeof(Double);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueBE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファに <see cref="Decimal"/> 型の値をビッグエンディアン形式で追加します。
        /// </summary>
        /// <param name="value">
        /// 追加する値です。
        /// </param>
        /// <exception cref="UnexpectedEndOfBufferException">
        /// バッファの空き領域が不足しています。
        /// </exception>
        public void AppendDecimalBE(Decimal value)
        {
            const Int32 valueLength = sizeof(Decimal);
            if (checked(_currentIndex + valueLength) > _destinationArray.Length)
                throw new UnexpectedEndOfBufferException();
            _destinationArray.SetValueBE(_currentIndex, value);
            checked
            {
                _currentIndex += valueLength;
            }
        }

        /// <summary>
        /// バッファの内容をバイト列として取得します。
        /// </summary>
        /// <returns>
        /// バッファの内容を示すバイト列です。
        /// </returns>
        public ReadOnlyMemory<Byte> ToByteArray()
        {
            var buffer = new Byte[_currentIndex];
            _destinationArray.CopyTo(0, buffer, 0, _currentIndex);
            return buffer;
        }
    }
}
