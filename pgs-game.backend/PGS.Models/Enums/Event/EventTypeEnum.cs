using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{

    public enum EventTypeEnum
    {       

        /// <summary>
        /// spin抽紅包
        /// </summary>
        [UniqueForMerchant]
        BetAmountDrawRedEnvelope = 32,

        /// <summary>
        /// RTP活動
        /// </summary>
        [UniqueForMerchantAndGame] 
        RTPEvent = 51,


        /// <summary>
        /// 任務型活動_獲得轉盤次數
        /// </summary>
        QuestEvent_GetLuckWheelTicket = 61,


        //任務型活動 獲得現金
        //QuestEvent_GetCash = 63,


        /// <summary>
        /// 排名活動
        /// </summary>
        [UniqueForMerchant] 
        RankCompetition = 81,


        /// <summary>
        /// AGG捕魚獎勵 (統一歸類成)
        /// </summary>
        DefaultFishBonusPrize = 87,


        /// <summary>
        /// 幸運轉盤
        /// </summary>
        [UniqueForMerchant] 
        LuckyWheel = 93,



        /// <summary>
        /// 空白活動
        /// </summary>
        EmptyEvent = 999,

    }


}
