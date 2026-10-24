using System.ComponentModel;

namespace target.Models.Enums
{
    public enum OneTimesJobTypeEnum
    {
        Event_RTPEvent_ResetAllPlayer = 2031,

        Event_RankingEvent_SendRankingPrize = 2037,

        BetRecord_CheckCancelNotFoundAgain = 3071,

        BetRecord_CheckAginWagerIsCancel = 3078,

        BetRecord_RefundWagerPlacebetException = 3085,


        Execute_TargetStoreProcedure = 5252,

        /// <summary>
        /// 如果是這種發通知類型
        /// 請把訊息填到 SettingJson 欄位中
        /// </summary>
        SendMessageToNotifyChannel = 9001,
    }

}
