using target.Models.Enums;
using System;

namespace target.Models.ViewModels
{

    public class GameVendorSettleCallbackParameter : SingleWalletBasePlayerInfoModel
    {
        /// <summary>
        /// 投注單號
        /// </summary>
        public string BetRecordNo { get; set; }

        /// <summary>
        /// 派彩金額 (可能是100倍的數值 需要轉)
        /// </summary>
        public decimal PayoutGamePoint { get; set; }

        /// <summary>
        /// 三方廠商的遊戲代碼
        /// </summary>
        public string GameCode { get; set; }



        public ThirdGameSettleExtraParameter SettleExtraInfo { get; set; }
    }


    public class SettleBalanceUpdateProcessConfig 
    {
       
        /// <summary>
        /// 若有指定 prizeType 會被視為活動支出。不會計算為一般帳務
        /// 會使用在幾種有bet情境。但又要視為活動成本的情境 (e.g. 虛寶卡免費投注、freespin)
        /// </summary>
        public EventPrizeTypeEnum ReferEventPrizeType { get; set; }

        public bool IsSkipUpdateBalance { get; set; }

        public string SessionToken { get; set; }

        public int MerchantWalletMode { get; set; }

        public string RequestUUID { get; set; }

        public string Wager_RoundID { get; set; }
        
        public decimal Wager_PayoutAmountWalletUpdate { get; set; }


        public SettleProcessEventDetailModel EventBonusDetail { get; set; }

    }



    public class SettleProcessEventDetailModel
    {
        public decimal? ReferValidAmount { get; set; }

        public string ReferThirdPartyEventID { get; set; }

    }



    public class ThirdGameSettleExtraParameter
    {
        public string BetRecordNoPGS { get; set; }

        /// <summary>
        /// 若有指定。則會在結算後更新投注金額 (僅特定遊戲商)
        /// </summary>
        public decimal? NewBetAmount { get; set; }

        public decimal? RefundPreserveAmount { get; set; }


        /// <summary>
        /// 是否略過錢包更新 (純粹寫一筆紀錄 不更新錢包金額)
        /// </summary>
        public bool IsSkipUpdateBalance { get; set; }


        public bool IsWriteRGDetailItem { get; set; }


        public bool IsBuyFeature { get; set; }

        /// <summary>
        /// 若為true 只會記錄一筆資料 但不會結算注單
        /// </summary>
        public bool IsNotCompleteSettleSubItem { get; set; }


        /// <summary>
        /// Jili某些棋牌遊戲 需要預扣一筆金額
        /// </summary>
        public bool IsJiliPreserveGame { get; set; }


        public string RGGameResultID { get; set; }


        public string SMTransactionID { get; set; }


        public string FumaTransactionID { get; set; }


        /// <summary>
        /// 部分三方遊戲，會有多次結算callback，寫入一些額外的detail資訊紀錄
        /// 是否為最後一筆Settle (isComplted)
        /// </summary>
        public bool MultipleSettleIsComplete { get; set; }


        /// <summary>
        /// 部分三方遊戲，會有多次結算callback，寫入一些額外的detail資訊紀錄
        /// 每一次結算的金額
        /// </summary>
        public decimal MultipleSettlePayoutAmount { get; set; }


        /// <summary>
        /// 若有標記 會視為活動支出
        /// </summary>
        public EventPrizeTypeEnum ReferEventPrizeType { get; set; }

        /// <summary>
        /// 僅特定遊戲商有這個東西可以判斷
        /// </summary>
        public bool IsFreeGame { get; set; }

        /// <summary>
        /// 僅特定遊戲商有這個東西可以判斷
        /// </summary>
        public bool IsJackpot { get; set; }


        public decimal? ReferValidAmount { get; set; }

        public string ReferThirdPartyEventID { get; set; }

    }


    public class SingleWalletVendorCustomizedResponseConvertModel
    {

        public GameVendorBetCommonInfo CommonInfo { get; set; }


    }


    public class GameVendorBetCommonInfo
    {
        public string BetRecordNo { get; set; }

        public string BetRecordNoPGS { get; set; }

        public string Currency { get; set; }

        public decimal BeforeGamePoint { get; set; }

        /// <summary>
        /// 如果是 NoneComplted的情況。數值會是
        /// AmountBeforeBet - BetAmount + extraPayoutAdjust
        /// </summary>
        public decimal AfterGamePoint { get; set; }

        public int GameID { get; set; }

        public int PlayerID { get; set; }

        public bool IsSingleWallet { get; set; }


        /// <summary>
        /// 僅部分遊戲商會用到此
        /// </summary>
        public DateTime? ThirdSettleDate { get; set; }


        /// <summary>
        /// 真實更新錢包的金額
        /// </summary>
        public decimal WalletUpdateAmount { get; set; }
    }




    public class SingleWalletChangeSettleInfoResponseModel
    {
        public string BetRecordNoPGS { get; set; }

    }




    public class TimeDebugModel
    {
        public TimeDebugModel()
        {

        }

        public TimeDebugModel(string memo , DateTime time)
        {
            this.Memo = memo;
            this.Time = time;
        }

        public string Memo { get; set; }

        public DateTime Time { get; set; }
    }



}
