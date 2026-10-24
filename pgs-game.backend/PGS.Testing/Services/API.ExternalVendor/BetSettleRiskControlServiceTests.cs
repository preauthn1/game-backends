using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class BetSettleRiskControlServiceTests : BaseTest
    {

        public IBetSettleRiskControlService BetSettleRiskControlService { get; set; }


        /// <summary>
        /// 這隻是拿來產生json用的模板
        /// </summary>
        [TestMethod]
        public void Debug123_Test()
        {
            var setting = InstanceRuleAndAction_WhenRateLargeThan_ThenFakeResponseSuccess();

            foreach (var target in setting.RuleTargetDefines)
            {
                foreach (var item in target.Rules)
                {
                    item.RuleType = RiskControlRuleTypeEnum.PayoutRateLargeThan.ToInteger();
                    item.CompareNumericValueLargeThan = 1.2M;
                }

                target.Action.IsNotify = true;
                target.Action.IsResponseFakeSuccessButPendingRecordStatus = false;
            }


            string json = setting.ConvertToJsonString();
            return;
        }


        [TestMethod]
        public void CheckRiskControl_PayoutRate_Test()
        {
            var fakePara = InstanceTestData(100 , 500);

            var setting = InstanceRuleAndAction_WhenRateLargeThan_ThenFakeResponseSuccess();
            var fakeEntity = new RiskControlConfigEntity
            {
                Sequence = 1,
                SettingJson = setting.ConvertToJsonString(),
                Status = 1,
                DisplayName = "MSTest",
                RiskControlConfigID = 999999,
            };

            var check = BetSettleRiskControlService.CheckRiskControlByEntity(fakeEntity , fakePara);

            Assert.IsNotNull(check);
            Assert.IsTrue(check.IsResponseFakeSuccessButPendingRecordStatus);
            return;
        }


        [TestMethod]
        public void CheckRiskControl_PayoutAmount_Test()
        {
            var fakePara = InstanceTestData(10000, 15000);

            var setting = InstanceRuleAndAction_WhenRateLargeThan_ThenFakeResponseSuccess();

            foreach(var target in setting.RuleTargetDefines)
            {
                foreach(var item in target.Rules)
                {
                    item.RuleType = RiskControlRuleTypeEnum.PayoutAmountLargeThan.ToInteger();
                    item.CompareNumericValueLargeThan = 14999;
                }
            }

            var fakeEntity = new RiskControlConfigEntity
            {
                Sequence = 1,
                SettingJson = setting.ConvertToJsonString(),
                Status = 1,
                DisplayName = "MSTest",
                RiskControlConfigID = 999999,
            };

            var check = BetSettleRiskControlService.CheckRiskControlByEntity(fakeEntity, fakePara);

            Assert.IsNotNull(check);
            Assert.IsTrue(check.IsResponseFakeSuccessButPendingRecordStatus);
            return;
        }


        [TestMethod]
        public void CheckRiskControl_IsValidFalse_Test()
        {
            var fakePara = InstanceTestData(1000, 500);

            var setting = InstanceRuleAndAction_WhenRateLargeThan_ThenFakeResponseSuccess();
            var fakeEntity = new RiskControlConfigEntity
            {
                Sequence = 1,
                SettingJson = setting.ConvertToJsonString(),
                Status = 1,
                DisplayName = "MSTest",
                RiskControlConfigID = 999999,
            };

            var check = BetSettleRiskControlService.CheckRiskControlByEntity(fakeEntity, fakePara);

            Assert.IsNotNull(check);
            Assert.IsFalse(check.IsResponseFakeSuccessButPendingRecordStatus);
            return;
        }



        private RiskControlSettingJsonModel InstanceRuleAndAction_WhenRateLargeThan_ThenFakeResponseSuccess()
        {
            return new RiskControlSettingJsonModel
            {
                RuleTargetDefines = new List<RiskControlRuleTargerBundleModel>
                {
                    new RiskControlRuleTargerBundleModel
                    {
                        Rules = new List<RiskControlSettingRuleModel>
                        {
                            new RiskControlSettingRuleModel
                            {
                                RuleType = RiskControlRuleTypeEnum.PayoutRateLargeThan.ToInteger(),
                                CompareNumericValueLargeThan = 2 //2倍
                            },
                        },
                        Action = new RiskControlSettingActionModel
                        {
                            IsResponseFakeSuccessButPendingRecordStatus = true
                        }
                    }
                }
            };
        }





        private RiskControlCheckParameterModel InstanceTestData(decimal betAmount, decimal payoutAmount)
        {
            return new RiskControlCheckParameterModel
            {
                BetRecordNoPGS = Guid.NewGuid().ToString(),
                BetAmount = betAmount,
                PayoutAmount = payoutAmount,
                GameID = _TEST_DATA.ExternalGameVendor.GameID,
                MerchantID = _TEST_DATA.NotSuperMerchant.MerchantID,
                PlayerID = _TEST_DATA.NotSuperMerchant.PlayerID
            };
        }


    }
}

