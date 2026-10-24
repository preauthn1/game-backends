using FW.Core.Helper.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure
{
    /// <summary>
    /// 实体卡 金币流通类型
    /// </summary>
    public enum CardSourceType
    {
        [EnumDescription("实体销售型实体卡充值")]
        Actual = 11101,
        [EnumDescription("虚拟推广型实体卡充值")]
        Virtual = 11102
    }
}
