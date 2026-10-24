using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryBetRecordMerchantNotifyQueueRequestParameter : QueryBetRecordMerchantNotifyQueueRequestViewModel
    {
        public int? Type { get; set; }

    }


    public class QueryBetRecordMerchantNotifyQueueRequestViewModel : PaginationParameter
    {
        public List<int> IDs { get; set; }

        public List<string> ListOfBetRecordNoPGS { get; set; }

        public List<int> Types { get; set; }

        public string BetRecordNoPGS { get; set; }

        public int? MerchantID { get; set; }


        /// <summary>
        /// 查出要發送請求的單
        /// </summary>
        public bool IsMustNotify { get; set; }


        public DateTime? CreateTimeBegin { get; set; }


        public DateTime? CreateTimeEnd { get; set; }

    }


}
