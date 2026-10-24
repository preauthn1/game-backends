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
    public class MySkypeUtility
    {
        /// <summary>
        /// 取得config設定
        /// </summary>
        public static (string AppID , string AppSerect) GetKeySetting()
        {
            //這些資訊需要去azure複製
            string skype_AppID = GetConfigValueByKey("Skype_AppID");
            string skype_AppSecret = [REDACTED]"Skype_AppSecret");

            if(string.IsNullOrWhiteSpace(skype_AppID))
            {
                skype_AppID = "739b2f77-5284-4599-b3ef-40d85266e7dc";
            }

            if (string.IsNullOrWhiteSpace(skype_AppSecret))
            {
                skype_AppSecret = [REDACTED]";
            }

            return (skype_AppID, skype_AppSecret);
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

            var newService = new BaseSkypeUtility();
            var result = newService.SendMessage(parameter , jwtToken);
            return result;
        }


        public static SkypeSendMessageResponse UploadAttachment(SkypeMessageParameter parameter)
        {
            string jwtToken = [REDACTED];

            var newService = new BaseSkypeUtility();
            var result = newService.UploadAttachment(parameter, jwtToken);
            return result;
        }


        public static void DeleteMessage(string conversationID, string messageActivityID)
        {
            string jtwToken = [REDACTED];

            var newService = new BaseSkypeUtility();
            newService.DeleteMessage(conversationID, messageActivityID , jtwToken);
        }


        public static string GetLastJwtToken()
        {
            var configSetting = GetKeySetting();

            string cacheKey = CacheKey.Skype_LastJwtToken(configSetting.AppID);

            string debugMsg = "";

            var cacheValue = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var newService = new BaseSkypeUtility();
                   var result = newService.RequestJwtToken(configSetting.AppID, configSetting.AppSerect);
                                   
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
