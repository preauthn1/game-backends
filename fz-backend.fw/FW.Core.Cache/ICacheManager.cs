using System;


namespace FW.Core.Cache
{
    public interface ICacheManager
    {
        /// <summary>
        /// 获取对象
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        TObject GetObject<TObject>(string key);

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cacheTime"></param>
        void Add(string key, object data, int cacheTime);

        /// <summary>
        /// 指定的缓存键是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsCached(string key);

        /// <summary>
        /// 移除缓存 
        /// </summary>
        /// <param name="key"></param>
        void Remove(string key);

    }
}
