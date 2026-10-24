using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;


namespace game.Infrastructure.Extensions
{
    /// <summary>
    /// Datetime 的擴充方法
    /// </summary>
    public static class ExtensionOfDatetime
    {
        public static string ToDateTimeMinuteStringOrEmpty(this DateTime? time)
        {
            if (!time.HasValue)
                return string.Empty;

            return time.Value.ToString("yyyy-MM-dd HH:mm");
        }


        public static string ToDateTimeStringOrEmpty(this DateTime? time)
        {
            if (!time.HasValue)
                return string.Empty;

            return time.Value.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string ToDateStringOrEmpty(this DateTime? time)
        {
            if (!time.HasValue)
                return string.Empty;

            return time.Value.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 取得這個月首日
        /// </summary>
        public static DateTime ToMonthFirstDay(this DateTime time)
        {
            return ToMonthOneDay(time, 1);
        }



        public static string ConvertToDateTimeGapChineseText(this DateTime sourceValue , DateTime compareValue , string lastText = "后")
        {
            DateTime bigValue = sourceValue > compareValue ? sourceValue : compareValue;
            DateTime smallValue = sourceValue > compareValue ? compareValue : sourceValue;


            var totalMinutes = (long)(bigValue - smallValue).TotalMinutes;
            var totalHours = (long)(bigValue - smallValue).TotalHours;
            var totalDays = (long)(bigValue - smallValue).TotalDays;

            if (totalMinutes <= 0)
                totalMinutes = 1;

            if (totalMinutes < 60)
                return $"{totalMinutes} 分钟{lastText}";

            if (totalHours < 24)
                return $"{totalHours} 小时{lastText}";

            return $"{totalDays} 天{lastText}";
        }


        /// <summary>
        /// 取得這個月某一日
        /// </summary>
        public static DateTime ToMonthOneDay(this DateTime time,int day)
        {
            return new DateTime(time.Year, time.Month, day);
        }

        /// <summary>
        /// 取得這個月最後一日
        /// </summary>
        public static DateTime ToMonthLastDay(this DateTime time)
        {
            DateTime resultTime = new DateTime(time.Year, time.Month, DateTime.DaysInMonth(time.Year, time.Month), 23, 59, 59, 999);
            return resultTime;
        }

        public static DateTime To_23_59_59(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, 23, 59, 59 , 999);
        }

        public static DateTime To_00_00_00(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
        }


        /// <summary>
        /// 轉成該小時的0秒
        /// </summary>
        public static DateTime To_00_ZeroSeconds(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour , time.Minute , 0);
        }


        /// <summary>
        /// 轉成該小時的0分0秒
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime To_00_00(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0);
        }

    
        /// <summary>
        /// 注意裡面的時間是 +0時區的
        /// </summary>
        public static long CovertToUnixTimestamp(this DateTime utcTime)
        {
            return (long)utcTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }


        /// <summary>
        /// 注意裡面的時間是 +0時區的
        /// </summary>
        public static long CovertToUnixTimestamp_Milliseconds(this DateTime utcTime)
        {
            return (long)utcTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }


        /// <summary>
        /// 返回拿到的時間是 +0時區的
        /// </summary>
        public static DateTime UnixTimeStampToDateTime(this double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }


        /// <summary>
        /// 返回拿到的時間是 +0時區的
        /// </summary>
        public static DateTime UnixTimeStampToDateTime(this long unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }


        /// <summary>
        /// 返回拿到的時間是 +0時區的
        /// </summary>
        public static DateTime UnixTimeStampToDateTimeFromMilliseconds(this long unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp);
            return dtDateTime;
        }


    }
    

}