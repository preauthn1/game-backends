using System;
using System.ComponentModel;
using FW.Core.Helper.Enum;

namespace game.Entity.Game
{
 

    public enum APLBetRecordStatusEnum
    {
        [EnumDescription("未开奖")]
        Pending = 0,

        [EnumDescription("已开奖")]
        Settlement = 1,

        [EnumDescription("取消单")]
        Cencel = 3,
    }


}
