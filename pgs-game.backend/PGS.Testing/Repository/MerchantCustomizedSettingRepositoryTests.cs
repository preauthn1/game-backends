using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.Attributes;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class MerchantCustomizedSettingRepositoryTests : BaseTest
    {

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }


        [TestMethod]
        public void GetMerchantSettingByCache_Test()
        {
            //故意寫個一個不存在merchantID
            int merchantID_unknow = -9999;
            var query1 = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID_unknow, MerchantCustomizedSettingKeyEnum.GameSetting_IsSlotsGameRoomLock);
            var query2 = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID_unknow, MerchantCustomizedSettingKeyEnum.GameSetting_IsAutoGenerateBackurl);
            var query3 = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID_unknow, MerchantCustomizedSettingKeyEnum.GameVendor_ConvertRate_Transfer);

            //檢查是否返回預設物件 並且defaultValue = 設置的  (此邏輯不可修改 不然會有大量錯誤)
            Assert.IsNotNull(query1);
            Assert.IsNotNull(query2);

            var defaultValueSetting1 = ((MerchantCustomizedSettingKeyEnum)query1.CustomizedSettingKey).GetAttributeFromEnum<SettingDefaultValue>();
            var defaultValueSetting2 = ((MerchantCustomizedSettingKeyEnum)query2.CustomizedSettingKey).GetAttributeFromEnum<SettingDefaultValue>();

            Assert.IsTrue(defaultValueSetting1.Value == query1.SettingValue);
            Assert.IsTrue(defaultValueSetting2.Value == query2.SettingValue);
            Assert.IsTrue(query3.SettingValue == "1");  //這邊預設是1 永遠不能改此邏輯

            return;
        }



        [TestMethod]
        public void Update_Test()
        {
            int merchantID = _TEST_DATA.Default.MerchantID;
            var settingKey = MerchantCustomizedSettingKeyEnum.Common_LobbyBrandingName;
            string beforeValue = DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS() + "1";
            string afterValue = DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS() + "2";

            MerchantCustomizedSettingRepository.CreateOrUpdate(merchantID, settingKey.ToInteger(), beforeValue);

            var queryExist1 = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
                {
                    MerchantIDs = merchantID.ToListCollection(),
                    CustomizedSettingKeyIDs = settingKey.ToInteger().ToListCollection()
                })
                .FirstOrDefault();
                
            Assert.IsTrue(queryExist1.SettingValue == beforeValue);

            queryExist1.SettingValue = afterValue;

            MerchantCustomizedSettingRepository.Update(queryExist1);

            var queryExist2 = MerchantCustomizedSettingRepository.GetMerchantSettingKeyWithoutCache(merchantID, settingKey);

            Assert.IsTrue(queryExist2.SettingValue == afterValue);
            return;
        }


    }
}

