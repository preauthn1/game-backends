using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.ComponentModel;
using example.invalid.Http;

namespace target.Utility.Utility.ThirdParty
{
    /// <summary>
    /// 基底的 BaseSlackUtility 基本上不會再改到
    /// 有需要根據公司邏輯 特別客製的，請修正 MySlackUtility
    /// </summary>
    internal class BaseTelegramUtility
    {

        private string _token;
        private string _prefixText;


        public BaseTelegramUtility(string token, string prefix = "")
        {
            _token = [REDACTED];
            _prefixText = prefix;
        }


        public void SendMessage(TelegramMessageParameter parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter.Message))
                return;

            if (parameter.Message == "EMPTY")
            {
                parameter.Message = "";
            }

            parameter.TelegramExtraSetting = parameter.TelegramExtraSetting ?? new TelegramExtraSetting();
            var param = parameter.TelegramExtraSetting;

            if (param.IsCleanPrefixText)
            {
                _prefixText = string.Empty;
            }

            string chatID = GetAttributeFromEnum<DescriptionAttribute>(parameter.TelegramChannelEnum)?.Description;

            if (!string.IsNullOrWhiteSpace(param.TargetChatID))
            {
                chatID = param.TargetChatID;
            }

            bool isSendPhoto = !string.IsNullOrWhiteSpace(param.ImageUrl);
            bool isSendFile = !string.IsNullOrWhiteSpace(param.PostFileName);

            string msg = parameter.Message;

            string parseModeText = "Markdown";

            if (!string.IsNullOrWhiteSpace(param.ParseMode))
            {
                parseModeText = param.ParseMode;
            }

            parameter.Message = _prefixText + parameter.Message.Replace("#", "＃");

            //預設帶markdown有時候會把 _文字_ 變斜體。有點困擾
            if (parseModeText == "Markdown")
            {
                parameter.Message = parameter.Message.Replace("_", "\\_");
            }

            var para = new RequestParameters
            {
                BaseUrl = "https://example.invalid",
                ApiUrl = $"bot{_token}/sendMessage",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 10000,
            };

            ApiRequestResultModel<TGAPIResponseRootModel> tgApiResult = null;

            if (isSendPhoto)
            {
                string apiUrl = "sendPhoto";
                para.ApiUrl = $"bot{_token}/{apiUrl}";

                var body = new
                {
                    photo = param.ImageUrl,
                    caption = parameter.Message,
                    parse_mode = parseModeText,
                    chat_id = chatID,
                    reply_markup = new
                    {
                        inline_keyboard = param.MultiRowButtonList,
                    }
                };

                tgApiResult = APIRequestHelper.RequestApiResult<TGAPIResponseRootModel>(para, body);
            }
            else if (isSendFile)
            {
                tgApiResult = UploadFileAndSendMessage(parameter, para, chatID);
            }
            else
            {
                Dictionary<string, string> keyValues = new Dictionary<string, string>();

                keyValues.Add("chat_id", chatID);
                keyValues.Add("text", parameter.Message);

                if (!string.IsNullOrWhiteSpace(parseModeText))
                {
                    keyValues.Add("parse_mode", parseModeText);
                }

                if (!string.IsNullOrWhiteSpace(param.MessageThreadID))
                {
                    keyValues.Add("message_thread_id", param.MessageThreadID);
                }

                if (!string.IsNullOrWhiteSpace(param.Reply_to_MessageID))
                {
                    keyValues.Add("reply_to_message_id", param.Reply_to_MessageID);
                }

                tgApiResult = APIRequestHelper.RequestApiResult<TGAPIResponseRootModel>(para, keyValues);
            }

            if(param.IsNotifyHttpError)
            {
                //常見錯誤: wrong type of the web page content (send photo 圖片異常 原因不明。嘗試更換另一張圖就正常了
                if (tgApiResult.ResultObject == null || !tgApiResult.ResultObject.ok)
                {
                    string msgOfError = $"TG發送失敗: {chatID} , {tgApiResult.ResponseText}";

                    MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                    {
                        Channel = SlackChannelEnum.n_error_pgs_common,
                        Message = msgOfError
                    });
                }
            }

            //callback紀錄
            if (parameter.TelegramExtraSetting.LogActionCallback != null)
            {
                parameter.TelegramExtraSetting.LogActionCallback(new IMMessageSendCallbackParameter
                {
                    ChatID = chatID,
                    MessageID = tgApiResult.ResultObject?.result?.message_id,
                    ResponseText = tgApiResult.ResponseText,
                });
            }

            return;
        }


        public ApiRequestResultModel<TGAPIResponseRootModel> UploadFileAndGetFileID(TelegramMessageParameter parameter)
        {
            parameter.TelegramExtraSetting = parameter.TelegramExtraSetting ?? new TelegramExtraSetting();

            var para = new RequestParameters
            {
                BaseUrl = "https://example.invalid",
                ApiUrl = $"bot{_token}/sendMessage",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 10000,
            };

            var response = UploadFileAndSendMessage(parameter, para, parameter.TelegramExtraSetting.TargetChatID);
            return response;
        }


        public ApiRequestResultModel<TGAPIResponseRootModel> SendDocumentByFileID(string chatID , string fileID , string captionText)
        {
            var para = new RequestParameters
            {
                BaseUrl = "https://example.invalid",
                ApiUrl = $"bot{_token}/sendDocument",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 10000,
            };

            var body = new
            {
                chat_id = chatID,
                document = fileID,
                caption = captionText
            };

            var response = APIRequestHelper.RequestApiResult<TGAPIResponseRootModel>(para, body);
            return response;
        }


        private ApiRequestResultModel<TGAPIResponseRootModel> UploadFileAndSendMessage(TelegramMessageParameter parameter, RequestParameters para, string chatID)
        {
            string url = string.Format("{0}/bot{1}/sendDocument", para.BaseUrl, _token);

            string responseText = "";

            // 创建一个HttpClient实例
            using (HttpClient client = new HttpClient())
            {
                // 创建一个MultipartFormDataContent对象
                using (MultipartFormDataContent content = new MultipartFormDataContent())
                {
                    content.Add(new StringContent(chatID, Encoding.UTF8, "application/json"), "chat_id");
                    content.Add(new StringContent(parameter.Message, Encoding.UTF8, "application/json"), "caption");
                    content.Add(new ByteArrayContent(parameter.TelegramExtraSetting.PostFileBytes), "document", parameter.TelegramExtraSetting.PostFileName);

                    // 发送请求
                    HttpResponseMessage response = client.PostAsync(url, content).Result;

                    // 检查响应是否成功
                    if (response.IsSuccessStatusCode)
                    {
                        responseText = response.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        responseText = response.Content.ReadAsStringAsync().Result;
                    }
                }
            }

            return new ApiRequestResultModel<TGAPIResponseRootModel>
            {
                ResponseText = responseText,
            };
        }



        public ApiRequestBriefResponseModel DeleteMessage(string chatID, string messageID)
        {
            var para = new RequestParameters
            {
                BaseUrl = "https://example.invalid/",
                ApiUrl = $"bot{_token}/deleteMessage",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 10000,
            };

            var body = new
            {
                chat_id = chatID,
                message_id = messageID,
            };

            //*超過一定時間就無法刪除了 (e.g. 7天或更短)
            var response = APIRequestHelper.RequestApiResult<string>(para, body);

            return ConvertToBriefModel(response);
        }



        public static T GetAttributeFromEnum<T>(System.Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        private static ApiRequestBriefResponseModel ConvertToBriefModel<T>(ApiRequestResultModel<T> result)
        {
            if (result == null)
            {
                return new ApiRequestBriefResponseModel();
            }

            return new ApiRequestBriefResponseModel
            {
                ResponseStatusCode = result.ResponseStatusCode,
                IsTimeoutException = result.IsTimeoutException,
                Exception = result.Exception,
                ResponseText = result.ResponseText
            };
        }


    }
}
