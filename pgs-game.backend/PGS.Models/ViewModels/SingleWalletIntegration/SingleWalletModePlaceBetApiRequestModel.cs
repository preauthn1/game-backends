using System;

namespace game.Entity.Game
{

    public class SingleWalletModePlaceBetParameter
    {
        public int PlayerID { get; set; }

        public string SessionTokenPGS { get; set; }

        public int GameID { get; set; }


        /// <summary>
        /// 標準單錢包商戶是投注金額、或另一種情境代表轉帳金額
        /// </summary>
        public decimal BetAmount { get; set; }

        /// <summary>
        /// 標準單錢包商戶是PGS投注單號、或另一種情境是轉帳單號
        /// </summary>
        public string BetRecordNoPGS { get; set; }

        public string DebugProcessUUID { get; set; }

        public PlaceBetCallbackExtraInfo ExtraInfo { get; set; }

    }


    public class SingleWalletModePlaceBetApiRequestModel
    {

        public string PlayerUniqueID { get; set; }

        public string SessionToken { get; set; }

        public int GameID { get; set; }

        public decimal BetAmount { get; set; }


        /// <summary>
        /// 傳給三方還是統一叫做 transactionNo 交易單號
        /// </summary>
        public string TransactionNo { get; set; }


        public object ExtraInfo { get; set; }


    }

    public class PlaceBetCallbackExtraInfo
    {
        public bool IsBuyFeature { get; set; }

        public string RoundID { get; set; }
    }


    public class SingleWalletModePlaceBetApiResponseModel
    {
        public string TransactionNo { get; set; }

        public decimal Balance { get; set; }

    }


}
