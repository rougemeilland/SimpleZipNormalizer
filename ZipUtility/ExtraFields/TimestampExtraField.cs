using System;

namespace ZipUtility.ExtraFields
{
    /// <summary>
    /// エントリのタイムスタンプを維持する拡張フィールドの基底クラスです。
    /// </summary>
    public abstract class TimestampExtraField
        : ExtraField, ITimestampExtraField
    {
        private DateTime? _lastWriteTimeUtc;
        private DateTime? _lastAccessTimeUtc;
        private DateTime? _creationTimeUtc;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="extraFieldId">
        /// 拡張フィールドの ID です。
        /// </param>
        protected TimestampExtraField(UInt16 extraFieldId)
            : base(extraFieldId)
        {
            _lastWriteTimeUtc = null;
            _lastAccessTimeUtc = null;
            _creationTimeUtc = null;
        }

        /// <inheritdoc/>
        public virtual DateTime? LastWriteTimeUtc
        {
            get => _lastWriteTimeUtc;
            set
            {
                if (value is not null && value.Value.Kind == DateTimeKind.Unspecified)
                    throw new ArgumentException($"Unexpected {nameof(DateTime.Kind)} value", nameof(value));

                _lastWriteTimeUtc = value?.ToUniversalTime();
            }
        }

        /// <inheritdoc/>
        public virtual DateTime? LastAccessTimeUtc
        {
            get => _lastAccessTimeUtc;
            set
            {
                if (value is not null && value.Value.Kind == DateTimeKind.Unspecified)
                    throw new ArgumentException($"Unexpected {nameof(DateTime.Kind)} value", nameof(value));

                _lastAccessTimeUtc = value?.ToUniversalTime();
            }
        }

        /// <inheritdoc/>
        public virtual DateTime? CreationTimeUtc
        {
            get => _creationTimeUtc;
            set
            {
                if (value is not null && value.Value.Kind == DateTimeKind.Unspecified)
                    throw new ArgumentException($"Unexpected {nameof(DateTime.Kind)} value", nameof(value));

                _creationTimeUtc = value?.ToUniversalTime();
            }
        }

        /// <inheritdoc/>
        public abstract TimeSpan DateTimePrecision { get; }
    }
}
