using target.Models.Attributes;
using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryBetRecordGeneralRequestViewModel : PaginationParameter
    {

        [ReferDbStringType(IsNVarchar = false, TextLength = 32)]
        public string BetRecordNoPGS { get; set; }

        [ReferDbStringType(IsNVarchar = false, TextLength = 50)]
        public string BetRecordNo { get; set; }

        public DateTime? PartitionTimeMatch { get; set; }

        public DateTime? PartitionTimeBegin { get; set; }

        public DateTime? PartitionTimeEnd { get; set; }

        public string ThirdPartyGameCode { get; set; }

        public int? GameID { get; set; }

        public List<int> PlayerIDs { get; set; }

        /// <summary>
        /// 三方單號
        /// </summary>
        public List<string> ListOfBetRecordNo { get; set; }

        public int? Status { get; set; }

        public DateTime? CreateTimeBegin { get; set; }

        public DateTime? CreateTimeEnd { get; set; }


        [SwaggerIgnore]
        public int? TimeoutSeconds { get; set; }


        /// <summary>
        /// 用來標注是哪個function呼叫的
        /// </summary>
        public string DebugSourceMethod { get; set; }
    }


}
