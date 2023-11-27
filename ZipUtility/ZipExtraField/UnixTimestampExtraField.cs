using System;
using Utility;

namespace ZipUtility.ZipExtraField
{
    /// <summary>
    /// UNIX 時刻を保持する拡張フィールドの基底クラスです。
    /// </summary>
    public abstract class UnixTimestampExtraField
        : TimestampExtraField
    {
        private static readonly DateTime _baseTime;

        static UnixTimestampExtraField()
        {
            _baseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="extraFieldId">
        /// 拡張フィールドの ID です。
        /// </param>
        protected UnixTimestampExtraField(UInt16 extraFieldId)
            : base(extraFieldId)
        {
        }

        /// <inheritdoc/>
        public override abstract ReadOnlyMemory<Byte>? GetData(ZipEntryHeaderType headerType);

        /// <inheritdoc/>
        public override abstract void SetData(ZipEntryHeaderType headerType, ReadOnlyMemory<Byte> data);

        /// <inheritdoc/>
        public override TimeSpan DateTimePrecision => TimeSpan.FromSeconds(1);

        /// <summary>
        /// UNIX エポック (1970年1月1日0時0分0秒) からの経過秒数を <see cref="DateTime"/> 構造体に変換します。 
        /// </summary>
        /// <param name="timestamp">
        /// 変換対象である、UNIX エポックからの経過秒数である整数です。
        /// </param>
        /// <returns>
        /// <paramref name="timestamp"/> に対応する <see cref="DateTime"/> 構造体が返ります。
        /// </returns>
        protected static DateTime FromUnixTimeStamp(Int32 timestamp)
        {
#if DEBUG // 無効な値として 0 が使用されていないか検証
            if (timestamp == 0)
                throw new Exception();
#endif
            return _baseTime.AddSeconds(timestamp);
        }

        /// <summary>
        /// <see cref="DateTime"/> 構造体を UNIX エポック (1970年1月1日0時0分0秒) からの経過秒数に変換します。 
        /// </summary>
        /// <param name="dateTime">
        /// 変換対象である <see cref="DateTime"/> 構造体です。
        /// </param>
        /// <returns>
        /// <paramref name="dateTime"/> に対応する、UNIX エポック からの経過秒数が返ります。
        /// もし、UNIX エポックからの経過秒数が <see cref="Int32"/> で表現できない場合は null が返ります。
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="dateTime"/> の <see cref="DateTime.Kind"/> プロパティの値が <see cref="DateTimeKind.Unspecified"/> です。
        /// </exception>
        protected static Int32? ToUnixTimeStamp(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
                throw new ArgumentException($"{nameof(DateTime)} structure whose {nameof(dateTime.Kind)} property value is {nameof(DateTimeKind.Unspecified)} was specified.", nameof(dateTime));

            try
            {
                var timestamp = (dateTime.ToUniversalTime() - _baseTime).TotalSeconds;
                if (!timestamp.IsBetween((Double)Int32.MinValue, Int32.MaxValue))
                    throw new OverflowException();

                return checked((Int32)timestamp);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
