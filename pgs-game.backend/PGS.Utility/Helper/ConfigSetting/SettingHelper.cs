using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using target.Utility.Extensions;
using example.invalid;
using example.invalid;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using target.Models;
using target.Models.Enums;

namespace target.Utility
{
    public static class SettingHelper
    {

        /// <summary>
        /// 如果判斷到環境非正式、卻使用DB。某些cache的使用可能要注意。
        /// </summary>
        public static bool IsLocalTestProductionDb
        {
            get
            {
                return !IsProduction && IsProductionDbConnection;
            }
        }


        public static bool IsProductionDbConnection
        {
            get
            {
                var conn = BaseSettingHelper.GetSettingValue($"ConnectionStrings:MasterDB") ?? "";
                return conn.Contains("172.18.");
            }
        }

        public static bool IsProductionBWGConnection
        {
            get
            {
                var conn = BaseSettingHelper.GetSettingValue($"ConnectionStrings:MasterDB") ?? "";
                return conn.Contains("172.") && conn.Contains(".196");
            }
        }

        public static bool IsProductionPGSConnection
        {
            get
            {
                var conn = BaseSettingHelper.GetSettingValue($"ConnectionStrings:MasterDB") ?? "";
                return conn.Contains("172.") && conn.Contains(".191");
            }
        }


        public static bool IsDev
        {
            get
            {
                //請用小寫判斷
                return Env == SystemConstant.Env_Name_Development || Env == SystemConstant.Env_Name_Dev || Env == SystemConstant.Env_Name_Local;
            }
        }
        

        /// <summary>
        /// 會強轉小寫
        /// </summary>
        public static string Env
        {
            get
            {
                return (BaseSettingHelper.GetSettingValue("Env") ?? "").ToLower();
            }
        }

        /// <summary>
        /// 是否是測試環境正在運行
        /// 有些地方判斷MSTest 是為了避免使用非同步。MSTest會出現異常
        /// </summary>
        public static bool IsMSTest
        {
            get
            {
                return WebSiteName == "target.MSTest.Local";
            }
        }


        public static bool IsProduction
        {
            get
            {
                return Env == SystemConstant.Env_Name_Production;
            }
        }


        public static string WebSiteName
        {
            get
            {
                return BaseSettingHelper.GetSettingValue("WebSiteName");
            }
        }

        public static bool IsVendorAPI
        {
            get
            {
                if (string.IsNullOrWhiteSpace(WebSiteName))
                    return false;

                return WebSiteName.Contains(".Vendor");
            }
        }

        public static bool IsAdminSysAPI
        {
            get
            {
                if (string.IsNullOrWhiteSpace(WebSiteName))
                    return false;

                return WebSiteName.ContainsWihtoutCase(".AdminSys");
            }
        }

        public static bool IsExternalVendorAPI
        {
            get
            {
                if (string.IsNullOrWhiteSpace(WebSiteName))
                    return false;

                return WebSiteName.ContainsWihtoutCase(".externalvendor");
            }
        }



        public static bool IsLocalhostWebSiteName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(WebSiteName))
                    return false;

                return WebSiteName.Contains(".Local");
            }
        }

        public static bool IsExternalVendorApiLogTable
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ApiRequestLogTableName))
                    return false;

                return ApiRequestLogTableName.ContainsWihtoutCase("ExternalVendorAPI");
            }
        }


        public static bool IsApiRequestLogEnable
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<bool>("IsApiRequestLogEnable", true);
            }
        }


        /// <summary>
        /// 是否使用 redis/遠端 型的cache (而非memory cache)
        /// </summary>
        public static bool IsRemoteCache
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<bool>("IsRemoteCache" , false);
            }
        }


        public static string IsEnableCache
        {
            get
            {
                return BaseSettingHelper.GetSettingValue("IsEnableCache");
            }
        }

        public static int PromoteBrandType
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<int>("PromoteBrandType", 0);
            }
        }


        public static bool IsDisablePGSLobby
        {
            get
            {
                return IsBWG;
            }
        }


        public static bool IsBWG
        {
            get
            {
                return PromoteBrandTypeEnum == PromoteBrandTypeEnum.BWG;
            }
        }


        public static PromoteBrandTypeEnum PromoteBrandTypeEnum
        {
            get
            {
                return (PromoteBrandTypeEnum)PromoteBrandType;
            }
        }


        public static string PromoteBrandCodeName
        {
            get
            {
                //參考 PromoteBrandTypeEnum.cs
                return PromoteBrandTypeEnum.GetDescription();
            }
        }


        public static string SeamlessAPIVendorCode
        {
            get
            {
                return PromoteBrandCodeName.ToLower();
            }
        }


        public static int MaxServerPlayerCount
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<int>("MaxServerPlayerCount", 300);
            }
        }

        public static string DefaultLang
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>("DefaultLang", "zh-cn");
            }
        }

        public static string DefaultUserName
        {
            get
            {
                string defaultValue = IsProduction ? "adminV2" : "admin";
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>("DefaultUserName", defaultValue);
            }
        }

        public static string ApiRequestLogTableName
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>("ApiRequestLogTableName", "[dbo].[ApiRequestLog]");
            }
        }


        public static string ApiRequestLogTableName_Error
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>("ApiRequestErrorLogTableName", "[dbo].[ApiRequestLogError]");
            }
        }


        /// <summary>
        /// 對應 ElasticLogTableTypeEnum
        /// </summary>
        public static int ApiRequestLog_ErrorLogTableType
        {
            get
            {
                var myValue = BaseSettingHelper.GetSettingValueOrDefaultValue<int>("ApiRequestLog_ErrorLogTableType", 0);

                if (myValue > 0)
                    return myValue;

                string tableName = ApiRequestLogTableName_Error ?? "";
                tableName = tableName.Replace("[dbo].[", "");
                tableName = tableName.Replace("]", "");

                var parseValue = ExtensionOfEnum.GetEnumByTextValue(tableName, (ElasticLogTableTypeEnum)0);
                return (int)parseValue;
            }
        }


        /// <summary>
        /// 是否採用hostService 定時同步localFile至儲存空間
        /// </summary>
        public static bool ApiRequestLogBackgroundSyncEnable
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<bool>("ApiRequestLogBackgroundSyncEnable", false);
            }
        }


        public static bool SWCallbackLogBackgroundSyncEnable
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<bool>("SWCallbackLogBackgroundSyncEnable", false);
            }
        }


        public static bool IsKestrelWebHost
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<bool>("IsKestrelWebHost", false);
            }
        }


        /// <summary>
        /// 是否採用hostService 定時同步localFile至儲存空間
        /// </summary>
        public static bool GetConfig_ApiRequestLogDailyScheculeSyncEnable(IConfiguration config)
        {
            var settingValue = config.GetSection("ApiRequestLogDailyScheculeSyncEnable")?.Value;
            return BaseSettingHelper.ConvertSettingValueToValueType<bool>(settingValue, false);
        }


        public static string WebHostListenPort
        {
            get
            {
                return BaseSettingHelper.GetSettingValue("WebHostListenPort");
            }
        }



        public static SlackNotifySetting SlackNotify => new SlackNotifySetting();

        public static GameAPISetting GameApiSetting => new GameAPISetting();

        public static AdminSysAPISetting AdminSysAPISetting => new AdminSysAPISetting();

        public static ThirdPartySolutionSetting ThirdParty => new ThirdPartySolutionSetting();

        public static PublicApiSetting PublicApiSetting => new PublicApiSetting();

        public static FileApiSetting FileApiSetting => new FileApiSetting();

    }

    public class FileApiSetting
    {
        public string MinioApiEndPoint
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>("MinioApiEndPoint", "");
            }
        }

        public string MinioApiBucket
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>("MinioApiBucket", "");
            }
        }

        public string MinioApiAccessKey
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>("MinioApiAccessKey", "");
            }
        }


        public string MinioApiSecretKey
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>("MinioApiSecretKey", "");
            }
        }

        public string MinioImageUrl
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>("MinioImageUrl", "");
            }
        }

        public bool IsMinioUseSSL
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<bool>("IsMinioUseSSL", false);
            }
        }
    }


    public class PublicApiSetting
    {
        public int Slot88MD5ExpiredMinutes
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<int>("Slot88MD5ExpiredMinutes", 10080);
            }
        }
    }


    public class ThirdPartySolutionSetting
    {

        private static string Prefix = "ThirdParty:";


        public string Telegram_Bot_SecretToken
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>(Prefix + "Telegram_Bot_SecretToken", "c88e9c24-87ad-4ba4-a670-12317f1fb669");
            }
        }


        public string RedisConn
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>(Prefix + "RedisConn", "");
            }
        }

        /// <summary>
        /// 某些環境會共用redis，然後會用 DB編號區別開來
        /// </summary>
        public int RedisDBNumber
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<int>(Prefix + "RedisDBNumber", 0);
            }
        }


        /// <summary>
        /// 連結逾時秒數
        /// </summary>
        public int RedisTimeoutSeconds
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<int>(Prefix + "RedisTimeoutSeconds", 10);
            }
        }


        public string MongoDBConn
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>(Prefix + "MongoDBConn", "");
            }
        }


        public string MongoDBDBName
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>(Prefix + "MongoDBDBName", "");
            }
        }


        public string ElastisearchAPIUrl
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>(Prefix + "ElastisearchAPIUrl", "");
            }
        }


        public string ElastisearchIndexPrefixName
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>(Prefix + "ElastisearchIndexPrefixName", "");
            }
        }



        /// <summary>
        /// 不含 Basic 只填入後面的token 或 帳號:密碼 base64
        /// </summary>
        public string ElastisearchAPIAuthorizationToekn
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>(Prefix + "ElastisearchAPIAuthorizationToekn", "");
            }
        }



        //public string Redis002_Conn
        //{
        //    get
        //    {
        //        return BaseSettingHelper.GetSettingValueOrDefaultValue<string>(Prefix + "Redis002_Conn", "");
        //    }
        //}


        ///// <summary>
        ///// 某些環境會共用redis，然後會用 DB編號區別開來
        ///// </summary>
        //public int Redis002_DBNumber
        //{
        //    get
        //    {
        //        return BaseSettingHelper.GetSettingValueOrDefaultValue<int>(Prefix + "Redis002_DBNumber", 0);
        //    }
        //}


        //public int Redis002_TimeoutSeconds
        //{
        //    get
        //    {
        //        return BaseSettingHelper.GetSettingValueOrDefaultValue<int>(Prefix + "Redis002_TimeoutSeconds", 0);
        //    }
        //}


    }


    public class AdminSysAPISetting
    {
        private static string Prefix = "AdminSys:";

        /// <summary>
        /// 對外開放可連線的url (需請 SRE 開通防火牆)
        /// </summary>
        public string PublicUrlDomain
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>(Prefix + "PublicUrlDomain", "");
            }
        }

    }






    public class GameAPISetting
    {

        /// <summary>
        /// Game API 請求都要帶這個
        /// </summary>
        public string ApiCertificateValue
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>("ApiCertificateValue", "851b-222e5f24024b");
            }
        }

    }




    public class SlackNotifySetting
    {
        private static string Prefix = "SlackNotify:";


        /// <summary>
        /// 當跳出一般C# exception的時候發通知
        /// </summary>
        public bool IsNotifyEnable_When_SystemException
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<bool>(Prefix + "IsNotifyEnable_When_SystemException", true);
            }
        }


        public string NotifyChannel_When_SystemException
        {
            get
            {
                return BaseSettingHelper.GetSettingValueOrDefaultValue<string>(Prefix + "NotifyChannel_When_SystemException", "n_error_game_api");
            }
        }

    }





}
