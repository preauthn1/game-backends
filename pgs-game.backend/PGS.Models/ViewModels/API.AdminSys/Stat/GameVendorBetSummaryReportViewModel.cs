using target.Models.Models.StoreProcedures;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class GameVendorBetSummaryReportViewModel
	{
		public List<GameVendorBetSummaryReportItemViewModel> Items { get; set; }

		public GameVendorBetSummaryReportItemViewModel TotalSummary { get; set; }
	}


	public class GameVendorBetSummaryReportItemViewModel
    {

		public int GameVendorType { get; set; }

		public string GameVendorTypeText { get; set; }

		public string ChargeCurrency { get; set; }

		public int TotalRound { get; set; }

		public decimal BetAmount { get; set; }

		public decimal PayoutAmount { get; set; }

		public decimal TaxAmount { get; set; }

		public decimal ValidAmount { get; set; }

		public decimal WinLossAmount { get; set; }

		/// <summary>
		/// 會另外算
		/// </summary>
		public decimal RTP { get; set; }

	}



}
