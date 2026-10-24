using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Game
{

    public enum Status
    {
        [EnumDescriptionAttribute("关")]
        Close = 0,
        [EnumDescriptionAttribute("开")]
        Open = 1,
        [EnumDescriptionAttribute("刪除")]
        Delete = -1
    }
}
