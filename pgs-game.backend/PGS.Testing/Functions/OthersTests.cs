using game.Entity.Game;
using Lck.Utility.Extensions.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.Core.Helper;
using target.DA.Repository;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Helper.Elasticsearch.Model;
using target.Utility.IOC;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using example.invalid;
using System.Linq;
using example.invalid;
using example.invalid.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace target.Testing
{
    [TestClass]
    public class OthersTests : BaseTest
    {

        public PlayerRepository PlayerRepository { get; set; }

        public ForbiddenIPAddressRepository ForbiddenIPAddressRepository { get; set; }

        public IExternalApiRequestLogRefService ExternalApiRequestLogRefService { get; set; }



        /// <summary>
        /// 各種雜項測試、驗證
        /// </summary>
        [TestMethod]
        public void SampleTest()
        {
            //非公司開發電腦 不執行這組test
            if (IsNotDeveloperComputerName())
                return;

            //var OneService = new OneTimesJobService();
            //var AGService = new AGinIntegrationService();
            //var AGGService = new AGGIntegrationService();
            //var service4 = new ClotPlayIntegrationService();
            //var RGIntegrationService = new RGIntegrationService();
            //var wagerFetchService = new BetRecordFetchAndConvertService();
            //var gameVendorFetchRoutineService = new GameVendorRoutineJobService();

            int playerID = _TEST_DATA.Default.PlayerID;

            //var qtech_result = QTechPlatformHelper.CreateGameSessionToken(ExternalApiRequestLogRefService.Create);
            //var qtech_result = QTechPlatformHelper.GenerateAccessToken(ExternalApiRequestLogRefService.Create);
            //var qtech_result_launch = QTechPlatformHelper.GetQTechGameLaunchUrl(ExternalApiRequestLogRefService.Create , qtech_result.access_token);

            //var qtechService = new SingleWalletQTechFormatApiRequestRefService();


            var qqqq = APIRequestHelper.RequestApiResult<string>(new RequestParameters
            {
                TimeoutMillisecond = 1000 * 10,
                BaseUrl = "https://example.invalid",

                //ApiUrl = "testcase/testTimeout",
                ApiUrl = "testcase/headers",
                //ApiUrl = "testcase/400Exception",
                Method = HttpMethodType.HttpGet
            });


            return;
            return;
            return;
            return;

            //var resultMsg = gameVendorFetchRoutineService.FetchVendorGamesAndInsert(new LazyCommonRequestModel
            //{
            //    GameVendorID = GameVendorTypeEnum.SlotMania.ToInteger(),
            //    VendorConfigID = 53038
            //});

            //獲取遊戲商清單 (FetchGames)
            //var resultMsg = gameVendorFetchRoutineService.FetchVendorGamesAndInsert(new LazyCommonRequestModel
            //{
            //    GameVendorID = GameVendorTypeEnum.SlotMania.ToInteger(),
            //    VendorConfigID = 53038
            //});


            //var qqqq = RGIntegrationService.FetchEachGameAccountingReport(new ExternalVendorGetReportRequestParameter
            //{
            //    StartTimeUTC = new DateTime(2025, 3, 4),
            //    EndTimeUTC = new DateTime(2025, 3, 5),
            //    VendorConfigID = 52855
            //});

            //service3.SetAGGPromotionSettingFlag(new AGGGameSettingFlagParameter
            //{
            //    MerchantID = 181186,
            //    ServiceType = AGGPromotionSettingServiceTypeEnum.ranking_setting,
            //    SettingID = ""
            //});  //181185 全開 181187 全關

            //service3.GetAGGPromotionEventSettings(181185);  //181185 全開 181187 全關


            //wagerFetchService.FetchApiAndInsertToTable(new WagetFetchJobParameter
            //{
            //    BeginDateTW = new DateTime(2025, 2, 5),
            //    EndDateTW = new DateTime(2025, 2, 6),
            //    VendorConfigID = 52766,
            //});

            //var qqqqqqqq = service4.FetchWagers(new ExternalVendorFetchWagersRequestParameter
            //{
            //    StartTimeUTC = new DateTime(2025, 2, 5).AddHours(-8),
            //    EndTimeUTC = new DateTime(2025, 2, 6).AddHours(-8),
            //    VendorConfigID = 52766,
            //    Page = 1
            //});


            //var qqqq = service4.FetchEachGameAccountingReport(new ExternalVendorGetReportRequestParameter
            //{
            //    StartTimeUTC = new DateTime(2025, 2, 5).AddHours(-8),
            //    EndTimeUTC = new DateTime(2025, 2, 6).AddHours(-8),
            //    VendorConfigID = 52766
            //});


            return;
            return;
        }


        static double GetY(double x)
        {
            if (x < 5)
            {
                return 150;
            }
            else
            {
                double A = 72.97;
                double k = 0.2398;
                double C = 98;
                return A * Math.Exp(-k * (x - 5)) + C;
            }
        }


        [TestMethod]
        public void TestApiRequest()
        {
            //非公司開發電腦 不執行這組test
            if (IsNotDeveloperComputerName())
                return;

            //臨時測試用
            //臨時測試用
            //臨時測試用
            //臨時測試用
            //臨時測試用

            var qqqq1 = Int32.Parse("000");
            var qqqq2 = Int32.Parse("001");



            var galaxy = new GalaxyIntegrationService();

            //galaxy.CreateVendorPlayerAccountOrNot(new ExternalVendorCreateAccountRequestParameter
            //{
            //    Currency = "IDR",
            //    PlayerID = 1000000,
            //    PlayerName = "marshall_test"
            //});

            //galaxy.GetGameLaunchUrl(new ExternalVendorLaunchUrlRequestModel
            //{
            //    PlayerID = 1000000,
            //    PlayerName = "marshall_test",
            //    MerchantID = 175001,
            //    GameID = 530001
            //});

            //var param = new ExternalVendorFetchWagersRequestParameter
            //{
            //    StartTimeUTC = DateTimeHelper.GetCurrentTime().AddDays(-7),
            //    EndTimeUTC = DateTimeHelper.GetCurrentTime(),
            //    Page = 1,
            //    VendorConfigID = 50594,
            //};

            //galaxy.FetchWagers(param);

            //var param2 = new ExternalVendorGetReportRequestParameter
            //{
            //    StartTimeUTC = DateTimeHelper.GetCurrentTime().AddDays(-1),
            //    EndTimeUTC = DateTimeHelper.GetCurrentTime(),
            //    VendorConfigID = 50594,
            //};

            //galaxy.FetchEachGameAccountingReport(param2);

            //param2.StartTimeUTC = DateTimeHelper.GetCurrentTime().AddDays(0);
            //galaxy.FetchEachGameAccountingReport(param2);

            //var qqqq = new OneTimesJobService();
            //qqqq.CheckOneTimesJobToExecute();

            return;
        }





        [TestMethod]
        public void CountArraySize_Test()
        {
            string jsonText = @"
[


175111,175115,175116,175117,175119,175120,175121,175122,175123,175124,175125,175126,175127,175128,175129,175130,175131,175132,175133,175134,175135,175136,175137,175138,175139,175142,175143,175144,175145,175146,175147,175148,175149,175150,175151,175152,175153,175154,175155,175156,175157,175158,175159,175160,175161,175162,175163,175164,175165,175166,175167,175168,175169,175170,175171,175172,175173,175174,175175,175176,175177,175178,175179,175180,175181,175182,175183,175186,175187,175188,175189,175190,175192,175193,175194,175195,175196,175197,175198,175199,175200,175201,175202,175203,175204,175205,175206,175207,175208,175209,175210,175211,175212,175213,175214,175215,175216,175217,175218,175219,175220,175221,175222,175223,175224,175225,175226,175227,175228,175229,175230,175231,175232,175233,175234,175235,175236,175237,175238,175239,175240,175241,175242,175243,175244,175245,175246,175247,175248,175249,175250,175251,175252,175253,175254,175255,175256,175257,175258,175259,175260,175261,175262,175263,175264,175265,175266,175267,175268,175269,175270,175271,175272,175273,175274,175275,175276,175277,175278,175279,175280,175281,175282,175284,175285,175286,175288,175289,175293,175294,175297,175298,175299,175300,175301,175304,175305,175307,175308,175309,175310,175311,175312,175313,175314,175315,175316,175317,175318,175319,175320,175321,175322,175323,175324,175325,175326,175327,175328,175329,175330,175331,175332,175333,175334,175335,175336,175337,175338,175339,175340,175341,175342,175343,175344,175345,175346,175347,175348,175349,175350,175351,175352,175353,175354,175355,175356,175357,175358,175359,175360,175361,175362,175363,175364,175365,175366,175367,175368,175369,175370,175371,175372,175373,175374,175375,175376,175377,175378,175379,175380,175381,175382,175383,175384,175385,175386,175387,175388,175389,175390,175391,175392,175393,175394,175395,175396,175397,175398,175399,175400,175401,175402,175403,175404,175405,175406,175407,175408,175409,175410,175411,175412,175413,175414,175415,175416,175417,175418,175419,175420,175421,175422,175423,175424,175425,175426,175427,175428,175999,175111,175115,175116,175117,175119,175120,175121,175122,175123,175124,175125,175126,175127,175128,175129,175130,175131,175132,175133,175134,175135,175136,175137,175138,175139,175142,175143,175144,175145,175146,175147,175148,175149,175150,175151,175152,175153,175154,175155,175156,175157,175158,175159,175160,175161,175162,175163,175164,175165,175166,175167,175168,175169,175170,175171,175172,175173,175174,175175,175176,175177,175178,175179,175180,175181,175182,175183,175186,175187,175188,175189,175190,175192,175193,175194,175195,175196,175197,175198,175199,175200,175201,175202,175203,175204,175205,175206,175207,175208,175209,175210,175211,175212,175213,175214,175215,175216,175217,175218,175219,175220,175221,175222,175223,175224,175225,175226,175227,175228,175229,175230,175231,175232,175233,175234,175235,175236,175237,175238,175239,175240,175241,175242,175243,175244,175245,175246,175247,175248,175249,175250,175251,175252,175253,175254,175255,175256,175257,175258,175259,175260,175261,175262,175263,175264,175265,175266,175267,175268,175269,175270,175271,175272,175273,175274,175275,175276,175277,175278,175279,175280,175281,175282,175284,175285,175286,175288,175289,175293,175294,175297,175298,175299,175300,175301,175304,175305,175307,175308,175309,175310,175311,175312,175313,175314,175315,175316,175317,175318,175319,175320,175321,175322,175323,175324,175325,175326,175327,175328,175329,175330,175331,175332,175333,175334,175335,175336,175337,175338,175339,175340,175341,175342,175343,175344,175345,175346,175347,175348,175349,175350,175351,175352,175353,175354,175355,175356,175357,175358,175359,175360,175361,175362,175363,175364,175365,175366,175367,175368,175369,175370,175371,175372,175373,175374,175375,175376,175377,175378,175379,175380,175381,175382,175383,175384,175385,175386,175387,175388,175389,175390,175391,175392,175393,175394,175395,175396,175397,175398,175399,175400,175401,175402,175403,175404,175405,175406,175407,175408,175409,175410,175411,175412,175413,175414,175415,175416,175417,175418,175419,175420,175421,175422,175423,175424,175425,175426,175427,175428,175999


]";
            List<string> parseToArray = jsonText.JsonConvertToModel<List<string>>();
            List<string> parseToArray2 = parseToArray.Distinct().ToList();
            return;
        }

    }



    public class MyClass
    {
        public int Text = 0;

        public int Calculate()
        {
            return 1 + 1 * 3;
        }
    }

    public class MyStaticClass
    {
        public static int Calculate()
        {
            return 1 + 1 * 3;
        }
    }


}
