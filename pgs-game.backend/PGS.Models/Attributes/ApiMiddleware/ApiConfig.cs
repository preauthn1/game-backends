using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using target.Models.ViewModels.Base;
using example.invalid.Http;
using System.Collections.Generic;
using System.Linq;
using System;
using target.Models.Enums;
using Newtonsoft.Json;
using System.ComponentModel;

namespace target.Web.Modules.Filter
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class ApiConfig : System.Attribute
    {

        public bool IsLogApiEnable { get; set; }

        public bool IsRequestBodyLogEnable { get; set; }

        /// <summary>
        /// 只有在定義的時間期間內才會停止紀錄
        /// </summary>
        public bool IsDisabledRequestBodyLogOnlyForWeeks { get; set; }

        /// <summary>
        /// 是否紀錄 responseText 
        /// </summary>
        public bool IsResponseLogEnable { get; set; }

        public bool IsRedirectToErrorPage { get; set; }

        public bool IsHeaderLogEnable { get; set; }

        /// <summary>
        /// 僅部分Log資料才有實作此邏輯
        /// </summary>
        public bool IsWriteToOtherLogTable { get; set; }

        public ApiConfigCategoryEnum ApiCategory { get; set; }

        public ApiConfigSubCategoryEnum SubCategory { get; set; }

        public HeaderLogLevelType HeaderLogLevelType { get; set; }



        public ApiConfig()
        {
            IsLogApiEnable = true;
            IsResponseLogEnable = true;
            IsRequestBodyLogEnable = true;
            IsDisabledRequestBodyLogOnlyForWeeks = false;
            IsRedirectToErrorPage = false;
            IsWriteToOtherLogTable = false;
            IsHeaderLogEnable = true;
            ApiCategory = ApiConfigCategoryEnum.Default;
            SubCategory = ApiConfigSubCategoryEnum.Default;
            HeaderLogLevelType = HeaderLogLevelType.Default;

            //請參考 ApiConfigAnalyzeModel  那邊也要定義一次model
            //請參考 ApiConfigAnalyzeModel  那邊也要定義一次model
            //請參考 ApiConfigAnalyzeModel  那邊也要定義一次model
            //請參考 ApiConfigAnalyzeModel  那邊也要定義一次model
        }
    }


    public enum HeaderLogLevelType
    {

        Default = 0,

        [Description("X-Real-IP,True-Client-Ip,cf-connecting-ip,X-Forwarded-For,X-Original-Forwarded-For")]
        ExternalVendorAPI_AGGBrief = 1001,
    }



}
