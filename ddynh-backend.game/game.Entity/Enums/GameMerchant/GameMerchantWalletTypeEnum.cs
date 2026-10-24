using System;
using FW.Core.Helper.Enum;
using game.Entity.Game;

namespace game.Entity.Gamegame.Entity.Game
{
 

    public enum GameMerchantWalletTypeEnum
    {
        [EnumDescriptionAttribute("中心钱包")]
        [EnumSettingValue("")]
        CenterWallet = 0,

        [EnumDescriptionAttribute("APL彩票錢包")]
        [EnumSettingValue("LO")]
        APL_Lottery = 15001001,
        

        [EnumDescriptionAttribute("APL捕鱼錢包")]
        [EnumSettingValue("FH")]
        APL_FishHunt = 15001002,


        [EnumDescriptionAttribute("APL电子游戏錢包")]
        [EnumSettingValue("APL_E")]
        APL_EGame = 15001003,
        

        [EnumDescriptionAttribute("EG娛樂城錢包")]
        [EnumSettingValue("EGaming")]
        EGaming_Casino = 15031001,


        [EnumDescriptionAttribute("BG游戏錢包")]
        [EnumSettingValue("BG")]
        BG_GameWallet = 15032001,
    }


}
