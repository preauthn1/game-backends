//using Newtonsoft.Json;
//using System;

//namespace game.Infrastructure.Extensions.Cache
//{
//    /// <summary>
//    /// 採用Redis進行Cache機制的基礎類別
//    /// </summary>
//    public class RedisCache : BaseCache
//    {      
//        private static Lazy<ConnectionMultiplexer> Connection;

//        private int _dbNumber;        

//        public IDatabase redisDB
//        {
//            get
//            {
//                return Connection.Value.GetDatabase(_dbNumber);
//            }
//        }
        

//        public RedisCache()
//        {
//            InitialRedis();
//        }


//        private void InitialRedis()
//        {
//            //若已經初始化的話 就不再重新連線
//            if(Connection == null)
//            {
//                string configSetting = System.Configuration.ConfigurationManager.AppSettings["RedisConn"];

//                if (string.IsNullOrWhiteSpace(configSetting))
//                    throw new LckException(Enums.LckExceptionEnum.MissConfigSetting, "RedisConn");

//                int timeoutMilliseconds = 30000;

//                ConfigurationOptions redisConfig = ConfigurationOptions.Parse(configSetting);
//                redisConfig.ConnectTimeout = timeoutMilliseconds;
//                redisConfig.ResponseTimeout = timeoutMilliseconds;
//                redisConfig.SyncTimeout = timeoutMilliseconds;
                
//                Connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(redisConfig));
//            }

//            string configSettingDBNumber = System.Configuration.ConfigurationManager.AppSettings["RedisDBNumber"];

//            _dbNumber = 0;

//            //若有指定dbNumber
//            if (string.IsNullOrWhiteSpace(configSettingDBNumber) == false)
//                _dbNumber = Int32.Parse(configSettingDBNumber);

//        }
        
//        public override T Get<T>(string key)
//        {
//            try
//            {
//                if(typeof(T) == typeof(byte[]))
//                    return (dynamic)redisDB.StringGet(key);

//                string jsonText = redisDB.StringGet(key);

//                if (typeof(T) == typeof(string))
//                    return (dynamic)jsonText;

//                return DeserializeObject<T>(jsonText);
//            }
//            catch(Exception ex)
//            {
//                return default(T);
//            }
//        }


//        public override bool Set(string key, object value, TimeSpan? expiredTime = null)
//        {
//            try
//            {               
//                string jsonText = null;
//                if (value != null)
//                {
//                    Type myValueType = value.GetType();

//                    if(myValueType == typeof(byte[]))
//                    {
//                        return SetValue(key, (byte[])value, expiredTime);
//                    }

//                    if (myValueType == typeof(string))
//                    {
//                        jsonText = value.ToString();
//                    }
//                    else
//                    {
//                        jsonText = JsonConvert.SerializeObject(value);
//                    }
//                }

//                return SetValue(key, jsonText, expiredTime);

//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//        }


//        private bool SetValue(string key, RedisValue jsonValue, TimeSpan? expiredTime = null)
//        {
//            if(expiredTime == null)
//            {
//                return redisDB.StringSet(key, jsonValue);
//            }
            
//            return redisDB.StringSet(key, jsonValue, expiredTime.Value);
//        }
        

//        /// <summary>
//        /// 清除所有Cache
//        /// </summary>
//        public override void FlushAll()
//        {
//            throw new NotImplementedException();
//        }

//        /// <summary>
//        /// 清除鍵值為key的特定Cache
//        /// </summary>
//        public override void Flush(string key)
//        {
//            redisDB.KeyDelete(key);
//        }

//        protected static T DeserializeObject<T>(string jsonText)
//        {
//            if (typeof(T) == typeof(string))
//                return (dynamic)jsonText;

//            return JsonConvert.DeserializeObject<T>(jsonText);
//        }
//    }
//}