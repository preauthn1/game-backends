using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Enums.ThirdPartyPay
{
    /// <summary>
    /// 訂單狀態
    /// </summary>
    public enum SpeedPayOrderType: byte
    {
        Check = 0,
        Buy = 1,
        Sell = 2
    }

    /// <summary>
    /// 訂單回覆Code
    /// </summary>
    public enum SpeedPayReturnCode:int
    {
        Succese = 0
    }

    /// <summary>
    /// 異步回調狀態
    /// </summary>
    public enum SpeedPayResponseStatus:byte
    {
        CreateOrder = 0,
        Payed = 1,
        Finish = 2,
        ComplaintCancel = 3,
        NomalCancel = 4,
        Complaining = 5,
        VerificationFail = 9
    }
}
