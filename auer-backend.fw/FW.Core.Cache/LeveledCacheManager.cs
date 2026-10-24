using System;

namespace FW.Core.Cache
{
    public class LeveledCacheManager : ICacheManager
    {
        private ICacheManager _localCache;
        private ICacheManager _removeCache;
        private int _localedTime;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="localCache"></param>
        /// <param name="remoteCache"></param>
        /// <param name="localedTime"></param>
        public LeveledCacheManager(ICacheManager localCache, ICacheManager remoteCache, int localedTime = 1000 * 60 * 10)
        {
            FW.Core.Helper.Check.NotEmpty<ICacheManager>(localCache, "localCache");
            FW.Core.Helper.Check.NotEmpty<ICacheManager>(remoteCache, "remoteCache");

            _localCache = localCache;
            _removeCache = remoteCache;
            _localedTime = localedTime;
        }

        public TObject GetObject<TObject>(string key)
        {
            try
            {
                if (_localCache.IsCached(key))
                {
                    return _localCache.GetObject<TObject>(key);
                }

                var data = _removeCache.GetObject<TObject>(key);
                if (data != null)
                {
                    _localCache.Add(key, data, _localedTime);
                }

                return data;
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
                _localCache.Add(key, data, cacheTime);
                _removeCache.Add(key, data, cacheTime);
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
                if (_localCache.IsCached(key))
                {
                    return true;
                }

                return _removeCache.IsCached(key);
            }
            catch(System.Exception ex)
            {
                throw new CacheException("检查缓存错误," + ex.Message, ex);
            }
        }

        public void Remove(string key)
        {
            try
            {
                _localCache.Remove(key);
                _removeCache.Remove(key);
            }
            catch (System.Exception ex)
            {
                throw new CacheException("移除缓存错误," + ex.Message, ex);
            }
        }
    }
}
