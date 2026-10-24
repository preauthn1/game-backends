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

    public enum EventAwardTypeEnum
    {
        [EnumDescription("金币")]
        Cash = 1,

        [EnumDescription("其他")]
        Other = 2,

        [EnumDescription("铭谢惠顾")]
        Missed = 3
    }
}
