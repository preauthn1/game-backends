using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class CheckPlayerMoneyRequestModel
    {
        public int PlayerID { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }
    }

    public class CheckPlayerMoneyResultViewModel
    {
        public int PlayerID { get; set; }

        public bool IsAmountMatched { get; set; }

        public List<CheckPlayerMoneyGameResultViewModel> GameResults { get; set; }
    }

    public class CheckPlayerMoneyGameResultViewModel
    {
        public int GameID { get; set; }

        public decimal PerGameAmount { get; set; }

        public decimal TransactionAmount { get; set; }

        public bool IsAmountMatched { get; set; }
    }
}
