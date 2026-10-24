using System;
using System.Collections.Generic;
using game.Entity;
using game.Service.DAO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using FW.Core.Repository;
using game.Entity.ViewModels;
using game.Infrastructure.Extensions;
using System.Reflection;
using game.Infrastructure.Extensions;
using game.Entity.Event;

namespace game.Test
{
    [TestClass]
    public class EnumsValuesTests
    {

        [TestMethod]
        public void CheckEnumsValue_EventRuleTypeEnum_Duplicate()
        {
            var allEnums = ExtensionOfEnum.GetEnumAllValue<EventRuleTypeEnum>();
            
            var duplocateItems = allEnums
                    .GroupBy(x => (int)x)
                    .Where(x => x.Count() > 1)
                    .Select(x => new
                    {
                        Key = x.Key,
                        Count = x.Count()
                    });

            //檢查編號是否有重複定義
            if (duplocateItems.Any(x => x.Count > 1))
            {
                var matchKey = duplocateItems.FirstOrDefault().Key;
                Assert.Fail($"編號 {matchKey} 重複了。");
            }
        }


        [TestMethod]
        public void CheckEnumsValue_ApiErrorCode_Duplicate()
        {
            var allEnums = ExtensionOfEnum.GetEnumAllValue<ApiErrorCode>();
            
            //檢查是否是6位數字編號
            foreach (ApiErrorCode item in allEnums)
            {
                int myValue = (int)item;

                if (myValue < 400000)
                {
                    Assert.Fail($"{myValue} 數字過小，編號定義少了一位數？");
                }
                if (myValue > 900000)
                {
                    Assert.Fail($"{myValue} 數字過大，編號定義多了一位數？");
                }
            }

            var duplocateItems = allEnums
                    .GroupBy(x => (int)x)
                    .Where(x => x.Count() > 1)
                    .Select(x => new
                    {
                        Key = x.Key,
                        Count = x.Count()
                    });

            //檢查編號是否有重複定義
            if (duplocateItems.Any(x => x.Count > 1))
            {
                var matchKey = duplocateItems.FirstOrDefault().Key;
                Assert.Fail($"編號 {matchKey} 重複了。");
            }
        }


    }


}
