using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Game
{

    public enum SlotGameHistoryLogTypeEnum
    {

        [EnumDescription("轮轴资讯")]
        LogTypeREEL = 1,

        [EnumDescription("翻图案（三个一样结束）")]
        LogTypeOPEN = 2,

        [EnumDescription("累计出现金币统计")]
        LogTypeTOTALCOIN = 3,

        [EnumDescription("背景")]
        LogTypeBG = 4

    }
}
