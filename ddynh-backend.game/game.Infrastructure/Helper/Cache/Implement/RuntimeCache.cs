using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization.Formatters.Binary;
using example.invalid;
using game.Infrastructure.Extensions.Cache;
using System.Web.Caching;
using game.Infrastructure.Extensions.Helper;

namespace game.Infrastructure.Extensions.Cache
{
    /// <summary>
    /// 採用Redis進行Cache機制的基礎類別
    /// </summary>
    internal class RuntimeCache : BaseCache
    {

        /// <summary>
        /// 取得Cache中的物件
        /// </summary>
        public override T Get<T>(string key)
        {
            try
            {
                return (T)HttpRuntime.Cache[key];
            }
            catch
            {
                return default(T);
            }
        }


        public override bool Set(string key, object value, TimeSpan? expiredTime = null)
        {
            HttpRuntime.Cache.Add(
                key,
                value,
                null,
                DateTime.UtcNow.Add(expiredTime.Value),
                System.Web.Caching.Cache.NoSlidingExpiration,
                CacheItemPriority.Default,
                null);

            return true;
        }

        /// <summary>
        /// 清除所有Cache
        /// </summary>
        public override void FlushAll()
        {
            return;
        }


        /// <summary>
        /// 清除鍵值為key的特定Cache
        /// </summary>
        public override void Flush(string key)
        {
            try
            {
                HttpRuntime.Cache.Remove(key);
            }
            catch
            {

            }
        }

    }
}