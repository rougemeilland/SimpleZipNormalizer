using System;
using System.Collections.Generic;
using System.Text;

namespace Utility.Text
{
    public readonly struct ShiftJisChar
        : IEquatable<ShiftJisChar>, IComparable<ShiftJisChar>, IEquatable<UInt16>, IComparable<UInt16>, IEquatable<Int32>, IComparable<Int32>
    {
        private static readonly Encoding _shiftJisEncoding;
        private static readonly Byte?[] _mapFromPlane1RowToShiftJisFirstByte;
        private static readonly Byte?[] _mapFromPlane2RowToShiftJisFirstByte;
        private static readonly Byte?[] _mapFromCellToShiftJisSecondByte;
        private static readonly (Byte Plane, Byte Row)?[] _mapFromShiftJisFirstByteToPlaneRow;
        private static readonly Byte?[] _mapFromShiftJisSecondByteToCell;

        private readonly Byte _data1;
        private readonly Byte _data2;

        static ShiftJisChar()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            _shiftJisEncoding = Encoding.GetEncoding("shift_jis");
            _mapFromPlane1RowToShiftJisFirstByte = new Byte?[256];
            _mapFromPlane1RowToShiftJisFirstByte.FillArray(_ => null);
            _mapFromPlane2RowToShiftJisFirstByte = new Byte?[256];
            _mapFromPlane2RowToShiftJisFirstByte.FillArray(_ => null);
            _mapFromCellToShiftJisSecondByte = new Byte?[256];
            _mapFromShiftJisFirstByteToPlaneRow = new (Byte Plane, Byte Row)?[256];
            _mapFromShiftJisFirstByteToPlaneRow.FillArray(_ => null);
            _mapFromShiftJisSecondByteToCell = new Byte?[256];
            _mapFromShiftJisSecondByteToCell.FillArray(_ => null);

            for (var row = 1; row <= 94; ++row)
            {
                {
                    var plane = 1;
                    if (row <= 62)
                    {
                        var firstByte = (Byte)((row + 257) >> 1);
                        _mapFromPlane1RowToShiftJisFirstByte[row] = firstByte;
                        _mapFromShiftJisFirstByteToPlaneRow[firstByte] = ((Byte)plane, (Byte)row);
                    }
                    else
                    {
                        var firstByte = (Byte)((row + 385) >> 1);
                        _mapFromPlane1RowToShiftJisFirstByte[row] = firstByte;
                        _mapFromShiftJisFirstByteToPlaneRow[firstByte] = ((Byte)plane, (Byte)row);
                    }
                }

                {
                    var plane = 2;
                    if (row.IsAnyOf(1, 3, 4, 5, 8) || row.IsBetween(12, 15))
                    {
                        var firstByte = (Byte)(((row + 479) >> 1) - (row >> 3) * 3);
                        _mapFromPlane2RowToShiftJisFirstByte[row] = firstByte;
                        _mapFromShiftJisFirstByteToPlaneRow[firstByte] = ((Byte)plane, (Byte)row);
                    }
                    else if (row.IsBetween(78, 94))
                    {
                        var firstByte = (Byte)((row + 411) >> 1);
                        _mapFromPlane2RowToShiftJisFirstByte[row] = firstByte;
                        _mapFromShiftJisFirstByteToPlaneRow[firstByte] = ((Byte)plane, (Byte)row);
                    }
                    else
                    {
                        // NOP
                    }
                }
            }

            for (var cell = 1; cell <= 94; ++cell)
            {
                if ((cell & 1) == 0)
                {
                    var secondByte = (Byte)(cell + 158);
                    _mapFromCellToShiftJisSecondByte[cell] = secondByte;
                    _mapFromShiftJisSecondByteToCell[secondByte] = (Byte)cell;
                }
                else if (cell <= 63)
                {
                    var secondByte = (Byte)(cell + 63);
                    _mapFromCellToShiftJisSecondByte[cell] = secondByte;
                    _mapFromShiftJisSecondByteToCell[secondByte] = (Byte)cell;
                }
                else
                {
                    var secondByte = (Byte)(cell + 64);
                    _mapFromCellToShiftJisSecondByte[cell] = secondByte;
                    _mapFromShiftJisSecondByteToCell[secondByte] = (Byte)cell;
                }
            }
        }

        [Obsolete("Do not call the default constructor.")]
        public ShiftJisChar()
        {
            throw new NotImplementedException();
        }

        public ShiftJisChar(Byte data)
        {
            if (IsFirstByteOfMultiByteChar(data))
                throw new ArgumentOutOfRangeException(nameof(data));
            _data1 = data;
            _data2 = 0;
        }

        public ShiftJisChar(Byte data1, Byte data2)
        {
            if (!IsFirstByteOfMultiByteChar(data1))
                throw new ArgumentOutOfRangeException(nameof(data1));
            if (!IsSecondByteOfMultiByteChar(data2))
                throw new ArgumentOutOfRangeException(nameof(data2));

            _data1 = data1;
            _data2 = data2;
        }

        public static ShiftJisChar? FromPlaneRowCell(PlaneRowCellNumber planeRowCell)
        {
            if (planeRowCell.Plane == 0)
            {
                if (planeRowCell.Row != 0)
                    throw new ArgumentException($"{nameof(planeRowCell)}.Plane or {nameof(planeRowCell)}Row is invalid value.", nameof(planeRowCell));

                return new ShiftJisChar(planeRowCell.Cell);
            }
            else if (planeRowCell.Plane == 1)
            {
                var firstByte = _mapFromPlane1RowToShiftJisFirstByte[planeRowCell.Row];
                if (firstByte is null)
                    return null;
                var secondByte = _mapFromCellToShiftJisSecondByte[planeRowCell.Cell];
                if (secondByte is null)
                    return null;

                return new ShiftJisChar(firstByte.Value, secondByte.Value);
            }
            else if (planeRowCell.Plane == 2)
            {
                var firstByte = _mapFromPlane2RowToShiftJisFirstByte[planeRowCell.Row];
                if (firstByte is null)
                    return null;
                var secondByte = _mapFromCellToShiftJisSecondByte[planeRowCell.Cell];
                if (secondByte is null)
                    return null;

                return new ShiftJisChar(firstByte.Value, secondByte.Value);
            }
            else
            {
                throw new ArgumentException($"{nameof(planeRowCell)}.Plane is invalid value.", nameof(planeRowCell));
            }
        }

        public PlaneRowCellNumber ToPlaneRowCell()
        {
            if (_data2 == 0)
                return new PlaneRowCellNumber(0, 0, _data1);
            var cell =
                _mapFromShiftJisSecondByteToCell[_data2]
                ?? throw new InternalLogicalErrorException();
            var plane1AndRow = _mapFromPlane1RowToShiftJisFirstByte[_data1];
            if (plane1AndRow is not null)
                return new PlaneRowCellNumber(1, plane1AndRow.Value, cell);
            var plane2AndRow = _mapFromPlane2RowToShiftJisFirstByte[_data1];
            if (plane2AndRow is not null)
                return new PlaneRowCellNumber(1, plane2AndRow.Value, cell);
            throw new InternalLogicalErrorException();
        }

        public ReadOnlyMemory<Byte> ToByteArray()
        {
            var buffer = new Byte[2];
            var length = ToByteArray(buffer);
            return buffer.AsMemory()[..length];
        }

        public static Boolean IsSingleByteChar(Byte data) => !IsFirstByteOfMultiByteChar(data);

        public Int32 CompareTo(UInt16 other) => InternalCode.CompareTo(other);
        public Int32 CompareTo(Int32 other) => ((Int32)InternalCode).CompareTo(other);
        public Int32 CompareTo(ShiftJisChar other) => InternalCode.CompareTo(other.InternalCode);
        public Boolean Equals(UInt16 other) => InternalCode == other;
        public Boolean Equals(Int32 other) => InternalCode == other;
        public Boolean Equals(ShiftJisChar other) => _data1 == other._data1 && _data2 == other._data2;
        public override Boolean Equals(Object? other) => other is not null && GetType() == other.GetType() && Equals((ShiftJisChar)other);
        public override Int32 GetHashCode() => HashCode.Combine(_data1, _data2);

        public override String ToString()
        {
            Span<Byte> buffer = stackalloc Byte[2];
            var length = ToByteArray(buffer);
            buffer = buffer[..length];
            try
            {
                var s = _shiftJisEncoding.GetString(buffer);
                return
                    _data2 == 0
                    ? $"'{s}'(0x{_data1:x2})"
                    : $"'{s}'(0x{_data1:x2}{_data2:x2})";
            }
            catch (Exception)
            {
                return
                    _data2 == 0
                    ? $"'???'(0x{_data1:x2})"
                    : $"'???'(0x{_data1:x2}{_data2:x2})";
            }
        }

        public static IEnumerable<ShiftJisChar> EnumerateAllCharacters()
        {
            foreach (var c in PlaneRowCellNumber.EnumerateAllCharacters())
            {
                var s = FromPlaneRowCell(c);
                if (s is not null)
                    yield return s.Value;
            }
        }

        public static ReadOnlyMemory<Byte> Encode(ReadOnlyMemory<ShiftJisChar> characters)
        {
            var buffer = new Byte[characters.Length * 2];
            var region = buffer.AsSpan();
            for (var index = 0; index < characters.Length; ++index)
            {
                var length = characters.Span[index].ToByteArray(region);
                region = region[length..];
            }

            Array.Resize(ref buffer, buffer.Length - region.Length);
            return buffer;
        }

        public static ReadOnlyMemory<ShiftJisChar> Decode(ReadOnlyMemory<Byte> bytes)
        {
            var buffer = new ShiftJisChar[bytes.Length];
            var region = buffer.AsSpan();
            for (var index = 0; index < bytes.Length;)
            {
                var firstByte = bytes.Span[index];
                if (IsFirstByteOfMultiByteChar(firstByte))
                {
                    if (index + 1 >= bytes.Length)
                        throw new ShiftJisEncodingException("Failed to decode byte array as SHIFT-JIS.");
                    var secondByte = bytes.Span[index + 1];
                    if (!IsSecondByteOfMultiByteChar(secondByte))
                        throw new ShiftJisEncodingException("Failed to decode byte array as SHIFT-JIS.");
                    region[0] = new ShiftJisChar(firstByte, secondByte);
                    index += 2;
                }
                else
                {
                    region[0] = new ShiftJisChar(firstByte);
                    index += 1;
                }

                region = region[1..];
            }

            Array.Resize(ref buffer, buffer.Length - region.Length);
            return buffer;
        }

        public static Boolean operator ==(ShiftJisChar x, ShiftJisChar y) => x.Equals(y);
        public static Boolean operator ==(ShiftJisChar x, UInt16 y) => x.Equals(y);
        public static Boolean operator ==(ShiftJisChar x, Int32 y) => x.Equals(y);
        public static Boolean operator ==(UInt16 x, ShiftJisChar y) => y.Equals(x);
        public static Boolean operator ==(Int32 x, ShiftJisChar y) => y.Equals(x);
        public static Boolean operator !=(ShiftJisChar x, ShiftJisChar y) => !x.Equals(y);
        public static Boolean operator !=(ShiftJisChar x, UInt16 y) => !x.Equals(y);
        public static Boolean operator !=(ShiftJisChar x, Int32 y) => !x.Equals(y);
        public static Boolean operator !=(UInt16 x, ShiftJisChar y) => !y.Equals(x);
        public static Boolean operator !=(Int32 x, ShiftJisChar y) => !y.Equals(x);
        public static Boolean operator >(ShiftJisChar x, ShiftJisChar y) => x.CompareTo(y) > 0;
        public static Boolean operator >(ShiftJisChar x, UInt16 y) => x.CompareTo(y) > 0;
        public static Boolean operator >(ShiftJisChar x, Int32 y) => x.CompareTo(y) > 0;
        public static Boolean operator >(UInt16 x, ShiftJisChar y) => y.CompareTo(x) < 0;
        public static Boolean operator >(Int32 x, ShiftJisChar y) => y.CompareTo(x) < 0;
        public static Boolean operator >=(ShiftJisChar x, ShiftJisChar y) => x.CompareTo(y) >= 0;
        public static Boolean operator >=(ShiftJisChar x, UInt16 y) => x.CompareTo(y) >= 0;
        public static Boolean operator >=(ShiftJisChar x, Int32 y) => x.CompareTo(y) >= 0;
        public static Boolean operator >=(UInt16 x, ShiftJisChar y) => y.CompareTo(x) <= 0;
        public static Boolean operator >=(Int32 x, ShiftJisChar y) => y.CompareTo(x) <= 0;
        public static Boolean operator <(ShiftJisChar x, ShiftJisChar y) => x.CompareTo(y) < 0;
        public static Boolean operator <(ShiftJisChar x, UInt16 y) => x.CompareTo(y) < 0;
        public static Boolean operator <(ShiftJisChar x, Int32 y) => x.CompareTo(y) < 0;
        public static Boolean operator <(UInt16 x, ShiftJisChar y) => y.CompareTo(x) < 0;
        public static Boolean operator <(Int32 x, ShiftJisChar y) => y.CompareTo(x) < 0;
        public static Boolean operator <=(ShiftJisChar x, ShiftJisChar y) => x.CompareTo(y) <= 0;
        public static Boolean operator <=(ShiftJisChar x, UInt16 y) => x.CompareTo(y) <= 0;
        public static Boolean operator <=(ShiftJisChar x, Int32 y) => x.CompareTo(y) <= 0;
        public static Boolean operator <=(UInt16 x, ShiftJisChar y) => y.CompareTo(x) <= 0;
        public static Boolean operator <=(Int32 x, ShiftJisChar y) => y.CompareTo(x) <= 0;

        internal Int32 ToByteArray(Span<Byte> buffer)
        {
            if (_data2 == 0)
            {
                buffer[0] = _data1;
                return 1;
            }
            else
            {
                buffer[0] = _data1;
                buffer[1] = _data2;
                return 2;
            }
        }

        private UInt16 InternalCode => _data2 == 0 ? _data1 : (UInt16)((_data1 << 8) | (_data2 << 0));

        private static Boolean IsFirstByteOfMultiByteChar(Byte data1)
            => data1.IsBetween((Byte)0x081, (Byte)0x9f) || data1.IsBetween((Byte)0xe0, (Byte)0xfc);

        private static Boolean IsSecondByteOfMultiByteChar(Byte data2)
            => data2.IsBetween((Byte)0x040, (Byte)0x7e) || data2.IsBetween((Byte)0x80, (Byte)0xfc);
    }
}
