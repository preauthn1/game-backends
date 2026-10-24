using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Users
{
    /// <summary>
    /// 稽核流水的狀態 
    /// </summary>
    public enum FlowLimitStatus 
    {
      
        [EnumDescription("未通過")]
        NotPass = 1,


        [EnumDescription("通過")]
        Pass = 2,
        

        [EnumDescription("删除")]
        Delete = 999

    }
}
