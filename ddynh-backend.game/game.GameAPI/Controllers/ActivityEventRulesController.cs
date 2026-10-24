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
    public class ActivityEventRulesController : BaseAPIController
    {
        private IEventRuleService _eventRuleService;

        public ActivityEventRulesController(IEventRuleService eventRuleService)
        {
            _eventRuleService = eventRuleService;
        }

        [HttpGet]
        public List<EventSettingRuleItemEntity> GetEventRulesBySettingID(int settingID)
        {
            var query = _eventRuleService.GetEventRulesBySettingID(settingID);
            return query;
        }

        [HttpGet]
        public List<EvnetSettingBonusEntity> GetEventBonusBySettingID(int settingID)
        {
            var query = _eventRuleService.GetEventBonusBySettingID(settingID);
            return query;
        }

        [HttpPost]
        public CRUDResultModel UpdateEventRules([FromBody]ModifyEventRuleRequestModel rules)
        {
            var result = _eventRuleService.UpdateEventRules(rules);
            return result;
        }

        [HttpGet]
        public List<EventSettingAwardItemEntity> GetEventAwardBySettingID(int settingID)
        {
            var query = _eventRuleService.GetEventAwardBySettingID(settingID);
            return query;
        }

        [HttpGet]
        public List<EventSettingLotteryConditionsItemEntity> GetEventLotteryConditionsBySettingID(int settingID)
        {
            var query = _eventRuleService.GetEventLotteryConditionsBySettingID(settingID);
            return query;
        }

        [HttpPost]
        public CRUDResultModel UpdateEventLuckyRouletteRules([FromBody]ModifyEventLuckyRouletteRequestModel rules)
        {
            var result = _eventRuleService.UpdateEventLuckyRouletteRules(rules);
            return result;
        }

        [HttpPost]
        public CRUDResultModel UpdateAwardImage([FromBody]ModifyEventLuckyRouletteRequestModel rules)
        {
            var result = _eventRuleService.UpdateAwardImage(rules);
            return result;
        }

        [HttpPost]
        public CRUDResultModel AddAwardRow(EventSettingAwardItemEntity model)
        {
            var result = _eventRuleService.AddAwardRow(model);
            return result;
        }

        [HttpGet]
        public CRUDResultModel DeleteAwardRow(int awardID)
        {
            var result = _eventRuleService.DeleteAwardRow(awardID);

            return result;
        }

        [HttpPost]
        public CRUDResultModel AddLotteryRow(EventSettingLotteryConditionsItemEntity model)
        {
            var result = _eventRuleService.AddLotteryRow(model);
            return result;
        }

        [HttpGet]
        public CRUDResultModel DeleteLotteryRow(int lotteryID)
        {
            var result = _eventRuleService.DeleteLotteryRow(lotteryID);
            return result;
        }

    }
}