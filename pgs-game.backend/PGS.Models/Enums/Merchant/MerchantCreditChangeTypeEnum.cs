using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;


namespace target.Models.Enums
{

    public enum MerchantCreditChangeTypeEnum
    {
        PlayerTransferToGameWallet = 1051,

        PlayerTransferBackToPlatformWallet = 1057,

        /// <summary>
        /// 後台加值 額度增加
        /// </summary>
        Agent_UpdateCredit = 7777,

        /// <summary>
        /// 超級管理員-調整額度
        /// </summary>
        SuperAdmin_UpdateCredit = 9999,
    }

}
