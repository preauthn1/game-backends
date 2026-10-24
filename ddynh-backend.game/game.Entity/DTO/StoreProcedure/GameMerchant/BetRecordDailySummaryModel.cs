using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels
{


    public class BetRecordDailySummaryModel
    {

        public int TotalCount { get; set; }

        public int TotalPayout { get; set; }

        public int TotalBetAmount { get; set; }

        public int TotalWinLoss { get; set; }

    }



}
