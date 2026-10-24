using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.ComponentModel;
using target.Models.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using TG_Hook_API.Models;
using DocumentFormat.OpenXml.Wordprocessing;

namespace target.Utility.Utility.ThirdParty
{
    /// <summary>
    /// 基底的 BaseSlackUtility 基本上不會再改到
    /// 有需要根據公司邏輯 特別客製的，請修正 MyXXXXXUtility
    /// </summary>
    public class BaseTeamsUtility
    {

        private static string _apiBaseUrl = "https://example.invalid";

        private static string _apiBasePrefixRouterUrl = "apis/v3/";


        public BaseTeamsUtility()
        {
            //請參考 Microsoft graph API 相關關鍵字
        }


        public TeamsTokenGenerateApiResponse RequestJwtToken(string tenant_id , string appID, string appSecret)
        {
            var para = new RequestParameters
            {
                BaseUrl = "https://example.invalid",
                ApiUrl = $"{tenant_id}/oauth2/v2.0/token",
                Method = HttpMethodType.HttpPost,
                ContentType = "application/x-www-form-urlencoded",
                TimeoutMillisecond = 1000 * 10,
            };

            string postFrom = $"grant_type=client_credentials&client_id={appID}&client_secret=[REDACTED]";

            var apiResult = APIRequestHelper.RequestApiResult<TeamsTokenGenerateApiResponse>(para, postFrom);

            return apiResult.ResultObject ?? new TeamsTokenGenerateApiResponse();
        }

                
        public SkypeSendMessageResponse GetChats(string jwtToken)
        {            
            var para = new RequestParameters
            {
                BaseUrl = "https://example.invalid",
                ApiUrl = $"/v1.0/me/chats",
                Method = HttpMethodType.HttpGet,
                TimeoutMillisecond = 1000 * 10,
            };

            para.AddHeaderData("Authorization", "Bearer " + jwtToken);

            var listOfImageAttachment = new List<object>();

            var apiResult = APIRequestHelper.RequestApiResult<SkypeSendMessageResponse>(para);   
            
            var result = apiResult.ResultObject ?? new SkypeSendMessageResponse();
            result.RawResponseText = apiResult.ResponseText;
            return result;
        }


        public SkypeSendMessageResponse TestSendMessage(string jwtToken)
        {
            var para = new RequestParameters
            {

                BaseUrl = "https://example.invalid",
                ApiUrl = $"/v1.0/chats/FEAlgQRszIheqtXCQE/messages",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 1000 * 10,
            };

            para.AddHeaderData("Authorization", "Bearer " + jwtToken);

            var listOfImageAttachment = new List<object>();

            var myBody = new
            {
                body = new
                {
                    content = "Hello"
                }
            };

            var apiResult = APIRequestHelper.RequestApiResult<SkypeSendMessageResponse>(para , myBody);

            var result = apiResult.ResultObject ?? new SkypeSendMessageResponse();
            result.RawResponseText = apiResult.ResponseText;
            return result;
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
            };

            para.AddHeaderData("Authorization", "Bearer " + jwtToken);

            var listOfImageAttachment = new List<object>();

            var postBody = new
            {
                type = "message",
                text = param.Message,
                attachments = extra.ListOfAttachment
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
