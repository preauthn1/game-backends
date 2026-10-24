using System;

namespace target.Models.ViewModels
{



    public class MarioBalanceAPIRequestModel : IMarioSeamlessRequest
    {
        public string api_id { get; set; }

        public long timestamp { get; set; }

        public string sign { get; set; }

        public int user_id { get; set; }

    }


    public class MarioBalanceAPIResponseModel : MarioAPICommonResponse
    {

        public decimal Balance { get; set; }

    }

}
