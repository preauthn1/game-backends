using System;

namespace game.Entity.Game
{


    public class SingleWalletSettleCallbackParameter
    {
        public int MerchantID { get; set; }

        public int GameID { get; set; }

        public int PlayerID { get; set; }

        public string PlayerName { get; set; }


        /// <summary>
        /// 若是玩三方遊戲商遊戲，請傳入PGS自訂的注單號
        /// </summary>
        public string BetRecordNoPGS { get; set; }

        public decimal BetAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal ValidAmount { get; set; }

        /// <summary>
        /// 不傳也沒關係 (會嘗試用另一種方法查到)
        /// 但如果有傳 會省一點點查詢成本
        /// </summary>
        public string SessionTokenPGS { get; set; }


        public DateTime? SettlementTime { get; set; }

        /// <summary>
        /// debug用 (方便查詢 通知請求的API紀錄)
        /// 寫Log會截斷最多50字
        /// </summary>
        public string DebugProcessUUID { get; set; }


        public SettleBetCallbackExtraInfo ExtraInfo { get; set; }

    }


    public class SettleBetCallbackExtraInfo
    {
        //請注意這邊如果加新欄位 至少要修改以下地方
        // 1. UpdateWalletAmountSettleProcess (API.External )
        // 2. SettleProcess (API.Job)
        // 3. Settlement (SingleWalletPlatformApiRequestRefService)

        public bool IsBuyFeature { get; set; }

        public bool IsJackpot { get; set; }

        public string RoundID { get; set; }

    }


    /// <summary>
    /// 錢包加值的行為 (派彩、退款) 必備的參數
    /// </summary>
    public class SingleWalletBalanceCreditApiParameter
    {

        public int PlayerID { get; set; }
                        
        /// <summary>
        /// 若是玩三方遊戲商遊戲，請傳入PGS自訂的注單號
        /// </summary>
        public string BetRecordNoPGS { get; set; }

        public decimal Amount { get; set; }

        /// <summary>
        /// debug用 (方便查詢 通知請求的API紀錄)
        /// 寫Log會截斷最多50字
        /// </summary>
        public string DebugProcessUUID { get; set; }


        public bool IsCancelProcess { get; set; }



    }



    public class SingleWalletModeSettleApiRequestModel
    {

        public int GameID { get; set; }

        public string SessionToken { get; set; }

        public decimal BetAmount { get; set; }

        public decimal ValidAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal TaxAmount { get; set; }

        /// <summary>
        /// 傳給三方還是統一叫做 transactionNo 交易單號
        /// </summary>
        public string TransactionNo { get; set; }

        public string SettlementTime { get; set; }

        public string PlayerUniqueID { get; set; }

        public object ExtraInfo { get; set; }

    }



    public class SingleWalletModeSettleApiResponseModel
    {
        /// <summary>
        /// 對方需返回相同的單號、我方才會視為拿到成功回應
        /// </summary>
        public string TransactionNo { get; set; }
                
        /// <summary>
        /// 更新後的金幣
        /// </summary>
        public decimal Balance { get; set; }

    }



}
