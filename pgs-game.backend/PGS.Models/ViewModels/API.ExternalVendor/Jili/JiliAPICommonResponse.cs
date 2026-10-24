using System;

namespace target.Models.ViewModels
{
    public class JiliAPICommonResponse
    {
        public JiliAPICommonResponse()
        {
            this.ErrorCode = 0;
            this.Message = "Success";
        }

        /// <summary>
        /// 0代表成功
        /// </summary>
        public int ErrorCode { get; set; }

        public string Message { get; set; }
    }

    public class JiliAPIExceptionCommonResponse : JiliAPICommonResponse
    {

    }



    public class JiliGenerateSignKeyResult
    {
        public string SignKey { get; set; }
        public string QueryStringText { get; set; }
        public string QueryStringWithKey { get; set; }
        public string BeforeMD5 { get; set; }
        public string BeforeMD5_KeyG { get; set; }
    }


}
