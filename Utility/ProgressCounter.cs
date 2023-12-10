using System;
using System.Numerics;

namespace Utility
{
    public class ProgressCounter<VALUE_T>
        where VALUE_T : struct, IComparable<VALUE_T>, IAdditionOperators<VALUE_T, VALUE_T, VALUE_T>
    {
        private readonly IProgress<VALUE_T>? _progress;
        private readonly VALUE_T _minimumStepValue;
        private VALUE_T _previousCounter;

        public ProgressCounter(IProgress<VALUE_T>? progress, VALUE_T minimumStepValue)
        {
            if (minimumStepValue.CompareTo(default) <= 0)
                throw new ArgumentOutOfRangeException(nameof(minimumStepValue));

            _progress = progress;
            _minimumStepValue = minimumStepValue;
            Value = default;
            _previousCounter = default;
        }

        public VALUE_T Value { get; private set; }

        public void AddValue(VALUE_T value)
        {
            var needToReport = false;

            lock (this)
            {
                checked
                {
                    Value += value;
                }

                if (Value.CompareTo(checked(_previousCounter + _minimumStepValue)) >= 0)
                {
                    needToReport = true;
                    _previousCounter = Value;
                }
            }

            if (needToReport)
                Report();
        }

        public void ReportIfInitial()
        {
            if (Value.CompareTo(default) <= 0)
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
