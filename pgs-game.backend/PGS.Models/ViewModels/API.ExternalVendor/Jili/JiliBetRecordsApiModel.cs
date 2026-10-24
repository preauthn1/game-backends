using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class JiliBetRecordsApiResponseModel : JiliAPICommonResponse
    {
        public JiliBetRecordsApiResponseDataModel Data { get; set; }
    }

    public class JiliBetRecordsApiResponseDataModel
    {
        public List<JiliBetRecordsApiItemModel> Result { get; set; }

        public JiliBetRecordsApiPagination Pagination { get; set; }
    }

    public class JiliBetRecordsApiPagination
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageLimit { get; set; }
        public int TotalNumber { get; set; }
    }

    public class JiliBetRecordsApiItemModel
    {
        public string Account { get; set; }
        public string WagersId { get; set; }
        public string GameId { get; set; }
        public string WagersTime { get; set; }
        public decimal BetAmount { get; set; }
        public string PayoffTime { get; set; }
        public decimal PayoffAmount { get; set; }
        public int Status { get; set; }
        public string SettlementTime { get; set; }
        public int GameCategoryId { get; set; }
        public int VersionKey { get; set; }
        public int Type { get; set; }
        public string AgentId { get; set; }
        public object RoundIndex { get; set; }
        public decimal Turnover { get; set; }
    }
   

}
