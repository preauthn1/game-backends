using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Models.Attributes;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class EnumsValueTests : BaseTest
    {

        /// <summary>
        /// 檢查ErrroCode編號是否重複
        /// </summary>
        [TestMethod]
        public void CheckDuplicateEnumValue()
        {
            string typeName = typeof(ApiErrorCode).Name;
            var allEnums = ExtensionOfEnum.GetEnumAllValue<ApiErrorCode>();

            //檢查是否是6位數字編號
            foreach(ApiErrorCode item in allEnums)
            {
                //特定的code編號比較特殊 不檢查
                if(item == ApiErrorCode.Success || item == ApiErrorCode.SuccessOK)
                {
                    continue;
                }

                int myValue = (int)item;

                if(myValue < 400000)
                {
                    Assert.Fail($"{typeName} {myValue} 數字過小，編號定義少了一位數？");
                }
                if (myValue > 999900)
                {
                    Assert.Fail($"{typeName} {myValue} 數字過大，編號定義多了一位數？");
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
            if(duplocateItems.Any(x=> x.Count > 1))
            {
                var matchKey = duplocateItems.FirstOrDefault().Key;
                Assert.Fail($"編號 {matchKey} 重複了。");
            }

        }

        /// <summary>
        /// 檢查Enum編號是否重複
        /// </summary>
        [TestMethod]
        public void CheckDuplicateEnumValue_DefinedEnums()
        {
            //這邊有定義了不少 重要enums
            var pgsEnums = BackendUtilityHelper.GetAutoGeneratei18nKeysEnums();

            foreach(Type eachEnum in pgsEnums)
            {
                string enumName = eachEnum.Name;

                if (eachEnum == typeof(ApiErrorCode))
                    continue;

                List<int> enumValues = new List<int>();

                foreach (var enumValue in System.Enum.GetValues(eachEnum))
                {
                    enumValues.Add((int)enumValue);
                }

                var duplocateItems = enumValues
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
                    Assert.Fail($"{enumName} 編號 {matchKey} 重複了。");
                }
            }

            return;
        }


        /// <summary>
        /// 檢查編號是否位數有誤
        /// </summary>
        [TestMethod]
        public void Check_MerchantCustomizedSettingKeyEnum_EnumValue()
        {
            var allEnums = ExtensionOfEnum.GetEnumAllValue<MerchantCustomizedSettingKeyEnum>();

            foreach (MerchantCustomizedSettingKeyEnum item in allEnums)
            {
                if(item.ToString().Contains("ConvertRate"))
                {
                    int parseInt = (int)item;

                    if(parseInt < 9000000 && parseInt > 9999900)
                    {
                        Assert.Fail("編號數字必須在定義的範圍內");
                    }
                }
            }
        }


        /// <summary>
        /// 檢查編號是否有異動
        /// </summary>
        [TestMethod]
        public void ConfirmEnumValue_IsNotChange()
        {
            bool isNumberMatch = BackendButtonEnum.EnableEditEverything.ToInteger() == 9901;

            if(!isNumberMatch)
            {
                Assert.Fail("此編號於後台前端有寫死用到 不建議修改");
            }
        }


        /// <summary>
        /// 檢查編號是否有異動
        /// </summary>
        [TestMethod]
        public void EventPrizeTypeEnum_IsDefine()
        {
            var eventPrizeTypes = ExtensionOfEnum.GetEnumAllValue<EventPrizeTypeEnum>();

            foreach (var myValue in eventPrizeTypes)
            {
                bool isDefine = EventDefinitionHelper.IsVisibleDefine(myValue);

                if(!isDefine)
                {
                    Assert.Fail($"EventPrizeTypeEnum: {myValue} 沒定義。請確保 EventDefinitionHelper 建構式有定義新增的type ");
                }
            }

            return;
        }


        /// <summary>
        /// 檢查編號是否對應得起來
        /// </summary>
        [TestMethod]
        public void CheckMerchantSettingKeyMappingVendorConfigSetting()
        {
            var allEnums = ExtensionOfEnum.GetEnumAllValue<MerchantCustomizedSettingKeyEnum>();

            foreach(var item in allEnums)
            {
                var dataCategory = item.GetAttributeFromEnum<DataCategoryAttribute>();

                if(dataCategory == null)
                {
                    Assert.Fail("請標記 DataCategoryAttribute ");
                }

                if(dataCategory.Value == DataCategoryEnum.GameVendor_ReplaceSetting)
                {
                    int myEnumID = item.ToInteger();
                    var mappingVendorConfigSetting = (VendorConfigSettingEnum)myEnumID;

                    if (!mappingVendorConfigSetting.IsDefinedValue())
                    {
                        Assert.Fail($"編號 {myEnumID} 沒有對應的 VendorConfigSettingEnum，不符合設計原則");
                    }
                }
            }
        }

    }
}
