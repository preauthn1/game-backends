using System;
using System.ComponentModel;
using FW.Core.Helper.Enum;

namespace game.Entity.Game
{
 

    public enum BGBetRecordStatusEnum
    {

        [EnumDescription("注单不存在(仅对免转模式有效)")]
        NotFound = 0,
        
        [EnumDescription("新注单")]
        NotSettlement = 1,

        [EnumDescription("正常结算")]
        Settlement_Win = 2,

        [EnumDescription("正常结算")]
        Settlement_Draw = 3,

        [EnumDescription("正常结算")]
        Settlement_Loss = 4,

        [EnumDescription("用户取消")]
        UserCancel = 5,

        [EnumDescription("")]
        Expired = 6,

        [EnumDescription("系统取消")]
        SystemCancel = 7,


    }


}
