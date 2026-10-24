using System;
using System.ComponentModel;
using FW.Core.Helper.Enum;

namespace game.Entity.Game
{
    public enum BGModuleEnum
    {
        [EnumDescription("平台")]
        Platform = 0,

        [EnumDescription("彩票模块 ")]
        Lottery = 1,

        [EnumDescription("视讯模块")]
        LiveCasino =2,

        [EnumDescription("体育模块")]
        Sportbook = 3,

        [EnumDescription("游戏模块")]
        GameModule = 4,
    }

}
