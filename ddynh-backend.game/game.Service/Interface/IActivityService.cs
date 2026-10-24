using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity;
using game.Entity.Event;
using game.Entity.Users;

namespace game.Service
{
    /// <summary>
    /// 活動相關
    /// </summary>
    public interface IActivityService
    {
        PagedList<EventSettingEntity> GetEventSettingList(QueryEventSettingRequestViewModel filter);

        EventSettingEntity GetEventSettingById(int id);

        CRUDResultModel CreateEventSetting(EventSettingEntity model);

        CRUDResultModel ModifyEvent(EventSettingEntity model);

        CRUDResultModel UpdateStatus(CommonUpdateStatusRequestModel model);

        List<EventLog> GetUserEventAccessList(DateTime? beginTime, DateTime? endTime, int? userID, string loginName = null);

        PagedList<EventDetailLog> GetUserEventAccessDetail(DateTime? beginTime, DateTime? endTime, int? userID, int pageIndex, int pageSize);


        List<EventContest> GetGamesEventMGMT(int? contestID, DateTime? beginTime, DateTime? endTime);

        List<EventBonus> GetEventBonus(int? contestID);

        List<EventReward> GetEventReward(int? contestID);

        List<EventRank> GetContestRank(int? contestID, DateTime? rankDate, int rankType);

        bool UpdateEventContest(EventContest model, ref int contestID, ref string message);
        bool UpdateEventBonus(EventBonus model, ref string message);
        bool UpdateEventReward(EventReward model, ref string message);
        bool DeleteEventBonus(int id, ref string message);
        bool DeleteEventReward(int id, ref string message);

    }
}
