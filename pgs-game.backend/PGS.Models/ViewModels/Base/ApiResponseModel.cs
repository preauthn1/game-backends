using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models.ViewModels.Base
{
    public class ApiResponseModel
    {

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }
    }


    public class ApiResponseGenericModel<T>
    {

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }
    }


    public class ApiResponseErrorModel
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }
    }

    public class GameVendorExceptionParsingParameter
    {

        public ApiResponseErrorModel ApiResponseError { get; set; }

        /// <summary>
        /// 部分遊戲商會需要帶過去額外參數
        /// </summary>
        public object ExtraData { get; set; }
    }


}
