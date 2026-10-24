using System;


namespace FW.Core.Cache
{
    public class EmptyCacheManager : ICacheManager
    {

        public TObject GetObject<TObject>(string key)
        {
            return default(TObject);
        }

        public void Add(string key, object data, int cacheTime)
        {
            
        }

        public bool IsCached(string key)
        {
            return false;
        }

        public void Remove(string key)
        {
            
        }
    }
}
