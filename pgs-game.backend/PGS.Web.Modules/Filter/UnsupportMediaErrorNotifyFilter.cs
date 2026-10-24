using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using target.Models.ViewModels.Base;
using example.invalid.Http;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using target.Utility.Extensions;
using target.Models;
using target.Utility.Helper;
using target.Utility.Utility.ThirdParty;

namespace target.Web.Modules.Filter
{

    /// <summary>
    /// Dev環境才會標記此Filter。開發環境錯誤提示用
    /// unsupported 錯誤很容易發生在 新對接遊戲商的時候。忘記標記attribute
    /// 但是因為標準的exception 攔截不到 導致疏漏找不到錯誤原因
    /// </summary>
    public class UnsupportMediaErrorNotifyFilter : System.Attribute, IResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {

        }


        public void OnResultExecuting(ResultExecutingContext context)
        {
            var parseResult = context.Result as ObjectResult;

            if (parseResult == null)
                return;

            SendNotifyWhenUnsupportMediaError(context, parseResult);
        }


        private void SendNotifyWhenUnsupportMediaError(ResultExecutingContext context , ObjectResult objectResult)
        {
            if (objectResult.StatusCode == 415)
            {
                var parseToProblemDetail = objectResult.Value as ProblemDetails;

                if (parseToProblemDetail != null && (parseToProblemDetail.Title ?? "").ToLower().Contains("unsupported"))
                {
                    string hintMsg = $" `{context.HttpContext.Request.Path}` 發生415錯誤。" + parseToProblemDetail.Title;

                    //總之就是發訊息通知到 錯誤通知頻道、讓開發人員知道出現 unsupported media 了 (因為415錯誤傳統的exception hanlder攔截不到。造成了誤解)
                    MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                    {
                        Message = hintMsg,
                        Channel = SlackChannelEnum.n_error_external_api
                    });

                    MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                    {
                        Message = hintMsg,
                        TelegramChannelEnum = TelegramChannelEnum.pgs_externalAPI
                    });
                }
            }
        }

    }


}
