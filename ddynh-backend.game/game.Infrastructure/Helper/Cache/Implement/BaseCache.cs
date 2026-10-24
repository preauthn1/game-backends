using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace game.Infrastructure.Extensions.Cache
{
    public abstract class BaseCache
    {
        public abstract T Get<T>(string key);

        public abstract bool Set(string key, object data , TimeSpan? expiredTime = null);

        public abstract void FlushAll();

        public abstract void Flush(string key);
    }
}

