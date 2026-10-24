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

namespace target.Web.Modules.Filter
{

    /// <summary>
    /// 全域api log日誌
    /// </summary>
    public class GlobalRequestLogMiddleware
    {
        private readonly RequestDelegate _next;


        public GlobalRequestLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var url = context.Request.Path.ToString();

            // Swagger 與停用 request log 時不需要緩衝整個 response。
            if (url.ContainsWihtoutCase("swagger") ||
                IsUselessHttpMethod(context) ||
                !SettingHelper.IsApiRequestLogEnable)
            {
                await _next(context);
                return;
            }

            DateTime requestBeginTime = DateTimeHelper.GetCurrentTime();

            HeaderValueHelper.SetHeaderValue(SystemConstant.HeaderKey_RequestBeginTime, requestBeginTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            Stream originalBodyStream = context.Response.Body;
            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;
                try
                {
                    await _next(context);

                    DateTime requestEndTime = DateTimeHelper.GetCurrentTime();
                    int executedMilliseconds = (int)(requestEndTime - requestBeginTime).TotalMilliseconds;

                    WriteRequestAndResponseLogAsync(context, executedMilliseconds);

                    // _next 寫完後 position 在 stream 尾端；不要依賴 log helper 的副作用重設它。
                    responseBody.Position = 0;
                    await responseBody.CopyToAsync(originalBodyStream);
                }
                finally
                {
                    context.Response.Body = originalBodyStream;
                }
            }
        }


        private bool IsUselessHttpMethod(HttpContext context)
        {
            string httpMethod = context.Request.Method?.ToUpper();

            if (httpMethod == "OPTIONS" || httpMethod == "HEAD" || httpMethod == "TRACE")
                return true;

            return false;
        }


        private void WriteRequestAndResponseLogAsync(HttpContext context, int executedMilliseconds)
        {
            try
            {
                bool isSuccess = context.Response.StatusCode < 400;
                bool is404_code = context.Response.StatusCode == StatusCodes.Status400BadRequest;

                bool isWriteErrorLog = is404_code && HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_MustWriteErrorLog) == "1";

                if (!isSuccess && !isWriteErrorLog)
                    return;

                var logModel = HttpRequestLogHelper.InstanceResponseLogModel(context.Response);                 
                HttpRequestLogHelper.SetRequestInfo(context.Request, logModel);

                logModel.ExecutionTime = executedMilliseconds;

                if (isSuccess)
                {
                    WriteApiRequestLogToTable(logModel);
                }
                else
                {
                    WriteApiRequestLogToTable(logModel, isWriteToErrorLog: true);
                }
            }
            catch(Exception ex)
            {
                string debug = "";
            }
        }


        private void WriteApiRequestLogToTable(ApiRequestLogEntity logModel, bool isWriteToErrorLog = false)
        {
            bool isLogEnable = ApiConfigDefineHelper.GetApiConfigSettingByCurrentRequest().IsLogApiEnable;

            if (!isLogEnable)
                return;

            bool is404NotFound = logModel.ResponseStatusCode == 404;
            string tableName = DetermineTableName(is404NotFound);

            if (is404NotFound)
            {
                List<string> skipKeyword = new List<string>
                {
                    "acme-challenge" ,
                    ".env" ,
                    "favicon.ico" ,
                    "wp-content" ,
                    "robots.txt" ,
                    "owa/"
                };

                //好像有一些 爬蟲/惡意讀取的請求 這些404也不記錄了
                foreach (string word in skipKeyword)
                {
                    if (logModel.RelativeUrl.Contains(word))
                    {
                        return;
                    }
                }
            }

            var service = IOCHelper.Resolve<IApiRequestLogService>();

            bool isSyncToBufferingMode = SettingHelper.ApiRequestLogBackgroundSyncEnable;

            //根據設置採用直接寫db 或 先寫到local
            if (isWriteToErrorLog)
            {
                string errorTableName = SettingHelper.ApiRequestLogTableName_Error;
                service.CreateErrorLogTryCatch(errorTableName, logModel);
            }
            else if (isSyncToBufferingMode)
            {
                var extraModel = DataModelConvertHelper.InstanceLogQueueExtraModel(tableName);
  
                service.WriteToQueueBuffering(new ApiRequestLogQueueTempModel
                {
                    ApiRequestLogEntity = logModel,
                    ExtraInfoV2 = extraModel
                });
            }
            else
            {
                service.CreateTryCatch(tableName, logModel);
            }
        }


        private string DetermineTableName(bool is404NotFound)
        {
            if (is404NotFound)
                return SettingHelper.ApiRequestLogTableName_Error;

            return SettingHelper.ApiRequestLogTableName;
        }
    }

}
