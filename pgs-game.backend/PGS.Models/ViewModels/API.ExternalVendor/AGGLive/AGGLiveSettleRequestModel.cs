using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class AGGLiveSettleRequestModel
    {
        public AGGLiveSettleDetailRequestModel data { get; set; }

        public string hash { get; set; }
    }



    public class AGGLiveSettleDetailRequestModel
    {
        public string usertoken { get; set; }
        public string sessiontoken { get; set; }
        public string gameid { get; set; }
        public string roundid { get; set; }
        public string bettime { get; set; }
        public string betnumber { get; set; }
        public decimal bet { get; set; }
        public decimal winlose { get; set; }
        public decimal fee { get; set; }
        public bool? jackpot { get; set; }
        public object detail { get; set; }

    }



    public class AGGLiveRoundTotal
    {
        public decimal? PlayerTotalBet { get; set; }

        public decimal? PlayerTotalWin { get; set; }

        public decimal? PlayerTotalTax { get; set; }

        public decimal? PlayerTotalFlow { get; set; }

        public List<AGGLiveSettlementDetailBetListModel> betList { get; set; }

        public List<string> transactionList { get; set; }

    }









    public class AGGLiveSettleResponseModel : IAGGLiveResponse
    {

        public int code { get; set; }

        public string message { get; set; }

        public AGGLiveSettleResponseDetailModel result { get; set; }

    }


    public class AGGLiveSettleResponseDetailModel
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
