using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryPlaceBetRaiseRecordRequestViewModel : PaginationParameter
    {

        public string BetRecordNoPGS { get; set; }

        public string RaiseTransactionNo { get; set; }

        public DateTime? PartitionTimeMatch { get; set; }

        /// <summary>
        /// 參考 BetRecordRaiseBetStatusEnum
        /// </summary>
        public int? Status { get; set; }

        public bool IsLockQuery { get; set; }

    }


}
