namespace target.Models.ViewModels
{


    public class AGGLiveCancelRequestModel
    {
        public AGGLiveCancelDetailRequestModel data { get; set; }

        public string hash { get; set; }
    }


    public class AGGLiveCancelDetailRequestModel
    {
        public string usertoken { get; set; }
        public string sessiontoken { get; set; }
        public string gameid { get; set; }
        public string roundid { get; set; }
        public string betnumber { get; set; }

    }


    public class AGGLiveCancelResponseModel : IAGGLiveResponse
    {
        public int code { get; set; }

        public string message { get; set; }

        public AGGLiveCancelResponseDetailModel result { get; set; }

    }


    public class AGGLiveCancelResponseDetailModel
    {

        /// <summary>
        /// 更新前
        /// </summary>
        public decimal balance { get; set; }


        /// <summary>
        /// 更新後金幣
        /// </summary>
        public decimal renew_balance { get; set; }

    }





}
