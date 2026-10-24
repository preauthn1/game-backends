using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryExternalApiRequestLogApiRequestParameterModel : QueryExternalApiRequestLogRequestViewModel
    {
        public int? SpecialFilterType { get; set; }

    }

    public class QueryExternalApiRequestLogRequestViewModel : PaginationParameter
    {

        public long? Id { get; set; }

        public List<long> IDs { get; set; }

        public string RequestID { get; set; }

        public string Category { get; set; }

        public string SubCategory { get; set; }

        public int? ResponseStatusCode { get; set; }

        public string ResponseTextKeyword { get; set; }

        public string RequestContentKeyword { get; set; }

        public string RequestUrlKeyword { get; set; }


        public string DebugInfo2 { get; set; }


        public DateTime? CreateTimeBegin { get; set; }

        public DateTime? CreateTimeEnd { get; set; }

        public int? ResponseTimeLargeThan { get; set; }


        //以下欄位是新版table v2才有
        //以下欄位是新版table v2才有
        //以下欄位是新版table v2才有
        //以下欄位是新版table v2才有


        public string UniqueID { get; set; }

        public List<string> UniqueIDs { get; set; }

        public string ReferOrderNo { get; set; }

        public int? TopSize { get; set; }

        public int? PlayerID { get; set; }

        public int? MerchantID { get; set; }
    }


}
