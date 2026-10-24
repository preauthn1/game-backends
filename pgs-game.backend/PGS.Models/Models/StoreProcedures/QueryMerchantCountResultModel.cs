using target.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    public class QueryMerchantCountResultModel
    {
        public int MerchantID { get; set; }

        public int TotalCount { get; set; }
    }

    public class QueryGameEnableMerchantCountResultModel
    {
        public int GameID { get; set; }

        public int TotalCount { get; set; }
    }
}
