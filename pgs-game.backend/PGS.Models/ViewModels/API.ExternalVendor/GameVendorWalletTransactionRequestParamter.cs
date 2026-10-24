using target.Models.Enums;
using System;

namespace target.Models.ViewModels
{
    public class GameVendorWalletTransactionRequestParamter
    {
        public string SessionToken { get; set; }

        public int PlayerID { get; set; }

        /// <summary>
        /// 非必填
        /// </summary>
        public string ThirdPartyGameCode { get; set; }

      
        /// <summary>
        /// 遊戲商單號
        /// </summary>
        public string TransactionID { get; set; }

        public decimal Amount { get; set; }

        public TransactionTypeEnum TransactionType { get; set; }

        public string Memo { get; set; }


        public GameVendorWalletBonusExtraParamter Extra { get; set; }


        /// <summary>
        /// 如果要寫成 EventPrize資料 則需要傳入此物件
        /// </summary>
        public GameVendorConvertToEventPrizeInfoModel EventPrizeInfo { get; set; }

    }



    public class GameVendorWalletBonusExtraParamter
    {

        /// <summary>
        /// 可以不填寫
        /// </summary>
        public string PgsTransactionNo { get; set; }


        /// <summary>
        /// 非必填
        /// </summary>
        public int? GameID { get; set; }



        public bool IsThrowDuplicateException { get; set; }

    }



    public class GameVendorConvertToEventPrizeInfoModel
    {
        /// <summary>
        /// 三方單號
        /// </summary>
        public string ThirdOrderNo { get; set; }
      

        public string BonusTypeName { get; set; }


        /// <summary>
        /// 可填寫NULL，若NULL會根據遊戲商的特殊標記分類
        /// 若填寫特定幾種AGG type。會寫入特殊的eventID
        /// </summary>
        public int? EventPrizeType { get; set; }

        public decimal Amount { get; set; }

        /// <summary>
        /// 資料表 EventPrizeRecord 的 ReceiveTime 也會寫入這個時間
        /// 如果三方的時間跨日了...可能會有一些判斷要處理
        /// 目前的作法 如果跨日。而且超過1小時...寫入時間則會變成系統當前時間
        /// 跨日比較麻煩的問題在於 跟三方對帳會逐漸出現誤差
        /// </summary>
        public DateTime ReceiveTime { get; set; }

        public bool IsFishGameBonus { get; set; }

        public bool IsOtherBonus { get; set; }


        public decimal? ReferValidAmount { get; set; }


        public string ReferThirdPartyEventID { get; set; }


        /// <summary>
        /// 程式碼另外查
        /// </summary>
        public int? GameID { get; set; }


    }




    public class GameVendorWalletTransactionResultModel
    {
        public int PlayerID { get; set; }

        public string PgsTransactionNo { get; set; }

        public decimal BeforeAmount { get; set; }

        public decimal AfterAmount { get; set; }

        public decimal BeforeGamePoint { get; set; }

        public decimal AfterGamePoint { get; set; }
    }


}
