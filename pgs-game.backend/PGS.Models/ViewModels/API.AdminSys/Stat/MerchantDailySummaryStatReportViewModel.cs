using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{


    public class MerchantDailySummaryStatReportInfoViewModel
	{		

		public int MerchantID { get; set; }

		public string MerchantName { get; set; }

		public string MerchantCurrency { get; set; }

		public bool IsAnyPromotionCost { get; set; }

		public bool IsSearchFishGameExtraInfo { get; set; }

		public bool IsShowGoToAgentReportHint { get; set; }

		public string ServerTimeTW { get; set; }


		public DateTime QueryBeginUTC { get; set; }

        public DateTime QueryEndUTC { get; set; }



        /// <summary>
        /// 是否使用小數點四位來顯示
        /// </summary>
        public bool Is4DecimalPlacesDisplay { get; set; }

		public List<MerchantDailySummaryStatReportViewModel> Items { get; set; }

		public MerchantDailySummaryStatReportViewModel TotalSummary { get; set; }

		public HierarchyExtraInfoViewModel HierarchyExtraInfo { get; set; }

	}



	public class HierarchyExtraInfoViewModel
    {
		public int RootMerchantID { get; set; }

		public int AvailableMerchantID { get; set; }



		public string DebugInfo { get; set; }

		public bool IsParentOwner { get; set; }

		public bool IsRootMerchantOwnerOrAdmin { get; set; }
	}


	public class MerchantDailySummaryStatReportViewModel
	{
		public DateTime DateTime { get; set; }
		public string Date { get; set; }
		public DateTime Begin { get; set; }
		public DateTime End { get; set; }
		public int MerchantID { get; set; }
		public string MerchantName { get; set; }
		public int TotalRound { get; set; }
		public int TotalLoginCount { get; set; }
		public decimal TransferInAmount { get; set; }
		public decimal TransferOutAmount { get; set; }
		public int TotalLoginPlayer { get; set; }
		public decimal TaxAmount { get; set; }
		public decimal BetAmount { get; set; }
		public decimal PayoutAmount { get; set; }
		public decimal ValidAmount { get; set; }
		public decimal MerchantWinLossAmount { get; set; }
		public decimal MerchantWinLossAmountWithPromotionCost { get; set; }

		/// <summary>
		/// 捕魚玩家剩餘金幣
		/// </summary>
		public decimal FishGameRemainingBalance { get; set; }
		public decimal WinLossAmount { get; set; }
		public decimal RTP { get; set; }
		public decimal PromotionCostAmount { get; set; }
	}



}
