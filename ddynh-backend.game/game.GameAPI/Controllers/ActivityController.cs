using FW.Core.Helper;
using game.Entity;
using game.Entity.Event;
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

        private IEventRuleService _eventRuleService;


        public ActivityController(IActivityService service , IEventRuleService eventRuleService)
        {
            _activityService = service;
            _eventRuleService = eventRuleService;
        }

        [HttpGet]
        public PagedList<EventSettingEntity> GetEventList([FromUri]QueryEventSettingRequestViewModel filter)
        {
            return _activityService.GetEventSettingList(filter);
        }


        [HttpGet]
        public EventSettingEntity GetEventSettingById(int id)
        {
            var query = _activityService.GetEventSettingById(id);
            return query;
        }

        [HttpPost]
        public CRUDResultModel CreateEventSetting(EventSettingEntity model)
        {
            //step 1. 建立setting資料
            var result = _activityService.CreateEventSetting(model);

            //step 2. 建立eventRule預設資料
            if(result.IsSuccess)
                _eventRuleService.InitializeEventRuleByType(result.ResultId , (ApplyEventTypeEnum)model.EventType);

            return result;
        }
        
        [HttpPost]
        public CRUDResultModel ModifyEvent(EventSettingEntity model)
        {
            var _result = _activityService.ModifyEvent(model);
            return _result;
        }

        [HttpPost]
        public CRUDResultModel UpdateStatus(CommonUpdateStatusRequestModel model)
        {
            var result = _activityService.UpdateStatus(model);
            return result;
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
    }
}