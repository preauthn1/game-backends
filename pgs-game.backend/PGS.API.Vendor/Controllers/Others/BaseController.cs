using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using example.invalid;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using target.Models.Models;
using target.Utility.IOC;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Models.Enums;

namespace target.API.Vendor.Controllers
{
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class BaseController : ControllerBase
    {
        public BaseController()
        {
            InstanceAllServiceImplement(this);
        }


        protected void InstanceAllServiceImplement<T>(T controller) where T : class
        {
            var allService = this.GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.PropertyType.IsInterface)
                .Where(x => x.Name.ToLower().Contains("service"))
                .ToList();

            foreach (var item in allService)
            {
                var instanceValue = IOCHelper.Resolve(item.PropertyType);
                item.SetValue(this, instanceValue);
            }
        }


        protected ActionResult GenerateErrorContentResult(Exception ex)
        {
            var pgsEx = ex as TerproException;

            if(pgsEx == null)
            {
                int defaultCode = ApiErrorCode.InternalServerError.ToInteger();
                return GenerateErrorContentResult(defaultCode, "" , "");
            }

            int code = pgsEx.ErrroCode.ToInteger();
            return GenerateErrorContentResult(code , "" , pgsEx?.ErrorDetail?.ColumnName);
        }


        protected ActionResult GenerateErrorContentResult(int errorCode = 0, string msg = "您的連結以失效。請重新聯繫我們", string extraHint = "")
        {
            string msg2 = string.Empty;

            if (errorCode > 40000)
            {
                msg = MultiLanguageTextHelper.GetErrorCodeMsg((ApiErrorCode)errorCode, null, new List<LanguageEnum> { LanguageEnum.en_us }) + Environment.NewLine;
                msg2 = MultiLanguageTextHelper.GetErrorCodeMsg((ApiErrorCode)errorCode, null, new List<LanguageEnum> { LanguageEnum.zh_cn }) + Environment.NewLine;
            }

            if (!string.IsNullOrWhiteSpace(extraHint))
            {
                extraHint = $"({extraHint})";
            }

            string finalHtml = $@"<!DOCTYPE html>
                                <html lang='en'>

                                <head>
                                    <meta name='viewport' content='width=device-width,initial-scale=1'>
                                    <meta charset='utf-8' />
                                    <title> Error occur! (發生錯誤)</title>
                                    <link rel='stylesheet' href='/css/swagger_customized.css' />
                                    <link rel='stylesheet' href='https://example.invalid/bootstrap/3.4.1/css/bootstrap.min.css' >
                                </head>
                                <body>
                                    <div class='container text-c' style='padding-top: 25px;max-width: 1024px; margin: 0 auto;'>

                                        <div class='error-msg-1'>
                                          <span class='f24'> ☹️ </span>  Something went wrong
                                        </div>

                                        <div class='errorMsgText' >
                                            <div>{msg} {extraHint}</div>
                                            <div class='text-m-size' style='color: #666'>{msg2} {extraHint}</div>          
                                        </div>
                                    
                                        <div style='color:#AAA;margin-top:20px;'>
                                         Message generate by Pegasus 
                                        </div>

                                    </div>                                   
                                </body>
                                </html>";

            return Content(finalHtml, "text/html");
        }


    }
}
