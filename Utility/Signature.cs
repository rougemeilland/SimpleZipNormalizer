using System;
using System.Runtime.CompilerServices;

namespace Utility
{
    public static class Signature
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt32 MakeUInt32LESignature(Byte byte0, Byte byte1, Byte byte2, Byte byte3)
            => ((UInt32)byte0 << (8 * 0))
                | (UInt32)byte1 << (8 * 1)
                | (UInt32)byte2 << (8 * 2)
                | (UInt32)byte3 << (8 * 3);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt32 MakeUInt32LESignature(Char c0, Char c1, Char c2, Char c3)
        {
            if (c0 is < '\x00' or > '\xff')
                throw new ArgumentOutOfRangeException(nameof(c0));
            if (c1 is < '\x00' or > '\xff')
                throw new ArgumentOutOfRangeException(nameof(c1));
            if (c2 is < '\x00' or > '\xff')
                throw new ArgumentOutOfRangeException(nameof(c2));
            if (c3 is < '\x00' or > '\xff')
                throw new ArgumentOutOfRangeException(nameof(c3));

            return
                (UInt32)(Byte)c0 << (8 * 0)
                | (UInt32)(Byte)c1 << (8 * 1)
                | (UInt32)(Byte)c2 << (8 * 2)
                | (UInt32)(Byte)c3 << (8 * 3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt32 MakeUInt32BESignature(Byte byte0, Byte byte1, Byte byte2, Byte byte3)
            => ((UInt32)byte0 << (8 * 3))
                | (UInt32)byte1 << (8 * 2)
                | (UInt32)byte2 << (8 * 1)
                | (UInt32)byte3 << (8 * 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static UInt32 MakeUInt32BESignature(Char c0, Char c1, Char c2, Char c3)
        {
            if (c0 is < '\x00' or > '\xff')
                throw new ArgumentOutOfRangeException(nameof(c0));
            if (c1 is < '\x00' or > '\xff')
                throw new ArgumentOutOfRangeException(nameof(c1));
            if (c2 is < '\x00' or > '\xff')
                throw new ArgumentOutOfRangeException(nameof(c2));
            if (c3 is < '\x00' or > '\xff')
                throw new ArgumentOutOfRangeException(nameof(c3));

            return
                (UInt32)(Byte)c0 << (8 * 3)
                | (UInt32)(Byte)c1 << (8 * 2)
                | (UInt32)(Byte)c2 << (8 * 1)
                | (UInt32)(Byte)c3 << (8 * 0);
        }
    }
}
