using System;

namespace target.Models.ViewModels
{
    public class GalaxyAPICommonResponse
    {
        public GalaxyAPICommonResponse()
        {
            this.Status = true;
            this.Message = "Success";
        }

        public GalaxyAPICommonResponse(object data)
        {
            this.Status = true;
            this.Message = "Success";
            this.Data = data;
        }

        /// <summary>
        /// 0代表成功
        /// </summary>
        public bool Status { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }

        public object Data2 { get; set; }

        public object Data3 { get; set; }

        public object Data4 { get; set; }
    }




    public class GalaxyAPIExceptionCommonResponse : JiliAPICommonResponse
    {

    }



    public class GalaxyAPIGenerateSignKeyResult
    {
        public string SignKey { get; set; }
        public string QueryStringText { get; set; }
        public string QueryStringWithKey { get; set; }
        public string BeforeMD5 { get; set; }
        public string BeforeMD5_KeyG { get; set; }
    }


}
