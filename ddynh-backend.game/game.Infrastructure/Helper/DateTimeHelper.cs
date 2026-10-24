using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using example.invalid;
using example.invalid;
using System.Reflection;
using System.Runtime.Versioning;
using game.Infrastructure.Extensions;

namespace game.Infrastructure.Helper
{

    public static class DateTimeHelper
    {
        private const double DefaultTimeZone = 8;


        public const string ISO_8601_Format = "yyyy-MM-ddTHH:mm:ssZ";


        public static DateTime GetDefaultTime()
        {
            return new DateTime(1900, 1 ,1);
        }

        public static DateTime GetCurrentTime(double timezone = DefaultTimeZone)
        {
            return DateTime.UtcNow.AddHours(timezone);
        }

        public static long GetCurrentTimeTimestamp()
        {
            DateTime utcNow = GetCurrentTime(0);
            return utcNow.CovertToUnixTimestamp();
        }

        public static long GetCurrentTimeTimestampMilliseconds()
        {
            DateTime utcNow = GetCurrentTime(0);
            return utcNow.CovertToUnixTimestamp_Milliseconds();
        }

        public static DateTime? TryParseDate(string dateText)
        {
            DateTime result;
            var isSuccess = DateTime.TryParse(dateText, out result);

            if (isSuccess)
                return result;

            return null;
        }


        public static TimeZoneAreaModel GetTimeZoneToday(double timezone)
        {
            var now = DateTime.UtcNow.AddHours(timezone);
            var begin = now.To_00_00_00();
            var end = now.To_23_59_59();

            return new TimeZoneAreaModel
            {
                TimeZone = timezone,
                TodayBegin = begin,
                TodayEnd = end,
                TodayBeginUTC = begin.AddHours(-1 * timezone),
                TodayEndUTC = end.AddHours(-1 * timezone),
            };
        }

    }


    public class TimeZoneAreaModel
    {
        public double TimeZone { get; set; }

        public DateTime TodayBegin { get; set; }

        public DateTime TodayEnd { get; set; }

        public DateTime TodayBeginUTC { get; set; }

        public DateTime TodayEndUTC { get; set; }

    }
}
