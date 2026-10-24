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
    public class QueryBetRecordExtraInfoRequestViewModel : PaginationParameter
    {
        [ReferDbStringType(isNVarchar: true, textLegnth: 32)]
        public string BetRecordNoPGS { get; set; }

        public DateTime? PartitionTime { get; set; }
    }


}
