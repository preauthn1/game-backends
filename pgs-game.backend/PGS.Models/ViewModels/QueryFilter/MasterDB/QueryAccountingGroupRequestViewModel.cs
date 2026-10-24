using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryAccountingGroupRequestViewModel : PaginationParameter
    {

        public List<int> AccountingGroupIDs { get; set; }

        public bool IsIncludeDelete { get; set; }
                

        /// <summary>
        /// 標準格式：ION那種格式，一家商戶一筆帳單
        /// 非標準格式：Richmond那種。根據代理。合併整合成一筆大帳單
        /// </summary>
        public int? InvoiceExportCategory { get; set; }

        /// <summary>
        /// 要排除特定
        /// </summary>
        public List<int> ExcludeInvoiceExportCategory { get; set; }

    }


}
