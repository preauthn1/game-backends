using System;
using System.Collections;
using System.Web;
using FW.Core.Helper;


namespace FW.Core.Cache
{
    public class HttpRequestCacheManager : ICacheManager
    {
        private IDictionary cacheItems
        {
            get
            {
                return HttpContext.Current.Items;
            }
        }

        public TObject GetObject<TObject>(string key)
        {
            try
            {
                if (cacheItems.Contains(key))
                {
                    return (TObject)cacheItems[key];
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
                cacheItems.Set(key, data);
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
                return cacheItems.Contains(key);
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
                cacheItems.Remove(key);
            }
            catch (System.Exception ex)
            {
                throw new CacheException("移除缓存错误." + ex.Message, ex);
            }
        }
    }
}
