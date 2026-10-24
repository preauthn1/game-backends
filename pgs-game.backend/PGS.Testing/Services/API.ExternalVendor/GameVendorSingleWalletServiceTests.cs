using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace target.Testing
{
    [TestClass]
    public class GameVendorSingleWalletServiceTests : BaseTest
    {
        public GameListRepository GameListRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public PerGameDetailLogTagMappingRepository PerGameDetailLogTagMappingRepository { get; set; }

        public BetRecordAGGRepository BetRecordAGGRepository { get; set; }

        public BetRecordAGGLiveRepository BetRecordAGGLiveRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public IGameVendorSingleWalletService ServiceInstance { get; set; }

        private static int _restrictModeMerchantID = _TEST_DATA.NotSuperMerchant.MerchantID;

        [TestMethod]
        public void CheckPlayerIsValidAndSystemMaintain_Test()
        {
            var service = new GameVendorSingleWalletService();
            service.CheckPlayerIsValidAndSystemMaintain(GameVendorTypeEnum.AGGFish, _TEST_DATA.Default.PlayerID);
            return;
        }


        [TestMethod]
        public void CheckAnyTagToAdd_Test()
        {
            var service = new GameVendorSingleWalletService();

            var now = DateTimeHelper.GetCurrentTime();

            decimal amount = 10;

            var perGame = new ConvertPerGameLogResultModel
            {
                PerGameDetailLog = new PerGameDetailLogEntity
                {
                    PlayerName = _TEST_DATA.Default.PlayerName,
                    PlayerID = _TEST_DATA.Default.PlayerID,
                    TransactionNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo(),
                    CreateTime = now,
                    GameTime = now,
                    MerchantID = _TEST_DATA.Default.MerchantID,
                    BetAmount = amount,
                    UpdateAmount = amount * 500,
                    PayoutAmount = amount * 500,
                    JackpotAmount = amount * 500,
                    GameID = _TEST_DATA.Default.ThirdPartyGameID,
                    ThirdSettleDate = new DateTime(2020, 1, 1),
                }
            };

            var tags = service.CheckAnyTagToAdd(perGame);

            Assert.IsTrue(tags.Contains(SystemTagEnum.PerGameDetailLog_ThirdSettleDateNotEqual));
            Assert.IsTrue(tags.Contains(SystemTagEnum.PerGameDetailLog_PayoutRate_OverThan50));
            Assert.IsTrue(tags.Contains(SystemTagEnum.PerGameDetailLog_Jackpot));


            perGame.PerGameDetailLog.JackpotAmount = 0;
            perGame.PerGameDetailLog.ThirdSettleDate = now.AddHours(8).Date;
            perGame.PerGameDetailLog.PayoutAmount = amount;

            //重置
            perGame.ExtraAddTags = new List<SystemTagEnum>();
            var tags_case2 = service.CheckAnyTagToAdd(perGame);

            Assert.IsFalse(tags_case2.Contains(SystemTagEnum.PerGameDetailLog_ThirdSettleDateNotEqual));
            Assert.IsFalse(tags_case2.Contains(SystemTagEnum.PerGameDetailLog_PayoutRate_OverThan50));
            Assert.IsFalse(tags_case2.Contains(SystemTagEnum.PerGameDetailLog_Jackpot));

            return;
        }


        [TestMethod]
        public void GetPlayerExtraInfo_AGGFish_Test()
        {
            var query = ServiceInstance.GetPlayerExtraInfo(GameVendorTypeEnum.AGGFish, _TEST_DATA.Default.PlayerID);
            Assert.IsNotNull(query);
            Assert.IsTrue(query.PlayerID > 0);
            Assert.IsTrue(query.AGGFishMerchantID.IsNotEmptyTextNotNull());
            return;
        }


        [TestMethod]
        public void GetWalletBalance_Test()
        {
            var query = ServiceInstance.GetWalletBalance(GameVendorTypeEnum.AGG, new SingleWalletGetWalletBalanceRequestParameter
            {
                PlayerID = _TEST_DATA.Default.PlayerID
            });

            Assert.IsNotNull(query);
            return;
        }


        /// <summary>
        /// 注單金額異動改單 (視訊偶爾會發生)
        /// </summary>
        [TestMethod]
        public void GameVendor_ChangeSettle_AGGLive_Test()
        {
            //這個測試略過不要了
            //這個測試略過不要了
            //這個測試略過不要了
            return;

            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.AGGLive;

            var query = BetRecordAGGLiveRepository.GetPagingByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                PageSize = 1,
                Status = BetRecordStatusEnum.SettleComplete.ToInteger(),
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-30)

            }).Items.FirstOrDefault();

            if (query == null)
                return;

            string thirdBetRecordNo = query.BetRecordNo;

            var queryPerGame = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(query.BetRecordNoPGS);

            //MSTest前 先清除tag
            PerGameDetailLogTagMappingRepository.DeleteByModel(new
            {
                PerGameDetailLogID = queryPerGame.Id,
                TagId = SystemTagEnum.PerGameDetailLog_ChangeSettleAmount
            });

            decimal newPayout = (query.BetAmount > query.PayoutAmount ? query.BetAmount : query.PayoutAmount) + 5;
            decimal newTax = RandomHelper.GetRandIntegerNumbers(100, 250);

            var resultPlaceBet = ServiceInstance.ChangeSettleAmount(targetGameVendor, new SingleWalletChangeBetPayoutRequestParameter
            {
                BetRecordNo = thirdBetRecordNo,
                GameCode = "",
                NewSettleInfo = new SingleWalletChangeSettleInfo
                {
                    PayoutAmount = newPayout,
                    TaxAmount = newTax,
                    ValidAmount = query.ValidAmount,
                }
            });

            var queryAgain = BetRecordAGGLiveRepository.GetByBetRecordNoPGS(query.BetRecordNoPGS);

            Assert.IsTrue(queryAgain.PayoutAmount == newPayout);
            Assert.IsTrue(queryAgain.TaxAmount == newTax);
            return;
        }


        /// <summary>
        /// 檢查 InstanceLoopTimesQueryTimeRange 產生出來的參數是否合乎預期
        /// </summary>
        [TestMethod]
        public void InstanceLoopTimesQueryTimeRange_Test()
        {
            var aggService = new AGGIntegrationService();
            var aggLiveService = new AGGLiveIntegrationService();

            string orderNo = Guid.NewGuid().ToString();

            var result1_AGG = aggService.InstanceLoopTimesQueryTimeRange(new Models.ViewModels.GameVendor.BetRecordInfoQueryParameter
            {
                IsQueryRecentRange = true,
                MaxQueryHourRange = 120,
                BetRecordNo = orderNo
            });

            Assert.IsTrue(result1_AGG.Count() > 1);
            Assert.IsTrue(result1_AGG.All(x => x.PartitionTimeBegin.HasValue));

            var result1_AGGLive = aggLiveService.InstanceLoopTimesQueryTimeRange(new Models.ViewModels.GameVendor.BetRecordInfoQueryParameter
            {
                IsQueryRecentRange = true,
                MaxQueryHourRange = 120,
                BetRecordNo = orderNo
            });

            if(!GameVendorDefinitionHelper.IsSupportPartitionTable(GameVendorTypeEnum.AGGLive))
            {
                Assert.IsTrue(result1_AGGLive.Count() == 1);
                Assert.IsTrue(result1_AGGLive.All(x => x.PartitionTimeBegin == null));
            }

            var result2_agg = aggService.InstanceLoopTimesQueryTimeRange(new Models.ViewModels.GameVendor.BetRecordInfoQueryParameter
            {
                IsQueryRecentRange = true,
                BetRecordNo = orderNo
            });

            Assert.IsTrue(result2_agg.Any(x => x.PartitionTimeBegin.HasValue));
            Assert.IsTrue(result2_agg.Any(x => x.PartitionTimeBegin == null));
            return;
        }

        [TestMethod]
        public void InstanceLoopTimesQueryTimeRange_FilterGame_Test()
        {
            var aggService = new AGGIntegrationService();

            string orderNo = Guid.NewGuid().ToString();
            int targetGameID = _TEST_DATA.Default.ThirdPartyGameID;

            var result1 = aggService.InstanceLoopTimesQueryTimeRange(new Models.ViewModels.GameVendor.BetRecordInfoQueryParameter
            {
                IsQueryRecentRange = true,
                MaxQueryHourRange = 120,
                BetRecordNo = orderNo,
                GameID = targetGameID,
            } , isFilterGame: true);

            Assert.IsTrue(result1.All(x => x.GameID == targetGameID));

            var result2 = aggService.InstanceLoopTimesQueryTimeRange(new Models.ViewModels.GameVendor.BetRecordInfoQueryParameter
            {
                IsQueryRecentRange = true,
                MaxQueryHourRange = 120,
                BetRecordNo = orderNo,
                GameID = targetGameID,
            }, isFilterGame: false);

            Assert.IsTrue(result2.All(x => x.GameID == null));
            Assert.IsTrue(result2.All(x => x.ThirdPartyGameCode == null));


            var result3 = aggService.InstanceLoopTimesQueryTimeRange(new Models.ViewModels.GameVendor.BetRecordInfoQueryParameter
            {
                IsQueryRecentRange = true,
                MaxQueryHourRange = 120,
                IsPlaceBetProcess = true,
                IsPartitionMatchQuery = true,
                BetRecordNo = orderNo,
                GameID = targetGameID,
            }, isFilterGame: false);

            Assert.IsTrue(result3.All(x => x.GameID == null));
            Assert.IsTrue(result3.All(x => x.ThirdPartyGameCode == null));

            var result4 = aggService.InstanceLoopTimesQueryTimeRange(new Models.ViewModels.GameVendor.BetRecordInfoQueryParameter
            {
                MaxQueryHourRange = 60,
                BetRecordNo = orderNo,
                GameID = targetGameID,

            }, isFilterGame: false);

            Assert.IsTrue(result4.All(x => x.GameID == null));
            Assert.IsTrue(result4.All(x => x.ThirdPartyGameCode == null));
            return;
        }



        /// <summary>
        /// 嚴格token模式
        /// </summary>
        [TestMethod]
        public void CheckRestrictModeAndGameMatch_Test()
        {
            int settingKey = MerchantCustomizedSettingKeyEnum.GameSetting_IsRestrictTokenMode.ToInteger();

            MerchantCustomizedSettingRepository.CreateOrUpdate(_restrictModeMerchantID, settingKey, NormalYesOrNoEnum.Yes.ToIntegerString());

            int agg = GameVendorTypeEnum.AGG.ToInteger();
            int aggLive = GameVendorTypeEnum.AGGLive.ToInteger();

            try
            {
                //相同gameID
                TestTargetGameIsPassRestrictMode(agg + 1, agg + 1, expectedCode: null);

                //AGG 不同gameID
                TestTargetGameIsPassRestrictMode(agg + 1, agg + 2);

                //AGGLive 不同gameID
                TestTargetGameIsPassRestrictMode(aggLive + 1, aggLive + 2, expectedCode: null);

                //AGGFish 不同gameID
                TestTargetGameIsPassRestrictMode(GameVendorTypeEnum.AGGFish.ToInteger() + 1, GameVendorTypeEnum.AGGFish.ToInteger() + 2, expectedCode: null);

            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                //還原數值
                MerchantCustomizedSettingRepository.CreateOrUpdate(
                    _restrictModeMerchantID,
                    MerchantCustomizedSettingKeyEnum.GameSetting_IsRestrictTokenMode.ToInteger(),
                    NormalYesOrNoEnum.No.ToIntegerString()
                );
            }


            return;
        }


        private void TestTargetGameIsPassRestrictMode(int tokenGameID, int currentGameID, ApiErrorCode? expectedCode = ApiErrorCode.Authorize_TokenAvailableGameNotMatch)
        {
            var service = new GameVendorSingleWalletService();

            var param = new SingleWalletPlaceBetExtraParameter
            {
                TokenGameID = tokenGameID
            };

            //預期發生錯誤 或 不發生錯誤
            if (expectedCode.HasValue)
            {
                try
                {
                    var resultCode = service.CheckRestrictModeAndGameMatch(param, _restrictModeMerchantID, currentGameID);
                    AssertFail($"{tokenGameID} vs {currentGameID} 無發生錯誤、不合預期 , {resultCode}");
                }
                catch (Exception ex)
                {
                    AssertExpectException(ex, expectedCode.Value);
                }
            }
            else
            {
                service.CheckRestrictModeAndGameMatch(param, _restrictModeMerchantID, currentGameID);
            }
        }


    }
}

