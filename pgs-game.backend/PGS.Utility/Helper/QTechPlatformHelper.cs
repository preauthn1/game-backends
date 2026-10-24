using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Http;
using target.Models.CultureText;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace target.Utility.Helper
{

    /// <summary>
    /// 某家平台的特殊機制實作
    /// </summary>
    public static class QTechPlatformHelper
    {
        public static QTechTestAPICreateSessionTokenModel CreateGameSessionToken(Action<ApiRequestLogTemporaryInfoModel> logAction)
        {
            //example.invalid   usd.test
            //example.invalid   pegop-usd.test

            var param = new RequestParameters
            {
                BaseUrl = "https://example.invalid/",
                ApiUrl = $"providers/PEG/login",
                Method = HttpMethodType.HttpPost,
                UserAgent = "PEG",
                LogAction = logAction
            };

            param.AddHeaderData("Pass-Key", "454d6b37-fc9f-47c2-9ad0-28a48abc85d4");

            var body = new
            {
                playerId = "pegop-usd.test",
                gameId = "290086",
                device = "DESKTOP",
                //screenName = "Player1",
                //language = "ZH_CN",
            };

            var result = APIRequestHelper.RequestApiResult<QTechTestAPICreateSessionTokenModel>(param, body);
            return result.ResultObject;
        }


        public static QTechTestAPIAuthTokenModel GenerateAccessToken(Action<ApiRequestLogTemporaryInfoModel> logAction)
        {
            string username = "api_peg";
            string password = [REDACTED]";

            var param = new RequestParameters
            {
                BaseUrl = "https://example.invalid/",
                ApiUrl = $"v1/auth/token?grant_type=password&response_type=token&username={username}&password=[REDACTED]",
                Method = HttpMethodType.HttpGet,
                UserAgent = "PEG",
                LogAction = logAction
            };

            var result = APIRequestHelper.RequestApiResult<QTechTestAPIAuthTokenModel>(param);
            return result.ResultObject;
        }



        public static QTechUrlResultModel GetQTechGameLaunchUrl(Action<ApiRequestLogTemporaryInfoModel> logAction , string myToken)
        {
            //example.invalid   usd.test
            //example.invalid   pegop-usd.test

            string gameId = "PEG-crazypenguin";

            var param = new RequestParameters
            {
                BaseUrl = "https://example.invalid/",
                ApiUrl = $"v1/games/{gameId}/launch-url",
                Method = HttpMethodType.HttpPost,
                UserAgent = "PEG",
                LogAction = logAction
            };

            string tokenValue = "ed4d9fcc-4108-3b69-9f9a-46863d74ec53";

            if(!string.IsNullOrWhiteSpace(myToken))
            {
                tokenValue = myToken;
            }

            param.AddHeaderData("Authorization", $"Bearer {tokenValue}");

            var body = new
            {
                playerId = "usd.test",
                currency = "USD",
                country = "US",
                lang = "zh_CN",
                mode = "real",
                device = "desktop",
                returnUrl = "https://example.invalid/",
            };

            var result = APIRequestHelper.RequestApiResult<QTechUrlResultModel>(param , body);
            return result.ResultObject;
        }
    }

}