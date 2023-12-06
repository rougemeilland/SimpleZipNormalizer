using System;

namespace Utility
{
    public class ProgressCounterUInt64
        : ProgressCounter<UInt64>
    {
        private const UInt64 DEFAULT_MINIMUM_STEP = 64 * 1024;

        public ProgressCounterUInt64(IProgress<UInt64>? progress)
            : this(progress, DEFAULT_MINIMUM_STEP)
        {
        }

        public ProgressCounterUInt64(IProgress<UInt64>? progress, UInt64 minimumStepValue)
            : base(progress, minimumStepValue)
        {
            if (minimumStepValue <= 0)
                throw new ArgumentOutOfRangeException(nameof(minimumStepValue));
        }

        public void Increment() => AddValue(1);
    }
}
