using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class InsertRawBetRecordRequiredInfoModel
    {

        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public int MerchantID { get; set; }

        public int GameID { get; set; }

        public string ThirdGameCode { get; set; }

        /// <summary>
        /// 投注金額 (法幣)
        /// </summary>
        public decimal BetAmount { get; set; }


        /// <summary>
        /// 投注金額 (遊戲幣)
        /// </summary>
        public decimal BetGamePoint { get; set; }


        /// <summary>
        /// PGS自訂注單號
        /// </summary>
        public string BetRecordNoPGS { get; set; }


        public string BetRecordNo{ get; set; }



        public DateTime PartitionTime { get; set; }



        public decimal ConvertRate { get; set; }

        public int VendorConfigID { get; set; }


        /// <summary>
        /// 投注扣錢之前的錢包金額
        /// </summary>
        public decimal AmountBeforeBet { get; set; }


        /// <summary>
        /// 只有特定狀況才會在placeBet階段寫入
        /// </summary>
        public decimal? ValidAmount { get; set; }

    }


    public class SettlementBetRecordInfoModel
    {

        public long Id { get; set; }

        public string BetRecordNo { get; set; }

        public string BetRecordNoPGS { get; set; }

        public int PlayerID { get; set; }

        public decimal BetAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? SettlementTime { get; set; }


        public string Extra_RoundID { get; set; }

        /// <summary>
        /// extraInfo要額外紀錄的東西 (如果有需要的話)
        /// </summary>
        public string DebugInfoForNextProcess { get; set; }

        /// <summary>
        /// Funky TransferGame 實際派獎金額
        /// </summary>
        public int? TransferGameRealPayoutAmount { get; set; }

    }




    public class SettlementBetRecordRequiredBriefInfoModel
    {

        public int MerchantID { get; set; }


        public int GameID { get; set; }


        /// <summary>
        /// PGS自訂注單號
        /// </summary>
        public string BetRecordNoPGS { get; set; }


        public decimal PayoutGamePoint { get; set; }


        public decimal PayoutAmount { get; set; }

        /// <summary>
        /// 部分遊戲商可能會需要用到betAmount 如果能事前查出來的話建議查一下
        /// </summary>
        public decimal QueryBetAmountBeforeUpdate { get; set; }


        /// <summary>
        /// Debug用 可以跟external logs 匹配用
        /// </summary>
        public string DebugExtraMemo { get; set; }


        /// <summary>
        /// 如果是 加注型注單。結算時會更新總投注金額
        /// </summary>
        public decimal? NewBetAmount { get; set; }



        /// <summary>
        /// 投注當時的比例
        /// </summary>
        public decimal ConvertRateWhenPlaceBet { get; set; }


    }



    public class SettlementBetRecordRequiredInfoModel
    {

        public int MerchantID { get; set; }


        /// <summary>
        /// PGS自訂注單號
        /// </summary>
        public string BetRecordNoPGS { get; set; }



        /// <summary>
        /// Debug用 可以跟external logs 匹配用
        /// </summary>
        public string DebugExtraMemo { get; set; }


        public decimal PayoutAmount { get; set; }



        public decimal ValidAmount { get; set; }


        /// <summary>
        /// 目前只有AGG需要這欄位
        /// </summary>
        public decimal JackpotAmount { get; set; }



        /// <summary>
        /// 視訊/棋牌 遊戲 會有這欄位
        /// </summary>
        public decimal TaxAmount { get; set; }



        /// <summary>
        /// 是否為後台手動操作
        /// </summary>
        public bool IsAdminSysManualOperate { get; set; }


        /// <summary>
        /// 如果手動結算要記錄操作人
        /// </summary>
        public int? UserID { get; set; }



        public ExtraColumnByGameVendorParameter ExtraColumn { get; set; }

    }





    public class ExtraColumnByGameVendorParameter
    {

        public string AGinBillNo { get; set; }

        public string RoundID { get; set; }

        public string GameResultID { get; set; }

        /// <summary>
        /// 使用投注金額作為有效流水
        /// </summary>
        public bool UseBetAmountAsValidAmount { get; set; }

        public decimal RoundTotalFlowAmount { get; set; }

        /// <summary>
        /// 請轉成UTC+0的時間
        /// </summary>
        public DateTime? ThirdPartySettleTime { get; set; }

        /// <summary>
        /// 加注型注單。結算後的投注金額會改變
        /// </summary>
        public decimal? NewBetAmount { get; set; }


        public object AGGLiveDetail { get; set; }
    }




    public class ConvertPerGameLogResultModel
    {
        /// <summary>
        /// 某些遊戲商會有0元注單 可以自行決定是否要insert
        /// </summary>
        public bool IsAllAmountZeroThenSkipProcess { get; set; }

        public List<SystemTagEnum> ExtraAddTags { get; set; }


        public PerGameDetailLogEntity PerGameDetailLog { get; set; }
    }



}
