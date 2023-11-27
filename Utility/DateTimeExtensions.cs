using System;

namespace Utility
{
    public static class DateTimeExtensions
    {
        public static DateTime FromDosDateTimeToDateTime(this (UInt16 dosDate, UInt16 dosTime) dosDateTimeValue, DateTimeKind kind)
        {
            var second = (dosDateTimeValue.dosTime & 0x1f) << 1;
            if (second > 59)
                second = 59;
            var minute = (dosDateTimeValue.dosTime >> 5) & 0x3f;
            if (minute > 59)
                minute = 59;
            var hour = (dosDateTimeValue.dosTime >> 11) & 0x1f;
            if (hour > 23)
                hour = 23;
            var month = (dosDateTimeValue.dosDate >> 5) & 0xf;
            if (month < 1)
            {
                month = 1;
            }
            else if (month > 12)
            {
                month = 12;
            }
            else
            {
            }

            var year = ((dosDateTimeValue.dosDate >> 9) & 0x7f) + 1980;
            var day = dosDateTimeValue.dosDate & 0x1f;
            if (day < 1)
            {
                day = 1;
            }
            else
            {
                var maximumDayValue = DateTime.DaysInMonth(year, month);
                if (day > maximumDayValue)
                    day = maximumDayValue;
            }

            return new DateTime(year, month, day, hour, minute, second, kind).ToUniversalTime();
        }

        public static (UInt16 dosDate, UInt16 dosTime) FromDateTimeToDosDateTime(this DateTime dateTimeValue, DateTimeKind kind)
        {
            if (dateTimeValue.Kind == DateTimeKind.Unspecified)
                throw new ArgumentException($"The value of the {nameof(dateTimeValue.Kind)} property of {nameof(dateTimeValue)} must not be DateTimeKind.Unspecified.");

            var dateTime =
                kind switch
                {
                    DateTimeKind.Utc => dateTimeValue.ToUniversalTime(),
                    DateTimeKind.Local => dateTimeValue.ToLocalTime(),
                    _ => throw new ArgumentOutOfRangeException(nameof(kind)),
                };

            // 1980年から2107年までの間ではない場合はエラー
            if (!dateTime.Year.IsBetween(1980, 1980 + (Int32)unchecked(((UInt32)(-1) << 25) >> 25)))
                throw new ArgumentOutOfRangeException(nameof(dateTimeValue));

            checked
            {
                var date =
                    (((UInt32)dateTime.Year - 1980U) << (25 - 16))
                    | ((UInt32)dateTime.Month << (21 - 16))
                    | ((UInt32)dateTime.Day << (16 - 16));

                var time =
                    ((UInt32)dateTime.Hour << 11)
                    | ((UInt32)dateTime.Minute << 5)
                    | ((UInt32)dateTime.Second >> 1);

                return ((UInt16)date, (UInt16)time);
            }
        }
    }
}
