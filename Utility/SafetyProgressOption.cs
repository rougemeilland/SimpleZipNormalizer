using System;

namespace Utility
{
    [Flags]
    public enum SafetyProgressOption
    {
        None = 0,
        AllowDecrease = 1 << 0,
    }
}
