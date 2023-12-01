using System;

namespace Utility
{
    public class ProgressCounterUInt64
    {
        private const UInt64 DEFAULT_MINIMUM_STEP = 64 * 1024;
        private readonly IProgress<UInt64>? _progress;
        private readonly UInt64 _minimumStepValue;
        private UInt64 _previousCounter;

        public ProgressCounterUInt64(IProgress<UInt64>? progress)
            : this(progress, DEFAULT_MINIMUM_STEP)
        {
        }

        public ProgressCounterUInt64(IProgress<UInt64>? progress, UInt64 minimumStepValue)
        {
            if (minimumStepValue <= 0)
                throw new ArgumentOutOfRangeException(nameof(minimumStepValue));

            _progress = progress;
            _minimumStepValue = minimumStepValue;
            Value = 0;
            _previousCounter = 0;
        }

        public UInt64 Value { get; private set; }
        public void Increment() => AddValue(1);

        public void AddValue(UInt64 value)
        {
            var needToReport = false;

            lock (this)
            {
                checked
                {
                    Value += value;
                }

                if (Value >= checked(_previousCounter + _minimumStepValue))
                {
                    needToReport = true;
                    _previousCounter = Value;
                }
            }

            if (needToReport)
                Report();
        }

        public void Report()
        {
            try
            {
                _progress?.Report(Value);
            }
            catch (Exception)
            {
            }
        }
    }
}
