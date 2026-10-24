using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class AGGFishSettleRequestModel
    {
        public AGGFishSettleDetailRequestModel data { get; set; }

        public string hash { get; set; }
    }



    public class AGGFishSettleDetailRequestModel
    {
        public string usertoken { get; set; }
        public string sessiontoken { get; set; }
        public string gameid { get; set; }
        public string roundid { get; set; }
        public string bettime { get; set; }
        public string betnumber { get; set; }
        public decimal bet { get; set; }

        /// <summary>
        /// 派彩金額
        /// </summary>
        public decimal winlose { get; set; }
        public bool isfree { get; set; }
        public decimal fee { get; set; }
        public bool jackpot { get; set; }
        public decimal bonus { get; set; }
        public AGGFishSettleDetailModel external { get; set; }

    }



    public class AGGFishSettleDetailModel
    {
        public string Currency { get; set; }
        public decimal BetAmount { get; set; }
        public decimal WinAmount { get; set; }
        public int Rounds { get; set; }
    }









    public class AGGFishSettleResponseModel : IAGGLiveResponse
    {

        public int code { get; set; }

        public string message { get; set; }

        public AGGFishSettleResponseDetailModel result { get; set; }

    }


    public class AGGFishSettleResponseDetailModel
    {

        /// <summary>
        /// 結算成功後餘額
        /// </summary>
        public decimal balance { get; set; }

        /// <summary>
        /// 更新後金幣
        /// </summary>
        public decimal renew_balance { get; set; }


    }






}
