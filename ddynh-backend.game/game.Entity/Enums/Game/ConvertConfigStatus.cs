using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;

namespace game.Entity.Game
{
    /// <summary>
    /// 兑换配置状态
    /// </summary>
    public enum ConvertConfigStatus : byte
    {
        /// <summary>
        /// 关闭
        /// </summary>
        [EnumDescription("关闭")]
        Closed = 0,

        /// <summary>
        /// 开启
        /// </summary>
        [EnumDescription("开启")]
        Opened = 1
    }
}