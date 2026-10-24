using Newtonsoft.Json;
using target.Models.Paging;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class UFA5MinutesReportRequestModel : PaginationParameter , IQueryWagerRequest
    {
        public string MerchantSecretKey { get; set; }

        public int MerchantID { get; set; }

        public string PlayerName { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        [SwaggerIgnore]
        [SwaggerIgnoreOutputWhenPagedList]
        public int? TagID { get; set; }

    }



    public interface IQueryWagerRequest 
    {
        public int MerchantID { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string PlayerName { get; set; }

        public int? TagID { get; set; }
    }


    public class UFA5MinutesReportViewModel
    {
        public string BatchID { get; set; }

        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public int GameID { get; set; }

        public string GameName { get; set; }

        public int GameType { get; set; }

        [JsonIgnore]
        public long TimeID { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int TotalRound { get; set; }

        public decimal TotalBetAmount { get; set; }

        public decimal TotalValidAmount { get; set; }

        public decimal TotalPayoutAmount { get; set; }

        public decimal TotalTaxAmount { get; set; }

        public decimal TotalWinLossAmount { get; set; }

        public DateTime LastGameTime { get; set; }

    }


    public class UFA5MinutesReportSqlQueryFilterParameter : PaginationParameter
    {
        public int MerchantID { get; set; }

        public List<int> PlayerIDs { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }
    }


    public class UFA5MinutesReportSqlQueryModel
    {
        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public int GameID { get; set; }

        public int GameType { get; set; }

        public long TimeID { get; set; }

        public int TotalRound { get; set; }

        public decimal TotalBetAmount { get; set; }

        public decimal TotalValidAmount { get; set; }

        public decimal TotalPayoutAmount { get; set; }

        public decimal TotalTaxAmount { get; set; }

        public decimal TotalWinLossAmount { get; set; }

        public DateTime LastGameTime { get; set; }
    }

}
