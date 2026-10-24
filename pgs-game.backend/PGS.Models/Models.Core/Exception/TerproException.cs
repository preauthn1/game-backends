using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models.Models
{
    public class TerproException: System.Exception
    {

        public ApiErrorCode ErrroCode { get; set; }

        public ErrorDetailModel ErrorDetail { get; set; }


        public TerproException(ApiErrorCode errorCode, ErrorDetailModel errorDetail = null)
        {
            this.ErrroCode = errorCode;
            this.ErrorDetail = errorDetail;

            SetExtraDebugInfo();
        }

        public TerproException(ApiErrorCode errorCode, string replaceMsg)
        {
            this.ErrroCode = errorCode;
            this.ErrorDetail = new ErrorDetailModel
            {
                ReplaceMessage = replaceMsg
            };

            SetExtraDebugInfo();
        }

        public TerproException(ApiErrorCode errorCode, bool isAlwaysNotify)
        {
            this.ErrroCode = errorCode;
            this.ErrorDetail = new ErrorDetailModel
            {
                IsAlwaysNotify = isAlwaysNotify
            };

            SetExtraDebugInfo();
        }

        private void SetExtraDebugInfo()
        {
            if (ErrroCode == ApiErrorCode.ThirdPartyApiFail_ResponseError ||
                ErrroCode == ApiErrorCode.ThirdPartyApiFail_ParsingFail)
            {
                this.ErrorDetail = this.ErrorDetail ?? new ErrorDetailModel();

                if(string.IsNullOrWhiteSpace(this.ErrorDetail.ColumnName))
                {
                    string timeText = DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
                    this.ErrorDetail.ColumnName = $"{timeText} (GMT+8)";
                }
            }
        }


        public override string Message
        {
            get
            {
                return $"TerproException_{(int)ErrroCode}_{ErrroCode.ToString()}_{ErrorDetail?.ColumnName}_{ErrorDetail?.LogExtraInfo}_{ErrorDetail?.ReplaceMessage}";
            }
        }


        public override string ToString()
        {
            return Message;
        }

    }


    public class ErrorDetailModel
    {

        public ErrorDetailModel()
        {
            this.RiskRate = 1.0M;
        }


        public ErrorDetailModel(string columnName)
        {
            this.ColumnName = columnName;
            this.RiskRate = 1.0M;
        }


        public ErrorDetailModel(string columnName , bool isAlwatsNotify)
        {
            this.ColumnName = columnName;
            this.IsAlwaysNotify = IsAlwaysNotify;
        }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ColumnName { get; set; }


        /// <summary>
        /// log額外紀錄訊息 (Production環境會隱藏此資訊)
        /// ps. im notify也會包含此處的訊息
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LogExtraInfo { get; set; }


        /// <summary>
        /// response會額外返回訊息 (#1)
        /// 一般商戶也會看到 因此請勿回傳一些重要資訊
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DebugExtraInfo { get; set; }


        /// <summary>
        /// response會額外返回訊息 (#2)
        /// 一般商戶也會看到 因此請勿回傳一些重要資訊
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DebugExtraInfo2 { get; set; }



        /// <summary>
        /// 是否總是發 TG/slack 通知 (如果包try catch會無效)
        /// </summary>
        [JsonIgnore]
        public bool IsAlwaysNotify { get; set; }


        /// <summary>
        /// 風險加權倍數 (用於判定異常用戶)
        /// </summary>
        [JsonIgnore]
        public decimal RiskRate { get; set; }


        /// <summary>
        /// 是否總是發slack通知 (如果包try catch會無效)
        /// </summary>
        [JsonIgnore]
        public bool IsAlwaysNotifySlackOnly { get; set; }


        /// <summary>
        /// 如果要強制覆蓋訊息
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ReplaceMessage { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object ExtraData { get; set; }

    }


}
