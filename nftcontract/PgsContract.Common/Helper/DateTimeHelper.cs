using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using target.Utility.Extensions;
using example.invalid;
using example.invalid;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Runtime.Versioning;

namespace target.Utility.Helper
{

    public static class DateTimeHelper
    {
        public const string ISO_8601_Format = "yyyy-MM-ddTHH:mm:ssZ";


        public static DateTime GetDefaultTime()
        {
            return new DateTime(1900, 1 ,1);
        }

        public static DateTime GetCurrentTime(double timezone = 0)
        {
            return DateTime.UtcNow.AddHours(timezone);
        }

        public static string GetCurrentTimeYYYYMMDDHHMMSS(double timezone = 0)
        {
            return DateTime.UtcNow.AddHours(timezone).ToString("yyyyMMddHHmmss");
        }
        public static string GetCurrentTimeYYYY_MM_DD_HH_MM_SS(double timezone = 0)
        {
            return DateTime.UtcNow.AddHours(timezone).ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static long GetCurrentTimeTimestamp(double timezone = 0)
        {
            DateTime now = GetCurrentTime();
            return now.ConvertToUnixTimestamp();
        }

        public static List<DateTime> GetDateListBetweenTwoDate(DateTime begin , DateTime end)
        {
            int loopTimes = 0;
            List<DateTime> list = new List<DateTime>();

            while(true)
            {
                if (loopTimes >= 9999)
                    break;

                DateTime targetDate = begin.Date.AddDays(loopTimes);

                if (targetDate > end)
                    break;

                list.Add(targetDate);                    
                loopTimes++;
            }

            return list;
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

        public static string GetDiffDisplayTime(int minutes , bool isIncludeParentheses = false)
        {
            if (minutes <= 0)
                return "";

            string msgTemplate = "{0}";

            if (isIncludeParentheses)
                msgTemplate = "({0})";

            if (minutes < 60)
                return string.Format(msgTemplate, $"{minutes}分鐘後");

            if (minutes < 60 * 24)
                return string.Format(msgTemplate, $"{minutes / 60}小時後");

            return string.Format(msgTemplate, $"{minutes / 1440}天後");
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
