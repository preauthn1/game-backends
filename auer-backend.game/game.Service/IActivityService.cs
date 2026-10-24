using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Users;

namespace game.Service
{
    /// <summary>
    /// 活動相關
    /// </summary>
    public interface IActivityService
    {
        List<EventSetting> GetEventList(DateTime? beginTime, DateTime? endTime, int? status, string eventName);

        bool ModifyEvent(EventSetting model, ref string msg);

        List<EventLog> GetUserEventAccessList(DateTime? beginTime, DateTime? endTime, int? userID, string loginName = null);

        PagedList<EventDetailLog> GetUserEventAccessDetail(DateTime? beginTime, DateTime? endTime, int? userID, int pageIndex, int pageSize);

        bool ResetGrantTime(int eventID, int userID, string updater, int resetTime, ref string message);

        List<EventContest> GetGamesEventMGMT(int? contestID, DateTime? beginTime, DateTime? endTime);

        List<EventBonus> GetEventBonus(int? contestID);

        List<EventReward> GetEventReward(int? contestID);

        List<EventRank> GetContestRank(int? contestID, DateTime? rankDate, int rankType);

        bool UpdateEventContest(EventContest model, ref int contestID, ref string message);
        bool UpdateEventBonus(EventBonus model, ref string message);
        bool UpdateEventReward(EventReward model, ref string message);
        bool DeleteEventBonus(int id, ref string message);
        bool DeleteEventReward(int id, ref string message);

        List<SevenDaysLoginBonus> GetSevenDaysLoginBonus(int? platformID, int? areaID);

        bool AddSevenDaysLoginBonus(SevenDaysLoginBonus model, ref string msg);
        bool DelSevenDaysLoginBonus(SevenDaysLoginBonus model, ref string msg);

    }
}
