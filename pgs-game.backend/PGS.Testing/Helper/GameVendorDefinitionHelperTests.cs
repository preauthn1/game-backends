using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class GameVendorDefinitionHelperTests : BaseTest
    {


        [TestMethod]
        public void ConvertGameVendorSupportLanguageCode_AT_Test()
        {
            int myGameVendor = GameVendorTypeEnum.AT.ToInteger();

            var finalSupportLangAGin_1 = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode
                (
                    myGameVendor,
                    LanguageEnum.zh_tw
                );

            var finalSupportLangAGin_2 = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode
                (
                    myGameVendor,
                    LanguageEnum.th_th
                );

            var finalSupportLangAGin_3 = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode
                (
                    myGameVendor,
                    LanguageEnum.en_us
                );

            Assert.IsTrue(finalSupportLangAGin_1 == "cht");
            Assert.IsTrue(finalSupportLangAGin_2 == "thai");
            Assert.IsTrue(finalSupportLangAGin_3 == "en");
            return;
        }



        [TestMethod]
        public void ConvertGameVendorSupportLanguageCode_AGin_Test()
        {
            int myGameVendor = GameVendorTypeEnum.AGin.ToInteger();

            var finalSupportLangAGin_1 = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode
                (
                    myGameVendor,
                    LanguageEnum.zh_tw
                );

            var finalSupportLangAGin_2 = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode
                (
                    myGameVendor,
                    LanguageEnum.vi_vn
                );

            var finalSupportLangAGin_3 = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode
                (
                    myGameVendor,
                    LanguageEnum.en_us
                );

            Assert.IsTrue(finalSupportLangAGin_1 == "1");
            Assert.IsTrue(finalSupportLangAGin_2 == "8");
            Assert.IsTrue(finalSupportLangAGin_3 == "3");
            return;
        }



        [TestMethod]
        public void ConvertGameVendorSupportLanguageCode_Test()
        {
            var finalSupportLang1 = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode
                (
                    GameVendorTypeEnum.AGG.ToInteger(), 
                    LanguageEnum.id_id
                );

            Assert.IsTrue(finalSupportLang1 == LanguageEnum.en_us.ToDashLangCode());

            var finalSupportLang2 = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode
                (
                    GameVendorTypeEnum.AGG.ToInteger(),
                    LanguageEnum.zh_cn
                );

            Assert.IsTrue(finalSupportLang2 == LanguageEnum.zh_cn.ToDashLangCode());

            var finalSupportLang3 = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode
                (
                    GameVendorTypeEnum.CrazyFishing.ToInteger(),
                    LanguageEnum.th_th
                );

            Assert.IsTrue(finalSupportLang3 == LanguageEnum.en_us.ToDashLangCode());
            return;
        }


        [TestMethod]
        public void BasicGameVendorInfo_Test()
        {
            var aggSetting = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(GameVendorTypeEnum.AGG, true);

            Assert.IsTrue(aggSetting.ServerIPs.CountOrZero() >= 10);
            Assert.IsTrue(aggSetting.GameVendorTableName.CountOrZero() >= 10);
            Assert.IsTrue(aggSetting.IsSupportExternalLinkScreenShot);

            return;
        }




        [TestMethod]
        public void SupportFlag_Test()
        {
            Assert.IsTrue(GameVendorDefinitionHelper.IsSupportExternalLinkUrl(GameVendorTypeEnum.AGG));
            Assert.IsFalse(GameVendorDefinitionHelper.IsSupportExternalLinkUrl(GameVendorTypeEnum.CrazyFishing));


            Assert.IsTrue(GameVendorDefinitionHelper.HasTaxAmount(GameVendorTypeEnum.AGGLive));
            Assert.IsFalse(GameVendorDefinitionHelper.HasTaxAmount(GameVendorTypeEnum.AGG));
            Assert.IsFalse(GameVendorDefinitionHelper.HasTaxAmount(GameVendorTypeEnum.CrazyFishing));
            Assert.IsFalse(GameVendorDefinitionHelper.HasTaxAmount(GameVendorTypeEnum.Funky));


            Assert.IsTrue(GameVendorDefinitionHelper.IsCheckBetRecordNoByCacheWhenPlaceBet(GameVendorTypeEnum.AGG ));
            Assert.IsFalse(GameVendorDefinitionHelper.IsCheckBetRecordNoByCacheWhenPlaceBet(GameVendorTypeEnum.AGGLive));
            Assert.IsFalse(GameVendorDefinitionHelper.IsCheckBetRecordNoByCacheWhenPlaceBet(GameVendorTypeEnum.CrazyFishing));
            Assert.IsFalse(GameVendorDefinitionHelper.IsCheckBetRecordNoByCacheWhenPlaceBet(GameVendorTypeEnum.Funky));


            Assert.IsTrue(GameVendorDefinitionHelper.IsSupportRTPConfig(GameVendorTypeEnum.AGG));
            Assert.IsFalse(GameVendorDefinitionHelper.IsSupportRTPConfig(GameVendorTypeEnum.AGGLive));
            Assert.IsFalse(GameVendorDefinitionHelper.IsSupportRTPConfig(GameVendorTypeEnum.CrazyFishing));
            Assert.IsFalse(GameVendorDefinitionHelper.IsSupportRTPConfig(GameVendorTypeEnum.Funky));

            return;
        }

        [TestMethod]
        public void IsExecuteBonusSettleProcess_Test()
        {
            Assert.IsTrue(GameVendorExtraLogicHelper.IsExecuteBonusSettleProcess(GameVendorTypeEnum.AGG, 0));
            Assert.IsTrue(GameVendorExtraLogicHelper.IsExecuteBonusSettleProcess(GameVendorTypeEnum.target, 0));
            Assert.IsFalse(GameVendorExtraLogicHelper.IsExecuteBonusSettleProcess(GameVendorTypeEnum.target, 123));
            Assert.IsTrue(GameVendorExtraLogicHelper.IsExecuteBonusSettleProcess(GameVendorTypeEnum.Jili, 0));
            Assert.IsFalse(GameVendorExtraLogicHelper.IsExecuteBonusSettleProcess(GameVendorTypeEnum.ClotPlay, 0));
            Assert.IsFalse(GameVendorExtraLogicHelper.IsExecuteBonusSettleProcess(GameVendorTypeEnum.ClotPlay, 123));

            return;
        }




    }
}

