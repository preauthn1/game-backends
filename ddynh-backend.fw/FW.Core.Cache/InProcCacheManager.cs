using System;
using System.Runtime.Caching;

namespace FW.Core.Cache
{
    public class InProcCacheManager : ICacheManager
    {
        private ObjectCache innerCache
        {
            get
            {
                return MemoryCache.Default;
            }
        }

        public TObject GetObject<TObject>(string key)
        {
            try
            {
                if (innerCache.Contains(key))
                {
                    return (TObject)innerCache[key];
                }

                return default(TObject);
            }
            catch (System.Exception ex)
            {
                throw new CacheException("获取缓存错误," + ex.Message, ex);
            }
        }

        public void Add(string key, object data, int cacheTime)
        {
            try
            {
                Remove(key);

                innerCache.Add(key, data, DateTime.Now.AddMilliseconds(cacheTime));
            }
            catch (System.Exception ex)
            {
                throw new CacheException("添加缓存错误," + ex.Message, ex);
            }
        }

        public bool IsCached(string key)
        {
            try
            {
                return innerCache.Contains(key);
            }
            catch (System.Exception ex)
            {
                throw new CacheException("检查缓存错误," + ex.Message, ex);
            }
        }

        public void Remove(string key)
        {
            try
            {
                if (IsCached(key))
                {
                    innerCache.Remove(key);
                }
            }
            catch (System.Exception ex)
            {
                throw new CacheException("移除缓存失败," + ex.Message, ex);
            }
        }
    }
}
