using FW.Core.Helper.Enum;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Event
{  

    public enum EventBonusTypeEnum
    {
        [EnumDescription("赠送金币")]
        [Description("(元)")]
        [ToCash]
        CashMoney = 1,

        [EnumDescription("存款回馈百分比")]
        [Description("(%)")]
        PercentageFromChargeAmount = 2,


        [EnumDescription("实体物品")]
        [Description("个")]
        SubstanceGift = 11,
    }


}
