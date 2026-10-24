using target.Models.Table;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class QueryAgentSummaryReportByHierarchyChildMappingResultViewModel
    {

		public string ExportFileSuffixText { get; set; }

		public int TargetMerchantID { get; set; }

		public string TargetMerchantName { get; set; }

		public string TargetMerchantCurrency { get; set; }

		/// <summary>
		/// 返回上層按鈕的上級ID (ps. 如果是root則不顯示按鈕)
		/// </summary>
		public int ParentMerchantID { get; set; }

		public string ParentMerchantName { get; set; }

		public string ServerTimeTW { get; set; }


		public List<QueryDirectAgentSummaryReportByHierarchyChildViewModel> Items { get; set; }


		public List<AgentTotalReportGameTypeSummaryItem> GameTypeSummaryItems { get; set; }
	}



}
