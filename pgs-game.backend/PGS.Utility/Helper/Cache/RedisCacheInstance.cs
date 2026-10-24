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

namespace Lck.Utility.Helper
{

    public enum CacheInstanceCategory
    {
        /// <summary>
        /// 一般常用資料
        /// </summary>
        Redis = 1,

        /// <summary>
        /// 比較雜亂、注單型的cache會存在這個
        /// 如果有刪除的需求 (e.g. 後台更新資料後 要刷新cache) 可能不適合用這個
        /// </summary>
        Redis002 = 2,


        /// <summary>
        /// VendorAPI MerchantIP log
        /// </summary>
        Redis003 = 3,
    }


    public class RedisCacheInstance
    {
        public Guid UUID { get; set; }


        private CacheInstanceCategory _category { get; set; }



        public RedisCacheInstance(CacheInstanceCategory category)
        {
            _category = category;
            UUID = Guid.NewGuid();
        }


        protected static TimeSpan _defaultCacheTime
        {
            get
            {
                return new TimeSpan(0, 2, 0);
            }
        }


        public T GetCacheOrQuery<T>(string key, Func<T> func, int expiredMinute)
        {
            return GetCacheOrQuery(key, func, new TimeSpan(0, expiredMinute, 0));
        }


        /// <summary>
        /// 請注意null物件。不會存cache (因此如果查100次都是null 並沒有因cache而減低存取次數
        /// </summary>
        public T GetCacheOrQuery<T>(string key, Func<T> func, TimeSpan? expiredTime = null)
        {
            T cacheValue = GetCacheValue<T>(key);

            if (cacheValue != null)
                return cacheValue;

            T queryResult = func();

            //null的話 就不用cache存了 (請務必不能調整此邏輯)
            if (queryResult == null)
                return queryResult;

            //似乎是曾經遇過 reference value 被改變、mutable的問題 發生bug、才有這樣的寫法
            var cloneResult = queryResult.ConvertToJsonString().JsonConvertToModel<T>();

            SetCacheValue(key, cloneResult, expiredTime);
            return queryResult;
        }


        public T SetCacheOnlyWhenQueryValueMatch<T>(string key, Func<T> func, T mustMatchValue, int expiredMinutes = 5)
        {
            T cacheValue = GetCacheValue<T>(key);

            if (cacheValue != null)
                return cacheValue;

            T queryResult = func();

            //null的話 就不用cache存了
            if (queryResult == null || !queryResult.Equals(mustMatchValue))
                return queryResult;

            var cloneResult = queryResult.ConvertToJsonString().JsonConvertToModel<T>();
            SetCacheValue(key, cloneResult, new TimeSpan(0, expiredMinutes, 0));

            return queryResult;
        }


        public bool IsCacheKeyExist(string key)
        {
            var myCacheValue = GetCacheValue<string>(key);

            if (string.IsNullOrWhiteSpace(myCacheValue))
                return false;

            return true;
        }


        public T GetCacheValue<T>(string key)
        {
            var _cacheInstance = InstanceCacheByType();
            T cacheValue = _cacheInstance.Get<T>(key);
            var cloneResult = cacheValue.ConvertToJsonString().JsonConvertToModel<T>();
            return cloneResult;
        }


        /// <summary>
        /// expiredTime 帶入null，會有預設過期時間
        /// </summary>
        public bool SetCacheValue(string key, object value, TimeSpan? expiredTime)
        {
            var _cacheInstance = InstanceCacheByType();
            expiredTime = GetExpireTimeBySetting(expiredTime);
            bool isOK = _cacheInstance.Set(key, value, GetExpireTimeBySetting(expiredTime));
            return isOK;
        }


        /// <summary>
        /// 建立一個短期cache鎖 (避免重複執行...之類的locker)
        /// </summary>
        public void CheckShortCacheLockerExist(string key, int seconds = 30)
        {
            var cacheValue = GetCacheValue<string>(key);

            if (string.IsNullOrWhiteSpace(cacheValue))
            {
                string randomValue = Guid.NewGuid().ToString();
                SetCacheValue(key, randomValue, new TimeSpan(0, 0, seconds));
            }
            else
            {
                //如果當前有cache locker
                string secondsText = $"{seconds} seconds";
                throw new TerproException(ApiErrorCode.Forbidden_RequestTooManyTimes, new ErrorDetailModel(secondsText));
            }
        }


        public List<string> GetCacheKeysByKeyword(string keyword)
        {
            var _cacheInstance = InstanceCacheByType();
            return _cacheInstance.GetCacheKeysByKeyword(keyword); ;
        }


        public List<string> GetCacheKeysByPrefixKeyword(string keyword)
        {
            var _cacheInstance = InstanceCacheByType();
            return _cacheInstance.GetCacheKeysByPrefixKeyword(keyword); ;
        }



        public void RemoveCacheAll()
        {
            if (SettingHelper.IsProduction)
            {
                MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                {
                    Message = " RemoveCacheAll 被呼叫 ",
                    TelegramChannelEnum = TelegramChannelEnum.pgs_common_error
                });
            }

            var _cacheInstance = InstanceCacheByType();
            _cacheInstance.FlushAll();
        }

        public void RemoveCacheByKey(string key)
        {
            var _cacheInstance = InstanceCacheByType();
            _cacheInstance.Flush(key);
        }


        public void RemoveCacheByKeyword(string keyword)
        {
            var _cacheInstance = InstanceCacheByType();
            var queryAllKeys = GetCacheKeysByKeyword(keyword);

            foreach (var key in queryAllKeys)
            {
                _cacheInstance.Flush(key);
            }
        }


        public void RemoveCacheByPrefixKeyword(string keyword)
        {
            var _cacheInstance = InstanceCacheByType();
            var queryAllKeys = GetCacheKeysByPrefixKeyword(keyword);

            foreach (var key in queryAllKeys)
            {
                _cacheInstance.Flush(key);
            }
        }



        private BaseCache InstanceCacheByType()
        {
            if (_category == CacheInstanceCategory.Redis002)
            {
                return new RedisCache2();
            }
            else if (_category == CacheInstanceCategory.Redis003)
            {
                return new RedisCache3();
            }
            else
            {
                return new RedisCache();
            }
        }


        private TimeSpan GetExpireTimeBySetting(TimeSpan? expiredTime)
        {
            if (expiredTime.HasValue == false)
                expiredTime = _defaultCacheTime;

            return expiredTime.Value;
        }


    }
}
