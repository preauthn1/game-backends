using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryVendorMaintenanceApiRequestParameterModel : QueryVendorMaintenanceRequestViewModel
    {

    }

    public class QueryVendorMaintenanceRequestViewModel : PaginationParameter
    {

        public List<int> VendorIDs { get; set; }

        public bool IsIncludeDelete { get; set; }

        /// <summary>
        /// 正在時間範圍內 (進行中)
        /// </summary>
        public bool IsOnProgressing { get; set; }

        /// <summary>
        /// 進行中 或 即將開始(1小時內)
        /// </summary>
        public bool IsOnProgressingOrReadyToStart { get; set; }

        public int? Status { get; set; }

        public DateTime? EndTimeBegin { get; set; }

        public DateTime? EndTimeEnd { get; set; }

        public string DebugSourceMethod { get; set; }
    }


}
