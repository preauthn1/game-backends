using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryPlayerGameSummaryStatReportReportApiRequestParameterModel : IFilterBeginEndTime
    {
        public int? PlayerID { get; set; }

        public string PlayerName { get; set; }

        public DateTime? Begin { get; set; }

        public DateTime? End { get; set; }

    }



    public class QueryRetentionReportRequestParameterModel
    {
        public int MerchantID { get; set; }

        public double TimeZone { get; set; }

        public DateTime? Begin { get; set; }

        //public DateTime? End { get; set; }
    }



    public class QueryMerchantSummaryStatReportReportApiRequestParameterModel
    {

        /// <summary>
        /// 多選 (參考 StatReportSearchColumnType.cs)
        /// </summary>
        public List<int> IncludeExtraSearchInfo { get; set; }

        public List<int> MerchantIDs { get; set; }

        public DateTime? Begin { get; set; }

        public DateTime? End { get; set; }

    }


    public class MerchantSummaryStatReportViewModel
    {

        public string ServerTimeTW { get; set; }

        public List<MerchantSummaryStatReportItemViewModel> Merchants { get; set; }

    }

    public class PlayerGameSummaryStatReportViewModel
    {
        public PlayerGameSummaryStatReportViewModel()
        {
            this.PlayerGameSummary = new List<PlayerGameSummaryStatItemViewModel>();
            this.TotalSummary = new PlayerGameSummaryStatItemViewModel();
        }

        public int MerchantID { get; set; }

        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public string ServerTimeTW { get; set; }


        public List<PlayerEventSummaryStatItemViewModel> EventBonusSummary { get; set; }

        public PlayerEventSummaryStatItemViewModel TotalEventSummary { get; set; }

        public List<PlayerGameSummaryStatItemViewModel> PlayerGameSummary { get; set; }

        public PlayerGameSummaryStatItemViewModel TotalSummary { get; set; }

    }


    public class PlayerGameSummaryStatItemViewModel
    {
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int MerchantID { get; set; }
        public string MerchantName { get; set; }


        public int GameID { get; set; }
        public string GameName { get; set; }
        public int TotalRound { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal RTP { get; set; }

    }



    public class PlayerEventSummaryStatItemViewModel
    {
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int MerchantID { get; set; }
        public string MerchantName { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; }
        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }

    }









    public class MerchantSummaryStatReportItemViewModel
    {
        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public string Currency { get; set; }

        public MerchantSummaryStatReportItemBaseInfoViewModel BasicSummary { get; set; }

        public MerchantSummaryStatReportItemLoginInfoViewModel LoginSummary { get; set; }

        public MerchantSummaryStatReportItemGameInfoViewModel GameSummary { get; set; }

        public MerchantGameVendorSummaryViewModel GameVendorSummary { get; set; }

        public List<ReportCountSummaryViewModel> LanguageDistributeReport { get; set; }

        public List<ReportCountSummaryViewModel> DeviceDistributeReport { get; set; }

        public BetAmountRangeReportViewModel PlayerTWDBetAmountDistribute { get; set; }

    }


    //public class MerchantSummaryLanguageCodeReportViewModel
    //{
    //    public string LanguageCode { get; set; }

    //    public string LanguageCodeText { get; set; }

    //    public int TotalCount { get; set; }

    //    public decimal Rate { get; set; }
    //}


    public class ReportCountSummaryViewModel
    {
        public string DisplayText { get; set; }

        public string Memo { get; set; }

        public int TotalCount { get; set; }

        public decimal Rate { get; set; }

        public decimal Rate_100X
        {
            get
            {
                return Math.Round(this.Rate * 100, 2);
            }
        }
    }


    public class BetAmountRangeReportViewModel
    {
        public string Currency { get; set; }

        public decimal ExchangeRate { get; set; }

        public int TotalPlayers { get; set; }

        public decimal TotalBetAmount { get; set; }

        public List<BetAmountRangeItemViewModel> Items { get; set; }
    }

    public class PlayerAmountTempModel
    {
        public int PlayerID { get; set; }

        public decimal Amount { get; set; }
    }


    public class BetAmountRangeItemViewModel
    {

        public BetAmountRangeItemViewModel() { }

        public BetAmountRangeItemViewModel(decimal min, decimal max)
        {
            this.MinBetAmount = min;
            this.MaxBetAmount = max;
        }

        public decimal MaxBetAmount { get; set; }

        public decimal MinBetAmount { get; set; }

        public int TotalCount { get; set; }

        public decimal TotalBetAmount { get; set; }

        public decimal Rate { get; set; }

        public decimal Rate_100X
        {
            get
            {
                return Math.Round(this.Rate * 100, 2);
            }
        }


        public decimal BetAmountRate { get; set; }

        public decimal BetAmountRate_100X
        {
            get
            {
                return Math.Round(this.BetAmountRate * 100, 2);
            }
        }


        public bool IsAnyFishingGame { get; set; }

        public List<int> PlayerIDs { get; set; }
    }


    public class MerchantSummaryStatReportItemBaseInfoViewModel
    {
        public string Currency { get; set; }
        public int TotalLoginPlayer { get; set; }
        public decimal TransferAmount { get; set; }
        public decimal TransferInAmount { get; set; }
        public decimal TransferOutAmount { get; set; }
        public decimal WalletAmount { get; set; }
    }



    public class MerchantSummaryStatReportItemLoginInfoViewModel
    {

        public int TotalLoginCount { get; set; }
        public int NewLoginPlayer { get; set; }
        public int MaxLoginPlayer { get; set; }
        public int AvgLoginPlayer { get; set; }


    }


    public class MerchantSummaryStatReportItemGameInfoViewModel
    {
        public int MerchantID { get; set; }
        public string MerchantName { get; set; }
        public decimal TotalPromotionCost { get; set; }
        public int TotalRound { get; set; }
        public int TotalBetPlayer { get; set; }
        public decimal WinAmount { get; set; }
        public decimal LoseAmount { get; set; }
        public decimal MoneyChangeAmount { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal RTP { get; set; }

    }





    public class MerchantGameVendorSummaryViewModel
    {

        public int MerchantID { get; set; }

        public List<MerchantGameVendorSummaryItemViewModel> Items { get; set; }

        public MerchantGameVendorSummaryItemViewModel TotalSummary { get; set; }

    }


    public class MerchantGameVendorSummaryItemViewModel
    {
        public int GameVendorType { get; set; }
        public string GameVendorTypeText { get; set; }


        public int TotalRound { get; set; }
        public int BetPlayer { get; set; }
        public decimal BetAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal RTP { get; set; }


    }











}
