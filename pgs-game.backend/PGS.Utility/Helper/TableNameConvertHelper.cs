using DocumentFormat.OpenXml.Office2013.Excel;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Http;
using target.Models.CultureText;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace target.Utility.Helper
{

    public static class TableNameConvertHelper
    {

        public static string GetElasticsearchIndexName(ElasticLogTableTypeEnum logType)
        {
            string indexName = logType.ToString().ToLower();         
            return indexName;
        }


        public static string GetElasticsearchIndexNameByTableName(string tableName)
        {
            ElasticLogTableTypeEnum logTableType = GetElasticsearchIndexLogType(tableName);
            string indexName = GetElasticsearchIndexName(logTableType);
            return indexName;
        }


        public static bool IsEnableElasticLogByTable(ElasticLogTableTypeEnum table)
        {
            //BWG 開放的比較多。
            //target 則是逐步慢慢轉移
            if (table == ElasticLogTableTypeEnum.SinglewalletMerchantApiRequestLog ||
                table == ElasticLogTableTypeEnum.ExternalApiRequestlog ||
                table == ElasticLogTableTypeEnum.ApiRequestLogAdminSysAPI ||
                table == ElasticLogTableTypeEnum.ApiRequestLogAdminSysAPIError ||
                table == ElasticLogTableTypeEnum.ApiRequestLogExternalVendorApi ||
                table == ElasticLogTableTypeEnum.ApiRequestLogExternalVendorAPIError ||
                table == ElasticLogTableTypeEnum.ApiRequestLogGameApi ||
                table == ElasticLogTableTypeEnum.ApiRequestLogGameAPIError ||
                table == ElasticLogTableTypeEnum.ApiRequestLogPublicApi ||
                table == ElasticLogTableTypeEnum.ApiRequestLogPublicAPIError ||
                table == ElasticLogTableTypeEnum.ApiRequestLogJobAPI ||
                table == ElasticLogTableTypeEnum.ApiRequestLogJobAPIError ||
                table == ElasticLogTableTypeEnum.ApiRequestLogVendorAPI ||
                table == ElasticLogTableTypeEnum.ApiRequestLogVendorAPIError)
            {
                return true;
            }

            if(table == ElasticLogTableTypeEnum.None)
            {
                return false;
            }

            if (SettingHelper.IsBWG)
            {
                return true;
            }

            return false;
        }


        public static bool IsEnableElasticLogByTableName(string tableName)
        {
            ElasticLogTableTypeEnum logType = GetElasticsearchIndexLogType(tableName);

            bool isEnable = IsEnableElasticLogByTable(logType);
            return isEnable;
        }


        public static ElasticLogTableTypeEnum GetElasticsearchIndexLogType(string tableName)
        {
            tableName = tableName.Replace("[dbo].", "").Replace("[", "").Replace("]", "");
            tableName = tableName.Replace("APIErrorBrief", "APIError"); //elastic命名不會有後面的index

            var logTableType = ExtensionOfEnum.GetEnumByTextValue(tableName, ElasticLogTableTypeEnum.None);
            return logTableType;
        }


    }

}