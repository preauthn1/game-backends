using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using target.Models.ViewModels.Base;
using example.invalid.Http;
using System.Collections.Generic;
using System.Linq;
using target.Models;
using target.Utility.Extensions.Web;
using target.Utility;
using target.Utility.Helper;
using System;

namespace target.Web.Modules.Filter
{
    /// <summary>
    /// 將api response格式 封裝成統一規格
    /// </summary>
    public class ApiResponseFormatterFilter : IActionFilter
    {

        private static List<string> skipFormatterApiKeywords = new List<string>
        {
            "target/validate-session",
            "secure/fishing",
        };

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }


        public void OnActionExecuted(ActionExecutedContext context)
        {
            var url = context.HttpContext.Request.Path.ToString();


            //如果dev有指定要返回Ts物件格式 (自製的懶人用轉換工具)
            if (SettingHelper.IsDev && HeaderValueHelper.IsResponseTypeScriptClass())
            {
                var myApiConfig = ApiConfigDefineHelper.GetApiConfigSettingByCurrentRequest();

                string responseText = HeaderValueHelper.HeaderTemp_ApiUniqueRouteName() + Environment.NewLine + Environment.NewLine;
                responseText += myApiConfig?.Develop_TypeScriptClassDefine;

                context.Result = new ContentResult()
                {
                    Content = responseText,
                    ContentType = "text/html",
                    StatusCode = 200,
                };

                return;
            }

            var apiCategory = ApiConfigDefineHelper.GetApiConfigSettingByCurrentRequest().ApiCategory;

            //如果有指定特定的API 不採用包裝的格式
            if (skipFormatterApiKeywords.Any(keyword => url.Contains(keyword)))
                return;

            if (apiCategory == Models.Enums.ApiConfigCategoryEnum.VendorPlaformCustomizedAPI)
                return;

            var emptyResult = context.Result as EmptyResult;

            if (emptyResult != null)
            {
                var newInstance = new ApiResponseModel
                {
                    Message = "OK",
                    Data = null
                };

                context.Result = new ObjectResult(newInstance)
                {
                    StatusCode = 200,
                    DeclaredType = typeof(ApiResponseModel)
                };

                return;
            }

            //判斷是否為OKObject
            var okObject = context.Result as OkObjectResult;
            if (okObject != null)
            {
                return;
            }

            //多包裝一層data欄位
            var objectResult = context.Result as ObjectResult;
            if (objectResult != null && objectResult.DeclaredType != typeof(HttpResponseMessage))
            {
                objectResult.Value = new ApiResponseModel
                {
                    Message = "OK",
                    Data = objectResult.Value,
                };

                objectResult.DeclaredType = typeof(ApiResponseModel);
                return;
            }

            return;
        }

    }

}
