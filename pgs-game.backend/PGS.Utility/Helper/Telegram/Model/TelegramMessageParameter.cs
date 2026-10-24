using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Utility.Utility.ThirdParty
{
    public class TelegramMessageParameter
    {
        public TelegramMessageParameter()
        {

        }

        public string Message { get; set; }


        public TelegramChannelEnum TelegramChannelEnum { get; set; }


        public TelegramExtraSetting TelegramExtraSetting { get; set; }


    }


    public class TelegramExtraSetting
    {
        /// <summary>
        /// 訊息不要包含 prefix (e.g. WebSiteName)
        /// </summary>
        public bool IsCleanPrefixText { get; set; }

        public string ImageUrl { get; set; }

        /// <summary>
        /// MSTest預設會發通知 (如果是Sync模式)
        /// </summary>
        public bool IsSkipWhenMSTest { get; set; }

        /// <summary>
        /// 如果發送失敗 會額外通知
        /// </summary>
        public bool IsNotifyHttpError { get; set; }

        public byte[] PostFileBytes { get; set; }

        public string PostFileName { get; set; }

        public string TargetChatID { get; set; }

        public string DeleteMessageID { get; set; }

        public string Reply_to_MessageID { get; set; }


        public string MessageThreadID { get; set; }

        /// <summary>
        /// 支援 HTML 或 Markdown
        /// </summary>
        public string ParseMode { get; set; }

        /// <summary>
        /// 若成功發送request 會額外紀錄
        /// TG發送 目前會記錄在EazyTemp 資料表 (type=5577
        /// </summary>
        public Action<IMMessageSendCallbackParameter> LogActionCallback { get; set; }


        public List<List<TGApiButtonList>> MultiRowButtonList { get; set; }
    }




    public class IMMessageSendCallbackParameter
    {
        /// <summary>
        /// 通常是指頻道ID
        /// </summary>
        public string ChatID { get; set; }

        public string MessageID { get; set; }

        public string ResponseText { get; set; }
    }




    public class TGApiButtonList
    {

        public TGApiButtonList()
        {

        }

        public TGApiButtonList(string displayText, string callbackData, string myUrl = null)
        {
            this.text = displayText;

            if (!string.IsNullOrWhiteSpace(callbackData))
            {
                this.callback_data = callbackData;
            }

            if (!string.IsNullOrWhiteSpace(myUrl))
            {
                this.url = myUrl;
            }
        }



        public string text { get; set; }

        /// <summary>
        /// 請對應 CallbackAction
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string callback_data { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string url { get; set; }



        /// <summary>
        ///  value範例: new { url = homePageInfo.Data.event_link };
        ///  只能在機器人主頻道使用 (一般多人頻道不可使用)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object web_app { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? request_contact { get; set; }

    }


    public class TGAPIResponseResultModel
    {
        public string message_id { get; set; }

        public TGAPIResponseDocumentInfo document { get; set; }
    }


    public class TGAPIResponseDocumentInfo
    {
        public string file_id { get; set; }
    }


    public class TGAPIResponseRootModel
    {
        public bool ok { get; set; }
        public TGAPIResponseResultModel result { get; set; }
    }

    public class TGAPIErrorResponseModel
    {
        public bool ok { get; set; }
        public string error_code { get; set; }
        public string description { get; set; }
    }


}
