using System;
using System.Linq;
using Utility.Collections;

namespace ZipUtility
{
    internal class ProgressCounterUint64Uint64
    {
        private const UInt64 DEFAULT_MINIMUM_STEP = 64 * 1024;
        private readonly IProgress<(UInt64 value1, UInt64 value2)>? _progress;
        private readonly UInt64 _minimumStepValue;
        private UInt64 _previousCounter1;
        private UInt64 _previousCounter2;

        public ProgressCounterUint64Uint64(IProgress<(UInt64 value1, UInt64 value2)>? progress)
            : this(progress, DEFAULT_MINIMUM_STEP)
        {
        }

        public ProgressCounterUint64Uint64(IProgress<(UInt64 value1, UInt64 value2)>? progress, UInt64 minimumStepValue)
        {
            if (minimumStepValue <= 0)
                throw new ArgumentOutOfRangeException(nameof(minimumStepValue));

            _progress = progress;
            _minimumStepValue = minimumStepValue;
            InstandeId = RandomSequence.GetUInt64Sequence().First();
            Value1 = 0;
            Value2 = 0;
            _previousCounter1 = 0;
            _previousCounter2 = 0;
        }

        public UInt64 InstandeId { get; }
        public UInt64 Value1 { get; private set; }
        public UInt64 Value2 { get; private set; }

        public void AddValue1(UInt64 value)
        {
            var needToReport = false;
            lock (this)
            {
                checked
                {
                    Value1 += value;
                }

                needToReport = CheckIfNeedToReport();
            }

            if (needToReport)
                Report();
        }

        public void AddValue2(UInt64 value)
        {
            var needToReport = false;

            lock (this)
            {
                checked
                {
                    Value2 += value;
                }

                needToReport = CheckIfNeedToReport();
            }

            if (needToReport)
                Report();
        }

        public void SetValue1(UInt64 value)
        {
            var needToReport = false;
            lock (this)
            {
                if (value < Value1)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"A value smaller than the previous value was specified.: currentValue1=0x{Value1:x16}, newValue1=0x{value:x16}");
#endif
                }
                else
                {
                    Value1 = value;
                    needToReport = CheckIfNeedToReport();
                }
            }

            if (needToReport)
                Report();
        }

        public void SetValue2(UInt64 value)
        {
            var needToReport = false;
            lock (this)
            {
                if (value < Value2)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"A value smaller than the previous value was specified.: currentValue2=0x{Value2:x16}, newValue2=0x{value:x16}");
#endif
                }
                else
                {
                    Value2 = value;
                    needToReport = CheckIfNeedToReport();
                }
            }

            if (needToReport)
                Report();
        }

        public void Report()
        {
            try
            {
                _progress?.Report((Value1, Value2));
            }
            catch (Exception)
            {
            }
        }

        private Boolean CheckIfNeedToReport()
        {
            if (Value1 < checked(_previousCounter1 + _minimumStepValue)
                && Value2 < checked(_previousCounter2 + _minimumStepValue))
            {
                return false;
            }

            _previousCounter1 = Value1;
            _previousCounter2 = Value2;
            return true;
        }
    }
}
