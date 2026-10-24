using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using example.invalid;
using Microsoft.Extensions.Configuration;
using target.Models.Models;
using Lck.Utility.Extensions.Helper;

namespace target.Utility.Utility.ThirdParty
{
    public class MyTeamstility
    {

        /// <summary>
        /// 取得config設定
        /// </summary>
        public static (string Tenant_id, string AppID , string AppSerect) GetKeySetting()
        {
            //這些資訊需要去azure複製
            string tenant_id = GetConfigValueByKey("Teams_Tenant_id");
            string appID = GetConfigValueByKey("Teams_AppID");
            string appSecret = [REDACTED]"Teams_AppSecret");

            if (string.IsNullOrWhiteSpace(tenant_id))
            {
                tenant_id = "0d0ba42d-1324-4412-86b1-895cbcc4e946";
            }

            if (string.IsNullOrWhiteSpace(appID))
            {
                appID = "c6bbfc51-2e1f-4f0c-b961-592638d6c86d";
            }

            if (string.IsNullOrWhiteSpace(appSecret))
            {
                appSecret = [REDACTED]";
            }

            return (tenant_id, appID, appSecret);
        }


        public static void SendMessageToChannelAsync(SkypeMessageParameter parameter)
        {
            //單元測試不發通知
            if (SettingHelper.IsMSTest)
                return;

            //非同步
            Task.Factory.StartNew(() =>
                {
                    SendMessage(parameter);
                });
        }

       
        public static SkypeSendMessageResponse SendMessage(SkypeMessageParameter parameter)
        {
            string jwtToken = [REDACTED];

            var newService = new BaseTeamsUtility();
            var result = newService.SendMessage(parameter , jwtToken);
            return result;
        }


        public static void DeleteMessage(string conversationID, string messageActivityID)
        {
            string jtwToken = [REDACTED];

            var newService = new BaseTeamsUtility();
            newService.DeleteMessage(conversationID, messageActivityID , jtwToken);
        }


        public static string GetLastJwtToken()
        {
            var configSetting = GetKeySetting();

            string cacheKey = CacheKey.Skype_LastJwtToken(configSetting.AppID);

            string debugMsg = "";

            var cacheValue = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var newService = new BaseTeamsUtility();
                   var result = newService.RequestJwtToken(configSetting.Tenant_id, configSetting.AppID, configSetting.AppSerect);
                                   
                   if (string.IsNullOrWhiteSpace(result.access_token))
                   {
                       debugMsg = $"{result.error} {result.error_description}";
                       return null;
                   }

                   return result.access_token;

               }, expiredMinute: 60 * 6);


            if (string.IsNullOrWhiteSpace(cacheValue))
            {
                MyTelegramUtility.SendMessage(new TelegramMessageParameter
                {
                    Message = $"Skype API獲取JwtToken失敗 , {debugMsg}",
                });

                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel("Skype"));
            }

            return cacheValue;
        }


        private static string GetConfigValueByKey(string keyName)
        {
            return BaseSettingHelper.GetSettingValue(keyName);
        }


    }
}
