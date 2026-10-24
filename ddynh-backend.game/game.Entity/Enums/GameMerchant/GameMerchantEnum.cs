using System;
using System.ComponentModel;
using FW.Core.Helper.Enum;

namespace game.Entity.Game
{
 

    public enum GameMerchantEnum
    {
        [EnumDescriptionAttribute("APL彩票")]
        [DocumentMemo("API:金錢單位是元", "API:輸贏是玩家視角")]
        [EnumSettingValue("")]
        APL = 15001,


        [EnumDescriptionAttribute("EGaming")]
        [DocumentMemo("API:金錢單位是分(*100倍)" , "API:輸贏是莊家視角")]
        [EnumSettingValue("")]
        EGaming = 15031,


        [EnumDescriptionAttribute("BG")]
        [DocumentMemo("API:金錢單位是元", "API:輸贏是玩家視角")]
        [EnumSettingValue("password,sn")]
        BG = 15032,


    }


}
