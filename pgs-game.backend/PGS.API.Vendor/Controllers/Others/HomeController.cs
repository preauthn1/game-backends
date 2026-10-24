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
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Utility;
using target.Utility.Helper;
using target.Web.Modules.Filter;

namespace target.API.Vendor.Controllers
{
    public class HomeController : BaseController
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiConfig(IsLogApiEnable = false)]
        [Route("")]
        [HttpGet]
        public object HomePage(string password = "")
        {
            bool isPasswordIncorrect = password != SystemConstant.PublicAPI_SignKey_MagicPassword;

            if (isPasswordIncorrect && !IPHelper.IsCompanyIP())
            {
                return RuntimeHelper.GetHideInformationForProduction();
            }

            return RuntimeHelper.GetHomePageDebugInformation();
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiConfig(IsLogApiEnable = false)]
        [Route("v1")]
        [Route("v2")]
        [HttpGet]
        public object V1Api()
        {
            return new
            {
                msg = "Please refer the API Document to call correct api url. (请参考文件呼叫正确API url)"
            };
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiConfig(IsLogApiEnable = false)]
        [Route("common-error")]
        public object ShowCommonErrorMessage(int errorCode = 0 , string msg = "您的連結以失效。請重新聯繫我們" , string extraHint= "")
        {
            string msg2 = string.Empty;

            if (errorCode > 40000)
            {
                msg = MultiLanguageTextHelper.GetErrorCodeMsg((ApiErrorCode)errorCode, null, new List<LanguageEnum> { LanguageEnum.en_us }) + Environment.NewLine;
                msg2 = MultiLanguageTextHelper.GetErrorCodeMsg((ApiErrorCode)errorCode, null, new List<LanguageEnum> { LanguageEnum.zh_cn }) + Environment.NewLine;
            }

            if(!string.IsNullOrWhiteSpace(extraHint))
            {
                extraHint = $"({extraHint})";
            }

            string finalHtml = $@"<!DOCTYPE html>
                                <html lang='en'>

                                <head>
                                    <meta charset='utf-8' />
                                    <title> Error occur! (發生錯誤)</title>
                                    <link rel='stylesheet' href='/css/swagger_customized.css' />
                                    <link rel='stylesheet' href='https://example.invalid/bootstrap/3.4.1/css/bootstrap.min.css' >
                                </head>
                                <body>
                                    <div class='text-c' style='padding-top: 25px;'>

                                        <div class='error-msg-1'>
                                          ☹️  Something went wrong
                                        </div>

                                        <div style='font-size:28px'>
                                            <div>{msg} {extraHint}</div>
                                            <div style='font-size:24px'>{msg2} {extraHint}</div>          
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
