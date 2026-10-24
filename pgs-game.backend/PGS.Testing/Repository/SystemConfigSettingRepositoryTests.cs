using Lck.Utility.Extensions.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.Core.Repository;
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
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class SystemConfigSettingRepositoryTests : BaseTest
    {

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }


        [TestMethod]
        public void GetParseModelByEnum_DefaultValue_Test()
        {
            var targetEnum = SystemConfigSettingEnum.JobSetting_AGGPendingWagerCheckEnable;

            string defaultValue = targetEnum.GetAttributeFromEnum<SettingDefaultValue>()?.Value;
            string newValue = NormalYesOrNoEnum.No.ToIntegerString();

            //先清空
            SystemConfigSettingRepository.Delete(targetEnum.ToInteger());

            var query1_1 = SystemConfigSettingRepository.GetParseModelByEnum(targetEnum);
            var query1_2 = SystemConfigSettingRepository.GetParseModelByCache(targetEnum);

            //無資料 => 預設值
            Assert.IsTrue(defaultValue == query1_1.SettingValue);
            Assert.IsTrue(defaultValue == query1_2.SettingValue);

            //清cache
            CacheRemoveHelper.RemoveCache_WhenUpdateSystemSetting(targetEnum.ToInteger());


            //塞資料
            SystemConfigSettingRepository.Create(new Models.Table.SystemConfigSettingEntity
            {
                SettingValue = newValue,
                ID = targetEnum.ToInteger(),
            });

            var query2_1 = SystemConfigSettingRepository.GetParseModelByEnum(targetEnum);
            var query2_2 = SystemConfigSettingRepository.GetParseModelByCache(targetEnum);

            //有資料 => db設定的值
            Assert.IsTrue(newValue == query2_1.SettingValue);
            Assert.IsTrue(newValue == query2_2.SettingValue);

            //測試結束 還原資料
            SystemConfigSettingRepository.Delete(targetEnum.ToInteger());
            return;
        }

    }
}

