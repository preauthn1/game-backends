using System;
using System.ComponentModel.DataAnnotations;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Area;

namespace game.Entity.Game
{

    /// <summary>
    /// 游戏消息状态
    /// </summary>
    public enum GameMessageStatus : byte
    {
        [EnumDescription("启用")]
        Enabled = 1,
        [EnumDescription("禁用")]
        Disabled = 0,
    }
}
