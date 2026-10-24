using target.Models.Attributes;
using target.Models.CultureText;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace target.Models.Enums
{
   
    public enum GameListCustomizedSettingKeyEnum
    {
        LiveGameBetDetailMappingLanguageText = 1051,


        /// <summary>
        /// 完全忘記這是做啥用了 (AGG捕魚)
        /// </summary>
        AGGFish_FishIdMapping = 2221,


        /// <summary>
        /// 直版
        /// </summary>
        SupportedOrientation_Portrait = 3771,

        /// <summary>
        /// 橫版
        /// </summary>
        SupportedOrientation_Landscape = 3772,

        GameInfo_RTP = 4801,

        GameInfo_LinesWays = 4805,

        GameInfo_MaximumWin = 4808,

        /// <summary>
        /// 格子數: 3x5 
        /// </summary>
        GameInfo_GridFormat = 4809,


        /// <summary>
        /// 是否支援BuyFeature
        /// </summary>
        [SettingDefaultValue("0")]
        IsSupportBuyFeature = 5201,


        /// <summary>
        /// 是否支援 JP
        /// </summary>
        [SettingDefaultValue("0")]
        IsSupportJackpot = 5202,


        /// <summary>
        /// 是否支援 AGG freespin
        /// </summary>
        [SettingDefaultValue("0")]
        IsSupportAGGFreespin = 5261,


        /// <summary>
        /// 是否支援 遊戲內的JP (不可關閉)
        /// </summary>
        [SettingDefaultValue("0")]
        IsSupportUncloseJackpotFeature = 5275,


        /// <summary>
        /// 是否允許同一張注單號。多次押注扣款
        /// </summary>
        [SettingDefaultValue("0")]
        IsSupportRaiseProcessWhenPlaceBet = 5301,


        /// <summary>
        /// 禁止直接開啟此遊戲。強制要求需使用大廳或特殊入口
        /// </summary>
        [SettingDefaultValue("0")]
        IsForbiddenDirectLaunchGame = 5341,


        /// <summary>
        /// 勾選遊戲商全遊戲 不包含此遊戲
        /// </summary>
        DoNotIncludeThisGame_ForSeletedAll = 5388,


        /// <summary>
        /// GPI遊戲商的 launch入口設置
        /// ps. 不要設預設值。程式端會判斷空值來做特殊邏輯
        /// </summary>
        [SettingDefaultValue("")]
        GPI_GameLaunch_EntranceType = 6701,

    }
}
