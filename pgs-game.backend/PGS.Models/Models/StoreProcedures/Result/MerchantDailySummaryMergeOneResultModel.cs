using System;

namespace target.Models.Models.StoreProcedures
{
    public class MerchantDailySummaryMergeOneResultModel
	{
		public DateTime Date { get; set; }
		public int TotalRound { get; set; }
		public decimal TaxAmount { get; set; }
		public decimal BetAmount { get; set; }
		public decimal PayoutAmount { get; set; }
		public decimal ValidAmount { get; set; }

		public decimal WinLossAmount { get; set; }
	}




}
