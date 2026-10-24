using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using example.invalid;
using System.Text;
using target.Utility.Extensions;
using example.invalid;
using Newtonsoft.Json;
using target.Models.Table;
using target.Utility.Helper.Dependency;
using target.Utility.IOC;
using target.BL;
using target.Utility.Helper.HttpContext;
using target.Utility;
using target.BL.Interface;
using Microsoft.AspNetCore.Routing;
using target.Utility.Helper;
using target.Utility.Extensions.Web;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using target.Models;
using target.Models.Models;
using target.Models.ViewModels.Base;
using DocumentFormat.OpenXml.EMMA;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Linq;

namespace target.Web.Modules.Filter
{

    /// <summary>
    /// 攔截 http 405 錯誤 並返回標準錯誤訊息
    /// </summary>
    public class HttpStatusCodeHandleMiddleware
    {
        private readonly RequestDelegate _next;


        public HttpStatusCodeHandleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);

            bool is404 = context.Response.StatusCode == StatusCodes.Status404NotFound;
            bool is405 = context.Response.StatusCode == StatusCodes.Status405MethodNotAllowed;
            bool is406 = context.Response.StatusCode == StatusCodes.Status406NotAcceptable;

            int originalStatusCode = context.Response.StatusCode;

            string relativeUrl = context.Request?.Path;

            //EFG這家遊戲商不要走這種攔截邏輯
            if (is404 && relativeUrl.Contains("EFG/"))
            {
                return;
            }

            if (is404 || is405 || is406)
            {
                // 在这里处理 HTTP 405 错误
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 400;

                bool isSkipWriteErrorLog = false;
                var errorCode = ApiErrorCode.MethodNotAllow;

                relativeUrl = relativeUrl ?? "";

                string extraText = "";

                if(is406)
                {
                    errorCode = ApiErrorCode.HttpStatusCodeError;
                    extraText = $" StatusCode:{originalStatusCode}";
                }
                else if (is404)
                {
                    if (IsSkipWriteErrorLogUrl(context))
                    {
                        isSkipWriteErrorLog = true;
                    }

                    //同時滿足404 又長度超標 應該很明顯是異常請求了
                    if(relativeUrl.Count() >= 50)
                    {
                        errorCode = ApiErrorCode.NotFoundPageOrAPiUrlPathLengthInvalid;
                    }
                    else                        
                    {
                        errorCode = ApiErrorCode.NotFoundPageOrAPiUrl;
                    }
                }

                int timezone = 8;
                string debug_time = DateTimeHelper.GetCurrentTimeYYYY_MM_DD_HH_MM_SS(timezone);
                string print_time_info = $"{context.Request.Method} ／ {debug_time} (GMT+{((int)timezone)})";

                var errorModel = new ApiResponseErrorModel
                {
                    Code = errorCode.ToInteger(),
                    Data = new
                    {
                        DebugExtraInfo = print_time_info + extraText,
                        DebugExtraInfo2 = GetRequestUrlAndQueryForDebug(context.Request),
                    },
                    Message = MultiLanguageTextHelper.GetErrorCodeMsg(errorCode , null)
                };

                HeaderValueHelper.AppendDebugInfo(errorCode.ToIntegerString());

                if(!isSkipWriteErrorLog)
                {
                    //後面寫log需要判斷
                    HeaderValueHelper.AppendHeaderKey_MustWriteErrorLog();
                }

                await context.Response.WriteAsync(errorModel.ConvertToJsonString());
            }
        }


        private static bool IsSkipWriteErrorLogUrl(HttpContext context)
        {
            try
            {
                if (context.Request == null)
                    return false;

                if (context.Request.Method == "HEAD")
                    return true;

                string url = context.Request.Path.ToString();

                if (url.ContainsWihtoutCase("favicon.ico"))
                {
                    return true;
                }
            }
            catch
            {

            }

            return false;
        }


        private static string GetRequestUrlAndQueryForDebug(HttpRequest request)
        {
            return request.GetRequestUrlForVendorAPIDebug();
        }


    }

}
