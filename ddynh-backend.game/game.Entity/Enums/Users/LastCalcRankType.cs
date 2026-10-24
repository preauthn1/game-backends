
using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Users
{
  
    public enum LastCalcRankType : byte
    {
        /// <summary>
        /// 基本信息
        /// </summary>
        [EnumDescription("未完成")]
        undone = 0,
        /// <summary>
        /// 用户名
        /// </summary>
        [EnumDescription("完成")]
        done = 1,
    }
}
