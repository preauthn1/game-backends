using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure
{
    public class SettingHelper
    {

        public static string WebSiteName
        {
            get
            {
                return GetAppSetting<string>("WebSiteName", "");
            }
        }


        public static bool IsDev
        {
            get
            {
                string website = WebSiteName ?? "";
                return website.Contains("Local") || website.Contains("Dev");
            }
        }


        public static bool IsProduction
        {
            get
            {
                return (WebSiteName ?? "").Contains("Product");
            }
        }
        
        /// <summary>
        /// console專案才有的設定
        /// </summary>
        public static bool IsConsoleProgram
        {
            get
            {
                return GetAppSetting<bool>("IsConsoleProgram", false);
            }
        }

        /// <summary>
        /// 系統預設的 預設最高級組織Id
        /// </summary>
        public static int DefaultAdminOrganizationId
        {
            get
            {
                return GetAppSetting<int>("DefaultAdminOrganizationId", 1);
            }
        }


        /// <summary>
        /// 系統預設的 預設最高級組織Id
        /// </summary>
        public static string ApiRequestLogTableName
        {
            get
            {
                return GetAppSetting<string>("ApiRequestLogTableName", "ApiRequestLog");
            }
        }


        /// <summary>
        /// 使用哪種cache機制
        /// </summary>
        public static int CacheInstanceType
        {
            get
            {
                return GetAppSetting<int>("CacheInstanceType", 0);
            }
        }


        /// <summary>
        /// 是否開啟全域RequestLog
        /// </summary>
        public static bool IsGlobalApiLogEnable
        {
            get
            {
                return GetAppSetting<bool>("IsGlobalApiLogEnable", false);
            }
        }


        /// <summary>
        /// exception時，是否發slack通知
        /// </summary>
        public static bool IsAlwaysSlackNotifyWhenExceotion
        {
            get
            {
                return GetAppSetting<bool>("IsAlwaysSlackNotifyWhenExceotion", false);
            }
        }



        /// <summary>
        /// exception時，是否發slack通知
        /// </summary>
        public static bool IsAlwaysSlackNotifyWhenTerproExceotion
        {
            get
            {
                return GetAppSetting<bool>("IsAlwaysSlackNotifyWhenTerproExceotion", false);
            }
        }



        public static long ThirdPartyGamePlayMinMoney
        {
            get
            {
                return GetAppSetting<long>("ThirdPartyGamePlayMinMoney", 5);
            }
        }


        public static SettingOfJobAPI JobAPI = new SettingOfJobAPI();



        public static T GetAppSetting<T>(string key, T defaultValue)
        {
            return SettingExtension.GetAppSetting<T>(key, defaultValue);
        }
    }



    public class SettingOfJobAPI
    {

        /// <summary>
        /// 以每幾個小時為單位 切割待處理的注單
        /// </summary>
        public int SplitBetRecordHours
        {
            get
            {
                return GetAppSetting<int>("SplitBetRecordHours", 6);
            }
        }

        
        /// <summary>
        /// 三方排程，會往前含蓋某種時間範圍內的資料
        /// </summary>
        public int DataBeginTime_PreviousSeconds
        {
            get
            {
                return GetAppSetting<int>("DataBeginTime_PreviousSeconds", 600);
            }
        }
        

        public static T GetAppSetting<T>(string key, T defaultValue)
        {
            return SettingExtension.GetAppSetting<T>(key, defaultValue);
        }

    }






    internal class SettingExtension
    {
        public static T GetAppSetting<T>(string key)
        {
            var stringValue = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrWhiteSpace(stringValue))
                throw new Exception("miss settingConfig: " + key);

            return (T)Convert.ChangeType(stringValue, typeof(T));
        }


        public static T GetAppSetting<T>(string key, T defaultValue)
        {
            var stringValue = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrWhiteSpace(stringValue))
                return defaultValue;

            return (T)Convert.ChangeType(stringValue, typeof(T));
        }

    }



}
