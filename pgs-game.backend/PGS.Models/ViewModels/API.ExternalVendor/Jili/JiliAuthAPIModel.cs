using System;

namespace target.Models.ViewModels
{
    public class JiliAuthAPIRequestModel 
    {
        public string Token { get; set; }
    }


    public class JiliAuthAPIResponseModel : JiliAPICommonResponse
    {
        public string Username { get; set; }

        public string Currency { get; set; }

        public decimal Balance { get; set; }

        public string Token { get; set; }
    }

}
