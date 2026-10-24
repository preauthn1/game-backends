using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class ATFetchWagerPagingModel
    {
        public string TotalPage { get; set; }

        public string PageNo { get; set; }

        public string Num { get; set; }

        public List<ATFetchWagerItemModel> List { get; set; }
    }

    public class ATFetchWagerItemModel
    {
        public string Username { get; set; }

        public string Currency { get; set; }

        public string Gameid { get; set; }

        public string WagersId { get; set; }

        public long WagersBTime { get; set; }

        public long WagersETime { get; set; }

        public decimal BetAmount { get; set; }

        public decimal WinloseAmount { get; set; }

        public string SessionId { get; set; }

    }


}
