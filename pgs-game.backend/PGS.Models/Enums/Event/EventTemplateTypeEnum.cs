using target.Models.Attributes;
using System.ComponentModel;

namespace target.Models.Enums
{
    public enum EventTemplateTypeEnum
    {
        /// <summary>
        /// 投注抽紅包
        /// </summary>
        [ExtraIntValue((int)EventTypeEnum.BetAmountDrawRedEnvelope)]
        BetAmountDrawRedEnvelope = 32,


        /// <summary>
        /// 投注抽紅包 (前幾個紅包機率較高)
        /// </summary>
        [ExtraIntValue((int)EventTypeEnum.BetAmountDrawRedEnvelope)]
        BetAmountDrawRedEnvelope_LuckyDrawRateAdjustment = 33,


        /// <summary>
        /// RTP活動
        /// </summary>
        [ExtraIntValue((int)EventTypeEnum.RTPEvent)]
        RTPEvent = 51,


        [Description("一次性 (全遊戲合併)")]
        [ExtraIntValue((int)EventTypeEnum.QuestEvent_GetLuckWheelTicket)]
        QuestEvent_AllInOne_GetLuckWheelTicket_10_Quest = 61,


        [Description("一次性 (遊戲個別統計)")]
        [ExtraIntValue((int)EventTypeEnum.QuestEvent_GetLuckWheelTicket)]
        QuestEvent_SpecificGame_GetLuckWheelTicket_10_Quest = 62,


        [Description("每日任務 (全遊戲合併)")]
        [ExtraIntValue((int)EventTypeEnum.QuestEvent_GetLuckWheelTicket)]
        QuestEvent_DailyQuest_AllInOne_GetLuckWheelTicket_10_Quest = 63,


        [Description("每日任務 (遊戲個別統計)")]
        [ExtraIntValue((int)EventTypeEnum.QuestEvent_GetLuckWheelTicket)]
        QuestEvent_DailyQuest_SpecificGame_GetLuckWheelTicket_10_Quest = 64,


        /// <summary>
        /// 排名活動
        /// </summary>
        [ExtraIntValue((int)EventTypeEnum.RankCompetition)]
        RankCompetition = 81,



        [ExtraIntValue((int)EventTypeEnum.LuckyWheel)]
        LuckyWheel = 93,



        [ExtraIntValue((int)EventTypeEnum.LuckyWheel)]
        LuckyWheelWihtoutPointCounter = 94,

        /// <summary>
        /// 空白活動
        /// </summary>
        [ExtraIntValue((int)EventTypeEnum.EmptyEvent)]
        EmptyEvent = 999,
    }




}
