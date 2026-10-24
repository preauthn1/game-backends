using FW.Core.Helper;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;

namespace FW.Core.Cache
{
    public class RedisCacheManager : ICacheManager
    {
        private IEnumerable<string> _readWriteHosts;
        private IEnumerable<string> _readOnlyHosts;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="hosts"></param>
        public RedisCacheManager(params string[] hosts)
            :this(hosts, hosts)
        { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="readWriteHosts"></param>
        /// <param name="readOnlyHosts"></param>
        public RedisCacheManager(IEnumerable<string> readWriteHosts, IEnumerable<string> readOnlyHosts)
        {
            _readWriteHosts = readWriteHosts;
            _readOnlyHosts = readOnlyHosts;
        }

        public TObject GetObject<TObject>(string key)
        {
            try
            {
                return createManager().GetClient().Get<TObject>(key);
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

                createManager().GetClient().Add(key, data, DateTime.Now.AddMilliseconds(cacheTime));
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
                return createManager().GetClient().ContainsKey(key);
            }
            catch (System.Exception ex)
            {
                throw new CacheException("检查缓存错误,"+ ex.Message, ex);
            }
        }

        public void Remove(string key)
        {
            try
            {
                if (IsCached(key))
                {
                    createManager().GetClient().Remove(key);
                }
            }
            catch (System.Exception ex)
            {
                throw new CacheException("移除缓存错误," + ex.Message, ex);
            }
        }

        private PooledRedisClientManager createManager()
        {
            return new PooledRedisClientManager(_readWriteHosts, _readOnlyHosts,new RedisClientManagerConfig(){
            MaxWritePoolSize = 5,
            MaxReadPoolSize = 19,
            AutoStart = true
            });
        }
    }
}
