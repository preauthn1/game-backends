using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class AppHelperTests : BaseTest
    {

        public GameListRepository GameListRepository { get; set; }



        [TestMethod]
        public void  AppHelper_Local_Test()
        {
            if (IsNotDeveloperComputerName())
                return;

            var vipIDs = new List<int>
            {

            };

            int startMerchantID = 375001;
            int endMerchantID = 395000;

            var splitGroups = AppHelper.GeneratePartitionNumberSQLCommand(startMerchantID, endMerchantID, vipIDs);
            return;
        }


        [TestMethod]
        public void GeneratePartitionNumberGroup_Test_Case1()
        {
            var vipIDs = new List<int>
            {
                175016,
                175088,
                175089,
                175094,
                175133,
            };

            int startMerchantID = 175001;
            int endMerchantID = 175200;

            var splitGroups = AppHelper.GeneratePartitionNumberGroup(startMerchantID, endMerchantID, vipIDs);

            string resultNumbers = string.Join(",", splitGroups);
            Assert.IsTrue(resultNumbers == "175015,175016,175036,175056,175076,175087,175088,175089,175093,175094,175114,175132,175133,175153,175173,175193,175200");

            return;
        }


        [TestMethod]
        public void GeneratePartitionNumberGroup_Test_Case2()
        {
            var vipIDs = new List<int>
            {
                175208,
                175209,
                175237,
            };

            int startMerchantID = 175201;
            int endMerchantID = 175300;

            var splitGroups = AppHelper.GeneratePartitionNumberGroup(startMerchantID, endMerchantID, vipIDs);

            string resultNumbers = string.Join(",", splitGroups);
            Assert.IsTrue(resultNumbers == "175207,175208,175209,175229,175236,175237,175257,175277,175297,175300");
            return;
        }


        [TestMethod]
        public void GenerateHashKeyByServerTime_Test()
        {
            DateTime currentTime = DateTimeHelper.GetCurrentTime();

            var current = AppHelper.GenerateHashKeyByServerTime(currentTime);
            var other_day1 = AppHelper.GenerateHashKeyByServerTime(currentTime.AddDays(1));
            var other_day2 = AppHelper.GenerateHashKeyByServerTime(currentTime.AddDays(-1));

            Assert.IsTrue(current.IsMatchAnyKey(other_day1.Hash0, other_day1.Hash1));
            Assert.IsTrue(current.IsMatchAnyKey(other_day2.Hash0, other_day2.Hash1));
            return;
        }



        [TestMethod]
        public void AnalyzeIDsFromRawText_Test()
        {
            string text1 = "175001,  175002, 175003  ,175004 ";
            string text2 = @"175001  
                             175002  
                             175003  
                             175004    ";

            string text3 = @" 175001	AAA
                            175002	BBB
                            175003	CCC
                            175004	DDD";

            string text4 = @" 175001 175002 175003 175004 ";
            string text5 = @" 175001, 175002 ,175003, 175004 ";

            var result1 = AppHelper.AnalyzeIDsFromRawText(text1);
            var result2 = AppHelper.AnalyzeIDsFromRawText(text2);
            var result3 = AppHelper.AnalyzeIDsFromRawText(text3);
            var result4 = AppHelper.AnalyzeIDsFromRawText(text4);
            var result5 = AppHelper.AnalyzeIDsFromRawText(text5);

            Assert.IsTrue(result1.Count() == 4);
            Assert.IsTrue(result2.Count() == 4);
            Assert.IsTrue(result3.Count() == 4);
            Assert.IsTrue(result4.Count() == 4);
            Assert.IsTrue(result5.Count() == 4);

            return;

        }




        [TestMethod]
        public void ReplaceUrlToNewDomainHost_Test()
        {
            string url1 = "https://example.invalid/test001";
            string url2 = "https://example.invalid/";

            var afterValue = AppHelper.ReplaceUrlToNewDomainHost(url1, url2);

            Assert.IsTrue(afterValue == "https://example.invalid/test001");
            return;

        }



        [TestMethod]
        public void IsAMDefaultPassword_Test()
        {
            string account = "MYC-test001";
            string password = [REDACTED]";
            string passwordHash = AppHelper.GeneratePasswordHash(password);

            var correctResult = AppHelper.IsAMDefaultPassword(account, passwordHash);

            Assert.IsTrue(correctResult.IsPasswordMatch);
            Assert.IsTrue(correctResult.Password == password);
            Assert.IsFalse(AppHelper.IsAMDefaultPassword(account, "XXXXXXXXXXXXX").IsPasswordMatch);
            return;
        }


        [TestMethod]
        public void ForLoop_Test()
        {
            int loopTimes = 0;
            int expectTimes = 2;

            for (int i = 1; i <= expectTimes; i++)
            {
                loopTimes++;
            }

            Assert.IsTrue(loopTimes == expectTimes);
            return;
        }



        [TestMethod]
        public void GetGameVendorByGameID_Test()
        {
            var alLGames = GameListRepository.GetByFilter(new Models.ViewModels.QueryGameListRequestViewModel
            {
                Status = NormalActiveStatus.Enable.ToInteger(),
                IsNotDelete = true
            });

            foreach (var item in alLGames)
            {
                int myGameVendor = AppHelper.GetGameVendorByGameID(item.GameID);
                int expectGameVendor = item.GameVendorType;

                if (expectGameVendor == GameVendorTypeEnum.Slot88.ToInteger())
                {
                    expectGameVendor = GameVendorTypeEnum.target.ToInteger();
                }

                Assert.IsTrue(myGameVendor == expectGameVendor);
            }

            Assert.IsTrue(AppHelper.GetGameVendorByGameID(1023) == GameVendorTypeEnum.target.ToInteger());
            Assert.IsTrue(AppHelper.GetGameVendorByGameID(310001) == GameVendorTypeEnum.CrazyFishing.ToInteger());
            Assert.IsTrue(AppHelper.GetGameVendorByGameID(290008) == GameVendorTypeEnum.AGG.ToInteger());
            Assert.IsFalse(AppHelper.GetGameVendorByGameID(290008) == GameVendorTypeEnum.target.ToInteger());
            return;

        }



        [TestMethod]
        public void GetLanguageOrder_Test()
        {
            var order1 = AppHelper.GetLanguageOrder("zh-tw", LanguageEnum.zh_tw);
            var order2 = AppHelper.GetLanguageOrder("zh-cn", LanguageEnum.zh_tw);
            var order3 = AppHelper.GetLanguageOrder("en-us", LanguageEnum.zh_tw);

            Assert.IsTrue(order1 == 0);
            Assert.IsTrue(order2 == 1);
            Assert.IsTrue(order3 == 100);
            return;
        }


        [TestMethod]
        public void ConvertToChartDataModel_Test()
        {
            var testData = new Models.ViewModels.ChartDataConvertRequestParameterModel
            {
                ChartDisplayName = "XXXX",
                TargetName = "YYYY",
                Items = new System.Collections.Generic.List<Models.ViewModels.ChartDataConvertItemModel>
                {
                    new Models.ViewModels.ChartDataConvertItemModel
                    {
                        X_Label = "AAA",
                        Y_Value = 100,
                    },
                    new Models.ViewModels.ChartDataConvertItemModel
                    {
                        X_Label = "BBB",
                        Y_Value = 500,
                    },
                    new Models.ViewModels.ChartDataConvertItemModel
                    {
                        X_Label = "CCC",
                        Y_Value = 0,
                    }
                }
            };

            var qqq1 = AppHelper.ConvertToChartDataModel(testData, isExcludeZero: false);
            var qqq2 = AppHelper.ConvertToChartDataModel(testData, isExcludeZero: true);

            Assert.IsTrue(qqq1.Labels.IsNotEmptyNull());
            Assert.IsTrue(qqq2.Labels.IsNotEmptyNull());
            Assert.IsTrue(qqq1.Datasets.IsNotEmptyNull());
            Assert.IsTrue(qqq2.Datasets.IsNotEmptyNull());

            return;
        }


        [TestMethod]
        public void SimplifiedToTraditional_Test()
        {
            var q1 = AppHelper.SimplifiedToTraditional("AAA");
            var q2 = AppHelper.SimplifiedToTraditional("注意看這個男人太狠了");
            var q3 = AppHelper.SimplifiedToTraditional("注意看这个男人太狠了");

            var q21 = AppHelper.TraditionalToSimplified("注意看這個男人太狠了");
            var q22 = AppHelper.TraditionalToSimplified("注意看这个男人太狠了");
            var q23 = AppHelper.TraditionalToSimplified("AAA");

            return;
        }


        [TestMethod]
        public void CommonLogic_001_Test()
        {
            var hash1 = "XXXXXXXXXXAAAAAAAAAAAAWWWWWWWWWWWVVVVVVVVVXXXXX111111111111111XQWWWWWWWWWWWWWWWWWWQQQQQQQQQQQQQ".MD5Hash();
            var hash2 = "123".MD5Hash();

            //確保不管什麼長度的字串 MD5後的字 都是差不多長度
            Assert.IsTrue(hash1.Length <= 50);
            Assert.IsTrue(hash2.Length <= 50);
            return;
        }



        [TestMethod]
        public void ExecuteMethodAndRetryMaxTimes_Test()
        {
            int totalSum_case1 = 0;
            int totalSum_case2 = 0;
            int randomValue = RandomHelper.GetRandIntegerNumbers(1, 9);


            var loopParam = new RetryLoopParameter
            {
                DelayMilliseconds = 500,
                MaxTimes = 3,
                Tag = "MSTest"
            };

            //預計要retry3次
            AppHelper.ExecuteMethodAndRetryMaxTimes(loopParam, (param) =>
            {
                totalSum_case1 = totalSum_case1 + randomValue;
                return false; //強制返回false = 失敗。因此會執行最大次數迴圈
            });

            //預計要執行1次
            AppHelper.ExecuteMethodAndRetryMaxTimes(loopParam, (param) =>
            {
                totalSum_case2 = totalSum_case2 + randomValue;
                return true;
            });

            Assert.IsTrue(totalSum_case1 == randomValue * 3);
            Assert.IsTrue(totalSum_case2 == randomValue * 1);
            return;
        }


    }
}
