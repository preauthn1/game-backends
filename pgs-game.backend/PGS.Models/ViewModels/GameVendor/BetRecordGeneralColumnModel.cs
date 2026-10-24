using System;

namespace target.Models.ViewModels.GameVendor
{
    public class BetRecordGeneralColumnModel : IBetRecordBasicInfo
    {
        public long Id { get; set; }


        /// <summary>
        /// 三方的注單號
        /// </summary>
        public string BetRecordNo { get; set; }
        public string BetRecordNoPGS { get; set; }
        public string ThirdPartyGameCode { get; set; }
        public int MerchantID { get; set; }
        public int GameID { get; set; }
        public string GameName { get; set; }
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public string PlayerIPAddress { get; set; }
        public decimal BetAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }

        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime? SettlementTime { get; set; }
        public DateTime? ThirdSettleTime { get; set; }

        public DateTime? ThirdSettleDate 
        { 
            get
            {
                if (!ThirdSettleTime.HasValue)
                    return null;

                return ThirdSettleTime.Value.AddHours(8);
            }
        }

        public int? ManualSettleUserID { get; set; }
        public decimal ConvertRate { get; set; }

        public int? VendorConfigID { get; set; }


        /// <summary>
        /// 只有部分遊戲商才有
        /// </summary>
        public decimal? AmountBeforeBet { get; set; }

        /// <summary>
        /// 根據不同遊戲商可能有額外的資訊顯示
        /// e.g. 視訊遊戲的局號
        /// </summary>
        public string ExtraInfo1 { get; set; }

        public string ExtraInfo2 { get; set; }

    }




    public class BetRecordTransactionNoInfoModel
    {
        /// <summary>
        /// 大部分實作 是使用這個欄位查詢
        /// </summary>
        public string BetRecordNoPGS { get; set; }


        /// <summary>
        /// 某些廠商因為有特殊機制 會使用此欄位
        /// </summary>
        public string BetRecordNo { get; set; }
    }


    /// <summary>
    /// 查的欄位比較少的輕量化資訊
    /// </summary>
    public class BetRecordBriefInfoModel : IBetRecordBasicInfo
    {
        public string BetRecordNoPGS { get; set; }

        public string BetRecordNo { get; set; }

        public int PlayerID { get; set; }

        public int MerchantID { get; set; }

        public int GameID { get; set; }

        public int Status { get; set; }

        public decimal BetAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal ValidAmount { get; set; }

        public decimal ConvertRate { get; set; }

        public DateTime CreateTime { get; set; }


        /// <summary>
        /// 只有部分遊戲商才有
        /// </summary>
        public decimal? AmountBeforeBet { get; set; }

        /// <summary>
        /// 只有部分遊戲商才有
        /// </summary>
        public string RoundID { get; set; }      

    }


    public interface IBetRecordBasicInfo
    {
        public string BetRecordNoPGS { get; set; }

        public string BetRecordNo { get; set; }

        public int PlayerID { get; set; }

        public int MerchantID { get; set; }

        public int GameID { get; set; }

        public int Status { get; set; }

        public decimal BetAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        /// <summary>
        /// 只有部分遊戲商才有
        /// </summary>
        public decimal? AmountBeforeBet { get; set; }
    }

    public class BetRecordInfoQueryParameter
    {
        public string BetRecordNo { get; set; }
      
        public string ThirdPartyGameCode { get; set; }

        public int? GameID { get; set; }

        /// <summary>
        /// 特定遊戲商需要用這個額外比對 否則唯一性條件不夠
        /// </summary>
        public int PlayerID { get; set; }

        /// <summary>
        /// 不一定會有此值 (僅在特殊情況下會使用此方式查詢、僅部分遊戲商有實作此)
        /// </summary>
        public string BetRecordNoPGS { get; set; }


        /// <summary>
        /// 部分遊戲商的注單量很大。根據狀況 除了篩選三方單號、還會額外加入時間區間讓查詢更快 (如果有使用partition機制的話)
        /// </summary>
        public bool IsQueryRecentRange { get; set; }


        /// <summary>
        /// 需搭配 IsPlaceBetProcess 才會生效
        /// </summary>
        public bool IsPartitionMatchQuery { get; set; }


        public bool IsPlaceBetProcess { get; set; }



        /// <summary>
        /// 如果有指定 最多只會查N小時內的資料 (e.g 72小時內)
        /// ps. 只有部分遊戲商有使用此邏輯: (主要是支援partitionTable的遊戲商)
        /// </summary>
        public int? MaxQueryHourRange { get; set; }


        /// <summary>
        /// Debug用 用來記錄是哪邊呼叫的
        /// </summary>
        public string DebugSourceMethod { get; set; }

    }

}
