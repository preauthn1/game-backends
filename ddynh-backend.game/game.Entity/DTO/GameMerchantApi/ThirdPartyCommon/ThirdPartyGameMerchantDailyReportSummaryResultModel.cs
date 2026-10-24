using System;

namespace game.Entity.ViewModels
{
    public class ThirdPartyGameMerchantDailyReportSummaryResultModel
    {

        /// <summary>
        /// 總投注筆數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 總投注額 (除以100的單位)
        /// </summary>
        public decimal TotalBetAmount { get; set; }

        /// <summary>
        /// 總有效投注額 (除以100的單位)
        /// </summary>
        public decimal TotalValidBetAmount { get; set; }
        

        /// <summary>
        /// 總派彩 (除以100的單位)
        /// </summary>
        public decimal TotalPayoutAmount { get; set; }
        

        /// <summary>
        /// 總輸贏 (除以100的單位)
        /// </summary>
        public decimal TotalWinLossAmount { get; set; }



        public long TotalBetAmount_X_100
        { 
            get
            {
                return (long)(TotalBetAmount * 100);
            }
        }


        /// <summary>
        /// 總有效投注額
        /// </summary>
        public long TotalValidBetAmount_X_100
        {
            get
            {
                return (long)(TotalValidBetAmount * 100);
            }
        }

        public long TotalPayoutAmount_X_100
        {
            get
            {
                return (long)(TotalPayoutAmount * 100);
            }
        }


        /// <summary>
        /// 總輸贏(總派彩)
        /// </summary>
        public long TotalWinLossAmount_X_100
        {
            get
            {
                return (long)(TotalWinLossAmount * 100);
            }
        }




    }

}
