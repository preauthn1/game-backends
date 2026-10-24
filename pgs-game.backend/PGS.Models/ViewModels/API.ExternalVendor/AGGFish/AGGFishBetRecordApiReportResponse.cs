using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{

    public class AGGFishBetRecordApiRequestParameter
    {

        public int VendorConfigID { get; set; }

        public DateTime StartTimeUTC { get; set; }

        public DateTime EndTimeUTC { get; set; }


        /// <summary>
        /// 非必填
        /// </summary>
        public string RoundID { get; set; }

    }


    public class AGGFishBetRecordPgsRoundInfo
    {

        public string BetRecordNoPGS { get; set; }

        public int GameID { get; set; }

        public int MerchantID { get; set; }

        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public DateTime SettleTime { get; set; }

        /// <summary>
        /// 用來驗證fetch資料是否正確
        /// </summary>
        public AGGFishFetchDataCorrectValidInfo DataCorrectValidInfo { get; set; }
    }


    public class AGGFishFetchDataCorrectValidInfo
    {
        public decimal TotalBetAmount { get; set; }

        public decimal TotalWinLoss { get; set; }
    }


    public class AGGFishBetRecordApiReportResponse
    {
        public int code { get; set; }
        public string status { get; set; }
        public List<AGGFishBetRecordDetailApiResponse> result { get; set; }
    }

    public class AGGFishBetRecordDetailApiResponse
    {
        public string BetNumber { get; set; }
        public string RoundID { get; set; }
        public string RoundString { get; set; }
        public string Username { get; set; }
        public string MerchantId { get; set; }
        public string VendorCode { get; set; }
        public string GameID { get; set; }
        public string Currency { get; set; }
        public decimal TotalBet { get; set; }
        public decimal TotalWin { get; set; }
        public decimal WinLose { get; set; }
        public decimal Jackpot { get; set; }
        public decimal Bonus { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal ServicePercent { get; set; }
        public string Detail { get; set; }
        public int FishType { get; set; }
        public bool Cancel { get; set; }
        public int ErrorCode { get; set; }
        public string FishTypeDesc { get; set; }
        public AGGFishBetTimeConvert BetTime { get; set; }
    }


    public class AGGFishBetTimeConvert
    {
        public string value { get; set; }
    }


}
