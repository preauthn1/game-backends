using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using example.invalid;
using Microsoft.Extensions.Configuration;
using target.Utility.Extensions;

namespace target.Utility.Utility.ThirdParty
{
    public class MyTelegramUtility
    {

        /// <summary>
        /// 取得config設定的slack token
        /// </summary>
        /// <returns></returns>
        public static string GetConfigTokenSetting()
        {
            string configSetting = GetConfigValueByKey("TelegramBotToken");

            if (!string.IsNullOrWhiteSpace(configSetting))
                return configSetting;

            //預設機器人 @pgs_it_bot
            //查看TG cahtID 可以透過  https://example.invalid/bot{pgs_token}/getUpdates
            //查看TG cahtID 可以透過  https://example.invalid/bot1490132862:AAEV0BmAVOfWrXnnwNrfAvwT-nTqV_wbQ3Q/getUpdates
            return "[TG_BOT_TOKEN_REDACTED]";
        }


        public static void SendMessageToChannelAsync(TelegramMessageParameter parameter)
        {
            //非同步
            Task.Factory.StartNew(() =>
                {
                    SendMessage(parameter);
                });
        }

       
        public static void SendMessage(TelegramMessageParameter parameter)
        {
            if (SettingHelper.IsMSTest)
            {
                parameter.TelegramExtraSetting = parameter.TelegramExtraSetting ?? new TelegramExtraSetting();

                if (parameter.TelegramExtraSetting.IsSkipWhenMSTest)
                {
                    return;
                }
            }
                
            string token = [REDACTED];            
            string prefixText = GetPrefixText();

            var newService = new BaseTelegramUtility(token, prefixText);
            newService.SendMessage(parameter);
        }


        public static string UploadFileAndFileID(TelegramMessageParameter parameter)
        {
            string token = [REDACTED];
            string prefixText = GetPrefixText();

            var newService = new BaseTelegramUtility(token, prefixText);

            var res = newService.UploadFileAndGetFileID(parameter);
            var resAPI = res.ResponseText.JsonConvertToModel<TGAPIResponseRootModel>();
            return resAPI?.result?.document?.file_id;
        }


        public static void SendDocumentByFileID(string chatID, string fileID, string captionText)
        {
            string token = [REDACTED];
            string prefixText = GetPrefixText();

            var newService = new BaseTelegramUtility(token, prefixText);
            newService.SendDocumentByFileID(chatID , fileID , captionText);
        }



        public static ApiRequestBriefResponseModel DeleteMessage(string chatID, string messageID)
        {
            string token = [REDACTED];
            string prefixText = GetPrefixText();

            var newService = new BaseTelegramUtility(token, prefixText);
            var response = newService.DeleteMessage(chatID , messageID);
            return response;
        }



        private static string GetPrefixText()
        {
            string siteName = GetConfigValueByKey("WebSiteName");

            //可根據專案客製需求，不同環境要帶不同的前綴文字 提供識別
            if (IsDev())
                return $"【Debug環境】{siteName}" + Environment.NewLine;

            return $" `{siteName}` " + Environment.NewLine;
        }


        private static string GetConfigValueByKey(string keyName)
        {
            return BaseSettingHelper.GetSettingValue(keyName);
        }


        private static bool IsDev()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }



    }
}
