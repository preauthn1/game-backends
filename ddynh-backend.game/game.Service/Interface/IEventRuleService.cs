using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity;
using game.Entity.Event;
using game.Entity.Users;

namespace game.Service
{
    
    public interface IEventRuleService
    {

        void InitializeEventRuleByType(int eventSettingID, ApplyEventTypeEnum type);

        List<EventSettingRuleItemEntity> GetEventRulesBySettingID(int eventSettingID);

        List<EvnetSettingBonusEntity> GetEventBonusBySettingID(int eventSettingID);

        List<EventSettingAwardItemEntity> GetEventAwardBySettingID(int eventSettingID);

        List<EventSettingLotteryConditionsItemEntity> GetEventLotteryConditionsBySettingID(int eventSettingID);

        CRUDResultModel UpdateEventRules(ModifyEventRuleRequestModel rules);

        CRUDResultModel UpdateEventLuckyRouletteRules(ModifyEventLuckyRouletteRequestModel rules);

        CRUDResultModel UpdateAwardImage(ModifyEventLuckyRouletteRequestModel rules);

        CRUDResultModel AddAwardRow(EventSettingAwardItemEntity model);

        CRUDResultModel DeleteAwardRow(int awardID);

        CRUDResultModel AddLotteryRow(EventSettingLotteryConditionsItemEntity model);

        CRUDResultModel DeleteLotteryRow(int lotteryID);
        
    }
}
