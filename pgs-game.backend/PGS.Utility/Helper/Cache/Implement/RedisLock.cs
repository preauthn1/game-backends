using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization.Formatters.Binary;
using example.invalid;
using StackExchange.Redis;
using Lck.Utility.Extensions;
using Newtonsoft.Json;
using target.Models.Models;
using target.Utility;
using example.invalid;
using System.Threading;
using target.Utility.Utility.ThirdParty;
using System.Threading.Tasks;
using RedLockNet.SERedis.Configuration;
using RedLockNet.SERedis;

namespace Lck.Utility.Extensions.Cache
{

    public static class RedisLockHelper
    {
        private static RedLockFactory _lockFactory;


        static RedisLockHelper()
        {
            //var endPoints = new List<RedLockEndPoint>
            //{
            //    new RedLockEndPoint(new EndPoint("" , "")),
            //    new DnsEndPoint("redis2", 6379),
            //    new DnsEndPoint("redis3", 6379)
            //};

            //var redlockFactory = RedLockFactory.Create(endPoints);
        }


        public static void GetLockScope()
        {
            //var resource = "lock_key";//lock object
            //var expiry = TimeSpan.FromSeconds(30);//lock object 失效時間
            //var wait = TimeSpan.FromSeconds(10);//放棄重試時間
            //var retry = TimeSpan.FromSeconds(1);//重試間隔時間

            //// blocks 直到取得 lock 資源或是達到放棄重試時間

            //using (var redLock = redlockFactory.CreateLock(resource, expiry, wait, retry))
            //{

            //}

        }





    }
}