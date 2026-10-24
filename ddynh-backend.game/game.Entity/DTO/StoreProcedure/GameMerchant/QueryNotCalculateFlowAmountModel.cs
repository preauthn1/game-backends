using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels
{


    public class QueryNotCalculateFlowAmountModel
    {
        public int BetRecordID { get; set; }

        public string BetRecordNo { get; set; }

        public int UserID { get; set; }

        public int GameID { get; set; }


        /// <summary>
        /// 請注意一定要轉換成 絕對值正數
        /// </summary>
        public long ValidBetAmount { get; set; }

               
        public long WinLossAmount { get; set; }

        /// <summary>
        /// 投注金額
        /// </summary>
        public long BetAmount { get; set; }

        /// <summary>
        /// 返回金額 (要算返回率RTP)
        /// </summary>
        public long PayoutAmount { get; set; }

    }



}
