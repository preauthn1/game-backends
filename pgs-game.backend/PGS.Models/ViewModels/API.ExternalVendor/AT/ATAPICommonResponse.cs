using System;

namespace target.Models.ViewModels
{

    public class ATAPICommonResponseModel<T>
    {
        public ATAPICommonResponseModel()
        {
            this.Code = 0;
            this.Msg = "Success";
        }

        /// <summary>
        /// 0代表成功
        /// </summary>
        public int Code { get; set; }

        public string Msg { get; set; }

        public T Data { get; set; }
    }



    public class ATAPIExceptionCommonResponse
    {
        /// </summary>
        public int Code { get; set; }

        public string Msg { get; set; }
    }




    public class ATGenerateSignKeyResult
    {
        public string SignKey { get; set; }
        public string QueryStringText { get; set; }
        public string QueryStringWithKey { get; set; }
        public string BeforeMD5 { get; set; }
        public string BeforeMD5_KeyG { get; set; }
    }


}
