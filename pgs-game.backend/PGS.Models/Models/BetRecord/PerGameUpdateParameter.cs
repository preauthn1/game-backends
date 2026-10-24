using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models.Models
{
    public class PerGameChangeSettleInfoParameter
    {
        /// <summary>
        /// 派彩金額 - 投注金額 (請自己算)
        /// </summary>
        public decimal UpdateAmount { get; set; }

        public decimal BetAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal ValidAmount { get; set; }

        public decimal TaxAmount { get; set; }

    }


    public class PerGameChangeSettleInfoBeforeAfterViewModel
    {
        /// <summary>
        /// 派彩金額 - 投注金額 (請自己算)
        /// </summary>
        public decimal WinLossAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal ValidAmount { get; set; }

        public decimal TaxAmount { get; set; }

    }



}
