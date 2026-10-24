using target.Models.Attributes;
using target.Models.CultureText;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace target.Models.Enums
{
    public enum SingleWalletPlatformApiLogicTypeEnum
    {    
        Default = 0,

        W88 = 1011,

        QTech = 1022,

        OPS = 1023,

        THub = 1024,

        Notix = 1025,

        /// <summary>
        /// 不做任何扣款/加值 處理
        /// </summary>
        W88_SkipDebitOrCredit = 9999
    }
}
