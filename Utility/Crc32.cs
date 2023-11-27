using System;

namespace Utility
{
    public static class Crc32
    {
        public static ICrcCalculationState<UInt32, UInt64> CreateCalculationState() => ByteArrayExtensions.CreateCrc32CalculationState();
    }
}
