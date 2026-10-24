using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class PublicPageEventQuestRequestModel
    {

    }


    public class PublicPageEventQuestReceiveRequestModel
    {
        public int EventID { get; set; }

        public int EventPrizeBundleID { get; set; }

        public long Ts { get; set; }

        public string SignKey { get; set; }
    }


    public class PublicPageEventQuestViewModel
    {
        public bool IsAnyDailyQuest { get; set; }

        public int DailyQuestRemaningSeconds { get; set; }

        public List<PublicPageEventQuestItemViewModel> QuestItems { get; set; }
    }


    public class PublicPageEventQuestItemViewModel
    {
        public int EventID { get; set; }

        public int EventType { get; set; }

        public int EventPrizeBundleID { get; set; }

        public int SpecificGameID { get; set; }

        /// <summary>
        /// 主要需要比對值的欄位
        /// </summary>
        public int MainRuleType { get; set; }

        public string SpecificGameName { get; set; }

        public string SpecificGameImageUrl { get; set; }

        public bool IsSpecificSlotsGame { get; set; }

        public bool IsSpecificPokerGame { get; set; }

        public string QuestDescription { get; set; }

        public decimal PrizeAmountOrQuantity { get; set; }

        public string i18nKey_PrizeName { get; set; }

        public string i18nKey_RuleName { get; set; }

        public string PrizeName { get; set; }

        public string PrizeImageUrl { get; set; }

        public decimal MustLargeThanValue { get; set; }

        public decimal CurrentValue { get; set; }

        /// <summary>
        /// 剩餘時間 (天)
        /// </summary>
        public int RemainingExpiredDays { get; set; }

        /// <summary>
        /// 剩餘時間 (小時)
        /// </summary>
        public int RemainingExpiredHours { get; set; }


        public bool IsDailyEvent { get; set; }


        public DateTime EventBeginTime { get; set; }


        public DateTime EventEndTime { get; set; }


        /// <summary>
        /// 是否已領取
        /// </summary>
        public bool IsCompleted { get; set; }


        /// <summary>
        /// 是否可領取
        /// </summary>
        public bool IsReadyToReceive { get; set; }

        /// <summary>
        /// 是否通用規則不符合 (會直接禁止參加)
        /// </summary>
        public bool IsForbiddenByAlwaysRule { get; set; }

        public List<PublicPageSupportGameItemViewModel> QuestSupportGames { get; set; }



    }



}
