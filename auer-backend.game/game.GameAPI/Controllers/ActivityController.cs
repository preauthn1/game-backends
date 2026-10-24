using FW.Core.Helper;
using game.Entity.Users;
using game.Infrastructure;
using game.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace game.GameAPI.Controllers
{
    public class ActivityController : BaseAPIController
    {
        private IActivityService _activityService;

        public ActivityController(IActivityService service)
        {
            _activityService = service;
        }

        [HttpGet]
        public List<EventSetting> GetEventList(DateTime? beginTime, DateTime? endTime, int? status, string eventName)
        {
            return _activityService.GetEventList(beginTime, endTime, status, eventName);
        }

        [HttpPost]
        public APIResult ModifyEvent(EventSetting model)
        {
            string _msg = "";
            var _result = _activityService.ModifyEvent(model, ref _msg);

            return new APIResult(_result, _msg);
        }

        [HttpGet]
        public List<EventLog> GetUserEventAccessList(DateTime? beginTime, DateTime? endTime, int? userID, string loginName = null)
        {
            return _activityService.GetUserEventAccessList(beginTime, endTime, userID, loginName);
        }

        [HttpGet]
        public PagedList<EventDetailLog> GetUserEventAccessDetail(DateTime? beginTime, DateTime? endTime, int? userID, int pageIndex, int pageSize)
        {
            return _activityService.GetUserEventAccessDetail(beginTime, endTime, userID, pageIndex, pageSize);
        }

        [HttpGet]
        public APIResult ResetGrantTime(int eventID, int userID, string updater, int resetTime)
        {
            string message = "";
            var _result = _activityService.ResetGrantTime(eventID, userID, updater, resetTime, ref message);
            return new APIResult(_result, message);
        }


        [HttpGet]
        public List<EventContest> GetGamesEventMGMT(int? contestID, DateTime? beginTime, DateTime? endTime)
        {
            return _activityService.GetGamesEventMGMT(contestID, beginTime, endTime);
        }

        [HttpGet]
        public List<EventBonus> GetEventBonus(int? contestID)
        {
            return _activityService.GetEventBonus(contestID);
        }

        [HttpGet]
        public List<EventReward> GetEventReward(int? contestID)
        {
            return _activityService.GetEventReward(contestID);
        }
        
        [HttpGet]
        public List<EventRank> GetContestRank(int? contestID, DateTime? rankDate, int rankType)
        {
            return _activityService.GetContestRank(contestID, rankDate, rankType);
        }

        [HttpPost]
        public APIResult UpdateEventContest(EventContest model)
        {
            var msg = string.Empty;
            var contestID = 0;
            bool result = _activityService.UpdateEventContest(model, ref contestID, ref msg);
            return new APIResult(result, msg, contestID);
        }

        [HttpPost]
        public APIResult UpdateEventBonus(EventBonus model)
        {
            string msg = string.Empty;
            bool result = _activityService.UpdateEventBonus(model, ref msg);
            return new APIResult(result, msg);
        }

        [HttpPost]
        public APIResult UpdateEventReward(EventReward model)
        {
            string msg = string.Empty;
            bool result = _activityService.UpdateEventReward(model, ref msg);
            return new APIResult(result, msg);
        }

        [HttpGet]
        public APIResult DeleteEventBonus(int id)
        {
            string msg = string.Empty;
            bool result = _activityService.DeleteEventBonus(id, ref msg);
            return new APIResult(result, msg);
        }

        [HttpGet]
        public APIResult DeleteEventReward(int id)
        {
            string msg = string.Empty;
            bool result = _activityService.DeleteEventReward(id, ref msg);
            return new APIResult(result, msg);
        }

        [HttpGet]
        public List<SevenDaysLoginBonus> GetSevenDaysLoginBonus(int? platformID, int? areaID)
        {
            return _activityService.GetSevenDaysLoginBonus(platformID, areaID);
        }

        [HttpPost]
        public APIResult AddSevenDaysLoginBonus(SevenDaysLoginBonus model)
        {
            string _msg = "";
            var _result = _activityService.AddSevenDaysLoginBonus(model, ref _msg);

            return new APIResult(_result, _msg);
        }

        [HttpPost]
        public APIResult DelSevenDaysLoginBonus(SevenDaysLoginBonus model)
        {
            string _msg = "";
            var _result = _activityService.DelSevenDaysLoginBonus(model, ref _msg);

            return new APIResult(_result, _msg);
        }
    }
}