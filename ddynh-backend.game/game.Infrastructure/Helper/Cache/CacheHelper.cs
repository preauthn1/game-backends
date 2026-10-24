using game.Infrastructure.Extensions.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.Infrastructure.Extensions.Cache;


namespace game.Infrastructure.Extensions.Helper
{
    /// <summary>
    /// 設定檔一定要加 IsCacheActive 這個參數，否則 CacheHelper 無作用
    /// CacheInstanceType 用來指定要使用的 Cache 類型，未指定會用 RuntimeCache
    /// Redis Cache 需要加 Connection String 設定 。 設定名稱叫 RedisConn
    /// RedisDBNumber 則視情況加
    /// redis 的库（database） 等同于 mysql 的表（table）
    /// 表（table ）的存在意义不大， 它更多地是做为命名空间（name space）
    /// 由于使用的是很不友好的数字命名（默认 0-15）， redis 中的 库（database）形同鸡肋
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
            return GetCacheOrQuery(key, func, new TimeSpan(0, expiredMinute, 0));
        }


        public static T GetCacheOrQuery<T>(string key, Func<T> func, TimeSpan? expiredTime = null)
        {
            //若無啟用cache
            if (IsCacheActive() == false)
            {
                return func();
            }

            T cacheValue = GetCacheValue<T>(key);

            if (cacheValue != null)
                return cacheValue;

            T queryResult = func();

            //null的話 就不用cache存了
            if (queryResult == null)
                return queryResult;

            SetCacheValue(key, queryResult, expiredTime);

            return queryResult;
        }


        public static T GetCacheValue<T>(string key)
        {
            if (IsCacheActive() == false)
                return default(T);

            var cacheService = InstanceCacheService();

            T cacheValue = cacheService.Get<T>(key);

            return cacheValue;
        }


        public static bool SetCacheValue(string key, object value, TimeSpan? expiredTime)
        {
            if (IsCacheActive() == false)
                return false;

            expiredTime = GetValidExpireTime(expiredTime);

            var cacheService = InstanceCacheService();

            bool isOK = cacheService.Set(key, value, expiredTime);
            return isOK;
        }


        public static void RemoveCacheByKey(string key)
        {
            if (IsCacheActive() == false)
                return;

            var cacheService = InstanceCacheService();

            cacheService.Flush(key);
        }


        private static bool IsCacheActive()
        {
            return true;
            //return SettingHelper.IsCacheActive;
        }


        private static BaseCache InstanceCacheService()
        {
            if (!IsCacheActive())
                return null;

            //如果之後要更換成 redisCache 應該改一下這邊就好
            return InstanceCacheByConfigSetting();
        }
        

        private static BaseCache InstanceCacheByConfigSetting()
        {
            CacheInstanceTypeEnum cacheType = (CacheInstanceTypeEnum)SettingHelper.CacheInstanceType;
            return InstanceCacheByType(cacheType);
        }


        public static BaseCache InstanceCacheByType(CacheInstanceTypeEnum cacheType)
        {
            switch (cacheType)
            {
                case CacheInstanceTypeEnum.RuntimeCache:
                    return new RuntimeCache();

                default:
                    throw new NotImplementedException();
            }
        }


        public static TimeSpan GetValidExpireTime(TimeSpan? expiredTime)
        {
            if (expiredTime.HasValue == false)
                expiredTime = _defaultCacheTime;

            //if (SettingHelper.isDevelopEnv)
            //{
            //    //如果是測試環境 時間縮成10分之一 (避免開發上遇到一些 cache更新問題)
            //    double totalSeconds = (expiredTime.Value.TotalSeconds / 10) + 1;
            //    expiredTime = new TimeSpan(0, 0, (int)totalSeconds);
            //}

            return expiredTime.Value;
        }
    }
}
