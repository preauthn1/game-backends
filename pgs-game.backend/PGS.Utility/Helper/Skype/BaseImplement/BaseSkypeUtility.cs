using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.ComponentModel;
using target.Models.Models;

namespace target.Utility.Utility.ThirdParty
{
    /// <summary>
    /// 基底的 BaseSlackUtility 基本上不會再改到
    /// 有需要根據公司邏輯 特別客製的，請修正 MyXXXXXUtility
    /// </summary>
    internal class BaseSkypeUtility
    {

        private static string _apiBaseUrl = "https://example.invalid";

        private static string _apiBasePrefixRouterUrl = "apis/v3/";


        public BaseSkypeUtility()
        {
            //請參考 botframework REST API 相關關鍵字
        }


        public SkypeLoginApiResponse RequestJwtToken(string appID, string appSecret)
        {
            var para = new RequestParameters
            {
                BaseUrl = "https://example.invalid/",
                ApiUrl = $"example.invalid/oauth2/v2.0/token",
                Method = HttpMethodType.HttpPost,
                ContentType = "application/x-www-form-urlencoded",
                TimeoutMillisecond = 1000 * 10,
            };

            string postFrom = $"grant_type=client_credentials&client_id={appID}&client_secret=[REDACTED]";

            var apiResult = APIRequestHelper.RequestApiResult<SkypeLoginApiResponse>(para, postFrom);

            return apiResult.ResultObject ?? new SkypeLoginApiResponse();
        }


        public SkypeSendMessageResponse SendMessage(SkypeMessageParameter param , string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(param.Message))
                return new SkypeSendMessageResponse();

            string msg = param.Message;

            var extra = param.ExtraSetting ?? new SkypeExtraSetting();

            var para = new RequestParameters
            {
                BaseUrl = _apiBaseUrl,
                ApiUrl = $"{_apiBasePrefixRouterUrl}/conversations/{param.TargetConversationID}/activities",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 1000 * 10,
                //ContentType = "text/html",
            };

            para.AddHeaderData("Authorization", "Bearer " + jwtToken);

            var listOfImageAttachment = new List<object>();

            var postBody = new
            {
                type = "message",
                text = param.Message,
                //attachments = extra.ListOfAttachment,
                //textFormat = "markdown"
            };

            var apiResult = APIRequestHelper.RequestApiResult<SkypeSendMessageResponse>(para , postBody);   
            
            //callback紀錄
            if (param.ExtraSetting?.LogActionCallback != null)
            {
                param.ExtraSetting.LogActionCallback(new IMMessageSendCallbackParameter
                {
                    ChatID = param.TargetConversationID,
                    MessageID = apiResult.ResultObject?.id,
                    ResponseText = apiResult.ResponseText,
                });
            }

            var result = apiResult.ResultObject ?? new SkypeSendMessageResponse();
            result.RawResponseText = apiResult.ResponseText;
            return result;
        }

        public SkypeSendMessageResponse UploadAttachment(SkypeMessageParameter param, string jwtToken)
        {
            //對接失敗 一直拿到404 黑人問號 感覺是基於安全性問題已移除功能
            //對接失敗 一直拿到404 黑人問號 感覺是基於安全性問題已移除功能
            //對接失敗 一直拿到404 黑人問號 感覺是基於安全性問題已移除功能
            //對接失敗 一直拿到404 黑人問號 感覺是基於安全性問題已移除功能
            //對接失敗 一直拿到404 黑人問號 感覺是基於安全性問題已移除功能
            //對接失敗 一直拿到404 黑人問號 感覺是基於安全性問題已移除功能
            //對接失敗 一直拿到404 黑人問號 感覺是基於安全性問題已移除功能
            //對接失敗 一直拿到404 黑人問號 感覺是基於安全性問題已移除功能
            //對接失敗 一直拿到404 黑人問號 感覺是基於安全性問題已移除功能

            //google到的一些說明 有些是建議自己上傳到其他地方 並提供連結

            throw new TerproException(ApiErrorCode.NotImplement);
            throw new TerproException(ApiErrorCode.NotImplement);
            throw new TerproException(ApiErrorCode.NotImplement);
            throw new TerproException(ApiErrorCode.NotImplement);
            throw new TerproException(ApiErrorCode.NotImplement);

            string msg = param.Message;

            var extra = param.ExtraSetting ?? new SkypeExtraSetting();

            var para = new RequestParameters
            {
                BaseUrl = _apiBaseUrl,
                ApiUrl = $"apis/v3/conversations/{param.TargetConversationID}/attachments/UploadAttachment",
                Method = HttpMethodType.HttpPut,
                TimeoutMillisecond = 1000 * 10,
            };

            para.AddHeaderData("Authorization", "Bearer " + jwtToken);

            var postBody = new
            {
                name = extra.AttachmentDataV2.name,
                originalBase64 = extra.AttachmentDataV2.originalBase64,
                type = extra.AttachmentDataV2.contentType
            };

            var apiResult = APIRequestHelper.RequestApiResult<SkypeSendMessageResponse>(para, postBody);

            //callback紀錄
            if (param.ExtraSetting?.LogActionCallback != null)
            {
                param.ExtraSetting.LogActionCallback(new IMMessageSendCallbackParameter
                {
                    ChatID = param.TargetConversationID,
                    MessageID = apiResult.ResultObject?.id,
                    ResponseText = apiResult.ResponseText,
                });
            }

            return apiResult.ResultObject ?? new SkypeSendMessageResponse();
        }


        public void DeleteMessage(string conversationID, string messageActivityID , string jwtToken)
        {
            var para = new RequestParameters
            {
                BaseUrl = _apiBaseUrl,
                ApiUrl = $"{_apiBasePrefixRouterUrl}/conversations/{conversationID}/activities/{messageActivityID}",
                Method = HttpMethodType.HttpPut,
                TimeoutMillisecond = 1000 * 10,
            };

            para.AddHeaderData("Authorization", "Bearer " + jwtToken);

            var time = DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
            var body = new
            {
                type = "message",
                text = $"Message deleted* ({time} GMT+8)", //官方提供的deleted API 不曉得為何沒作用
            };

            var response = APIRequestHelper.RequestApiResult<string>(para, body);
            return;
        }





    }
}
