using System;
using System.ComponentModel;
using FW.Core.Helper.Enum;

namespace game.Entity.Game
{
 

    public enum GameMerchantStatusEnum
    {
        [EnumDescription("已关闭")]
        Inactive = 0,

        [EnumDescription("启用")]
        Active = 1,

        [EnumDescription("例行维护中")]
        Maintain = 7,

        [EnumDescription("永久隐藏")]
        PermanentInvisible = 999,

    }


}
