using System;

namespace target.Models.ViewModels
{

    public interface IMarioSeamlessRequest
    {
        public string api_id { get; set; }

        public long timestamp { get; set; }

        public string sign { get; set; }

        public int user_id { get; set; }
    }


    public class MarioAPICommonResponse
    {
        public MarioAPICommonResponse()
        {
            this.Code = 0;
            this.Msg = "Success";
        }

        public int Code { get; set; }

        public string Msg { get; set; }

    }

    public class MarioAPIExceptionCommonResponse : MarioAPICommonResponse
    {

    }



    public class MarioGenerateSignKeyResult
    {
        public string SignKey { get; set; }
        public string QueryStringText { get; set; }
        public string QueryStringWithKey { get; set; }
        public string BeforeMD5 { get; set; }
        public string BeforeMD5_KeyG { get; set; }
    }


}
