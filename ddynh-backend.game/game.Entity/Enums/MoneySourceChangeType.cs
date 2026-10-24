using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Money
{

    /// <summary>
    /// 金币变化方式
    /// </summary>
    public enum MoneySourceChangeType : byte
    {
        [EnumDescription("流入")]
        InFlow = 0,
        [EnumDescription("流出")]
        OutFlow = 1,
        [EnumDescription("流通")]
        Flow = 2

        //若帶入999 代表不篩選

    }
}
