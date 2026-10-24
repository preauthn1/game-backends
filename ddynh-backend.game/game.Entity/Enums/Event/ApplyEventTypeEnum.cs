using FW.Core.Helper.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Event
{  

    public enum ApplyEventTypeEnum
    {
        [EnumDescription("7日登录")]
        SeventDaysBonus = 1,

        [EnumDescription("首存优惠")]
        FirstChargeBonus = 51,

        [EnumDescription("绑定银行卡送金币")]
        BindBankCard = 71,

        [EnumDescription("绑定支付宝送金币")]
        BindAlipay = 72,

        [EnumDescription("轮盘_充值金额")]
        Roulette_Recharge = 101,

        [EnumDescription("轮盘_流水金额")]
        Roulette_FlowAmount = 102,
    }


}
