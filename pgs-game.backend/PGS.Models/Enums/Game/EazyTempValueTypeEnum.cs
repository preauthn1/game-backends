using target.Models.Attributes;
using target.Models.CultureText;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace target.Models.Enums
{

    public enum EazyTempValueTypeEnum
    {         
        PlaceBetTooMuchTime = 1000,
        PlaceBetUnknowException = 1050,
        PgsGame_SettlementFail = 2500,

        SingleWallet_PlaceBet_CallbackProceeError = 3888,

        SendTGNotify = 5577,
        SendSkypeNotify = 5579,



        AGGBonusWinSkipProcess = 6101,
        AGGBonusWinRetryTriggerRecord = 6151,

        ATFishBetFail = 6331,
    }
}
