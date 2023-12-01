using System;

namespace Utility
{
    public class SafetyProgress
    {
        private class ProgressWithValidation<PROGRESS_VALUE_T>
            : IProgress<PROGRESS_VALUE_T>
        {
            private readonly Action<PROGRESS_VALUE_T> _action;
            private readonly Func<PROGRESS_VALUE_T, Boolean>? _validator;

            public ProgressWithValidation(Action<PROGRESS_VALUE_T> action, Func<PROGRESS_VALUE_T, Boolean>? validator)
            {
                _action = action;
                _validator = validator;
            }

            public virtual void Report(PROGRESS_VALUE_T value)
            {
                if (_validator is not null && !_validator(value))
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"** An attempt was made to report a progress value that is an invalid value.: {value}");
#endif
                    throw new ArgumentException($"An attempt was made to report a progress value that is an invalid value.: {value}", nameof(value));
                }

                try
                {
                    _action(value);
                }
                catch (Exception)
                {
                }
            }
        }

        private class IncreasingProgressWithValidation<PROGRESS_VALUE_T>
            : ProgressWithValidation<PROGRESS_VALUE_T>
            where PROGRESS_VALUE_T : notnull, IComparable<PROGRESS_VALUE_T>
        {
            private readonly SafetyProgressOption _option;
            private PROGRESS_VALUE_T? _previousValue;

            public IncreasingProgressWithValidation(Action<PROGRESS_VALUE_T> action, Func<PROGRESS_VALUE_T, Boolean>? validator, SafetyProgressOption option)
                : base(action, validator)
            {
                _option = option;
                _previousValue = default;
            }

            public override void Report(PROGRESS_VALUE_T value)
            {
                var needToReport = false;
                lock (this)
                {
                    if (!_option.HasFlag(SafetyProgressOption.AllowDecrease) && _previousValue is not null && _previousValue.CompareTo(value) > 0)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"** Progress value is small compared to previous value.: previousValue={_previousValue}, currentValue={value}");
#endif
                    }
                    else
                    {
                        needToReport = true;
                        _previousValue = value;
                    }
                }

                if (needToReport)
                    base.Report(value);
            }
        }

        public static IProgress<PROGRESS_VALUE_T> CreateProgress<PROGRESS_VALUE_T>(
            Action<PROGRESS_VALUE_T> action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            return
                new ProgressWithValidation<PROGRESS_VALUE_T>(
                    action,
                    _ => true);
        }

        public static IProgress<PROGRESS_VALUE_T> CreateProgress<PROGRESS_VALUE_T>(
            Action<PROGRESS_VALUE_T> action,
            Func<PROGRESS_VALUE_T, Boolean> validator)
            where PROGRESS_VALUE_T : IComparable<PROGRESS_VALUE_T>
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));
            if (validator is null)
                throw new ArgumentNullException(nameof(validator));

            return
                new ProgressWithValidation<PROGRESS_VALUE_T>(
                    action,
                    validator);
        }

        public static IProgress<PROGRESS_VALUE_T>? CreateProgress<PROGRESS_VALUE_T, SOURCE_PROGRESS_T>(
            IProgress<SOURCE_PROGRESS_T>? destinationProgress,
            Func<PROGRESS_VALUE_T, SOURCE_PROGRESS_T> progressValueSelector)
            where PROGRESS_VALUE_T : IComparable<PROGRESS_VALUE_T>
        {
            if (progressValueSelector is null)
                throw new ArgumentNullException(nameof(progressValueSelector));

            return
                destinationProgress is null
                ? null
                : new ProgressWithValidation<PROGRESS_VALUE_T>(
                    progressValue => destinationProgress.Report(progressValueSelector(progressValue)),
                    _ => true);
        }

        public static IProgress<PROGRESS_VALUE_T>? CreateProgress<PROGRESS_VALUE_T, SOURCE_PROGRESS_T>(
            IProgress<SOURCE_PROGRESS_T>? destinationProgress,
            Func<PROGRESS_VALUE_T, SOURCE_PROGRESS_T> progressValueSelector,
            Func<PROGRESS_VALUE_T, Boolean> validator)
            where PROGRESS_VALUE_T : IComparable<PROGRESS_VALUE_T>
        {
            if (progressValueSelector is null)
                throw new ArgumentNullException(nameof(progressValueSelector));
            if (validator is null)
                throw new ArgumentNullException(nameof(validator));

            return
                destinationProgress is null
                ? null
                : new ProgressWithValidation<PROGRESS_VALUE_T>(
                    progressValue => destinationProgress.Report(progressValueSelector(progressValue)),
                    validator);
        }

        public static IProgress<PROGRESS_VALUE_T> CreateIncreasingProgress<PROGRESS_VALUE_T>(
            Action<PROGRESS_VALUE_T> action,
            SafetyProgressOption option = SafetyProgressOption.None)
            where PROGRESS_VALUE_T : IComparable<PROGRESS_VALUE_T>
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            return
                new IncreasingProgressWithValidation<PROGRESS_VALUE_T>(
                    action,
                    _ => true,
                    option);
        }

        public static IProgress<PROGRESS_VALUE_T> CreateIncreasingProgress<PROGRESS_VALUE_T>(
            Action<PROGRESS_VALUE_T> action,
            PROGRESS_VALUE_T minimumValue,
            PROGRESS_VALUE_T maximumValue,
            SafetyProgressOption option = SafetyProgressOption.None)
            where PROGRESS_VALUE_T : IComparable<PROGRESS_VALUE_T>
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            return
                new IncreasingProgressWithValidation<PROGRESS_VALUE_T>(
                    action,
                    value => value.IsBetween(minimumValue, maximumValue),
                    option);
        }

        public static IProgress<PROGRESS_VALUE_T>? CreateIncreasingProgress<PROGRESS_VALUE_T, SOURCE_PROGRESS_T>(
            IProgress<SOURCE_PROGRESS_T>? destinationProgress,
            Func<PROGRESS_VALUE_T, SOURCE_PROGRESS_T> progressValueSelector,
            SafetyProgressOption option = SafetyProgressOption.None)
            where PROGRESS_VALUE_T : IComparable<PROGRESS_VALUE_T>
        {
            if (progressValueSelector is null)
                throw new ArgumentNullException(nameof(progressValueSelector));

            return
                destinationProgress is null
                ? null
                : new IncreasingProgressWithValidation<PROGRESS_VALUE_T>(
                    progressValue => destinationProgress.Report(progressValueSelector(progressValue)),
                    _ => true,
                    option);
        }

        public static IProgress<PROGRESS_VALUE_T>? CreateIncreasingProgress<PROGRESS_VALUE_T, SOURCE_PROGRESS_T>(
            IProgress<SOURCE_PROGRESS_T>? destinationProgress,
            Func<PROGRESS_VALUE_T, SOURCE_PROGRESS_T> progressValueSelector,
            PROGRESS_VALUE_T minimumValue,
            PROGRESS_VALUE_T maximumValue,
            SafetyProgressOption option = SafetyProgressOption.None)
            where PROGRESS_VALUE_T : IComparable<PROGRESS_VALUE_T>
        {
            if (progressValueSelector is null)
                throw new ArgumentNullException(nameof(progressValueSelector));

            return
                destinationProgress is null
                ? null
                : new IncreasingProgressWithValidation<PROGRESS_VALUE_T>(
                    value => destinationProgress.Report(progressValueSelector(value)),
                    value => value.IsBetween(minimumValue, maximumValue),
                    option);
        }
    }
}
