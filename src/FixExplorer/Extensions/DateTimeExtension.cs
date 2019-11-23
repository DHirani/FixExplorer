using System;
using System.Globalization;

namespace FixExplorer.Extensions
{
    public static class DateTimeExtension
    {
        public static DateTime UtcToLocalTime(this DateTime dateTime)
        {
            return TimeZone.CurrentTimeZone.ToLocalTime(dateTime);
        }

        /// <summary>
        /// To the date time.
        /// </summary>
        /// <param name="dateString">The date string.</param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string dateString)
        {
            var sendingTime = ToDateTime(dateString, "yyyyMMdd-HH:mm:ss");
            if (sendingTime == DateTime.MinValue)
            {
                sendingTime = ToDateTime(dateString, "yyyyMMdd-HH:mm:ss.fff");
            }
            return sendingTime;
        }

        public static DateTime ToDateTime(this string dateString, string format)
        {
            DateTime sendingTime;
            if (!DateTime.TryParseExact(dateString, format, null,
                                        DateTimeStyles.AdjustToUniversal, out sendingTime))
            {
                return DateTime.MinValue;
            }
            return sendingTime;
        }
    }
}
