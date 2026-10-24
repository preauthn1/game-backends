using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes.ColumnValue;
using target.Models.CultureText;


namespace target.Models.Enums
{

    public enum EventRuleTypeEnum
    {

        /// <summary>
        /// 進入遊戲
        /// </summary>
        [ValueUnnecessary]
        WhenEnterGame = 21,


        /// <summary>
        /// 遊戲結算
        /// </summary>
        [ValueUnnecessary]
        WhenGameSettlement = 22,


        /// <summary>
        /// 當使用抽獎券
        /// </summary>
        [ValueUnnecessary]
        WhenUseRedEnvelopeTicket = 25,


        /// <summary>
        /// 當進行總派獎
        /// </summary>
        [ValueUnnecessary]
        WhenTriggerSettleRank = 27,


        /// <summary>
        /// 當進行轉盤抽獎
        /// </summary>
        [ValueUnnecessary]
        WhenStartLuckyWheel = 31,


        /// <summary>
        /// 當獲得紅包獎勵
        /// </summary>
        [ValueUnnecessary]
        WhenDrawRedEnvelope = 35,


        /// <summary>
        /// 當領取任務獎勵
        /// </summary>
        [ValueUnnecessary]
        WhenAcquireQuestReward = 41,


        /// <summary>
        /// 擲骰機率判定 (資料庫設定的數字單位是%，因此設 1 = 1%)
        /// </summary>
        [AllowFloat]
        RollDiceByPercentageRate = 61,


        /// <summary>
        /// 期間內 (建立帳號)
        /// </summary>
        [DateTimeRangeValue]
        Duration_AccountCreateTime = 71,


        /// <summary>
        /// 數值需大於等於 (遊玩局數) (所有遊戲加總)
        /// </summary>
        [AllowFloat]
        [AllowZero]
        ValueCompare_TotalGameRound_AllInOne_MustLargeThan = 101,


        /// <summary>
        /// 數值需大於等於 (遊玩局數) (單一筆遊戲)
        /// </summary>
        [AllowFloat]
        [AllowZero]
        ValueCompare_TotalGameRound_SingleGame_MustLargeThan = 102,



        /// <summary>
        /// 累計積分需大於等於
        /// </summary>
        [AllowFloat]
        [AllowMinus]
        ValueCompare_TotalPoint_AllInOne_MustLargeThan = 111,


        /// <summary>
        /// 累計積分需大於等於(單一遊戲)
        /// </summary>
        [AllowFloat]
        [AllowMinus]
        ValueCompare_TotalPoint_SingleGame_MustLargeThan = 112,



        /// <summary>
        /// 數值需大於等於 (單次彩金金額)
        /// </summary>
        [AllowFloat]
        [AllowZero]
        ValueCompare_SingleBetPayoutAmount_MustLargeThan = 121,



        /// <summary>
        /// 數值需大於等於 (單次投注金額)
        /// </summary>
        [AllowFloat]
        [AllowZero]
        ValueCompare_SingleBetBetAmount_MustLargeThan = 125,


        /// <summary>
        /// 每多少積分兌換一抽
        /// </summary>
        [AllowFloat]
        ValueCompare_ConsumePointPerDraw = 131,


        /// <summary>
        /// 已抽到多少紅包 (大於等於)
        /// </summary>
        [AllowFloat]
        ValueCompare_PlayerRedEnvelopeCount_MustLargeThan = 135,



        /// <summary>
        /// 指定檢查某一款遊戲
        /// </summary>
        [AllowZero]
        SpecificGameID = 201,


        /// <summary>
        /// 檢查領獎記錄 (篩選條件含gameID) 是否存在
        /// </summary>
        [ValueUnnecessary]
        Check_PrizeRecordNotExist_ByGameID = 501,



        /// <summary>
        /// 檢查領獎記錄 (PrizeBundleID) 是否存在
        /// </summary>
        [ValueUnnecessary]
        Check_PrizeRecordNotExist = 503,


      
        /// <summary>
        /// 檢查尚未被標記 [獎品已發完]
        /// </summary>
        [ValueUnnecessary]
        Check_EventSettinhKey_PrizeEmptyTag_NotExist = 509,


    }
}
