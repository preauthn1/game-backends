using target.Models.Enums;
using System;

namespace target.Models.ViewModels
{



    public class GameVendorPlaceBetCallbackRequestParameter : SingleWalletBasePlayerInfoModel
    {

        public string GameCode { get; set; }

        public string BetRecordNo { get; set; }

        public decimal BetAmount { get; set; }

        public string PlayerIPAddress { get; set; }

        public DateTime? ApiRequestBeginTime { get; set; }

        /// <summary>
        /// 加注單號(僅部分遊戲商)
        /// </summary>
        public string RaiseTransactionNo { get; set; }

        public SingleWalletPlaceBetExtraParameter ExtraInfo { get; set; }

    }


    public class SingleWalletPlaceBetExtraParameter
    {
        public int TokenGameID { get; set; }

        public string RoundID { get; set; }

        public decimal? PayoutGamePoint { get; set; }

        public bool IsSupportZeroBet { get; set; }

        public bool IsAGGBuyFeature { get; set; }

        public bool IsFreeGame_BetAndSettleBundle { get; set; }

        public bool IsJiliPreserveGame { get; set; }

        /// <summary>
        /// 若bet=0 且有標記活動獎勵類型 會視為活動獎勵
        /// 會在PlaceBet流程 跳過某些操作
        /// </summary>
        public EventPrizeTypeEnum ReferEventPrizeType { get; set; }

        public decimal? ReferValidAmount { get; set; }


        /// <summary>
        /// 不會進行錢包更新、純粹寫紀錄用
        /// </summary>
        public bool IsSkipUpdateBalance { get; set; }

    }



    public class SingleWalletPlaceBetDebitAmountResponseParameter
    {
        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public string Currency { get; set; }

        public string BetRecordNo { get; set; }

        public string BetRecordNoPGS { get; set; }

        /// <summary>
        /// 更新前金額 (法幣)
        /// </summary>
        public decimal BeforeAmount { get; set; }

        /// <summary>
        /// 更新前金額 (遊戲幣)
        /// </summary>
        public decimal BeforeGamePoint { get; set; }




        /// <summary>
        /// 更新後金額 (法幣)
        /// </summary>
        public decimal AfterAmount { get; set; }

        /// <summary>
        /// 更新後金額 (遊戲幣)
        /// </summary>
        public decimal AfterGamePoint { get; set; }

    }





}
