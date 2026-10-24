using game.Infrastructure.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using example.invalid.Http;
using System.Web.Http;
using game.Infrastructure.Extensions;
using gitlab_hook.Models.MergeRequest;
using game.Infrastructure.Utility;
using gitlab_hook.Models;
using Newtonsoft.Json.Linq;

namespace gitlab_hook.Controllers
{

    public partial class SlackTriggerHookController : ApiController
    {

        private const string Channel_ID_UAT = "CQG1FBU49";
        private const string Channel_ID_Production = "CQB5JPPDY";

        private static List<string> AvailableSiteName = new List<string>
        {
            "admin",
            "agent",
            "gameplay",
            "payment",
        };

        private static List<string> AvailableSiteShortName = new List<string>
        {
            "ad",
            "ag",
            "pay",
            "gp",
        };

        [Route("Hook/Trigger-MR")]
        [HttpPost]
        public object TriggerMergeRequest([FromBody]JObject data)
        {
            string channel_id = (string)data["channel_id"];
            string channel_name = (string)data["channel_name"];
            string command = (string)data["command"];
            string text = (string)data["text"];

            if (channel_id != Channel_ID_UAT &&
                channel_id != Channel_ID_Production)
            {
                return new ResponseSlackApiResultModel("這個指令，不支援在此頻道執行～");
            }

            text = text ?? "";
            text = text.ToLower();

            var arg = text.Split(' ').Where(x=> !string.IsNullOrWhiteSpace(x)).ToList();

            if (string.IsNullOrWhiteSpace(text))
            {
                return new ResponseSlackApiResultModel("缺少參數1，請輸入 uat 或 pro (縮寫為 u 或 p)");
            }


            string OK_siteNames1 = string.Join(" / ", AvailableSiteName);
            string OK_siteNames2 = string.Join(" / ", AvailableSiteShortName);

            if (arg.Count() == 1)
            {
                return new ResponseSlackApiResultModel($"缺少參數2，請輸入站台分類 ( {OK_siteNames1} ) 或縮寫 ( {OK_siteNames2} )");
            }

            string env = arg[0];
            string targetSite = arg[1];


            if(!AvailableSiteName.Contains(targetSite) && 
               !AvailableSiteShortName.Contains(targetSite))
            {
                return new ResponseSlackApiResultModel($"參數2不合法，請輸入支援的站台分類 ( {OK_siteNames1} ) 或縮寫 ( {OK_siteNames2} )");
            }

            command = command.ToLower().Replace("/" , "");

            env = env == "u" ? "uat" : env;
            env = env == "p" ? "pro" : env;

            bool isUAT = env == "uat";
            bool isPro = env == "pro";

            text = text.Trim().ToLower();

            string scheme = System.Web.HttpContext.Current.Request.Url.Scheme;
            string host = System.Web.HttpContext.Current.Request.Url.Authority;
            
            string targetApiUrl = string.Empty;

            if (isUAT)
                targetApiUrl = "git/merge_to/UAT";
            else if(isPro)
                targetApiUrl = "git/merge_to/Production";
            else
            {
                return new ResponseSlackApiResultModel("參數1輸入不支援的數值:" + env);
            }

            string responseText = string.Empty;

            var qqq = APIRequestHelper.RequestApiResult<string>(new RequestParameters
            {
                BaseUrl = $"{scheme}://{host}",
                ApiUrl = $"{targetApiUrl}?type=" + targetSite,
                Method = HttpMethodType.HttpGet
            });

            responseText = qqq.ResponseText;

            string resulgMsg = $"api回應: `{responseText}` , (若不是得到OK回應 代表發生某些問題) {text}";

            if (responseText.Contains("OK"))
            {
                resulgMsg = $"成功 , 你指定環境為: `{env}` , 站台為 `{targetSite}` ";
            }

            var result = new ResponseSlackApiResultModel(resulgMsg);
            return result;
        }
    }



    public class ResponseSlackApiResultModel
    {
        public ResponseSlackApiResultModel(string msg)
        {
            this.response_type = "";
            this.text = msg;
        }

        public string response_type { get; set; }

        public string text { get; set; }
    }



}
