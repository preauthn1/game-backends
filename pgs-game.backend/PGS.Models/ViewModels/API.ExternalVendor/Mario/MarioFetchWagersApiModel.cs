using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class FunkyFetchWagersApiModel
    {
       
        public bool IsRequiredNextPage { get; set; }

    }


    public class MarioFetchWagersApiModel : MarioAPICommonResponse
    {
        public List<MarioFetchWagersItemModel> userGameLogDTO { get; set; }

        public int currentRecords { get; set; }

        public int currentPage { get; set; }

        public int totalRecords { get; set; }

        public int totalPage { get; set; }
    }


    public class MarioFetchWagersItemModel
    {
        public string logId { get; set; }
        public string loginId { get; set; }
        public string gameRoundId { get; set; }
        public string gameCode { get; set; }
        public string gameName { get; set; }
        public string createdDateStr { get; set; }
        public string createdDate { get; set; }
        public string currencyCode { get; set; }
        public decimal betAmount { get; set; }
        public decimal winAmount { get; set; }
        public string jackpotName { get; set; }
        public decimal jackpotAmount { get; set; }
        public decimal totalWinLoseAmount { get; set; }
    }


}
