using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models.Models.StoreProcedures
{
    public class Stat_GameVendorBetSummaryResultModel
	{
		public int MerchantID { get; set; }

		public string ChargeCurrency { get; set; }

		public int GameID { get; set; }

		public int GameVendorType { get; set; }

		public int TotalRound { get; set; }

		public decimal BetAmount { get; set; }

		public decimal PayoutAmount { get; set; }

		public decimal TaxAmount { get; set; }

		public decimal ValidAmount { get; set; }

		public decimal WinLossAmount { get; set; }

	

	}
}
