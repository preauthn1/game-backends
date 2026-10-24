using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class QuerySingleWalletMerchantApiRequestLogApiRequestParameterModel : QuerySingleWalletMerchantApiRequestLogRequestViewModel
    {

        /// <summary>
        /// 參考 SpecialFilterTypeOfSwApiRequestLog
        /// </summary>
        public int? SpecialFilterType { get; set; }

    }



    public class QuerySingleWalletMerchantApiRequestLogRequestViewModel : PaginationParameter
    {

        public string UniqueID { get; set; }

        public string RequestID { get; set; }

        public string Category { get; set; }

        public int? MerchantID { get; set; }

        public int? PlayerID { get; set; }

        public string SubCategory { get; set; }

        public int? ResponseStatusCode { get; set; }
        
        public string RequestContentKeyword { get; set; }

        public string ResponseTextKeyword { get; set; }

        public string RequestUrlKeyword { get; set; }

        public string ReferenceTransactionNo { get; set; }

        public DateTime? CreateTimeBegin { get; set; }

        public DateTime? CreateTimeEnd { get; set; }

        public string PlayerName { get; set; }

        public int? ResponseTimeLargeThan { get; set; }

        public bool IsErrorCodeOrNot200Response { get; set; }


        public int? TopSize { get; set; }

        public string ExtraSpecificOrderBy { get; set; }

        public int? TimeoutSeconds { get; set; }

        public string DebugInfo2 { get; set; }
    }


}
