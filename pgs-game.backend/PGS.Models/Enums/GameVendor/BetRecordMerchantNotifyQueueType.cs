using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;


namespace target.Models.Enums
{
    /// <summary>
    /// 補救排程 queue類型
    /// </summary>
    public enum BetRecordMerchantNotifyQueueTypeEnum
    {
        Cancel = 1000,

        /// <summary>
        /// 投注結算-一般投注
        /// </summary>
        Settle = 2000,

        /// <summary>
        /// 投注結算-獎勵/免費遊戲
        /// </summary>
        BonusSettle = 2100,



        SettleChange = 3550,




        SendEventBonus = 4777,
    }


    public enum GameVendorCallbackTypeEnum
    {
        /// <summary>
        /// 投注
        /// </summary>
        PlaceBet = 1,

        /// <summary>
        /// 投注結算
        /// </summary>
        Settle = 20,

        /// <summary>
        /// 取消
        /// </summary>
        Cancel = 30,
    }

}
