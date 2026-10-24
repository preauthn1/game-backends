using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Cache
{
    public class MyCacheHelper
    {

        public static T GetCacheOrQueryValue<T>(ICacheManager cacheInstance , string cacheKey , Func<T> Do_Query_Value , int expiredSeconds = 30)
        {
            //若資料存在 返回
            if(cacheInstance.IsCached(cacheKey))
            {
                return cacheInstance.GetObject<T>(cacheKey);
            }

            //資料不存在 重新查
            T queryValue = Do_Query_Value();

            cacheInstance.Add(cacheKey, queryValue, expiredSeconds * 1000);

            return queryValue;
        }


    }
}
