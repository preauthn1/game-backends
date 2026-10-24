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

namespace target.Web.Modules.Filter
{

    /// <summary>
    /// 建議欄位名稱/型別  都定義跟 ApiConfig 一樣
    /// InstanceDefaultApiConfigValue 這邊會inject
    /// </summary>
    public class ApiConfigAnalyzeModel
    {

        public bool IsLogApiEnable { get; set; }

        public bool IsRequestBodyLogEnable { get; set; }

        public bool IsResponseLogEnable { get; set; }

        public bool IsRedirectToErrorPage { get; set; }

        public bool IsHeaderLogEnable { get; set; }

        /// <summary>
        /// 定義的時間期間內。會停止紀錄
        /// </summary>
        public bool IsDisabledRequestBodyLogOnlyForWeeks { get; set; }


        /// <summary>
        /// 這個欄位幾乎廢棄了 不要再繼續使用了
        /// </summary>
        public bool IsWriteToOtherLogTable { get; set; }

        public ApiConfigCategoryEnum ApiCategory { get; set; }

        public ApiConfigSubCategoryEnum SubCategory { get; set; }

        public HeaderLogLevelType HeaderLogLevelType { get; set; }

        /// <summary>
        /// 對應 LoggingPlayerID attribute
        /// </summary>
        public bool IsLoggingPlayerID { get; set; }

        public string Develop_TypeScriptClassDefine { get; set; }

    }

}
