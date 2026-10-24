using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Game
{
   
    /// <summary>
    /// 用户类型
    /// </summary>
    public enum FlagType : int
    {
        [EnumDescription("初级场")]
        level1 = 1,
        [EnumDescription("中级场")]
        level2 = 2,
        [EnumDescription("高级场")]
        level3 = 3,
        [EnumDescription("富豪场")]
        level4 = 4,
        [EnumDescription("皇家场")]
        level5 = 5
    }
    
}
