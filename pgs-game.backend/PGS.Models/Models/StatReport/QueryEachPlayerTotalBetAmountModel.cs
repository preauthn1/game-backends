using target.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    public class QueryEachPlayerTotalBetAmountModel
    {
        public int PlayerID { get; set; }

        /// <summary>
        /// 如果是捕魚 會採用validAmount
        /// </summary>
        public decimal BetAmount { get; set; }

        public bool IsAnyFishingGame { get; set; }

    }
}
