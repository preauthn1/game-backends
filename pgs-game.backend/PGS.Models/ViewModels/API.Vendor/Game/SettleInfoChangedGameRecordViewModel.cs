using target.Models.Models;
using target.Models.Paging;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{

    public class SettleInfoChangedGameRecordRequestModel : PaginationParameter
    {
        [SwaggerApiRequiredField]
        public string MerchantSecretKey { get; set; }

        [SwaggerApiRequiredField]
        public int MerchantID { get; set; }

        [SwaggerApiRequiredField]
        public DateTime StartTime { get; set; }

        [SwaggerApiRequiredField]
        public DateTime EndTime { get; set; }
    }



    public class SettleInfoChangedParsingViewModel
    {
        public long Id { get; set; }

        public long PerGameDetailLogID { get; set; }

        public int MerchantID { get; set; }

        public PerGameChangeSettleInfoParameter BeforeObject { get; set; }

        public PerGameChangeSettleInfoParameter AfterObject { get; set; }

        public string Memo { get; set; }

        public DateTime CreateTime { get; set; }
    }



    public class SettleInfoChangedGameRecordViewModel
    {
        public long Id { get; set; }

        public long BetID { get; set; }

        public PerGameChangeSettleInfoBeforeAfterViewModel Before { get; set; }

        public PerGameChangeSettleInfoBeforeAfterViewModel After { get; set; }

        public string Memo { get; set; }

        public DateTime CreateTime { get; set; }
    }



}
