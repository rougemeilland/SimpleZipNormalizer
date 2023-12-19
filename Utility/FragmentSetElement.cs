using System;
using System.Numerics;

namespace Utility
{
    public class FragmentSetElement<POSITION_T, SIZE_T>
        where POSITION_T : IComparable<POSITION_T>, IAdditionOperators<POSITION_T, SIZE_T, POSITION_T>
    {
        public FragmentSetElement(POSITION_T startPosition, SIZE_T size)
        {
            var endPosition = startPosition + size;
            if (startPosition.CompareTo(endPosition) >= 0)
                throw new ArgumentException($"{nameof(size)} size must be positive.: {nameof(size)}={size}");

            StartPosition = startPosition;
            EndPosition = startPosition + size;
            Size = size;
        }

        public POSITION_T StartPosition { get; }
        public POSITION_T EndPosition { get; }
        public SIZE_T Size { get; }
        public override String ToString() => $"[{StartPosition}, {EndPosition})";
    }
}
