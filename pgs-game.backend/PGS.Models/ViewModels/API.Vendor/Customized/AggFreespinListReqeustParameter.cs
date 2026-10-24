using target.Models.ViewModels.GameVendor;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class VendorAPIFreespinListReqeustParameter : BaseVendorRequestParameter
    {

        public int GameID { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string FreespinID { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class AggFreespinListResponseViewModel
    {
        public List<AggFreespinListDetailItemModel> Items { get; set; }

    }


    public class AggFreespinListDetailItemModel
    {
        public string FreeSpinId { get; set; }
        public int Rounds { get; set; }
        public List<AGGFreespinGameListItemViewModel> GameSettings { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime UseEndTime { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ModifyTime { get; set; }


        //public string enable { get; set; }

    }

    public class VendorAPIFreespinSetPromotionQuotaReqeustParameter : BaseVendorRequestParameter
    {
        public List<string> Players { get; set; }

        public int Rounds { get; set; }

        public List<int> GameIDs { get; set; }

        public bool IsAutoCreateNonExistPlayer { get; set; }

    }



    public class VendorAPIFreespinSetReqeustParameter : BaseVendorRequestParameter
    {

        //public string FreeSpinId { get; set; }

        public string BudgetId { get; set; }
        public int Rounds { get; set; }
        public List<AGGFreespinGameListItemViewModel> GameSettings { get; set; }
        public List<string> Players { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime UseEndTime { get; set; }
        public bool IsAutoCreateNonExistPlayer { get; set; }

    }


    public class AggFreespinSetResponseViewModel
    {
        public string FreespinID { get; set; }
    }

    public class AggFreespinSetPromotionQuotaResponseViewModel
    {
        public string FreespinID { get; set; }

        public decimal RemainAmount { get; set; }

        public long RemainRounds { get; set; }

        public DateTime UseEndTime { get; set; }
    }


    public class AggFreespinStatusResultViewModel
    {
        public string FreespinID { get; set; }
    }

    public class AggFreespinStatusItemViewModel
    {
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public string FreeSpinId { get; set; }
        public int LastRounds { get; set; }
        public int TotalRounds { get; set; }
        public decimal TotalPayout { get; set; }
        public string FreeSpinUseStatus { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ModifyTime { get; set; }
    }



    public class AggFreespinRecordResultViewModel
    {
        public string FreespinID { get; set; }
    }


    public class AggFreespinSummaryResultViewModel
    {
        public List<AggFreespinSummaryDetailItemViewModel> Items { get; set; }

    }


    public class AggFreespinSummaryDetailItemViewModel
    {
        public DateTime StatTime { get; set; }
        public string FreeSpinId { get; set; }
        public int GameID { get; set; }
        public decimal TotalBet { get; set; }
        public decimal TotalPayout { get; set; }
        public decimal TotalUser { get; set; }
        public decimal TotalRounds { get; set; }
    }


    public class AggGameInfoReqeustParameter : BaseVendorRequestParameter
    {

    }


    public class AggGameInfoResponseViewModel
    {
        public int MerchantID { get; set; }

        public string Currency { get; set; }

        public List<GameBettingInfoDetailItem> GameInfoList { get; set; }
    }


    public class GameBettingInfoDetailItem
    {
        public int GameID { get; set; }

        public string GameName { get; set; }

        public List<decimal> BetRanges { get; set; }
    }


    public class VendorAPIFreespinDisableReqeustParameter : BaseVendorRequestParameter
    {
        public string FreeSpinId { get; set; }
    }



    public class VendorAPIFreespinStatusReqeustParameter : BaseVendorRequestParameter
    {
        public string FreeSpinID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public string freeSpinUseStatus { get; set; }
    }



    public class VendorAPIFreespinRecordReqeustParameter : BaseVendorRequestParameter
    {
        public string FreeSpinID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }



    public class VendorAPIFreespinSummaryReqeustParameter : BaseVendorRequestParameter
    {
        public string FreeSpinID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }


    public class VendorAPIFreespinBudgetListReqeustParameter : BaseVendorRequestParameter
    {
        public string BudgetId { get; set; }

    }

    public class VendorAPIFreespinBudgetListResultViewModel
    {
        public List<AGGAPIFreespinBudgetDetailItemModel> Items { get; set; }

    }


    public class AGGAPIFreespinBudgetDetailItemModel
    {
        public string BudgetId { get; set; }

        public bool Enable { get; set; }

        public string Name { get; set; }

        public decimal RemainAmount { get; set; }

        public long RemainRounds { get; set; }

        public DateTime CreateTime { get; set; }

        public string Remark { get; set; }
    }


    public class VendorAPIFreespinBudgetAdminCreateReqeustParameter : BaseVendorRequestParameter
    {
        public string Name { get; set; }

        public decimal DefaultBetAmount { get; set; }

        public decimal TotalBudgetAmount { get; set; }

        public long TotalBudgetRounds { get; set; }

        public DateTime EventStartTime { get; set; }

        public DateTime EventEndTime { get; set; }
    }



    public class VendorAPIFreespinBudgetSetReqeustParameter : BaseVendorRequestParameter
    {
        public string BudgetId { get; set; }

        public bool Enable { get; set; }

        public string Name { get; set; }

        public string Remark { get; set; }

    }

    public class VendorAPIFreespinBudgetSetResultViewModel
    {
        public string BudgetId { get; set; }

    }



    public class VendorAPIFreespinBudgetDepositReqeustParameter : BaseVendorRequestParameter
    {
        public string BudgetId { get; set; }

        public decimal Amount { get; set; }

        public long Rounds { get; set; }
    }

    public class VendorAPIFreespinBudgetWalletUpdateResultViewModel
    {
        public decimal RemainAmount { get; set; }

        public long RemainRounds { get; set; }

    }


    public class VendorAPIFreespinBudgetWithdrawalReqeustParameter : BaseVendorRequestParameter
    {
        public string BudgetId { get; set; }

        public decimal Amount { get; set; }

        public long Rounds { get; set; }
    }


}
