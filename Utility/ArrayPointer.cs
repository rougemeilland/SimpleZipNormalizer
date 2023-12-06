using System;

namespace Utility
{
    public readonly struct ArrayPointer<ELEMENT_T>
        : IEquatable<ArrayPointer<ELEMENT_T>>, IComparable<ArrayPointer<ELEMENT_T>>, IEquatable<ReadOnlyArrayPointer<ELEMENT_T>>, IComparable<ReadOnlyArrayPointer<ELEMENT_T>>
    {
        private readonly ELEMENT_T[] _sourceArray;
        private readonly Int32 _currentIndex;

        [Obsolete("Do not call constructor", true)]
        public ArrayPointer()
        {
            throw new NotSupportedException();
        }

        internal ArrayPointer(ELEMENT_T[] sourceArray, Int32 currentIndex)
        {
            _sourceArray = sourceArray;
            _currentIndex = currentIndex;
        }

        public ReadOnlyArrayPointer<ELEMENT_T> AsReadOnly()
        {
            if (_sourceArray is null)
                throw new InvalidOperationException();

            return new ReadOnlyArrayPointer<ELEMENT_T>(_sourceArray, _currentIndex);
        }

        public Memory<ELEMENT_T> GetMemory(Int32 length)
        {
            if (_sourceArray is null)
                throw new InvalidOperationException();
            if (!length.IsBetween(0, _sourceArray.Length - _currentIndex))
                throw new ArgumentOutOfRangeException(nameof(length));

            return new Memory<ELEMENT_T>(_sourceArray, _currentIndex, length);
        }

        public Memory<ELEMENT_T> GetMemory(UInt32 length)
        {
            if (_sourceArray is null)
                throw new InvalidOperationException();
            if (length > (UInt32)_sourceArray.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            return new Memory<ELEMENT_T>(_sourceArray, _currentIndex, (Int32)length);
        }

        public Span<ELEMENT_T> GetSpan(Int32 length)
        {
            if (_sourceArray is null)
                throw new InvalidOperationException();
            if (!length.IsBetween(0, checked(_sourceArray.Length - _currentIndex)))
                throw new ArgumentOutOfRangeException(nameof(length));

            return new Span<ELEMENT_T>(_sourceArray, _currentIndex, length);
        }

        public Span<ELEMENT_T> GetSpan(UInt32 length)
        {
            if (_sourceArray is null)
                throw new InvalidOperationException();
            if (length > (UInt32)_sourceArray.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            return new Span<ELEMENT_T>(_sourceArray, _currentIndex, (Int32)length);
        }

        public ELEMENT_T this[Int32 index]
        {
            get
            {
                if (_sourceArray is null)
                    throw new InvalidOperationException();
                var offset = checked(_currentIndex + index);
                if (!offset.InRange(0, _sourceArray.Length))
                    throw new ArgumentOutOfRangeException(nameof(index));

                return _sourceArray[offset];
            }

            set
            {
                if (_sourceArray is null)
                    throw new InvalidOperationException();
                var offset = checked(_currentIndex + index);
                if (!offset.InRange(0, _sourceArray.Length))
                    throw new ArgumentOutOfRangeException(nameof(index));

                _sourceArray[offset] = value;
            }
        }

        public ELEMENT_T this[UInt32 index]
        {
            get
            {
                if (_sourceArray is null)
                    throw new InvalidOperationException();
                var offset = checked((UInt32)_currentIndex + index);
                if (offset >= (UInt32)_sourceArray.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return _sourceArray[(Int32)offset];
            }

            set
            {
                if (_sourceArray is null)
                    throw new InvalidOperationException();
                var offset = checked((UInt32)_currentIndex + index);
                if (offset >= (UInt32)_sourceArray.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                _sourceArray[(Int32)offset] = value;
            }
        }

        public ArrayPointer<ELEMENT_T> Add(Int32 offset)
        {
            if (_sourceArray is null)
                throw new InvalidOperationException();
            var newIndex = checked(_currentIndex + offset);
            if (!newIndex.IsBetween(0, _sourceArray.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            return new ArrayPointer<ELEMENT_T>(_sourceArray, newIndex);
        }

        public ArrayPointer<ELEMENT_T> Add(UInt32 offset)
        {
            if (_sourceArray is null)
                throw new InvalidOperationException();
            var newIndex = checked((UInt32)_currentIndex + offset);
            if (newIndex > (UInt32)_sourceArray.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return new ArrayPointer<ELEMENT_T>(_sourceArray, (Int32)newIndex);
        }

        public ArrayPointer<ELEMENT_T> Subtract(Int32 offset)
        {
            if (_sourceArray is null)
                throw new InvalidOperationException();
            var newIndex = checked(_currentIndex - offset);
            if (!newIndex.IsBetween(0, _sourceArray.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            return new ArrayPointer<ELEMENT_T>(_sourceArray, newIndex);
        }

        public ArrayPointer<ELEMENT_T> Subtract(UInt32 offset)
        {
            if (_sourceArray is null)
                throw new InvalidOperationException();
            var newIndex = checked((Int32)(_currentIndex - offset));
            if (!newIndex.IsBetween(0, _sourceArray.Length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            return new ArrayPointer<ELEMENT_T>(_sourceArray, newIndex);
        }

        public Int32 Subtract(ArrayPointer<ELEMENT_T> other)
        {
            if (_sourceArray is null)
                throw new InvalidOperationException();
            if (!ReferenceEquals(_sourceArray, other.SourceArray))
                throw new ArgumentException($"'this' and {nameof(other)} point to different arrays.");

            return checked(_currentIndex - other.CurrentIndex);
        }

        public Int32 Subtract(ReadOnlyArrayPointer<ELEMENT_T> other)
        {
            if (_sourceArray is null)
                throw new InvalidOperationException();
            if (!ReferenceEquals(_sourceArray, other.SourceArray))
                throw new ArgumentException($"'this' and {nameof(other)} point to different arrays.");

            return checked(_currentIndex - other.CurrentIndex);
        }

        public Int32 CompareTo(ArrayPointer<ELEMENT_T> other)
        {
            if (_sourceArray is null)
                throw new InvalidOperationException();
            if (!ReferenceEquals(_sourceArray, other.SourceArray))
                throw new ArgumentException($"'this' and {nameof(other)} point to different arrays.");

            return _currentIndex.CompareTo(other.CurrentIndex);
        }

        public Int32 CompareTo(ReadOnlyArrayPointer<ELEMENT_T> other)
        {
            if (_sourceArray is null)
                throw new InvalidOperationException();
            if (!ReferenceEquals(_sourceArray, other.SourceArray))
                throw new ArgumentException($"'this' and {nameof(other)} point to different arrays.");

            return _currentIndex.CompareTo(other.CurrentIndex);
        }

        public Boolean Equals(ArrayPointer<ELEMENT_T> other)
        {
            if (_sourceArray is null)
                throw new InvalidOperationException();
            if (!ReferenceEquals(_sourceArray, other.SourceArray))
                throw new ArgumentException($"'this' and {nameof(other)} point to different arrays.");

            return _currentIndex == other.CurrentIndex;
        }

        public Boolean Equals(ReadOnlyArrayPointer<ELEMENT_T> other)
        {
            if (_sourceArray is null)
                throw new InvalidOperationException();
            if (!ReferenceEquals(_sourceArray, other.SourceArray))
                throw new ArgumentException($"'this' and {nameof(other)} point to different arrays.");

            return _currentIndex == other.CurrentIndex;
        }

        public override Boolean Equals(Object? obj)
            => obj is not null && GetType() == obj.GetType() && Equals((ArrayPointer<ELEMENT_T>)obj);

        public override Int32 GetHashCode() => _currentIndex.GetHashCode();

        public static ArrayPointer<ELEMENT_T> operator +(ArrayPointer<ELEMENT_T> p, Int32 offset)
            => p.Add(offset);

        public static ArrayPointer<ELEMENT_T> operator +(ArrayPointer<ELEMENT_T> p, UInt32 offset)
            => p.Add(offset);

        public static ArrayPointer<ELEMENT_T> operator -(ArrayPointer<ELEMENT_T> p, Int32 offset)
            => p.Subtract(offset);

        public static ArrayPointer<ELEMENT_T> operator -(ArrayPointer<ELEMENT_T> p, UInt32 offset)
            => p.Subtract(offset);

        public static Int32 operator -(ArrayPointer<ELEMENT_T> p1, ArrayPointer<ELEMENT_T> p2)
            => p1.Subtract(p2);

        public static Int32 operator -(ArrayPointer<ELEMENT_T> p1, ReadOnlyArrayPointer<ELEMENT_T> p2)
            => p1.Subtract(p2);

        public static ArrayPointer<ELEMENT_T> operator ++(ArrayPointer<ELEMENT_T> p)
            => p.Add(1);

        public static ArrayPointer<ELEMENT_T> operator --(ArrayPointer<ELEMENT_T> p)
            => p.Subtract(1);

        public static implicit operator ReadOnlyArrayPointer<ELEMENT_T>(ArrayPointer<ELEMENT_T> p)
            => p.AsReadOnly();

        public static Boolean operator ==(ArrayPointer<ELEMENT_T> p1, ArrayPointer<ELEMENT_T> p2)
            => p1.Equals(p2);

        public static Boolean operator ==(ArrayPointer<ELEMENT_T> p1, ReadOnlyArrayPointer<ELEMENT_T> p2)
            => p1.Equals(p2);

        public static Boolean operator !=(ArrayPointer<ELEMENT_T> p1, ArrayPointer<ELEMENT_T> p2)
            => !p1.Equals(p2);

        public static Boolean operator !=(ArrayPointer<ELEMENT_T> p1, ReadOnlyArrayPointer<ELEMENT_T> p2)
            => !p1.Equals(p2);

        public static Boolean operator >(ArrayPointer<ELEMENT_T> p1, ArrayPointer<ELEMENT_T> p2)
            => p1.CompareTo(p2) > 0;

        public static Boolean operator >(ArrayPointer<ELEMENT_T> p1, ReadOnlyArrayPointer<ELEMENT_T> p2)
            => p1.CompareTo(p2) > 0;

        public static Boolean operator >=(ArrayPointer<ELEMENT_T> p1, ArrayPointer<ELEMENT_T> p2)
            => p1.CompareTo(p2) >= 0;

        public static Boolean operator >=(ArrayPointer<ELEMENT_T> p1, ReadOnlyArrayPointer<ELEMENT_T> p2)
            => p1.CompareTo(p2) >= 0;

        public static Boolean operator <(ArrayPointer<ELEMENT_T> p1, ArrayPointer<ELEMENT_T> p2)
            => p1.CompareTo(p2) < 0;

        public static Boolean operator <(ArrayPointer<ELEMENT_T> p1, ReadOnlyArrayPointer<ELEMENT_T> p2)
            => p1.CompareTo(p2) < 0;

        public static Boolean operator <=(ArrayPointer<ELEMENT_T> p1, ArrayPointer<ELEMENT_T> p2)
            => p1.CompareTo(p2) <= 0;

        public static Boolean operator <=(ArrayPointer<ELEMENT_T> p1, ReadOnlyArrayPointer<ELEMENT_T> p2)
            => p1.CompareTo(p2) <= 0;

        internal ELEMENT_T[] SourceArray
        {
            get
            {
                if (_sourceArray is null)
                    throw new InvalidOperationException();

                return _sourceArray;
            }
        }

        internal Int32 CurrentIndex
        {
            get
            {
                if (_sourceArray is null)
                    throw new InvalidOperationException();
                return _currentIndex;
            }
        }
    }
}
