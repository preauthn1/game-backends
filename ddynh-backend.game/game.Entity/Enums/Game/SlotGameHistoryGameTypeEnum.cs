using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Game
{

    public enum SlotGameHistoryGameTypeEnum
    {

        [EnumDescription("主游戏")]
        HistoryTypeMAIN = 1,

        [EnumDescription("BonusGame")]
        HistoryTypeBONUS = 2,

        [EnumDescription("FreeGame")]
        HistoryTypeFREE = 3,

        [EnumDescription("单机JP")]
        HistoryTypeJPNOLINK = 4 ,

        [EnumDescription("连线JP")]
        HistoryTypeJPLINK = 5,

        [EnumDescription("重转")]
        HistoryTypeRESPIN = 6,

        [EnumDescription("Lightning結算")]
        HistoryTypeLIGHTNING = 7,

        [EnumDescription("金币统计结算")]
        HistoryTypeCoin = 8

    }
}
