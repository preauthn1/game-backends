using Lck.Utility.Extensions.Cache;
using Lck.Utility.Extensions.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class CacheHelperTests : BaseTest
    {

        [TestMethod]
        public void RedisCache_Test()
        {
            //非公司開發電腦 不執行這組test
            if (IsNotDeveloperComputerName())
                return;

            var redis = new RedisCache();

            string key = "MS_Test";
            redis.Set(key, "MS_Test_66666", new System.TimeSpan(24, 0, 0));

            var qqq = redis.Get<string>(key);
            return;

        }

      
        [TestMethod]
        public void CheckShortCacheLockerExist_Test()
        {
            string cacheKey = "CheckShortCacheLockerExist_MSTest" + Guid.NewGuid();

            var result1 = CacheHelper.CheckShortCacheLockerExist(cacheKey, 30, isThrowEx: false);
            var result2 = CacheHelper.CheckShortCacheLockerExist(cacheKey, 30, isThrowEx: false);

            Assert.IsFalse(result1);
            Assert.IsTrue(result2);
            return;

        }


        [TestMethod]
        public void IsObjectValueMatch_Test()
        {
            object compareValue_L = "true";
            string compareValue_R = "false";
            string compareValue_R2 = "true";

            Assert.IsFalse(CacheHelper.IsObjectValueMatch(compareValue_L, compareValue_R));
            Assert.IsTrue(CacheHelper.IsObjectValueMatch(compareValue_L, compareValue_R2));

            object compareValue_L_002 = 500;
            string compareValue_R_002 = "false";

            Assert.IsFalse(CacheHelper.IsObjectValueMatch(compareValue_L_002, compareValue_R_002));
            return;

        }


        [TestMethod]
        public void GetCacheOrQuery_ValueMustEqual_Test()
        {
            string correctValue = "true";

            string cacheKey = "MSTest_Cache_" + Guid.NewGuid();

            var cacheConfig = new CacheQueryConfig
            {
                ExpiredTime = new TimeSpan(0, 30, 0),
                SetCacheOnlyWhenValueEqual = correctValue,
            };

            var result1 = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                return "XXX";

            }, cacheConfig);

            var cacheValue_001 = CacheHelper.GetCacheValue<string>(cacheKey);

            Assert.IsFalse(cacheConfig.SetCacheOnlyWhenValueEqual.Equals(cacheValue_001));

            var result2 = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                return correctValue;

            }, cacheConfig);

            var cacheValue_002 = CacheHelper.GetCacheValue<string>(cacheKey);

            Assert.IsTrue(cacheConfig.SetCacheOnlyWhenValueEqual.Equals(cacheValue_002));

            return;
        }


        /// <summary>
        /// 驗證因為 reference指標、modify物件 導致抓到cache異常的問題
        /// </summary>
        [TestMethod]
        public void GetCacheAndModify_ValueAssert_Test()
        {
            string cacheKey = "MSTest_Cache_" + Guid.NewGuid();

            var originalCacheModel = new ApiResultCheckModel
            {
                Message = "HelloText"
            };

            string expectCacheValue = originalCacheModel.Message;

            var cacheConfig = new CacheQueryConfig
            {
                ExpiredTime = new TimeSpan(0, 30, 0)
            };

            var myValue = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                return originalCacheModel;

            }, cacheConfig);

            Assert.IsTrue(myValue != null);
            Assert.IsTrue(myValue.Message == expectCacheValue);

            //嘗試修改值
            myValue.Message = Guid.NewGuid().ToString();

            Assert.IsFalse(myValue.Message.Equals(cacheConfig.SetCacheOnlyWhenValueEqual));

            var queryAgain_001 = CacheHelper.GetCacheOrQuery<ApiResultCheckModel>(cacheKey, () =>
            {
                return null;

            }, cacheConfig);

            var queryAgain_002 = CacheHelper.GetCacheValue<ApiResultCheckModel>(cacheKey);

            //驗證是否受到referce 指標的影響 (照理說根本不可能發生...以前到底是怎寫的)
            Assert.IsTrue(queryAgain_001?.Message == expectCacheValue);
            Assert.IsTrue(queryAgain_002?.Message == expectCacheValue);

            return;

        }


    }
}
