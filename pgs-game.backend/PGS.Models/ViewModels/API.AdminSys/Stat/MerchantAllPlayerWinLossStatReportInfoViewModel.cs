using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{



    public class MerchantAllPlayerWinLossStatReportInfoViewModel
	{		
		public List<MerchantAllPlayerWinLossStatReportViewModel> Items { get; set; }

		public MerchantAllPlayerWinLossStatReportViewModel TotalSummary { get; set; }

		public string ServerTimeTW { get; set; }

		public bool IsShowGoToAgentReportHint { get; set; }

	}


	public class MerchantAllPlayerWinLossStatReportViewModel
	{
		public int MerchantID { get; set; }
		public int PlayerID { get; set; }
		public string PlayerName { get; set; }
		public int TotalRound { get; set; }
		public decimal BetAmount { get; set; }
		public decimal ValidAmount { get; set; }
		public decimal TaxAmount { get; set; }
		public decimal PayoutAmount { get; set; }
		public decimal WinLossAmount { get; set; }
		public decimal RTP { get; set; }

	}



}
