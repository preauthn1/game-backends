using Lck.Utility.Extensions.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using target.Utility.Extensions;
using Lck.Utility.Extensions.Cache;
using target.Utility;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using example.invalid;
using target.Models.Models;
using target.Utility.Utility.ThirdParty;

namespace Lck.Utility.Extensions.Helper
{


    /// <summary>
    /// 純粹針對 MemoryCache
    /// </summary>
    public class CacheHelper
    {
        protected static TimeSpan _defaultCacheTime
        {
            get
            {
                return new TimeSpan(0, 2, 0);
            }
        }


        public static T GetCacheOrQuery<T>(string key, Func<T> func, int expiredMinute)
        {
            return GetCacheOrQuery(key, func, new CacheQueryConfig
            {
                ExpiredTime = new TimeSpan(0, expiredMinute, 0)
            });
        }

        /// <summary>
        /// 請注意null物件。不會存cache (如果查100次都是null 並沒有因cache而減低存取次數)
        /// </summary>
        public static T GetCacheOrQuery<T>(string key, Func<T> func, CacheQueryConfig config = null)
        {
            config = config ?? new CacheQueryConfig();

            //若無啟用cache
            if (IsCacheEnable() == false)
            {
                return func();
            }

            T cacheValue = GetCacheValue<T>(key);

            if (cacheValue != null)
                return cacheValue;

            T queryResult = func();

            //null的話 就不用cache存了 (請務必不能調整此邏輯)
            if (queryResult == null)
                return queryResult;

            if(config.SetCacheOnlyWhenValueEqual != null)
            {
                //若有指定，數值與指定的不同 就不會存cache
                if(!IsObjectValueMatch(config.SetCacheOnlyWhenValueEqual , queryResult))
                {
                    return queryResult;
                }
            }

            if(SettingHelper.IsRemoteCache)
            {
                SetCacheValue(key, queryResult, config.ExpiredTime);
            }
            else
            {
                //似乎是曾經遇過 reference value 被改變、mutable的問題 發生bug、才有這樣的寫法 (memory cache才會有這種狀況)
                var cloneResult = queryResult.ConvertToJsonString().JsonConvertToModel<T>();
                SetCacheValue(key, cloneResult, config.ExpiredTime);                
            }

            return queryResult;
        }

      
        public static T GetCacheOrQueryByMemoryCache<T>(string key, Func<T> func, TimeSpan expiredTime)
        {
            //若無啟用cache
            if (IsCacheEnable() == false)
            {
                return func();
            }

            bool isRemoteCache = false;

            var cacheService = InstanceCacheService(isRemoteCache);

            T cacheValue = cacheService.Get<T>(key);

            if (cacheValue != null)
                return cacheValue;

            T queryResult = func();

            //null的話 就不用cache存了 (請務必不能調整此邏輯)
            if (queryResult == null)
                return queryResult;

            //不確定是不是曾經遇過 reference value 被改變 或 mutable的問題 發生bug、才有這樣的寫法...
            var cloneResult = queryResult.ConvertToJsonString().JsonConvertToModel<T>();

            cacheService.Set(key, cloneResult, expiredTime);
            return queryResult;
        }


        /// <summary>
        /// 每次呼叫次數+1 (一小時存活時間的緩存)
        /// 建議cacheKey規則  XXXLogic_YourID_MMddHH  根據小時拆開Key
        /// </summary>
        public static int CounterMaxTimesHourly(string cacheKey)
        {
            int cacheValue = GetCacheValue<int>(cacheKey);

            int newValue = cacheValue + 1;

            SetCacheValue(cacheKey, newValue, new TimeSpan(1, 0, 0));

            return newValue;
        }


        /// <summary>
        /// 每次呼叫次數+1 (12小時存活時間的緩存)
        /// 建議cacheKey規則  XXXLogic_YourID_MMdd  根據日期拆開Key
        /// </summary>
        public static int CounterMaxTimesDaily(string cacheKey)
        {
            int cacheValue = GetCacheValue<int>(cacheKey);

            int newValue = cacheValue + 1;

            SetCacheValue(cacheKey, newValue, new TimeSpan(12, 0, 0));

            return newValue;
        }


        public static bool IsCacheKeyExist(string key)
        {
            var myCacheValue = GetCacheValue<string>(key);

            if (string.IsNullOrWhiteSpace(myCacheValue))
                return false;

            return true;
        }



        public static T GetCacheValue<T>(string key)
        {
            if (IsCacheEnable() == false)
                return default(T);

            var cacheService = InstanceCacheService();

            T cacheValue = cacheService.Get<T>(key);

            return ReturnMyCacheValue(cacheValue);
        }


        private static T ReturnMyCacheValue<T>(T cacheValue)
        {
            //某幾隻API。一直有memory效能問題 所以不要再做這種處理了
            if (SettingHelper.IsRemoteCache && (SettingHelper.IsExternalVendorAPI || SettingHelper.IsAdminSysAPI || SettingHelper.IsMSTest))
            {
                return cacheValue;
            }
            else
            {
                //會這樣寫是因為以前遇過奇怪的雷... 但又不敢亂改回去 (針對變數作異動、cache又被改回去?)
                //可是假如真的發生refernce的問題。總覺得是有Set行為才會出現 所以純粹的 Get行為不會出問題 (應該?)
                var cloneResult = cacheValue.ConvertToJsonString().JsonConvertToModel<T>();
                return cloneResult;
            }
        }



        /// <summary>
        /// expiredTime 帶入null，會有預設過期時間
        /// </summary>
        public static bool SetCacheValue(string key, object value, TimeSpan? expiredTime)
        {
            if (IsCacheEnable() == false)
                return false;

            var cacheService = InstanceCacheService();

            bool isOK = cacheService.Set(key, value, GetExpireTimeBySetting(expiredTime));
            return isOK;
        }


        /// <summary>
        /// 建立一個短期cache鎖 (避免重複執行...之類的locker)
        /// </summary>
        public static bool CheckShortCacheLockerExist(string key, int seconds = 30, bool isThrowEx = true)
        {
            var cacheValue = GetCacheValue<string>(key);

            if (string.IsNullOrWhiteSpace(cacheValue))
            {
                string randomValue = Guid.NewGuid().ToString();
                SetCacheValue(key, randomValue, new TimeSpan(0, 0, seconds));
                return false;
            }

            if (isThrowEx)
            {
                //如果當前有cache locker
                string secondsText = $"{seconds} seconds";
                throw new TerproException(ApiErrorCode.Forbidden_RequestTooManyTimes, new ErrorDetailModel(secondsText));
            }

            return true;
        }


        public static List<string> GetCacheKeysByKeyword(string keyword)
        {
            if (IsCacheEnable() == false)
                return new List<string>();

            var cacheService = InstanceCacheService();
            return cacheService.GetCacheKeysByKeyword(keyword); ;
        }


        public static List<string> GetCacheKeysByPrefixKeyword(string keyword)
        {
            if (IsCacheEnable() == false)
                return new List<string>();

            var cacheService = InstanceCacheService();
            return cacheService.GetCacheKeysByPrefixKeyword(keyword); ;
        }



        public static void RemoveCacheAll()
        {
            if (IsCacheEnable() == false)
                return;

            if (SettingHelper.IsProduction)
            {
                MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                {
                    Message = " RemoveCacheAll 被呼叫 ",
                    TelegramChannelEnum = TelegramChannelEnum.pgs_common_error
                });
            }


            var cacheService = InstanceCacheService();
            var redis2 = new RedisCache2();
            var redis3 = new RedisCache3();

            cacheService.FlushAll();
            redis2.FlushAll();
            redis3.FlushAll();
        }

        public static void RemoveCacheByKey(string key, bool isAsync = false)
        {
            if (IsCacheEnable() == false)
                return;

            var cacheService = InstanceCacheService();
            cacheService.Flush(key, isAsync);
        }


        public static void RemoveCacheByKeyword(string keyword)
        {
            if (IsCacheEnable() == false)
                return;

            var queryAllKeys = GetCacheKeysByKeyword(keyword);

            var cacheService = InstanceCacheService();
            foreach (var key in queryAllKeys)
            {
                cacheService.Flush(key);
            }
        }


        public static void RemoveCacheByPrefixKeyword(string keyword)
        {
            if (IsCacheEnable() == false)
                return;

            var queryAllKeys = GetCacheKeysByPrefixKeyword(keyword);

            var cacheService = InstanceCacheService();
            foreach (var key in queryAllKeys)
            {
                cacheService.Flush(key);
            }
        }


        public static async Task RemoveCacheAsync(string key)
        {
            if (IsCacheEnable() == false)
                return;

            var cacheService = InstanceCacheService();
            _ = cacheService.FlushAsync(key);
        }


        /// <summary>
        /// memoryCache的部分要獨立
        /// 並且可以透過這個function 追蹤有用到memory cache的地方 & 知道哪些地方可能有cache同步不即時的問題
        /// </summary>
        public static TimeSpan InstanceMemoryCacheTime(int extendRate = 1)
        {
            //建議最多一分鐘就好 不然資料同步不及時、QA測到很麻煩
            TimeSpan cacheTime = new TimeSpan(0, 1 * extendRate, 0);

            //dev環境縮短cache時間減少測試出現不同步
            if (SettingHelper.IsDev)
            {
                cacheTime = new TimeSpan(0, 0, 10);
            }
            else if (SettingHelper.IsMSTest)
            {
                cacheTime = new TimeSpan(0, 0, 0);
            }

            return cacheTime;
        }

        public static bool IsObjectValueMatch<T>(object obj, T target)
        {
            // 檢查 obj 是否為 T 類型，且其值等於 target
            return obj is T tObject && tObject.Equals(target);
        }



        private static bool IsCacheEnable()
        {
            return true;
        }


        private static BaseCache InstanceCacheService()
        {
            //根據專案的config設置 決定要用哪一種
            bool isRemoteCache = SettingHelper.IsRemoteCache;
            return InstanceCacheService(isRemoteCache);
        }


        private static BaseCache InstanceCacheService(bool isRemoteCache)
        {
            if (!IsCacheEnable())
                return null;

            return InstanceCacheByType(isRemoteCache);
        }


        public static BaseCache InstanceRedisCache002()
        {
            return new RedisCache();

            //if (string.IsNullOrWhiteSpace(SettingHelper.ThirdParty.Redis002_Conn))
            //    return new RedisCache();

            //return new RedisCache2();
        }


        public static BaseCache InstanceCacheByType(bool isRemoteCache)
        {
            if (isRemoteCache)
                return new RedisCache();

            return new PGSMemoryCache();
        }


        public static TimeSpan GetExpireTimeBySetting(TimeSpan? expiredTime)
        {
            if (expiredTime.HasValue == false)
                expiredTime = _defaultCacheTime;

            return expiredTime.Value;
        }


    }
}
