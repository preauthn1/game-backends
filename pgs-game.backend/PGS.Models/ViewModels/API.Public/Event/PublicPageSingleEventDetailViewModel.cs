using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class PublicPageEnableEventsViewModel
    {
        public int PlayerID { get; set; }

        /// <summary>
        /// 是否有啟用迴圈刷新剩餘轉盤次數
        /// </summary>
        public bool IsEnableLuckyWheelRegularCheck { get; set; }

        public bool IsEnableRedEnvelope { get; set; }

        public bool IsEnableLuckyWheel { get; set; }

        public bool IsEnableQuestEvent { get; set; }

        public bool IsEnableRankingEvent { get; set; }

        /// <summary>
        /// 紅包活動說明頁
        /// </summary>
        public string RedEnvelopeEventDescriptionPageUrl { get; set; }

        public bool IsAnyQuestHint { get; set; }

        public int RemaningLuckyWheelTimes { get; set; }

        public int RemaningSecondsRankCompetition { get; set; }
    }

    public class PublicPageSingleEventDetailRequestModel
    {
        public int? EventType { get; set; }

        public int? EventID { get; set; }
    }



    public class PublicPageSingleEventDetailViewModel
    {
        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public int EventID { get; set; }

        public DateTime Begin { get; set; }

        public DateTime End { get; set; }

        public int RemaningSeconds { get; set; }


        public LuckyWheelEventDetailModel LuckyWheel { get; set; }

        public RankCompetitionEventDetailModel RankCompetition { get; set; }

        public List<PublicPageSupportGameItemViewModel> SupportGames { get; set; }
    }



    public class RankCompetitionEventDetailModel
    {
        public List<EventDropdownItemModel> EventsDropdown { get; set; }

        public decimal CurrentScore { get; set; }

        public int CurrentRank { get; set; }

        public int MaxRanking { get; set; }

        public bool IsMaxRankDisplay { get; set; }

        public string EventDescription { get; set; }

        public string EventBannerImageUrl { get; set; }

        public string DisplayCurrency { get; set; }

        public List<PlayerRankListViewModel> RankList { get; set; }
    }



    public class EventDropdownItemModel
    {
        public string Name { get; set; }

        public int EventID { get; set; }

        public int EventCompetiteType { get; set; }

        public string EventCompetiteTypeText { get; set; }

        public long RemainingSeconds_Begin { get; set; }

        public long RemainingSeconds_End { get; set; }

        public bool IsExpired { get; set; }

        public bool IsProgressing { get; set; }
    }




    public class PlayerRankListViewModel
    {
        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public decimal Score { get; set; }

        public int Rank { get; set; }

        public decimal PrizeAmount { get; set; }

        public bool IsPlayerSelf { get; set; }
    }



    public class LuckyWheelEventDetailModel
    {
        public int EventID { get; set; }

        public decimal PointPerDraw { get; set; }

        public decimal RemainingPoint { get; set; }

        public int RemainingDrawTimes { get; set; }

        public int MaxDrawTime { get; set; }

        public bool IsReachMaxDrawTime { get; set; }

        public bool IsHidePointDescription { get; set; }

        public List<LuckyWheelPrizeItemModel> PrizeItems { get; set; }
    }


    public class LuckyWheelPrizeItemModel
    {
        public int Id
        {
            get
            {
                return this.EventDrawPrizeItemID;
            }
        }

        public int EventDrawPrizeItemID { get; set; }

        public string DisplayName { get; set; }

        public decimal Amount { get; set; }

    }






}
