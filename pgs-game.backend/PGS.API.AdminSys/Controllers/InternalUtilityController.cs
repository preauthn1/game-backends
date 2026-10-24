using game.Entity.Game;
using Lck.Utility.Extensions.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using target.BL.Interface;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;
using target.Models.ViewModels.Base;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using target.Web.Modules.Filter;
using System;
using System.Collections.Generic;
using example.invalid;
using example.invalid;
using System.Linq;
using System.Threading;
using static example.invalid.WebRequestMethods;

namespace target.API.AdminSys.Controllers
{
    public class InternalUtilityController : BaseController
    {
        private static readonly object _lockObject_CheckOnlineVersion = new object();

        public IAdminSysComplexService AdminSysComplexService { get; set; }

        public IAdminSysInternalUtilityService AdminSysInternalUtilityService { get; set; }


        /// <summary>
        /// 內部自己人使用、常用設定
        /// </summary>
        [Route("internal-utility/favorit-setting")]
        [AdminTokenrAuthorizeFilter]
        [HttpGet]
        public FavoritSettingViewModel GetAdminSysFavoritSetting([FromQuery] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = AdminSysInternalUtilityService.GetFavoritSetting(userID, model);
            return result;
        }


        /// <summary>
        /// 外部客戶(含內部) 使用、常用設定
        /// </summary>
        [Route("internal-utility/common-favorit-setting")]
        [AdminTokenrAuthorizeFilter]
        [HttpGet]
        public CommonFavoritSettingViewModel GetCommonFavoritSetting([FromQuery] LazyCommonRequestModel model)
        {
            var result = AdminSysInternalUtilityService.GetCommonFavoritSetting(model);
            return result;
        }


        /// <summary>
        /// 產生table insert 語法
        /// </summary>
        [Route("internal-utility/table-sql-gen")]
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminTokenrAuthorizeFilter]
        [HttpPost]
        public GenerateSQLResultViewModel GenerateSqlInsertCommand([FromBody] LazyCommonRequestModel model)
        {
            var result = AdminSysInternalUtilityService.GenerateSqlInsertCommand(model);
            return result;
        }


        /// <summary>
        /// 檢查玩家金額
        /// </summary>
        [Route("internal-utility/check-player-money")]
        [AdminTokenrAuthorizeFilter]
        [HttpGet]
        public CheckPlayerMoneyResultViewModel CheckPlayerMoney([FromQuery] CheckPlayerMoneyRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = AdminSysComplexService.CheckPlayerMoney(userID, model);
            return result;
        }


        /// <summary>
        /// 測試取得遊戲網址
        /// </summary>
        [Route("internal-test/game-launch")]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting, AdminSysPermissionKey.MembersManagementDetail_AllowSimulateGameLaunch)]
        [HttpGet]
        public InternalTestResultViewModel LaunchGameAndGetGamePageUrlByPlayerID([FromQuery] InternalTestBundleRequestModel model)
        {
            int userID = GetCurrentUserID();
            string resultUrl = AdminSysComplexService.LaunchGameByAdminSys(userID, model);
            return new InternalTestResultViewModel
            {
                Url = resultUrl
            };
        }

        /// <summary>
        /// 客服工具
        /// </summary>
        [Route("internal-test/customer-service-utility-info")]
        [AdminTokenrAuthorizeFilter]
        [HttpGet]
        public CustomerServiceUtilityResultModel GetCustmoerServiceUtilityInfo()
        {
            int userID = GetCurrentUserID();
            var result = AdminSysComplexService.GetCustmoerServiceUtilityInfo(userID);
            return result;
        }


        /// <summary>
        /// 更換域名 (客服操作)
        /// </summary>
        [Route("internal-test/update-game-url-domain-by-cs")]
        [AdminTokenrAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting_Update, AdminSysPermissionKey.Special_PGS_CustomerService)]
        [HttpPost]
        public CRUDResultModel ChangeGameUrlDomainByCS()
        {
            int userID = GetCurrentUserID();
            var result = AdminSysComplexService.ChangeGameUrlDomainByCS(userID);
            return result;
        }


        /// <summary>
        /// 下載API文檔
        /// </summary>
        [ApiConfig(IsLogApiEnable = false)]
        [Route("internal-test/download-target-api-document")]
        [AdminTokenrAuthorizeFilter]
        [HttpPost]
        public IActionResult DownloadApiDocumentPDF([FromQuery] LazyCommonRequestModel filter)
        {
            var result = AdminSysInternalUtilityService.DownalodApiDocumentFile(filter);
            HttpContext.Response.Headers.Add(SystemConstant.PgsFileHeaderBase64FileName, result.FileName.Base64Encode());
            return File(result.DownloadStream, "application/pdf", result.FileName);
        }


        [ApiConfig(IsLogApiEnable = false)]
        [Route("gameList-download")]
        [AdminTokenrAuthorizeFilter]
        [HttpPost]
        public IActionResult DownloadImageList()
        {
            Stream stream = null;

            using (var client = new WebClient())
            {
                stream = client.OpenRead(AppHelper.GenerateGameListDownloadUrl());
            }

            if (stream == null)
                throw new TerproException(ApiErrorCode.InternalServerError);


            string brandName = SettingHelper.PromoteBrandCodeName;
            string fileName = string.Format("{0}_GameList_{1}.xlsx", brandName, DateTimeHelper.GetCurrentTime(8).ToString("MMdd"));
            HttpContext.Response.Headers.Add(SystemConstant.PgsFileHeaderBase64FileName, fileName.Base64Encode());
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName); // returns a FileStreamResult
        }


        /// <summary>
        /// 字串解析工具
        /// </summary>
        [Route("internal-test/parse-text-to-IDs")]
        [AdminTokenrAuthorizeFilter]
        [HttpPost]
        public TextParseToListIntResultModel ParseTextToListInt([FromBody] LazyCommonRequestModel model)
        {
            var parseIDs = AppHelper.AnalyzeIDsFromRawText(model.RawText);
            return new TextParseToListIntResultModel
            {
                ids = parseIDs
            };
        }

        /// <summary>
        /// 清除所有Cache
        /// </summary>
        [Route("internal/cache/removeAll")]
        [HttpGet]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting_Update)]
        public object RemoveAllCache()
        {
            CacheHelper.RemoveCacheAll();
            return "Cache Remove All OK";
        }


        /// <summary>
        /// (健康檢查用) 測試redis連線是否正常 
        /// 請不要擋權限
        /// </summary>
        [Route("internal/cache/test-redis-conn")]
        [HttpGet]
        public TestRedisKeyViewModel TestRedisConn()
        {
            string cacheKey = CacheKey.AdminSys_TestRedisKey();
            CacheHelper.SetCacheValue(cacheKey, DateTimeHelper.GetCurrentTime(8).ToString("yyyy-MM-dd HH:mm:ss"), new TimeSpan(0, 1, 0));
            var cacheValue = CacheHelper.GetCacheValue<string>(cacheKey);
            return new TestRedisKeyViewModel
            {
                CacheKey = cacheKey,
                CacheValueText = cacheValue
            };
        }


        [Route("internal-test/CheckElasticsearchLogHealthy")]
        [HttpGet]
        public object CheckElasticsearchLogHealthy()
        {
            var begin = DateTimeHelper.GetCurrentTime().AddHours(-1);
            var end = DateTimeHelper.GetCurrentTime();

            var log_vendorError = ElasticsearchHelper.GetEachHourCount_in48Hours(ElasticLogTableTypeEnum.ApiRequestLogVendorAPIError);
            var log_adminSysError = ElasticsearchHelper.GetEachHourCount_in48Hours(ElasticLogTableTypeEnum.ApiRequestLogAdminSysAPIError);
            var log_externalError = ElasticsearchHelper.GetEachHourCount_in48Hours(ElasticLogTableTypeEnum.ApiRequestLogExternalVendorAPIError);

            return new 
            {
                msg = "這個json資料是各API 48小時內。每小時錯誤次數。請AI檢查是否有特別異常",
                log_vendorError,
                log_adminSysError,
                log_externalError,
            };
        }



        /// <summary>
        /// 檢查dev與正式環境的pipelineID
        /// </summary>
        [Route("internal-test/pipeline-id-check")]
        [HttpGet]
        public object CheckOnlineVersion()
        {
            if (SettingHelper.IsProduction)
                return "正式環境不可使用";

            if (!Monitor.TryEnter(_lockObject_CheckOnlineVersion))
                return "檢查執行中，請勿重複觸發";

            try
            {
                List<string> listDev = new List<string>();
                List<string> list = new List<string>();
                List<string> listBWG = new List<string>();
                List<string> listOtherCheck = new List<string>();

                string line = "---------------------------------------";
                string password = [REDACTED];
                string query = $"?password=[REDACTED]";


                string adminSys_PGS = "https://example.invalid";
                string adminSys_BWG = "https://example.invalid";


                listDev.Add(CheckVersion("API.AdminSys", $"https://example.invalid{query}").msg);
                listDev.Add(CheckVersion("API.External", $"http://example.invalid{query}").msg);
                listDev.Add(CheckVersion("API.Job", $"https://example.invalid{query}").msg);
                listDev.Add(CheckVersion("API.Game", $"http://example.invalid{query}").msg);
                listDev.Add(CheckVersion("API.Public", $"http://example.invalid{query}").msg);
                listDev.Add(CheckVersion("API.Vendor", $"https://example.invalid{query}").msg);


                list.Add(CheckVersion("API.AdminSys", $"{adminSys_PGS}{query}").msg);
                list.Add(CheckVersion("API.External", $"https://example.invalid{query}").msg);
                list.Add(CheckVersion("API.Job", $"https://example.invalid{query}").msg);
                list.Add(CheckVersion("API.Game", $"https://example.invalid{query}").msg);
                list.Add(CheckVersion("API.Public", $"https://example.invalid{query}").msg);
                list.Add(CheckVersion("API.Vendor", $"https://example.invalid{query}").msg);

                listBWG.Add(CheckVersion("API.AdminSys", $"{adminSys_BWG}{query}").msg);
                listBWG.Add(CheckVersion("API.External", $"https://example.invalid{query}").msg);
                listBWG.Add(CheckVersion("API.Job", $"https://example.invalid{query}").msg);
                listBWG.Add(CheckVersion("API.Game", $"https://example.invalid{query}").msg);
                listBWG.Add(CheckVersion("API.Public", $"https://example.invalid{query}").msg);
                listBWG.Add(CheckVersion("API.Vendor", $"https://example.invalid{query}").msg);


                listOtherCheck.Add(CheckRedisConnAlive("target.API.AdminSys", adminSys_PGS));
                listOtherCheck.Add(CheckRedisConnAlive("BWG.API.AdminSys", adminSys_BWG));

                listOtherCheck = listOtherCheck.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                string slackMsg = "各API版本為" + Environment.NewLine;

                slackMsg += Environment.NewLine + "*Develop環境*" + Environment.NewLine + line + Environment.NewLine;

                foreach (var msg in listDev)
                {
                    slackMsg += msg + Environment.NewLine;
                }

                slackMsg += Environment.NewLine + "*Production環境*" + Environment.NewLine + line + Environment.NewLine;

                foreach (var msg in list)
                {
                    slackMsg += msg + Environment.NewLine;
                }

                slackMsg += Environment.NewLine + "*BWG環境*" + Environment.NewLine + line + Environment.NewLine;

                foreach (var msg in listBWG)
                {
                    slackMsg += msg + Environment.NewLine;
                }

                if (listOtherCheck.IsNotEmptyNull())
                {
                    slackMsg += Environment.NewLine + "*其他檢查項目*" + Environment.NewLine + line + Environment.NewLine;

                    foreach (var msg in listOtherCheck)
                    {
                        slackMsg += msg + Environment.NewLine;
                    }
                }

                var responseText = MySlackUtility.SendMessageToChannelNotAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.backend_release_k8s,
                    Message = slackMsg,
                });

                return "訊息已發送。" + responseText.Truncate(100);
            }
            finally
            {
                Monitor.Exit(_lockObject_CheckOnlineVersion);
            }
        }



        private (bool isPipelineExist, string msg) CheckVersion(string category, string homePageUrl)
        {
            var result = APIRequestHelper.RequestApiResult<ApiResponseGenericModel<Dictionary<string, object>>>(new RequestParameters
            {
                BaseUrl = homePageUrl,
                ApiUrl = APIRequestHelper.No_Url_Keyword,
                Method = HttpMethodType.HttpGet
            });

            if (result.ResultObject != null &&
               result.ResultObject.Data != null &&
               result.ResultObject.Data.IsNotEmptyNull() &&
               result.ResultObject.Data.ContainsKey("pipeline_ID")
               )
            {
                var pipeline = result.ResultObject.Data["pipeline_ID"].ToString();
                string url = "http://example.invalid:1688/backend/target/game.backend/pipelines/" + pipeline;
                string msg = $"PipelineID: " + $"<{url}|{pipeline}>" + $" <{homePageUrl}|`{category}`>";
                return (true, msg);
            }

            return (false, $"查無PipelineID `{category}`");
        }


        private string CheckRedisConnAlive(string category, string adminSysApiUrlDomain)
        {
            var result = APIRequestHelper.RequestApiResult<ApiResponseGenericModel<TestRedisKeyViewModel>>(new RequestParameters
            {
                BaseUrl = adminSysApiUrlDomain,
                ApiUrl = "internal/cache/test-redis-conn",
                Method = HttpMethodType.HttpGet
            });

            var nowText = DateTimeHelper.GetCurrentTime(8).ToString("yyyy-MM-dd");

            if (result.ResultObject == null ||
               result.ResultObject.Data == null ||
               result.ResultObject.Data.CacheKey != CacheKey.AdminSys_TestRedisKey() ||
               !result.ResultObject.Data.CacheValueText.ContainsWihtoutCase(nowText))
            {
                return $"`{category}` , API返回異常。Redis是否掛掉?";
            }

            return "";
        }




    }
}
