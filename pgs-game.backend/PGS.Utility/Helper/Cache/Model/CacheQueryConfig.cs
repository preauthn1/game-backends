using System;

namespace Lck.Utility.Extensions.Helper
{
    public class CacheQueryConfig
    {
        public TimeSpan? ExpiredTime { get; set; }


        /// <summary>
        /// 特殊情境專用，若要存進的cache不等於這邊指定的值 則不存入cache
        /// </summary>
        public object SetCacheOnlyWhenValueEqual { get; set; }

    }
}
