using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using target.Models.ViewModels.Base;
using target.Utility.Extensions;
using target.Models;
using target.Models.Models;
using target.Utility.Helper;
using System;
using System.Linq;
using target.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using example.invalid;
using Microsoft.AspNetCore.Diagnostics;
using System.Data.SqlClient;
using System.Collections.Generic;
using target.Utility.Utility.ThirdParty;
using example.invalid;

namespace target.Web.Modules.Filter
{

    public static class ExceptionHandler
    {

       
        public static void UseGlobalExceptionHandler(this IApplicationBuilder appBuilder)
        {
            appBuilder.UseExceptionHandler(app =>
            {
                app.Run(async context =>
                {
                    var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                    if (ex == null)
                        return;

                    var converResult = ConvertToApiModel(ex, context.Request);

                    int statusCode = converResult.StatusCode;
                    ApiResponseErrorModel res = converResult.Response;

                    context.Response.StatusCode = statusCode;
                    context.Response.ContentType = "application/json";

                    string requestBodyAsString = string.Empty;
                    var stream = context.Request.Body;

                    using (var reader = new StreamReader(stream))
                    {
                        if (context.Request.Method != "GET")
                        {
                            requestBodyAsString = await reader.ReadToEndAsync();
                        }

                        if (stream.CanSeek)
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                        }
                    }

                    string msg = $"{context.Request?.Path} API {converResult.Response.Message} Body:{Environment.NewLine}{requestBodyAsString} ";
                    MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                    {
                        TelegramChannelEnum = TelegramChannelEnum.pgs_contract_notify,
                        Message = msg
                    });

                    await context.Response.WriteAsync(res.ConvertToJsonString());
                });
            });
        }

        public static (ApiResponseErrorModel Response,  int StatusCode) ConvertToApiModel(Exception ex, HttpRequest request)
        {
            int statusCode = 400;
            var customizeException = ex as TerproException;


            var res = new ApiResponseErrorModel
            {
                Code = ApiErrorCode.InternalServerError.ToInteger(),
                Message = ex.GetMessageAndErrorLine(),
                Data = null
            };

            if (customizeException != null)
            {
                res.Code = customizeException.ErrroCode.ToInteger();
                res.Data = customizeException.ErrorDetail;

                string extraMsg = "";

                if(customizeException.ErrorDetail != null)
                {
                    if(!string.IsNullOrWhiteSpace(customizeException.ErrorDetail.ColumnName))
                    {
                        extraMsg = $" ({customizeException.ErrorDetail.ColumnName})";
                    }
                    else if(!string.IsNullOrWhiteSpace(customizeException.ErrorDetail.ReplaceMessage))
                    {
                        extraMsg = $" ({customizeException.ErrorDetail.ReplaceMessage})";
                    }
                }

                res.Message = customizeException.ErrroCode.ToString() + extraMsg;
            }
            else
            {
                int rndText = RandomHelper.GetRandIntegerNumbers(1000, 9999);    
            }

            return (res, statusCode);
        }


    }



}
