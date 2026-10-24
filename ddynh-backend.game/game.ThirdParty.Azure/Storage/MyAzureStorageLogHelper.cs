using game.ThirdParty.Azure.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.ThirdParty.Azure
{
    public static class MyAzureStorageLogHelper
    {
        //請確保以下config設定有設好

        private static string _configName_AzureStorageConn = "AzureStorageConn";
        private static string _configName_IsAzureStorageEnable = "IsAzureStorageEnable";
        private static string _configName_WebSiteEnv = "WebSiteEnv";



        private static string GetAzureStorageConn()
        {
            return ConfigurationManager.AppSettings[_configName_AzureStorageConn];
        }

        /// <summary>
        /// 支付API專用的log紀錄
        /// </summary>
        /// <param name="value"></param>
        public static void WritePaymentApiLog(PaymentApiStorageLogModel value)
        {
            //先檢查config開關是否有開
            if (!IsLogFlagEnable())
                return;

            string conn = GetAzureStorageConn();
            
            if (string.IsNullOrWhiteSpace(conn))
                return;

            try
            {
                var service = new AzureStorangeLogService(conn);

                string tableName = GetTableName();

                service.InsertDataAsync(tableName, value);

            }
            catch(Exception ex)
            {
                string msg = "";
            }
        }



        private static string GetTableName()
        {
            string webSiteEnv = ConfigurationManager.AppSettings[_configName_WebSiteEnv] ?? "Dev";
            var query = DateTime.UtcNow.AddHours(8);

            //table名稱禁止特殊符號
            return $"{webSiteEnv}PayApiLog{query.ToString("yyyyMM")}";
        }


        private static bool IsLogFlagEnable()
        {
            try
            {
                string setting = ConfigurationManager.AppSettings[_configName_IsAzureStorageEnable];

                if (string.IsNullOrWhiteSpace(setting))
                    return false;

                return Boolean.Parse(setting);
            }
            catch
            {

            }

            return false;
        }


    }
}
