using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class BetRecordManualSettleRequestModel
    {
        /// <summary>
        /// PGS單號
        /// </summary>
        public string TransactionNo { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal ValidAmount { get; set; }

        public string Memo { get; set; }

        public string ThirdSettleDate { get; set; }

    }


    public class BetRecordManualChangeSettleAmountRequestModel
    {
        /// <summary>
        /// PGS單號
        /// </summary>
        public string TransactionNo { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal ValidAmount { get; set; }

        public string Memo { get; set; }

        public bool IsTriggerCallback { get; set; }
    }




    public class BetRecordManualWritePerGameRequestModel
    {


        /// <summary>
        /// PGS單號
        /// </summary>
        public string TransactionNoPGS { get; set; }


        /// <summary>
        /// 備註
        /// </summary>
        public string Memo { get; set; }


        /// <summary>
        /// 如果有指定 可以標註tagID (SystemTagEnum)
        /// </summary>
        public int? TagID { get; set; }


        /// <summary>
        /// 若設為true，perGame 遊戲時間 GameTime (對帳的依據) 會被更新為目前時間 
        /// </summary>
        public bool IsReplaceGameTimeToNowTime { get; set; }


        /// <summary>
        /// 是否自動建立 單錢包補發結算通知的佇列
        /// </summary>
        public bool IsWriteSingleWalletNotifyQueue { get; set; }


        /// <summary>
        /// 是否派發彩金 並 更新錢包金額
        /// </summary>
        public bool IsUpdatePayoutAmount { get; set; }

    }



    public class BetRecordManualBatchWritePerGameRequestModel : BetRecordManualWritePerGameRequestModel
    {

        /// <summary>
        /// 多筆 PGS單號 (陣列格式)
        /// </summary>
        public List<string> ListOfTransactionNoPGS { get; set; }


        /// <summary>
        /// 貼上字串解析 PGS單號
        /// </summary>
        public string CommaSeperateTransactionNoPGS { get; set; }



        /// <summary>
        /// 只會搜尋72小時內的tag資料
        /// </summary>
        public int? QueryDataByTagID { get; set; }


    }


    public class BetRecordManualDeletePerGameRequestModel
    {
        /// <summary>
        /// 多筆 PGS單號 (陣列格式)
        /// </summary>
        public List<string> ListOfTransactionNoPGS { get; set; }


        public int ReplaceToThisPlayerID { get; set; }
    }



    public class BatchWriteTagsRequestModel
    {

        /// <summary>
        /// 多筆 PGS單號 (陣列格式)
        /// </summary>
        public List<string> ListOfTransactionNoPGS { get; set; }


        /// <summary>
        /// 請參考 SystemTagEnum
        /// </summary>
        public int TagID { get; set; }

    }

    public class ManualTestBonusSettleCallbackRequest
    {
        public int PlayerID { get; set; }
    }


}
