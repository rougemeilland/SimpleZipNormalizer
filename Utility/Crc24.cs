using System;

namespace Utility
{
    public static class Crc24
    {
        public static ICrcCalculationState<UInt32, UInt64> CreateCalculationState()
            => ByteArrayExtensions.CreateCrc24CalculationState();
    }
}
