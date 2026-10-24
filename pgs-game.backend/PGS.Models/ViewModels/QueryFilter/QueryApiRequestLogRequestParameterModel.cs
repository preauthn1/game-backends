using target.Models.Paging;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class QueryApiRequestLogRequestParameterModel : QueryApiRequestRequestViewModel
    {
        public int? SpecialFilterType { get; set; }

        public string SearchID { get; set; }

    }

    public class QueryApiRequestRequestViewModel : PaginationParameter
    {
        public string TargetTableName { get; set; }

        public long? Id { get; set; }

        public string RelativeUrl { get; set; }

        public string RelativeUrlKeyword { get; set; }

        public string RequestBodyKeyword { get; set; }

        public string ResponseTextKeyword { get; set; }

        public string SearchAllColumnsKeyword { get; set; }

        public string RequestUrlKeyword { get; set; }

        public int? ResponseStatusCode { get; set; }

        public string UserIdentity { get; set; }

        public string IdentityKey { get; set; }

        public string IPAddress { get; set; }        

        public string VendorAPITrySearchMerchantID { get; set; }

        public int? MerchantID { get; set; }

        public int? PlayerID { get; set; }


        /// <summary>
        /// 特定table才有此欄位
        /// </summary>
        public string UniqueID { get; set; }

        public List<string> UniqueIDs { get; set; }

        public string DebugInfo { get; set; }

        public string DebugInfo2 { get; set; }

        public string DebugInfo3 { get; set; }

        public DateTime? CreateTimeBegin { get; set; }

        public DateTime? CreateTimeEnd { get; set; }

        public int? TopSize { get; set; }

        public string ExtraSpecificOrderBy { get; set; }

        public int? TimeoutSeconds { get; set; }


        /// <summary>
        /// 參考單號 (只有部分table才有這個欄位)
        /// </summary>
        public string ReferOrderNo { get; set; }

        public int? MappingTagValuesSearchFilter_SettingKey { get; set; }

        public string MappingTagValuesSearchFilter_SettingValue { get; set; }

    }


}
