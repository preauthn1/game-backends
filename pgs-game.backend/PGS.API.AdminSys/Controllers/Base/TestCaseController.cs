using CoreHtmlToImage;
using Lck.Utility.Extensions.Cache;
using Lck.Utility.Extensions.Helper;
using Microsoft.AspNetCore.Mvc;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using target.Web.Modules.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using example.invalid.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace target.API.AdminSys.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [CompanyIPOrDevEnvAuthorizeFilter]
    public class TestCaseController : BaseController
    {
        public IGameRecordService GameRecordService { get; set; }


        //ps. AdminSys 似乎沒有自動注入 Repository的機制。

        [Route("testcase/001")]
        [HttpGet]
        public object Test001()
        {
            int userID = 103;

            object myResponseObject = null;

            var rep = new ExternalApiRequestLogRepository();
            var VendorConfigRep = new VendorConfigRepository();

            var aggService = new AGGIntegrationService();
            var fumaService = new FumaIntegrationService();
            var eventService = new EventProcessService();

            //var types = EventDefinitionHelper.EventTypes_WhenSettle();
            //var qqqqqqqqqqq = eventService.GetEventsByMerchantAndEventType(183442, types, 290046);


            var allEnumValues = ExtensionOfEnum.GetEnumAllValue<ElasticLogTableTypeEnum>();

            List<string> tables = new List<string>();

            DateTime expiredTime = new DateTime(2026, 6, 1);

            //foreach (var item in allEnumValues)
            //{
            //    string indexName = TableNameConvertHelper.GetElasticsearchIndexName(item);
            //    ElasticsearchHelper.DeleteDataByTime<object>(indexName, expiredTime);
            //}

            //string indexName = TableNameConvertHelper.GetElasticsearchIndexName(ElasticLogTableTypeEnum.SinglewalletMerchantApiRequestLog);
            //ElasticsearchHelper.DeleteDataByTime<object>(indexName, expiredTime);

            //myResponseObject = APIRequestHelper.RequestApiResult<string>(new RequestParameters
            //{
            //    BaseUrl = "https://example.invalid",
            //    ApiUrl = "404/vvvv",
            //    Method = HttpMethodType.HttpPost
            //});

            return new
            {
                token = [REDACTED]
            };


            var qqqq =  MySlackUtility.SendMessageToChannelNotAsync(new SlackMessageParameter
            {
                Channel = SlackChannelEnum.backend_release_k8s,
                Message = "AAAAAAAAAA"
            });

            //176347 / 186384

            //var queryVendorConfig = VendorConfigRep.GetByVendorTypeAndMerchants(GameVendorTypeEnum.AGG.ToInteger(), 186384.ToListCollection()).FirstOrDefault();

            //List<int> merchantIDs = new List<int>
            //{
            //    175170,
            //    175468,
            //    175467,
            //    176031,
            //    175303,
            //    175912,
            //    180530,
            //    176788,
            //    175889,
            //    176351,
            //    177047,
            //};

            //foreach(var m in merchantIDs)
            //{
            //    aggService.CreateAGGNamespaceByAPI_SpecifyVendorConfigID(m, queryVendorConfig.VendorConfigID);
            //}

            //aggService.CreateAGGNamespaceByAPI_TargetMerchant(186384);

            return myResponseObject;

            //SearchAllColumnsDataByAPI

            //var qqq = ElasticsearchHelper.KeywordSearchDataByAPI<object>("apirequestlogexternalvendorapierror", new ESKeywordSearchParameter
            //{
            //    Keyword = "0.0.0.0",
            //    Begin = DateTime.UtcNow.AddHours(-8),
            //    End = DateTime.UtcNow
            //});

            //var qqq = ElasticsearchHelper.EqualSearchDataByAPI<object>("apirequestlogexternalvendorapierror", new Utility.Helper.Elasticsearch.Model.ESKeywordSearchParameter
            //{
            //    ColumnName = "ReferOrderNo",
            //    Keyword = "1eb8b463e02d44e2966cbdf65e45343c",
            //    Begin = DateTime.UtcNow.AddDays(-10),
            //    End = DateTime.UtcNow
            //});


            //var queryABC = fumaService.GetWagerDetailByAPI(new ExternalVendorBetRecordDetailInfoRequestModel
            //{
            //    BetRecordNoPGS = "61278f6bdf67cc2c40e28894cf0ad030"
            //});

            //aggService.FetchBonusSummaryRerport(new AGGReportQueryRequestParameter
            //{
            //    VendorConfigID = 50009,
            //    StartTimeUTC = DateTimeHelper.GetCurrentTime().AddHours(-1 * 24),
            //    EndTimeUTC = DateTimeHelper.GetCurrentTime(),
            //});

            //var qqqq1 = aggService.GetBonusRetryRecords(new AGGGetBonusRetryReqeustParameter
            //{
            //    VendorConfigID = 50009
            //});

            string printText = string.Empty;

            //printText = string.Join(Environment.NewLine, qqqq1.Select(x => $"'{x.TransactionId}',").ToList()); 
            //printText = string.Join(Environment.NewLine, qqqq1.Select(x => $"{x.TransactionId}").ToList());

            return printText;

            //var swCallbackInstance = ServiceInstanceHelper.InstanceCallbackService(SingleWalletPlatformApiTypeEnum.Default);

            ////嘗試呼叫balance API 驗證基礎callback是否正常 (sw客戶常常自己沒設定好 在那邊哭妖)
            //swCallbackInstance.GetWalletBalance(new SingleWalletModeWalletApiRequestParameter
            //{
            //    SessionTokenPGS = "5BL6eb80b2ce3b4d4c959af2e7097032",
            //    PlayerID = 1017023
            //});

            //string indexName1 = ElasticsearcIndexName.ExternalApiRequestLog;
            //ElasticsearchHelper.DeleteDataByTime<object>(indexName1, new DateTime(2025, 11, 13));

            //var param = new SkypeMessageParameter
            //{
            //    //Message = $"【target Announcement】<br/> ",
            //    Message = $"【target Announcement】 <br/> 1. Our newest game has release \r\n\r\n 2. GameID: 290000  3. Marketing Material please refer the link",
            //    TargetConversationID = "19:user@example.invalid"
            //};

            //MySkypeUtility.SendMessage(param);





            return new
            {
                result2 = "XXXXXXXXXX"
            };

            var AdminSysComplexService = new AdminSysComplexService();

            //            List<string> listCurrency = new List<string>
            //            {

            //"KVND",
            //"KIDR",
            //"IDR",
            //"TWD",
            //"MMK",
            //"JPY",
            //"PKR",
            //"LKR",
            //"IRR",
            //"PHP",
            //"IQD",
            //"MNT",
            //"AED",
            //"NPR",
            //"KWD",
            //"BHD",
            //"JOD",
            //"RUB",
            //"UAH",
            //"PLN",
            //"TRY",
            //"NOK",
            //"KZT",
            //"BND",
            //"UZS",
            //"AZN",
            //"GEL",
            //"AMD",
            //"KGS",
            //"BDT",
            //"TMT",
            //"GBP",
            //"BYN",
            //"CZK",
            //"CHF",
            //"MDL",
            //"HUF",
            //"MAD",
            //"RSD",
            //"USD",
            //"EUR",
            //"AUD",
            //"NZD",
            //"BRL",
            //"CAD",
            //"MXN",
            //"PEN",
            //"CLP",
            //"BOB",
            //"PYG",
            //"ZAR",
            //"KES",
            //"TZS",
            //"ZMW",
            //"TND",
            //"NGN",
            //"UGX",
            //"XAF",
            //"GHS",
            //"CDF",
            //"XOF",
            //"SSP",
            //"HTG",
            //"NAD",
            //"ETB",
            //"EGP",
            //"MZN",
            //"UYU",
            //"RWF",
            //"DZD",
            //"LYD",
            //"BWP",

            //            };

            //            foreach(var eachCurrency in listCurrency)
            //            {
            //                string merchantName = "(Dev)QTech-" + eachCurrency;

            //                var result = AdminSysComplexService.CopyAndBatchCreateNewMerchant(userID, new MerchantBatchCopyAndCreateRequestModel
            //                {
            //                    SourceMerchantID = 183978,
            //                    Currency = eachCurrency,
            //                    IMChannelID = 10213,
            //                    Name = merchantName,
            //                    MerchantNameList = new List<string>
            //                    {
            //                        merchantName
            //                    }
            //                });
            //            }

            return "XXXXXXX";
            return "XXXXXXXX";
        }


        [HttpGet("memory-summary")]
        public IActionResult GetMemorySummary()
        {
            var memoryInfo = GC.GetGCMemoryInfo();

            long totalAllocatedBytes = GC.GetTotalMemory(false);

            var report = new
            {
                // 1. 整個 .NET 運行時目前分配的託管記憶體
                TotalAllocatedMemory = $"{totalAllocatedBytes / 1024 / 1024} MB",

                // 2. 垃圾回收器目前向作業系統申領的 Heap 總量
                HeapSize = $"{memoryInfo.HeapSizeBytes / 1024 / 1024} MB",

                // 3. 作業系統整體的記憶體負載（%）
                MemoryLoadPercentage = memoryInfo.MemoryLoadBytes > 0 ? $"{memoryInfo.MemoryLoadBytes}%" : "N/A",

                // 4. 各代垃圾回收次數（用來觀察哪一帶最常被觸發，次數愈多代表那一代物件愈活躍）
                Gen0CollectionCount = GC.CollectionCount(0),
                Gen1CollectionCount = GC.CollectionCount(1),
                Gen2CollectionCount = GC.CollectionCount(2),

                Note = "如果您看見 Gen 2 的回收次數很高，代表長生命週期的物件正在頻繁變動；如果 TotalAllocated 遠小於 HeapSize，代表 1GB 只是 .NET 佔著位置不還給 OS，是正常的。"
            };

            return Ok(report);
        }


        [Route("testcase/TestPartitionSQL")]
        [HttpGet]
        public ActionResult TestPartitionSQL()
        {
            var vipIDs = new List<int>
            {
                //179822,
                //179788,
                //179606,
                //179149,
                //179081,
                //179079,
                //179075,
                //178996,
                //178923,
                //178921,
                //178914,
                //178913,
                //178912,
                //178911,
                //178910,
                //178909,
                //178908,
                //178741,
                //178732,
                //178664,
                //178588,
                //178324,
                //178323,
                //178322,
                //178320,
                //178216,
                //178200,
                //177691,
                //177548,
                //177534,
                //176842,
                //176778,
                //176752,
                //176726,
                //176582,
                //176576,
                //176559,
                //176460,
                //176437,
                //176403,
                //176386,
                //176252,
                //176236,
                //176229,
                //176130,
                //176091,
                //176064,
                //176043,
                //175768,
                //175268,
                //175163,
                //175080,
                //175079,
                //175038
            };

            int startMerchantID = 175001;
            int endMerchantID = 185000;

            var sql = AppHelper.GeneratePartitionNumberSQLCommand(startMerchantID, endMerchantID, vipIDs);
            return Content(sql);
        }


        [Route("testcase/test_post")]
        [HttpPost]
        public object TestPost()
        {
            return "XXXXXXX";
        }


        [Route("testcase/testSendMessage")]
        [HttpGet]
        public object Test_testSendMessage(string msg = "AAABBBB555666")
        {
            var service = new MerchantService();
            service.SendMessageToMultipleChannel(msg + " marshall_1775566 `qqqqqqq` testttt_123 ");

            return new
            {
                name = RuntimeHelper.GetK8sInstanceName(),
                name2 = $"AAAA{Environment.NewLine}",
                name3 = $"AAAA{Environment.NewLine}".Trim(),
            };
        }


        [Route("internal/cache/testRequestUrl")]
        [HttpGet]
        public object testRequestUrl()
        {

            var param = new RequestParameters
            {
                BaseUrl = "https://example.invalid",
                ApiUrl = "callback/ion-slot/target/players/wallet",
                Method = HttpMethodType.HttpPost,
                AdvancedSetting = new AdvancedSetting
                {
                    IsIgnoreSSLCertificateError = true,
                }
            };

            var body = new
            {
                sessionToken = [REDACTED]"
            };

            param.AddHeaderData("X-target-V3-ID", "66666XXXXXXXXX");
            param.AddHeaderData("X-target-V3-Key", "7777777777776666");

            var apiResult = APIRequestHelper.RequestApiResult<string>(param, body);

            return new
            {
                ResponseText = apiResult.ResponseText,
            };
        }



        [Route("internal/cache/getCacheValueByKey")]
        [HttpGet]
        public object GetCacheValueByKey(string key = "test")
        {
            var myValue = CacheHelper.GetCacheValue<string>(key);
            return myValue;
        }


        [Route("testcase/headers")]
        [HttpGet]
        public object GetHeaders()
        {
            return new
            {
                myIP = IPHelper.GetIP(),
                Headers = MyHttpContext.Current.Request.Headers
            };
        }


        /// <summary>
        /// MSTest有呼叫 不要隨便改url
        /// </summary>
        [Route("testcase/testTimeout")]
        [HttpGet]
        public object TestTimeout(int seconds = 180)
        {
            Thread.Sleep(1000 * seconds);

            return MyHttpContext.Current.Request.Headers;
        }


        [Route("testcase/400Exception")]
        [HttpGet]
        public object Test400Exception()
        {
            throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);
        }


        [Route("internal/test/GetOutboundIPAddress")]
        [HttpGet]
        public object GetOutboundIPAddress()
        {
            var myValue = AppHelper.GetOutboundIPAddress();
            return myValue;
        }

        [Route("Debug/TestCase/AnalyzeMemory")]
        [HttpGet, HttpPost]
        public object GetTopMemoryObjects(string password = "")
        {
            if (SettingHelper.IsProduction && password != SystemConstant.PublicAPI_EazyToken_MagicToken)
                return "";

            return ApplicationMemoryHelper.GetTopMemoryObjects();
        }

    }
}
