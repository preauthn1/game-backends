using target.Models.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;


namespace target.Utility.Extensions
{
    /// <summary>
    /// Datetime 的擴充方法
    /// </summary>
    public static class ExtensionOfDatetime
    {

        /// <summary>
        /// 檢查 來源方 與 目標方 的起始結束時間 是否有衝突
        /// </summary>
        public static bool CheckBeginEndTimeConflict(DateTime begin_source, DateTime end_source, DateTime begin_compare, DateTime end_compare)
        {
            if (begin_source < begin_compare && begin_compare < end_source)
                return true;

            if (begin_source < end_compare && end_compare < end_source)
                return true;

            if (begin_compare < begin_source && end_source < end_compare)
                return true;

            return false;
        }


        /// <summary>
        /// 根據指定的小時數。將 begin~end 區間的時間分割成N等份
        /// 不含endTime 必須小於
        /// </summary>
        public static List<DateTime> SplitToDateListByHourRange(this DateTime beginTime , DateTime endTime , double hours = 24)
        {
            return SplitToDateListByMinutesRange(beginTime, endTime, hours * 60);
        }


        /// <summary>
        /// 根據指定的小時數。將 begin~end 區間的時間分割成N等份
        /// 不含endTime 必須小於
        /// </summary>
        public static List<DateTime> SplitToDateListByMinutesRange(this DateTime beginTime, DateTime endTime, double minutes = 60)
        {
            if (endTime < beginTime)
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("endTime beginTime"));

            List<DateTime> timeList = new List<DateTime>
            {
                beginTime
            };

            int loopTimes = 1;
            while (true)
            {
                DateTime nextTime = beginTime.AddMinutes(minutes * loopTimes);
                if (nextTime >= endTime || loopTimes >= 20000)
                {
                    break;
                }

                loopTimes++;
                timeList.Add(nextTime);
            }

            return timeList;
        }


        public static string ToISO8601(this DateTime time, double timeZone = 0)
        {
            return time.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        public static string ToDateTimeMinuteStringOrEmpty(this DateTime? time)
        {
            if (!time.HasValue)
                return string.Empty;

            return time.Value.ToString("yyyy-MM-dd HH:mm");
        }

        public static string ToAdminSysDateFormatText(this DateTime utcTime , double timeZoneOffset = 0)
        {
            return utcTime.AddHours(timeZoneOffset).ToString("yyyy-MM-dd HH:mm:ss");
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

        /// <summary>
        /// 取得這個月某一日
        /// </summary>
        public static DateTime ToMonthOneDay(this DateTime time, int day)
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
            return new DateTime(time.Year, time.Month, time.Day, 23, 59, 59, 999);
        }

        /// <summary>
        /// 0點0分0秒
        /// </summary>
        public static DateTime To_00_00_00(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
        }

        public static DateTime To_00_00(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0);
        }

        public static bool IsEmptyTime(this DateTime time)
        {
            return time == new DateTime();
        }


        /// <summary>
        /// 注意裡面的時間是 +0時區的
        /// </summary>
        public static long ConvertToUnixTimestamp(this DateTime utcTime)
        {
            return (long)utcTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }


        public static DateTime ISO8601_To_UtcDateTime(string iso8601 , double timeZoneOffset = 0)
        {
            DateTime parsingTime = DateTimeOffset.Parse(iso8601).DateTime.AddHours(timeZoneOffset);
            return parsingTime;
        }


        public static DateTime UnixTimeStampToDateTime(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }


        public static DateTime UnixTimeStampToDateTime(this long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

            if (unixTimeStamp >= 1500000000000000L)
            {
                // 微秒轉毫秒，使用 double 保留小數精度。
                dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp / 1000d);
            }
            else if (unixTimeStamp >= 1500000000000L)
            {
                dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp);
            }
            else
            {
                dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            }

            return dtDateTime;
        }

    }


}