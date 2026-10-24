using Lck.Utility.Extensions.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using target.Models;
using target.Models.Enums;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using example.invalid;
using System.Linq;
using example.invalid;
using example.invalid.Http;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace target.Utility
{

    public static class RuntimeHelper
    {
        public static IServiceProvider ServiceProvider { get; set; }

        public static string StartupTime { get; set; }

        public static string RuntimeID { get; set; }

        public static string EnvironmentName { get; set; }

        private static string K8SName { get; set; }


        static RuntimeHelper()
        {
            //用來識別 每台k8s機器獨立ID
            RuntimeID = DateTime.UtcNow.AddHours(8).ToString("MMddHHmmss") + "_" + Guid.NewGuid().ToString().Truncate(6);
            StartupTime = DateTime.UtcNow.AddHours(8).ToString("MMddHHmmss");
        }

        /// <summary>
        /// **重要 請在startup的時候呼叫此
        /// </summary>
        public static void Initialize(string envName)
        {
            EnvironmentName = envName;

            //測試用 看看有無差別 (redis相關的?)
            System.Threading.ThreadPool.SetMaxThreads(600, 800);
            System.Threading.ThreadPool.SetMinThreads(200, 300);

            //更新config provider
            BaseSettingHelper.ReloadAppSettingProvider();


            try
            {
                //這是v13.0.1 版之前的一個漏洞處理
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings { MaxDepth = 128 };
            }
            catch
            {

            }
        }

        public static void RegisterHttpClient(IServiceCollection service)
        {
            int defaultTimeoutSeconds = 300;

            service.AddHttpClient(HttpClientTypeEnum.Default.ToString() , client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(defaultTimeoutSeconds);
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                    AllowAutoRedirect = false,
                });

            service.AddHttpClient(HttpClientTypeEnum.IgnoreSSLError.ToString(), client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(defaultTimeoutSeconds);
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true ,
                    AllowAutoRedirect = false,
                });

            //特定幾種通用的interface直接這邊註冊
            service.AddTransient<IMyHttpClientProvider, MyHttpClientProvider>();

        }

        public static void SetGlobalServiceProvider(IServiceProvider provider)
        {
            ServiceProvider = provider;
        }


        /// <summary>
        /// 首頁可以顯示的資訊
        /// </summary>
        /// <returns></returns>
        public static object GetHomePageDebugInformation()
        {
            //可以透過query string 額外指定查特定setting
            string searchExtraSettingByKey = HeaderValueHelper.GetQueryString("key") ?? "XXXXXX";

            return new
            {
                EnvironmentName = SettingHelper.Env,
                WebSiteName = SettingHelper.WebSiteName,
                ExtraSearchSetting = BaseSettingHelper.GetSettingValue(searchExtraSettingByKey),
                IsEnableCache = SettingHelper.IsEnableCache,
                IsRemoteCache = SettingHelper.IsRemoteCache,
                ApiRequestLogBackgroundSyncEnable = SettingHelper.ApiRequestLogBackgroundSyncEnable,
                RedisCon = SettingHelper.ThirdParty.RedisConn,
                DefaultLang = SettingHelper.DefaultLang,
                RedisTimeout = SettingHelper.ThirdParty.RedisTimeoutSeconds,
                TgToken = [REDACTED]"TelegramBotToken").Truncate(30, "*********"),
                ConfigName = BaseSettingHelper.GetJsonSettingFileNameByEnv(),
                RemoveCacheApi = SystemConstant.ApiUrlRemoveAllCache,
                PromoteBrandType = SettingHelper.PromoteBrandType,
                SHA1 = GetGitSHA1(),
                Pipeline_ID = Get_Gitlab_Pipeline_ID(),
                YourIP = IPHelper.GetIP(),
                DateNow = DateTimeHelper.GetCurrentTime(),
                LastModifiedTime = GetBinFileLastModifiedTimeStringFormat(),
                Headers = HeaderValueHelper.GetHeaders(),
                RuntimeID = RuntimeID
            };
        }


        public static Dictionary<string, Dictionary<string, string>> Readi18nTextJsonKeyValues()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "CultureText", "i18nKeys.json");
            string json = example.invalid.File.ReadAllText(path);

            var convertToEachLangDic = json.JsonConvertToModel<Dictionary<string, Dictionary<string, string>>>();

            return convertToEachLangDic;
        }


        /// <summary>
        /// production環境，在未帶入password的情況下，首頁可以顯示的資訊有限制
        /// </summary>
        public static object GetHideInformationForProduction()
        {
            return new
            {
                EnvironmentName = SettingHelper.Env,
                WebSiteName = SettingHelper.WebSiteName,
            };
        }

        public static string GetGitSHA1()
        {
            try
            {
                return Environment.GetEnvironmentVariable("Git_SHA1");
            }
            catch
            {

            }

            return "";
        }

        public static string Get_Gitlab_Pipeline_ID()
        {
            try
            {
                return Environment.GetEnvironmentVariable("Pipeline_ID");
            }
            catch
            {

            }

            return "";
        }


        /// <summary>
        /// 判斷當前環境是否為 .net core
        /// </summary>
        /// <returns></returns>
        public static bool IsNetCoreRuntimeFramework()
        {
            var ver = GetRuntimeFrameworkVersion();

            if (string.IsNullOrWhiteSpace(ver))
                return false;

            if (ver.Contains("NETCoreApp"))
                return true;

            return false;
        }


        /// <summary>
        /// 取得當前運行的frameword版本 (.net core、.net frameword)
        /// </summary>
        /// <returns></returns>
        public static string GetRuntimeFrameworkVersion()
        {
            var framework = Assembly
              .GetEntryAssembly()?
              .GetCustomAttribute<TargetFrameworkAttribute>()?
              .FrameworkName;

            return framework;
        }



        public static DateTime? GetBinFileLastModifiedTime()
        {

            try
            {
                var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var time = File.GetLastWriteTime(location);
                return time;
            }
            catch
            {

            }

            return null;
        }


        public static string GetK8sInstanceName(bool isInstanceNameOnly = true , string replaceKeyword = "backend-api-")
        {
            string printName = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(K8SName))
                {
                    K8SName = example.invalid.File.ReadAllText("/etc/hostname") ?? "";
                    K8SName = K8SName.Trim();
                }

                printName = K8SName ?? "";

                if(!string.IsNullOrWhiteSpace(replaceKeyword))
                {
                    printName = printName.Replace(replaceKeyword, "");
                }

                if (isInstanceNameOnly)
                {
                    return printName;
                }
                else
                {
                    return $" k8s: `{printName}` "; ;
                }
            }
            catch
            {

            }

            if (string.IsNullOrWhiteSpace(printName) && SettingHelper.IsLocalhostWebSiteName)
            {
                printName = "*Localhost";
            }

            return printName;
        }


        public static string GetBinFileLastModifiedTimeStringFormat()
        {
            DateTime? date = GetBinFileLastModifiedTime();

            if (date == null)
                return string.Empty;

            return date.Value.ToString("yyyy-MM-dd HH:mm:ss");
        }

    }

}
