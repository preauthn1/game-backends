using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Game
{
 

    public enum GameMerchantJobTypeEnum
    {
       
        [EnumDescriptionAttribute("取得投注紀錄")]
        FetchBetRecordJob = 1,

        [EnumDescriptionAttribute("對帳昨日數據")]
        CheckYesterrdayDataIsCorrect = 2,

        [EnumDescriptionAttribute("检查未结算排程是否已结算")]
        CheckUnsettleBetRecords = 3,

        [EnumDescriptionAttribute("計算有效流水並更新稽核流水")]
        DataTransform_CalculateFlowAmount = 51,

        [EnumDescriptionAttribute("產生GameLog資料")]
        DataTransform_ConvertToGameLog = 52,

        [EnumDescriptionAttribute("後端小工具")]
        BackendUtility = 999,
    }


}
