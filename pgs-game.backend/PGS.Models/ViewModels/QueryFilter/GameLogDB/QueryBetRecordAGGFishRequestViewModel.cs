using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryBetRecordAGGFishRequestViewModel : PaginationParameter
    {
        public string BetRecordNoPGS { get; set; }

        public string BetRecordNo { get; set; }

        public string ThirdPartyGameCode { get; set; }

        public List<int> PlayerIDs { get; set; }

        /// <summary>
        /// 三方單號
        /// </summary>
        public List<string> ListOfBetRecordNo { get; set; }

        public int? Status { get; set; }

        public DateTime? CreateTimeBegin { get; set; }

        public DateTime? CreateTimeEnd { get; set; }


    }




}
