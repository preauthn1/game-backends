using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryUnionBetRecordRequestParameterModel : QueryUnionBetRecordRequestViewModel
    {
        public string PlayerName { get; set; }


        public int? PlayerID { get; set; }


        public int? MerchantID { get; set; }

    }


    public class QueryUnionBetRecordRequestViewModel : PaginationParameter
    {
     
        [SwaggerIgnore]
        public List<int> PlayerIDs { get; set; }

        [SwaggerIgnore]
        public List<int> MerchantIDs { get; set; }

        public int? GameID { get; set; }

        public int? GameVendorID { get; set; }

        public string TransactionNoPGS { get; set; }

        public List<string> ListOfTransactionNoPGS { get; set; }

        public string BetRecordNo { get; set; }

        public int? Status { get; set; }

        public int? TagID { get; set; }

        public string RoundID { get; set; }

        public DateTime? CreateTimeBegin { get; set; }

        public DateTime? CreateTimeEnd { get; set; }

        public DateTime? PartitionTimeBegin { get; set; }

        public DateTime? PartitionTimeEnd { get; set; }

        public DateTime? PartitionTimeMatch { get; set; }
    }


}
