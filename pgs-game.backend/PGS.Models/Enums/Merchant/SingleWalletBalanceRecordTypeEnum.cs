using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{
    
    public enum SingleWalletBalanceRecordTypeEnum
    {
        BeforePlaceBet = 1011,

        AfterPlaceBet = 1015,

        AfterSettlement = 2011,

        RetryFail = 9901,
    }
}
