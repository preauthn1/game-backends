using System;

namespace target.Models.ViewModels
{

    public class ATAuthAPIRequestModel 
    {
        public string Token { get; set; }

        public string Username { get; set; }

        public string ReqId { get; set; }
    }


    public class ATAuthAPIResponseDataModel
    {
        public string Username { get; set; }

        public string Token { get; set; }

        public string Headurl { get; set; }
    }

}
