using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Linq;
using target.Models.CoreLibs;
using target.Models.ViewModels;
using target.DA.Repository;
using target.Models.Enums;
using System.Collections.Generic;
using System;

namespace target.Testing
{
    [TestClass]
    public class PerGameDetailServiceTests : BaseTest
    {
        public BetRecordAGGRepository BetRecordAGGRepository { get; set; }

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }

        public BetRecordCustomizedSettingRepository BetRecordCustomizedSettingRepository { get; set; }

        public IViewPermissionCheckService ViewPermissionCheckService { get; set; }

        public IPerGameDetailService ServiceInstance { get; set; }


        [TestMethod]
        public void PerGameDetail_GetByPagingFilter_Test()
        {
            if(IsSkipTest_WhenSmallerThanTargetTime(_TEST_DATA.Default.Date_NewGameVendor_Intergrate))
            {
                return;
            }

            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID , new QueryPerGameDetailLogApiRequestParameterModel 
            { 
                PageSize = 10,
                GameID = _TEST_DATA.Default.GameID,
                GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 7),
                GameStartTimeEnd = DateTimeHelper.GetCurrentTime().AddDays(0),
                OrderByColumn = " GameTime "
            });


            if (query.Items.IsEmptyOrNull())
                return;

            Assert_NotEmptyCollection(query.Items);

            foreach(var matchFirst in query.Items)
            {
                try
                {
                    var querySetting = BetRecordCustomizedSettingRepository.GetParseModelByUniqueKey(matchFirst.TransactionNo, BetRecordCustomizedSettingEnum.PgsGame_PerGameLogID);

                    //必須是PGS自製遊戲才有perGameID
                    long perGameLogID = querySetting.SettingValueOfBigInt;

                    if (perGameLogID <= 0)
                    {
                        Assert.Fail($"perGameLogID為0 , {matchFirst.TransactionNo} {matchFirst.GameID} {matchFirst.PlayerID}");
                    }

                    var queryFull = ServiceInstance.GetFullInformationInPerGame(_TEST_DATA.Default.UserID, perGameLogID);

                    Assert.IsNotNull(queryFull);
                    Assert_NotEmptyCollection(queryFull.PlayerDetails);
                }
                catch (Exception ex)
                {
                    //忽略timeout錯誤 太常發生了
                    if (ex.IsSqlTimeoutException())
                    {

                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return;
        }



        [TestMethod]
        public void PerGameDetail_GetByPagingFilter_PGS_SelfGame_Test()
        {
            if (IsSkipTest_WhenSmallerThanTargetTime(_TEST_DATA.Default.Date_NewGameVendor_Intergrate))
            {
                return;
            }

            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID, new QueryPerGameDetailLogApiRequestParameterModel
            {
                PageSize = 10,
                GameID = _TEST_DATA.Default.GameID,
                GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                GameStartTimeEnd = DateTimeHelper.GetCurrentTime().AddDays(0),
                OrderByColumn = " GameTime "
            });
                        
            if(query.Items.IsNotEmptyNull())
            {
                //PGS自製遊戲應該必定有值 (PerGameLogID 還是會用到)
                var invalidData_1 = query.Items.Where(z => z.PerGameDetailLogID == z.PerGameLogID).ToList();
                var invalidData_2 = query.Items.Where(z => z.PerGameLogID == 0).ToList();

                if (invalidData_1.Count() > 0)
                {
                    Assert.Fail("資料不合乎預期", string.Join("、", invalidData_1.Select(x => x.TransactionNo)));
                }

                if (invalidData_2.Count() > 0)
                {
                    Assert.Fail("資料不合乎預期, " + string.Join("、", invalidData_2.Select(x => x.TransactionNo)));
                }
            }

            return;
        }



        [TestMethod]
        public void PerGameDetail_GetByPagingFilter_ManyColumnFilter_Test()
        {
            //篩選各種欄位 看SQL是否有寫錯
            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID, new QueryPerGameDetailLogApiRequestParameterModel 
            { 
                PageSize = 1,
                TagID = SystemTagEnum.PerGameDetailLog_CrossDateSettle.ToInteger(),
                ServerID = 123,
                GameID = 123,
                PlayerID = _TEST_DATA.Default.PlayerID,
                MerchantID = 175001,
                IsJackpotOnly = true,
                GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-7),
                GameStartTimeEnd = DateTimeHelper.GetCurrentTime()
            });
            
            return;
        }



        [TestMethod]
        public void PerGameDetail_GetByPagingFilter_AgentQuery_Test()
        {
            //這個總代底下有 數百個代理
            int userID = _TEST_DATA.CreditAgent.UserID;
            int pageSize = 10;

            DateTime begin = DateTimeHelper.GetCurrentTime().AddDays(-10);
            DateTime end = DateTimeHelper.GetCurrentTime();

            var query = ServiceInstance.GetByPagingFilter(userID, new QueryPerGameDetailLogApiRequestParameterModel
            {
                PageSize = pageSize,
                GameStartTimeBegin = begin,
                GameStartTimeEnd = end
            });

            if(query.Items.IsNotEmptyNull())
            {
                var matchFirst = query.Items.FirstOrDefault();

                //預期會用brieftJoin查詢
                var query2 = ServiceInstance.GetByPagingFilter(userID, new QueryPerGameDetailLogApiRequestParameterModel
                {
                    PageSize = pageSize,
                    MerchantID = matchFirst.MerchantID,
                    GameStartTimeBegin = begin,
                    GameStartTimeEnd = end
                });

                Assert.IsTrue(query2.Items.CountOrZero() > 0);
            }

            return;
        }


        [TestMethod]
        public void PerGameDetail_GetByPagingFilter_ViewMerchantPermission_Test()
        {
            //確保各個帳號能查到的商戶是正確的
            PerGameDetail_GetByPagingFilter_ViewMerchantPermission_ByUserID(_TEST_DATA.CreditAgent.UserID);
            PerGameDetail_GetByPagingFilter_ViewMerchantPermission_ByUserID(_TEST_DATA.CreditAgent.UserID_Layer2);
            PerGameDetail_GetByPagingFilter_ViewMerchantPermission_ByUserID(_TEST_DATA.NotSuperMerchant.UserID);
            PerGameDetail_GetByPagingFilter_ViewMerchantPermission_ByUserID(_TEST_DATA.Default.UserID);
            return;
        }


        public void PerGameDetail_GetByPagingFilter_ViewMerchantPermission_ByUserID(int userID)
        {
            var query = ServiceInstance.GetByPagingFilter(userID, new QueryPerGameDetailLogApiRequestParameterModel
            {
                GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                GameStartTimeEnd = DateTimeHelper.GetCurrentTime(),
                PageSize = 20
            });

            if (query.Items.IsNotEmptyNull())
            {
                var merchantIDs = query.Items.Select(x => x.MerchantID).Distinct().ToList();

                foreach (var mID in merchantIDs)
                {
                    ViewPermissionCheckService.CheckViewMerchantPermission(userID, mID);
                }
            }

            return;
        }


        [TestMethod]
        public void PerGameDetail_GetByPagingFilter_ByTransactionNoOrThirdNo()
        {
            var begin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 14);
            var end = DateTimeHelper.GetCurrentTime();

            var queryRecord = BetRecordAGGRepository.GetPagingByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                PartitionTimeBegin = begin,
                PartitionTimeEnd = end,
                PageSize = 10,
                Status = BetRecordStatusEnum.SettleComplete.ToInteger(),
            });

            if(queryRecord.Items.IsNotEmptyNull())
            {
                var top3 = queryRecord.Items
                    .OrderByDescending(x=> x.CreateTime)
                    .Take(3)
                    .ToList();

                foreach(var item in top3)
                {
                    //如果很不幸的這是屬於帳務排除資料。不進行此MStest (剛好在此功能才會遇到。平常不會有這種資料)
                    var queryTag = BetRecordTagMappingRepository.GetByUniqueKey(item.BetRecordNoPGS, SystemTagEnum.BetRecord_AccountingIgnore.ToInteger());

                    if (queryTag == null)
                    {
                        var filter1 = new QueryPerGameDetailLogApiRequestParameterModel
                        {
                            PageSize = 1,
                            TransactionNoOrThirdNo = item.BetRecordNoPGS,
                            GameStartTimeBegin = begin,
                            GameStartTimeEnd = end
                        };

                        var filter2 = new QueryPerGameDetailLogApiRequestParameterModel
                        {
                            PageSize = 1,
                            TransactionNoOrThirdNo = item.BetRecordNo,
                            GameStartTimeBegin = begin,
                            GameStartTimeEnd = end
                        };

                        var query1 = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID, filter1);
                        var query2 = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID, filter2);

                        //確保兩種查法 查到的資料是一樣的
                        Assert.IsTrue(query1.Items.Count() > 0 && query2.Items.Count() > 0);
                        Assert.IsTrue(query1.Items.FirstOrDefault().TransactionNo == query2.Items.FirstOrDefault().TransactionNo);
                    }
                }
            }

            return;
        }

        [TestMethod]
        public void PerGameDetail_GetByPagingFilter_ByPlayerName_Test()
        {
            //篩選各種欄位 看SQL是否有寫錯
            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID, new QueryPerGameDetailLogApiRequestParameterModel
            {
                PageSize = 1,
                PlayerName = _TEST_DATA.NotSuperMerchant.PlayerName + " " , //故意多一個空白
                GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                GameStartTimeEnd = DateTimeHelper.GetCurrentTime()
            });

            Assert_NotEmptyCollection(query.Items);
            return;
        }


        /// <summary>
        /// 測試非超級管理員 能否查到自家帳號
        /// </summary>
        [TestMethod]
        public void PerGameDetail_GetByPagingFilter_MerchantAndPlayerViewPermission_Test()
        {
            int userID = _TEST_DATA.NotSuperMerchant.UserID;
            int targetMerchantID = _TEST_DATA.NotSuperMerchant.MerchantID;

            var query = ServiceInstance.GetByPagingFilter(userID, new QueryPerGameDetailLogApiRequestParameterModel
            {
                PageSize = 5,
                GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                GameStartTimeEnd = DateTimeHelper.GetCurrentTime().AddDays(0),
                OrderByColumn = " GameTime "
            });

            if (query.Items.IsEmptyOrNull())
                return;

            //檢查只能查看自家商戶資料
            Assert.IsTrue(query.Items.All(z => z.MerchantID == targetMerchantID));

            var matchFirst = query.Items.FirstOrDefault();
            var queryAgain = ServiceInstance.GetByPagingFilter(userID, new QueryPerGameDetailLogApiRequestParameterModel
            {
                PlayerName = matchFirst.PlayerName,
                PageSize = 1,
                GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                GameStartTimeEnd = DateTimeHelper.GetCurrentTime().AddDays(0),
                OrderByColumn = " GameTime "
            });

            Assert.IsTrue(queryAgain.Items.Any(z=> z.PlayerName == matchFirst.PlayerName));
            return;
        }


        [TestMethod]
        public void GetSlotHistoryFromPerGameLog_Test()
        {
            long id = _TEST_DATA.Default.SlotGamePerGameDetailLogID;

            var query = ServiceInstance.GetSlotHistoryFromPerGameLog(_TEST_DATA.Default.UserID, id);

            Assert.IsNotNull(query);
            return;
        }


        [TestMethod]
        public void GetSlotHistoryFromPerGameLog_DetailFields_Test()
        {
            // 參考實作：應回傳與輸入 perGameDetailLogId 對應的資料，並包含 slot history 與盤面資訊
            long id = _TEST_DATA.Default.SlotGamePerGameDetailLogID;

            var result = ServiceInstance.GetSlotHistoryFromPerGameLog(_TEST_DATA.Default.UserID, id);

            // 基本檢查
            Assert.IsNotNull(result, "回傳物件不得為 null");
            Assert.AreEqual(id, result.PerGameDetailLogID, "PerGameDetailLogID 應與傳入的 id 相同");

            // slot history 與盤面相關欄位檢查
            Assert.IsTrue(result.SlotGameHistoryID > 0, "SlotGameHistoryID 應大於 0");
            Assert.IsTrue(result.SlotGameHistoryLogID > 0 || result.SlotGameHistoryID > 0, "應至少有一個 SlotGameHistory 的識別欄位");
            Assert.IsTrue(result.LineResult != null && result.LineResult.Count > 0, "LineResult 不應為空");
            Assert.IsTrue(result.DistinctIcons != null && result.DistinctIcons.Count > 0, "DistinctIcons 不應為空");

            // 玩家、注額、時間等基本資訊檢查
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.PlayerName), "PlayerName 不應為空");
            Assert.IsTrue(result.BetAmount >= 0, "BetAmount 應為非負值");
            Assert.IsTrue(result.GameTime != default(DateTime), "GameTime 應有有效時間");

            return;
        }


        [TestMethod]
        public void AddTag_Test()
        {
            if (IsSkipTest_WhenSmallerThanTargetTime(_TEST_DATA.Default.Date_NewGameVendor_Intergrate))
            {
                return;
            }

            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID, new QueryPerGameDetailLogApiRequestParameterModel
            {
                PageSize = 20,
                GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                GameStartTimeEnd = DateTimeHelper.GetCurrentTime().AddDays(-1),
                OrderByColumn = " GameTime "
            });

            if (query.Items.IsEmptyOrNull())
                return;

            var matchFirst = query
                .Items
                .Where(x=>  ((GameVendorTypeEnum) x.GameVendorID).IsDefinedValue()) //如果新遊戲商的code還沒合回去。會有錯誤。
                .FirstOrDefault();

            if(matchFirst != null)
            {
                ServiceInstance.AddTag(_TEST_DATA.Default.UserID, new PerGameTagAddRequestModel
                {
                    PerGameDetailLogID = matchFirst.PerGameDetailLogID,
                    TagID = SystemTagEnum.PerGameDetailLog_DevelopeSpecialTag.ToInteger()
                });
            }

            return;
        }



    }
}

