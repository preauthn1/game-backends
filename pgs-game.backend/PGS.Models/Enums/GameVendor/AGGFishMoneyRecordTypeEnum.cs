using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;


namespace target.Models.Enums
{


    public enum AGGFishMoneyRecordTypeEnum
    {
        DepositBalanceToFishRoom = 101,

        WithdrawalBalanceFromFishRoom = 102,

        Exchange_ToFishWallet = 103,

        Exchange_ToPlatformWallet = 104,

        LoginBonus = 105,

        LevelUpBonus = 106,

        Exchange_ToFishWallet_Fail = 107,

        //Exchange_ToFishWallet_Fail = 107,

        RefundExchangeAmount = 109,

        Bullet = 200
    }
}
