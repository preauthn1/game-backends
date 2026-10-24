using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryEazyTempValueRequestViewModel : PaginationParameter
    {
        public List<Guid> UUIDs { get; set; }

        public List<string> ListOfBetRecordNoPGS { get; set; }

        public string BetRecordNoPGS { get; set; }

        public string Memo1 { get; set; }


        /// <summary>
        /// 參考 EazyTempValueTypeEnum
        /// </summary>
        public int? Type { get; set; }

        public List<int> Types { get; set; }

        public DateTime? CreateTimeBegin { get; set; }

        public DateTime? CreateTimeEnd { get; set; }

    }


}
