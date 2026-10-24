using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class SettlementConvertRequiredInfoModel
    {

        /// <summary>
        /// 三方注單號
        /// </summary>
        public string BetRecordNo { get; set; }

        /// <summary>
        /// PGS注單號
        /// </summary>
        public string BetRecordNoPGS { get; set; }


        public decimal BetAmount { get; set; }


        public decimal BetGamePoint { get; set; }


        /// <summary>
        /// 更新前金額 (法幣)
        /// </summary>
        public decimal BeforeAmount { get; set; }


        /// <summary>
        /// 更新前金額 (遊戲幣)
        /// </summary>
        public decimal BeforeGamePoint { get; set; }


        /// <summary>
        /// 更新後金額 (法幣)
        /// </summary>
        public decimal AfterAmount { get; set; }


        /// <summary>
        /// 更新後金額 (遊戲幣)
        /// </summary>
        public decimal AfterGamePoint { get; set; }



        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public string Currency { get; set; }

        public DateTime? ThirdSettleDate { get; set; }




    }





}
