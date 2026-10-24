using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace game.Entity
{
    public class TerproException: System.Exception
    {

        public ApiErrorCode ErrroCode { get; set; }

        public ErrorDetailModel ErrorDetail { get; set; }


        public override string Message
        {
            get
            {
                return string.Format("{0} : {1} " , "TerproException" , ErrroCode.ToString());
            }
        }
        

        public TerproException(ApiErrorCode errorCode, ErrorDetailModel errorDetail = null)
        {
            this.ErrroCode = errorCode;
            this.ErrorDetail = errorDetail;
        }


        public TerproException(ApiErrorCode errorCode, string replaceMsg)
        {
            this.ErrroCode = errorCode;
            this.ErrorDetail = new ErrorDetailModel
            {
                ReplaceMessage = replaceMsg
            };
        }

    }


    public class ErrorDetailModel
    {

        public ErrorDetailModel()
        {

        }


        public ErrorDetailModel(string columnName)
        {
            this.ColumnName = columnName;
        }



        public string ColumnName { get; set; }

        /// <summary>
        /// 如果要強制覆蓋訊息
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ReplaceMessage { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object ExtraData { get; set; }

    }


}
