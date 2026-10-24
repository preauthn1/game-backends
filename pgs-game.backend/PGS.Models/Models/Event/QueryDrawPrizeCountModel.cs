using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryDrawPrizeCountModel
    {

        public int EventDrawPrizeID { get; set; }

        public int PrizeCount { get; set; }

        public int? TotalQuantity { get; set; }

        public int TotalUsageQuantity { get; set; }

    }


}
