using System.Collections.Generic;

namespace target.Models.ViewModels
{

    public class AGGLiveRefundRequestModel
    {
        public AGGLiveRefundDetailRequestModel data { get; set; }

        public string hash { get; set; }
    }


    public class AGGLiveRefundDetailRequestModel
    {
        public string merchantid { get; set; }
        public string reason { get; set; }
        public List<AGGLiveRefundDetailItemRequestModel> content { get; set; }     
    }


    public class AGGLiveRefundDetailItemRequestModel
    {
        public string roundid { get; set; }
        public string betnumber { get; set; }
        public decimal totalbet { get; set; }
        public decimal totalwin { get; set; }
        public decimal winlose { get; set; }
        public bool jackpot { get; set; }
        public decimal bonus { get; set; }
        public decimal taxamount { get; set; }
        public decimal servicefee { get; set; }
    }



    public class AGGLiveRefundResponseModel : IAGGLiveResponse
    {
        public int code { get; set; }

        public string message { get; set; }

    }



}
