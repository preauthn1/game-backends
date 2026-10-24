using target.Models.Attributes;
using target.Models.Attributes.ColumnValue;
using target.Models.CultureText;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;


namespace target.Models.Enums
{

    /// <summary>
    /// 如果有新增編號 請閱讀以下
    /// 此處有定義哪些類型是一般商戶端/代理 可以查看的 EventDefinitionHelper
    /// 或可以標記 SkipInsertPrizeRecordWhenProcess
    /// </summary>
    public enum EventPrizeTypeEnum
    {
        Default = 0,

        /// <summary>
        /// 轉盤抽獎券
        /// </summary>
        //[LoopSendPrize]
        //Ticket_Roulette = 51,


        /// <summary>
        /// 獲得抽獎次數 / 抽獎用積分 (抽獎券的概念)
        /// </summary>
        [SkipInsertPrizeRecordWhenProcess]
        SendDrawPointOfLuckyWheelEvent = 31,

        [ExtraIntValue((int)GameVendorTypeEnum.AT)]
        FishGame_GameBonus = 41,

        GameVendor_EventBonus = 42,

        /// <summary>
        /// 紅包抽獎券
        /// </summary>
        [LoopSendPrize]
        Ticket_RedEnvelope = 52,


        /// <summary>
        /// 按照比例抽現金(紅包) 使用對應好的機率抽獎箱設置
        /// </summary>
        [SkipInsertPrizeRecordWhenProcess]
        DrawPrize_ByDrawPrizeSetting = 63,


        /// <summary>
        /// 抽現金 (使用對應好的機率抽獎箱設置)
        /// </summary>
        [SkipInsertPrizeRecordWhenProcess]
        DrawCash_ByDrawPrizeSetting = 64,


        /// <summary>
        /// 使用 對應好的多重獎項設置 (排行榜活動)
        /// </summary>
        [SkipInsertPrizeRecordWhenProcess]
        UseSelectedMultiplePrizeSetting_RankBetAmount = 65,


        UpdatePlayerRTP = 71,


        [ValueUnnecessary]
        ResetPlayerAllGameRTP = 79,


        [ValueUnnecessary]
        [SkipInsertPrizeRecordWhenProcess]
        WriteEventCounterAllInOne = 151,


        [ValueUnnecessary]
        [SkipInsertPrizeRecordWhenProcess]
        WriteEventCounterByGameID = 153,



        [ValueUnnecessary]
        [SkipInsertPrizeRecordWhenProcess]
        WriteEventCounterByGameID_DailyCounter = 157,


        /// <summary>
        /// 每日型的 EventCounter
        /// </summary>
        [ValueUnnecessary]
        [SkipInsertPrizeRecordWhenProcess]
        WriteEventCounterAllInOne_DailyCounter = 158,



        [ValueUnnecessary]
        ExchangeTicketByPoint = 175,



        /// <summary>
        /// 調整用戶抽中紅包的機率
        /// </summary>
        [ValidateValueRange(1, 100)]
        UpdateEventPlayerTag_LuckyDrawRateAdjustment = 222,



        //IDG的這些獎勵類型 部分會有對應的 EventSubTypeEnum (如果報表預期要分開的話)
        //GameVendorSingleWalletCommonLogicProcessService 有一段switch case要調整


        /// <summary>
        /// AGG虛寶卡
        /// </summary>
        [ExtraIntValue((int)GameVendorTypeEnum.AGG)]
        AGG_ItemCard = 40,

        /// <summary>
        /// AGG freespin
        /// </summary>
        [ExtraIntValue((int)GameVendorTypeEnum.AGG)]
        AGG_Freespin = 650,

        [ExtraIntValue((int)GameVendorTypeEnum.AGG)]
        AGG_CashCard = 701,

        [ExtraIntValue((int)GameVendorTypeEnum.AGG)]
        AGG_GiftCode = 702,

        [ExtraIntValue((int)GameVendorTypeEnum.AGG)]
        AGG_BonusMission = 703,

        [ExtraIntValue((int)GameVendorTypeEnum.AGG)]
        AGG_Ranking = 704,

        [ExtraIntValue((int)GameVendorTypeEnum.AGG)]
        AGG_Raffle = 705,

        [ExtraIntValue((int)GameVendorTypeEnum.AGG)]
        AGG_VIP = 706,

        [ExtraIntValue((int)GameVendorTypeEnum.AGG)]
        AGG_Roulette = 707,

        [ExtraIntValue((int)GameVendorTypeEnum.AGG)]
        AGG_OtherBonus = 709,

        /// <summary>
        /// 若有領過此獎 在很多檢查判斷上 會直接略過 視為活動以參與完成
        /// (加快查詢流程用)
        /// </summary>
        [ValueUnnecessary]
        EventFinish = 999,

    }


}
