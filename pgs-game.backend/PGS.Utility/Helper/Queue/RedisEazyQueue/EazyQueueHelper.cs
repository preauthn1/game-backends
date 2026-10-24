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
using Lck.Utility.Extensions.Helper;

namespace target.Utility.Helper
{

    /// <summary>
    /// 雖然號稱queue 但是懶得建置服務 (MQ)
    /// 先用cache應急下 
    /// 使用情境 有時候DB掛掉卡住。寫入會失敗。cache也保留一份 避免資料遺失
    /// </summary>
    public static class EazyQueueHelper
    {
        public static void Append(MyEazyQueueModel data)
        {
            string cacheKey = GenerateCacheKey(data.UniqueKey);

            var myLoop = new RetryLoopParameter
            {
                MaxTimes = 2,
                DelayMilliseconds = 1000 * 10,
                ExceptionNotifyLevel = LoopExceptionNotifyLevelEnum.OnlyWhenLastTimeExceptioin
            };

            //若寫入失敗，會delay N秒後，再嘗試第二次
            AppHelper.ExecuteMethodAndRetryMaxTimes(myLoop , (param)=>
            {
                bool isSuccess = CacheHelper.SetCacheValue(cacheKey, data, new TimeSpan(9999, 0, 0, 0));
                return isSuccess;
            });
        }

        public static List<MyEazyQueueModel> GetMultipleItems(int takeSize = 50)
        {
            string cacheKey = GenerateCacheKey("");

            var keys = CacheHelper.GetCacheKeysByKeyword(cacheKey);

            var listOfItems = new List<MyEazyQueueModel>();

            if (keys.IsEmptyOrNull())
                return listOfItems;

            foreach(string key in keys)
            {
                var myItem = CacheHelper.GetCacheValue<MyEazyQueueModel>(key);
                listOfItems.Add(myItem);
            }

            return listOfItems;
        }

        public static void DeleteFromQueue(string uniqueKey = "")
        {
            string cacheKey = GenerateCacheKey(uniqueKey);
            CacheHelper.RemoveCacheByKey(cacheKey);
        }

        private static string GenerateCacheKey(string uniqueKey)
        {
            return $"EazyQueue_{uniqueKey}";
        }

    }


    public class MyEazyQueueModel
    {
        public MyEazyQueueModel()
        {
            this.UniqueKey = Guid.NewGuid().ToString();
        }

        public string UniqueKey { get; set; }

        public EazyQueueTypeEnum Type { get; set; }

        public string JsonValue { get; set; }
    }


    public enum EazyQueueTypeEnum
    {
        None = 0,

        EazyTempValue = 1001,

        BetRecordTagMapping = 2001,
    }


}
