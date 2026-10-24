using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Lck.Utility.Extensions.Cache
{
    internal class PGSMemoryCache : BaseCache
    {
        public static MemoryCache _memoryCache { get; set; }

        public static HashSet<string> _allCacheKeys { get; set; }


        private static object _threadLocker = new Object();


        static PGSMemoryCache()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _allCacheKeys = new HashSet<string>();
        }

        private MemoryCache InstanceCache()
        {
            return _memoryCache;
        }

        /// <summary>
        /// 取得Cache中的物件
        /// </summary>
        public override T Get<T>(string key)
        {
            try
            {
                var cacheInstance = InstanceCache();
                return cacheInstance.Get<T>(key);
            }
            catch
            {
                return default(T);
            }
        }


        public override bool Set(string key, object value, TimeSpan expiredTime)
        {
            var cacheInstance = InstanceCache();

            AddKeyToGlobalKeysArray(key);
            cacheInstance.Set(key, value, expiredTime);
            return true;
        }



        private void AddKeyToGlobalKeysArray(string key)
        {
            try
            {
                _allCacheKeys.Add(key);
            }
            catch
            {
                //失敗就算了 不致命的問題(某一組cacheKey無法加進去管理而已)
            }
        }


        /// <summary>
        /// 清除所有Cache
        /// </summary>
        public override void FlushAll()
        {
            MemoryCache cacheInstance = InstanceCache();

            foreach (string key in _allCacheKeys)
            {
                Flush(key);
            }

            //清空還原
            _allCacheKeys = new HashSet<string>();
            return;
        }


        /// <summary>
        /// 清除鍵值為key的特定Cache
        /// </summary>
        public override void Flush(string key, bool isAsync = false)
        {
            try
            {
                var cacheInstance = InstanceCache();
                cacheInstance.Remove(key);
            }
            catch
            {

            }
        }


        /// <summary>
        /// 關鍵字找出特定keys
        /// </summary>
        public override List<string> GetCacheKeysByKeyword(string keyword)
        {
            var keys = _allCacheKeys.Where(x => x.Contains(keyword)).ToList();
            return keys;
        }

        /// <summary>
        /// 關鍵字找出特定keys
        /// </summary>
        public override List<string> GetCacheKeysByPrefixKeyword(string keyword)
        {
            var keys = _allCacheKeys.Where(x => x.Contains(keyword)).ToList();
            return keys;
        }


        public override async Task FlushAsync(string key)
        {
            try
            {
                var cacheInstance = InstanceCache();
                cacheInstance.Remove(key);
            }
            catch (Exception ex)
            {

            }
        }

    }
}