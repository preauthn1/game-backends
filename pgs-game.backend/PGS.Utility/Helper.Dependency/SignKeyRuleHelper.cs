using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using target.Utility.Extensions;
using target.Models;
using target.Models.Models;
using target.Utility.Utility.ThirdParty;

namespace target.Utility.Helper.Dependency
{
  
    public static class SignKeyRuleHelper
    {

        public static string GeneratePublicApiLobbySignkey(long ts , string playerNameOrConcateText)
        {
            string textForEncrypt = "";

            if(!string.IsNullOrWhiteSpace(playerNameOrConcateText))
            {
                if(playerNameOrConcateText.First() == '_')
                {
                    textForEncrypt = playerNameOrConcateText;
                }
                else
                {
                    textForEncrypt = "_" + playerNameOrConcateText;
                }
            }

            return ($"{SystemConstant.PublicAPI_Lobby_Singkey_SaltKey}_{ts.ToString()}{textForEncrypt}").MD5Hash(System.Text.Encoding.UTF8);
        }


        public static void CheckIsValidPublicApiLobbySignkey(long ts, string playerNameOrConcateText ,string mySignkey)
        {
            if (mySignkey == SystemConstant.PublicAPI_SignKey_MagicPassword)
                return;
            
            string expectSignKey = GeneratePublicApiLobbySignkey(ts, playerNameOrConcateText);

            if (expectSignKey == mySignkey)
                return;

            throw new TerproException(ApiErrorCode.InvalidParameter_SignKeyError);
        }


        public static string GenerateF2E_HttpGet_SignKeyBeforeMD5(HttpRequest request)
        {
            var queryString = request.Query;

            List<string> eachColumns = new List<string>();

            foreach (var item in queryString.OrderBy(x => x.Key))
            {
                var columnName = item.Key.ToLower();
                var value = item.Value;

                eachColumns.Add($"{columnName}={value}");
            }

            var concatText = string.Join("&", eachColumns);
            return concatText;
        }


        public static string GenerateF2E_HttpGet_SignKey(HttpRequest request)
        {
            var concatText = GenerateF2E_HttpGet_SignKeyBeforeMD5(request);
            string expect_signKey = concatText.MD5Hash();
            return expect_signKey;
        }


        public static string GenerateSignErrorMessage(object originalObject, SignGenerateCheckResult hashResult, string expectHashValue = "", string secretKey = "")
        {
            string msg = $"加密物件：{originalObject.ConvertToJsonString()}" + Environment.NewLine;

            if (!string.IsNullOrWhiteSpace(secretKey))
            {
                msg += $"加密密鑰：{secretKey}" + Environment.NewLine;
            }

            msg += $"Hash加密前字串：{hashResult.BeforeHash}" + Environment.NewLine;
            msg += $"Hash加密後字串：{hashResult.AfterHash}" + Environment.NewLine;

            if (!string.IsNullOrWhiteSpace(expectHashValue) && hashResult.AfterHash != expectHashValue)
            {
                msg += $"與貴司傳入的 {expectHashValue} 不相符。" + Environment.NewLine;
            }

            MySlackUtility.SendMessageToChannelNotAsync(new SlackMessageParameter
            {
                Channel = SlackChannelEnum.n_error_pgs_common,
                Message = msg
            });

            return msg;
        }



    }
}
