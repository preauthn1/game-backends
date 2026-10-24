using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class RetentionReportViewModel
    {
        public List<RetentionReportItemViewModel> Items { get; set; }
    }



    public class RetentionReportItemViewModel
    {

        public DateTime Date { get; set; }

        public string DateText { get; set; }

        /// <summary>
        /// 1~7日 的返回率
        /// </summary>
        public List<decimal?> DaysRetentionRate { get; set; }

        public List<RetentionRateDayMapping> DaysRetentionMapping { get; set; }


        public int NewPlayerCount { get; set; }



        public RetentionRateDayMapping Day1 { get; set; }
        public RetentionRateDayMapping Day3 { get; set; }
        public RetentionRateDayMapping Day7 { get; set; }
        public RetentionRateDayMapping Day14 { get; set; }
        public RetentionRateDayMapping Day30 { get; set; }



    }



    public class RetentionRateDayMapping
    {
        public int TargetDay { get; set; }

        public decimal? RetentionRate { get; set; }

        public int RetentionPlayerCount { get; set; }

        public bool IsFutureDate { get; set; }

    }







}
