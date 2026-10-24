using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
   
    public class HomePageSummaryViewModel
    {
        public int MerchantID { get; set; }
        public string MerchantName { get; set; }
        public int TotalOnlineUsers { get; set; }
        public decimal TotalMoneyCash { get; set; }
        public decimal TotalMoneyTransferIn { get; set; }
        public decimal TotalMoneyTransferOut { get; set; }
        public decimal TotalMoney { get; set; }
    }


    public class HomePageSummaryTodayInfoViewModel
    {       

        public string LastRefreshTimeText { get; set; }

        public int CurrentOnlinePlayers { get; set; }

        public int TotalPlayers_Today { get; set; }

        public int TotalPlayers_Yesterday { get; set; }


        /// <summary>
        /// KValue (e.g. 22K = 22000)
        /// </summary>
        public int TotalRound_KValue_Today { get; set; }

        public int TotalRound_KValue_Yesterday { get; set; }
    }


}
