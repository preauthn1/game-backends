namespace target.Models.ViewModels
{
    public class SingleWalletCancelBetRequestParameter //: SingleWalletBasePlayerInfoModel
    {        
        public string BetRecordNo { get; set; }

        public string GameCode { get; set; }

        public string RaiseTransactionNo { get; set; }

        public CancelExtraParameterModel ExtraInfo { get; set; }

    }

    public class CancelExtraParameterModel
    {
        public int? GameID { get; set; }

        public int? MaxHourRange { get; set; }

        public string BetReocrdNoPGS { get; set; }

        public bool IsIgnoreCancelNotFoundProcess { get; set; }

    }


    public class SingleWalletChangeBetPayoutRequestParameter 
    {
        /// <summary>
        /// 三方單號
        /// </summary>
        public string BetRecordNo { get; set; }

        public string GameCode { get; set; }

        public string UpdateBy { get; set; }

        public bool IsSkipSeamlessCallback { get; set; }



        public CancelExtraParameterModel ExtraInfo { get; set; }


        public SingleWalletChangeSettleInfo NewSettleInfo { get; set; }

    }




    public class SingleWalletChangeSettleInfo
    {

        public decimal PayoutAmount { get; set; }
   
        public decimal ValidAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public string Reason { get; set; }

        /// <summary>
        /// 有些遊戲商不是直接指定新的payout值
        /// 而是指定差異值。e.g. 原本+10元。遊戲商會傳-10  代表afterValue=10-10 = 0
        /// </summary>
        public decimal? MinusPayoutAmount { get; set; }
    }

}
