namespace game.Entity.Game
{
    public class SingleWalletModeCancelApiParameter
    {
        public int GameID { get; set; }


        public int MerchantID { get; set; }


        public int PlayerID { get; set; }

        /// <summary>
        /// 等同於 BetRecordNoPGS
        /// 傳給三方還是統一叫做 transactionNo 交易單號
        /// </summary>
        public string TransactionNo { get; set; }

        public decimal BetAmount { get; set; }


        public string DebugProcessUUID { get; set; }


    }



    public class SingleWalletModeCancelApiRequestModel
    {
        public int GameID { get; set; }


        /// <summary>
        /// 傳給三方還是統一叫做 transactionNo 交易單號
        /// </summary>
        public string TransactionNo { get; set; }


        public string PlayerUniqueID { get; set; }

        public string SessionToken { get; set; }
    }


    public class SingleWalletModeCancelApiResponseModel
    {

        public string TransactionNo { get; set; }

    }


}
