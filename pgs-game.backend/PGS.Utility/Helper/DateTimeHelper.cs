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
using target.Models.Models;
using target.Models.CultureText;
using target.Models.Enums;
using DocumentFormat.OpenXml.Presentation;

namespace target.Utility.Helper
{

    public static class DateTimeHelper
    {
        public const string ISO_8601_Format = "yyyy-MM-ddTHH:mm:ssZ";

        public static DateTime GetEmptyTime()
        {
            return new DateTime();
        }

        public static DateTime GetDefaultTime()
        {
            return new DateTime(1900, 1 ,1);
        }

        /// <summary>
        /// 考慮到partition的因素。可能不適合把資料分類在 1970 (沒有切割這個日期)
        /// </summary>
        public static DateTime GetDefaultTime_2021()
        {
            return new DateTime(2021, 1, 1);
        }

        public static DateTime GetCurrentTime(double timezone = 0)
        {
            return DateTime.UtcNow.AddHours(timezone);
        }

        public static string GetCurrentTimeYYYYMMDD(double timezone = 0)
        {
            return DateTime.UtcNow.AddHours(timezone).ToString("yyyyMMdd");
        }

        public static string GetCurrentTimeYYYYMMDDHHMMSS(double timezone = 0)
        {
            return DateTime.UtcNow.AddHours(timezone).ToString("yyyyMMddHHmmss");
        }
        public static string ToYYYY_MM_DD_HH_MM_SS(DateTime date, double timezone = 0)
        {
            return date.AddHours(timezone).ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string GetCurrentTimeHHMMSS(double timezone = 0)
        {
            return DateTime.UtcNow.AddHours(timezone).ToString("HHmmss");
        }

        public static string GetCurrentTimeYYYY_MM_DD_HH_MM_SS(double timezone = 0)
        {
            return DateTime.UtcNow.AddHours(timezone).ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string GetCurrentTimeYYYYMMDDHH(double timezone = 0)
        {
            return DateTime.UtcNow.AddHours(timezone).ToString("yyyyMMddHH");
        }

        public static long GetCurrentTimeTimestamp()
        {
            DateTime now = GetCurrentTime();
            return now.ConvertToUnixTimestamp();
        }

        public static DateTime? ReplaceMillisecondsToZero(DateTime? time)
        {
            if (time == null)
                return time;

            return ReplaceMillisecondsToZero(time.Value);
        }

        public static DateTime ReplaceMillisecondsToZero(DateTime time)
        {
           var result = new DateTime(
                time.Year,
                time.Month,
                time.Day,
                time.Hour,
                time.Minute,
                time.Second,
                0, // 毫秒設為 0
                time.Kind     // 保留原本的 DateTimeKind（Local、Utc 或 Unspecified）
            );

            return result;
        }

        public static List<DateTime> GetDateListBetweenTwoDate(DateTime begin , DateTime end , bool isExcludeLastEqualDate = false)
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

            //如果指定true。排除最後一筆跟endTime相同的日期
            if(isExcludeLastEqualDate)
            {
                list = list
                    .Where(x => x != end)
                    .ToList();
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

        /// <summary>
        /// 即使傳入的時間不是ISO8601格式。也一律將傳入的時間視為 UTC+0 的時間 
        /// 不會因為機房的本地時區、而造成DateTime被擅自+8
        /// 範例一: 傳入 2022-01-01 08:00:00 就會認定是 UTC+0的 08:00:00時間、 Hour=8
        /// 範例二: 傳入 2022-01-01T08:00:00Z 因明確判斷出是ISO 8601格式。因此回傳的DateTime物件 Hour=8
        /// </summary>
        public static DateTime ConvertUnknowFormatTextToUtcDateTime(string dateText)
        {
            var parse1 = DateTime.Parse(dateText);
            var parse2 = DateTimeOffset.Parse(dateText);

            if(parse2.Offset.Hours == 0)
            {
                return parse2.DateTime;
            }

            if(parse2.Offset.Hours == 8)
            {
                return parse2.UtcDateTime.AddHours(parse2.Offset.Hours);
            }

            return parse1;
        }


        /// <summary>
        /// 一律將傳入的時間視為特定時區的時間 並轉成 UTC+0
        /// </summary>
        /// <param name="dateText"></param>
        /// <returns></returns>
        public static DateTime Convert_GMTDateString_ToUtcDateTime(string dateText , double expectSourceTimezone = 8.0)
        {
            var parse1 = DateTime.Parse(dateText);
            var parse2 = DateTimeOffset.Parse(dateText);

            if (parse2.Offset.Hours == 0)
            {
                return parse2.DateTime.AddHours(-1 * expectSourceTimezone);
            }

            if (parse2.Offset.Hours == 8)
            {
                return parse2.UtcDateTime;
            }

            return parse1;
        }


        public static string GetDisplayServerTime(DateTime? utcTime = null)
        {         
            if(!utcTime.HasValue)
            {
                utcTime = GetCurrentTime();
            }

            return utcTime.Value.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss") + " (GMT+8)";
        }


        /// <summary>
        /// isIncludeParentheses=括號
        /// </summary>
        /// <param name="minutes"></param>
        /// <param name="isIncludeParentheses">是否包含括號</param>
        /// <param name="extraTextBehind"></param>
        /// <param name="myLang"></param>
        /// <returns></returns>
        public static string GetDiffDisplayTime(int minutes , bool isIncludeParentheses = false , string extraTextBehind = "" , LanguageEnum? myLang = null)
        {
            if (minutes < 0)
                return "";

            string msgTemplate = "{0}";

            //是否包含括號
            if (isIncludeParentheses)
                msgTemplate = "({0})";

            string blank = " ";

            var targetLang = HeaderValueHelper.GetCurrentLanguage();

            if(myLang != null)
            {
                targetLang = myLang.Value;
            }

            if(AppHelper.IsChineseLang(targetLang))
            {
                blank = "";
            }

            if (minutes < 60)
            {
                string timeFormat = MultiLanguageTextHelper.GetCultureTextByFieldName("Common_LabelColumn_minutes", targetLang);
                return string.Format(msgTemplate, $"{minutes}{blank}{timeFormat}{blank}{extraTextBehind}");
            }

            if (minutes < 60 * 24)
            {
                string timeFormat = MultiLanguageTextHelper.GetCultureTextByFieldName("Common_LabelColumn_hour", targetLang);
                return string.Format(msgTemplate, $"{minutes / 60}{blank}{timeFormat}{blank}{extraTextBehind}");
            }

            string myTimeFormat = MultiLanguageTextHelper.GetCultureTextByFieldName("Common_LabelColumn_days", targetLang);
            return string.Format(msgTemplate, $"{minutes / 1440}{blank}{myTimeFormat}{blank}{extraTextBehind}");
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
