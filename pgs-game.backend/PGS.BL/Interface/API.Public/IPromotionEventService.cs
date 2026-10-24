using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IPromotionEventService
    {
        PublicPageEventViewModel GetAllPublicEventInfo(int playerID);

        PublicPageSingleEventDetailViewModel GetSingleEventDetail(int playerID, PublicPageSingleEventDetailRequestModel filter);

        PublicPageEnableEventsViewModel GetLobbyEnableEventsAndMessageHint(int playerID);

        LuckyDrawResultViewModel RedEnvelopeOpen(int playerID, EventLuckyDrawRequestModel model);

        LuckyDrawResultViewModel LuckyWheelStartDraw(int playerID, EventLuckyDrawRequestModel model);

        PublicPageEventDescriptionViewModel GetEventDescription(int playerID, PublicPageEventDescriptionRequestViewModel filter);

        PublicApiEventPrizeRecordModel GetPrizeRecords(int playerID, PublicApiEventPrizeRecordRequestModel model);

        PublicPageEventQuestViewModel GetAvailableQuest(int playerID, PublicPageEventQuestRequestModel filter);

        void ReceiveQuestReward(int playerID, PublicPageEventQuestReceiveRequestModel model);
    }
}
