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
    public class QueryBetRecordCustomizedSettingRequestViewModel : PaginationParameter
    {
        [ReferDbStringType(IsNVarchar = false, TextLength = 32)]
        public string BetRecordNoPGS { get; set; }

        public List<string> ListOfBetRecordNoPGS { get; set; }

        public DateTime? PartitionTimeMatch { get; set; }

        public DateTime? PartitionTimeBegin { get; set; }

        public DateTime? PartitionTimeEnd { get; set; }

        /// <summary>
        /// BetRecordCustomizedSettingEnum
        /// </summary>
        public int? SettingKey { get; set; }

        /// <summary>
        /// BetRecordCustomizedSettingEnum
        /// </summary>
        public List<int> SettingKeys { get; set; }

        public string SettingValueMatch { get; set; }

    }


}
