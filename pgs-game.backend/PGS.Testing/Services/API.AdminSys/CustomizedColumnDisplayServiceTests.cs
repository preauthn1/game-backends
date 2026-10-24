using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
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
    public class CustomizedColumnDisplayServiceTests : BaseTest
    {

        public ICustomizedColumnDisplayService ServiceInstance { get; set; }


        public class MockCustomizedColumnDisplayService : CustomizedColumnDisplayService
        {
            public override CustomizeQueryMethodResultModel QueryByExistServiceOrMethod(int userID, CustomizedColumnDisplayRequestModel filter , CustomizedColumnDynamicQueryExtraSettingJson extraSetting)
            {
                var fakeDic = new Dictionary<string, object>();
                fakeDic.Add("AAA", "AAA");
                fakeDic.Add("BBB", "BBB");

                return new CustomizeQueryMethodResultModel
                {
                    ListResult = new List<Dictionary<string, object>> {  fakeDic }
                };
            }
        }

        [TestMethod]
        public void QueryByExistServiceOrMethod_AgentHierararchtGameReport_Test()
        {
            var myService = new CustomizedColumnDisplayService();

            var query = myService.QueryByExistServiceOrMethod(_TEST_DATA.Default.UserID, new CustomizedColumnDisplayRequestModel
            {
                MerchantID = _TEST_DATA.CreditAgent.MerchantID,
                BeginTime = DateTimeHelper.GetCurrentTime().AddDays(-30),
                EndTime = DateTimeHelper.GetCurrentTime(),
                QuerySqlID = CustomerColumnSpecialQueryEnum.AgentHierararchtGameReport.ToInteger()
            } , null);

            return;
        }


        [TestMethod]
        public void QueryStatistic_GameReport_SpecificGameAllEachPlayer_Test()
        {
            var myService = new CustomizedColumnDisplayService();

            var query = myService.QueryByExistServiceOrMethod(_TEST_DATA.Default.UserID, new CustomizedColumnDisplayRequestModel
            {
                GameID = _TEST_DATA.NotSuperMerchant.EventID,
                MerchantID = _TEST_DATA.Default.MerchantID,
                BeginTime = DateTimeHelper.GetCurrentTime().AddDays(-30),
                EndTime = DateTimeHelper.GetCurrentTime(),
                QuerySqlID = CustomerColumnSpecialQueryEnum.QueryStatistic_GameReport_SpecificGameAllEachPlayer.ToInteger()
            }, null);

            return;
        }


        [TestMethod]
        public void HomePageReport_SingleCurrency_DailyTotalRound_Test()
        {
            var myService = new CustomizedColumnDisplayService();

            int daysRange = 30;

            var query = myService.QueryByExistServiceOrMethod(_TEST_DATA.Default.UserID, new CustomizedColumnDisplayRequestModel
            {
                BeginDateTW = DateTimeHelper.GetCurrentTime().AddDays(-1 * daysRange),
                Currency = "BETA",
                QuerySqlID = CustomerColumnSpecialQueryEnum.HomePageReport_SingleCurrency_DailyTotalRound.ToInteger()
            }, null);

            //2月只有28天
            if(DateTimeHelper.GetCurrentTime(8).Month == 3)
            {
                Assert.IsTrue(query.ListResult.Count() >= 28);
            }
            else
            {
                Assert.IsTrue(query.ListResult.Count() >= (daysRange - 1));
            }

            return;
        }



        [TestMethod]
        public void QueryByExistServiceOrMethod_EventDailyReport_Test()
        {
            var myService = new CustomizedColumnDisplayService();

            var query = myService.QueryByExistServiceOrMethod(_TEST_DATA.Default.UserID, new CustomizedColumnDisplayRequestModel
            {
                EventID = _TEST_DATA.NotSuperMerchant.EventID,
                QuerySqlID = CustomerColumnSpecialQueryEnum.EventDailySummary.ToInteger()
            }, null);

            return;
        }



        [TestMethod]
        public void QueryByExistServiceOrMethod_QueryRTPChangeLog_Test()
        {
            var myService = new CustomizedColumnDisplayService();

            var query = myService.QueryByExistServiceOrMethod(_TEST_DATA.Default.UserID, new CustomizedColumnDisplayRequestModel
            {
                PlayerID = _TEST_DATA.Default.PlayerID,
                QuerySqlID = CustomerColumnSpecialQueryEnum.QueryRTPChangeLog.ToInteger()
            }, null);

            return;
        }


        [TestMethod]
        public void QueryByExistServiceOrMethod_HomePageReport_Test()
        {
            var myService = new CustomizedColumnDisplayService();

            var query1 = myService.QueryByExistServiceOrMethod(_TEST_DATA.Default.UserID, new CustomizedColumnDisplayRequestModel
            {
                //PlayerID = _TEST_DATA.Default.PlayerID,
                //BeginTime = DateTimeHelper.GetCurrentTime().AddDays(-1 * 14),
                //EndTime = DateTimeHelper.GetCurrentTime(),
                OrderBy = "totalRound",
                TargetDayText = DateTimeHelper.GetCurrentTime().AddDays(-1).ToShortDateString(),
                QuerySqlID = CustomerColumnSpecialQueryEnum.HomePageReport_EachMerchant_OnlinePlayerCount.ToInteger()
            }, null);

            var query2 = myService.QueryByExistServiceOrMethod(_TEST_DATA.Default.UserID, new CustomizedColumnDisplayRequestModel
            {
                //PlayerID = _TEST_DATA.Default.PlayerID,
                //BeginTime = DateTimeHelper.GetCurrentTime().AddDays(-1 * 14),
                //EndTime = DateTimeHelper.GetCurrentTime(),
                OrderBy = "totalRound",
                TargetDayText = DateTimeHelper.GetCurrentTime().AddDays(-1).ToShortDateString(),
                QuerySqlID = CustomerColumnSpecialQueryEnum.HomePageReport_TargetDatePlayerCountAndWagerCount_EachMerchantCount.ToInteger()
            }, null);

            return;
        }


        [TestMethod]
        public void QueryStatistic_GameServerReport_EachGames_Test()
        {
            var myService = new CustomizedColumnDisplayService();

            var query1 = myService.QueryByExistServiceOrMethod(_TEST_DATA.Default.UserID, new CustomizedColumnDisplayRequestModel
            {
                VendorConfigID = _TEST_DATA.Default.VendorConfigID,
                BeginTime = DateTimeHelper.GetCurrentTime().AddDays(-14),
                EndTime = DateTimeHelper.GetCurrentTime(),
                QuerySqlID = CustomerColumnSpecialQueryEnum.QueryStatistic_GameServerReport_EachGames.ToInteger()
            }, null);

            var query2 = myService.QueryByExistServiceOrMethod(_TEST_DATA.Default.UserID, new CustomizedColumnDisplayRequestModel
            {
                VendorConfigID = _TEST_DATA.Default.VendorConfigID,
                BeginTime = DateTimeHelper.GetCurrentTime().AddDays(-14),
                EndTime = DateTimeHelper.GetCurrentTime(),
                QuerySqlID = CustomerColumnSpecialQueryEnum.QueryStatistic_GameServerReport_EachMerchant.ToInteger()
            }, null);

            return;
        }


        [TestMethod]
        public void QueryDynamicResult_Test()
        {
            var mockService = new MockCustomizedColumnDisplayService();

            var query = mockService.QueryDynamicResult(_TEST_DATA.Default.UserID , new CustomizedColumnDisplayRequestModel
            {
                QuerySqlID = 999999, //這個編號是MSTest專用
                BeginTime = DateTimeHelper.GetCurrentTime(),
                EndTime = DateTimeHelper.GetCurrentTime()
            });

            return;
        }



        [TestMethod]
        public void GetDynamicDropdownItems_Test()
        {
            var query = ServiceInstance.GetDynamicDropdownItems(_TEST_DATA.Default.UserID);
            Assert_NotEmptyCollection(query.EachCategory);
            Assert.IsNotNull(new CRUDResultModel());

            var firstMatch = query.EachCategory.FirstOrDefault().Items.FirstOrDefault();
            var querySetting = ServiceInstance.GetDynamicQueryCommandSetting(firstMatch.Id); 

            Assert.IsNotNull(querySetting);
            return;
        }


        [TestMethod]
        public void QueryDynamicResult_QueryRoleUserAccounts_Test()
        {
            int userID = _TEST_DATA.Default.UserID;

            var query = ServiceInstance.QueryDynamicResult(userID , new CustomizedColumnDisplayRequestModel
            {
                QuerySqlID = CustomerColumnSpecialQueryEnum.QueryRoleUserAccounts.ToInteger(),
                RoleID = _TEST_DATA.Default.RoleID,
            });

            Assert_NotEmptyCollection(query.Items);
            return;
        }

        [TestMethod]
        public void QueryDynamicResult_QueryOrganizationUserAccounts_Test()
        {
            int userID = _TEST_DATA.Default.UserID;

            var query = ServiceInstance.QueryDynamicResult(userID, new CustomizedColumnDisplayRequestModel
            {
                QuerySqlID = CustomerColumnSpecialQueryEnum.QueryOrganizationUserAccounts.ToInteger(),
                OrganizationID = _TEST_DATA.Default.OrganizationID,
            });

            Assert_NotEmptyCollection(query.Items);
            return;
        }


        [TestMethod]
        public void QueryDynamicResult_QueryRTPChangeLog_Test()
        {
            int userID = _TEST_DATA.Default.UserID;

            var query = ServiceInstance.QueryDynamicResult(userID, new CustomizedColumnDisplayRequestModel
            {
                QuerySqlID = CustomerColumnSpecialQueryEnum.QueryRTPChangeLog.ToInteger(),
                PlayerID = _TEST_DATA.Default.PlayerID,
            });

            return;
        }


        [TestMethod]
        public void FormattingColumnValue_Test()
        {
            var service = new CustomizedColumnDisplayService();

            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("Amount1_Diff", 0);
            dic.Add("Amount2_Diff", 1000);
            dic.Add("BetAmount", 100);
            dic.Add("Date", new DateTime(2021, 1, 1));
            dic.Add("CreateTime", new DateTime(2021, 1, 1));

            var listDic = new List<Dictionary<string, object>>
            {
                dic
            };

            service.FormattingColumnValue(listDic , new CustomizedColumnDisplayRequestModel());

            Assert_NotEmptyCollection(dic);
            return;
        }


    }
}

