using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.Event;
using game.Entity.Users;
using game.Infrastructure;
using game.Service.DAO;
using game.Infrastructure.Extensions;
using game.Infrastructure.Helper;

namespace game.Service.Service
{
    public class EventRuleService : BaseService , IEventRuleService
    {      
        public EventSettingBonusRepository EventSettingBonusRepository { get; set; }

        public EventSettingRuleItemRepository EventSettingRuleItemRepository { get; set; }

        public EventSettingAwardItemRepository EventSettingAwardItemRepository { get; set; }

        public EventSettingLotteryConditionsItemRepository EventSettingLotteryConditionsItemRepository { get; set; }

        public EventSettingRepository EventSettingRepository { get; set; }



        public void InitializeEventRuleByType(int eventSettingID , ApplyEventTypeEnum type)
        {
            if (eventSettingID <= 0)
                return;

            var initializeModel = EvnetRuleInitialHelper.InitialByEventType(eventSettingID, type);

            if (AppLogicHelper.GetRouletteEventTypes().Contains((int)type))
            {
                EventSettingAwardItemRepository.CreateMany(initializeModel.AwardItems);

                EventSettingLotteryConditionsItemRepository.CreateMany(initializeModel.LotteryConditionsItem);

                EventSettingRuleItemRepository.CreateMany(initializeModel.RuleItems);
            }
            else
            {
                //建立bonus
                EventSettingBonusRepository.CreateMany(initializeModel.BonuItems);

                //建立rules
                EventSettingRuleItemRepository.CreateMany(initializeModel.RuleItems);
            }
        }

        public List<EventSettingRuleItemEntity> GetEventRulesBySettingID(int eventSettingID)
        {
            var query = EventSettingRuleItemRepository.GetBySettingID(eventSettingID);
            return query;
        }

        public List<EvnetSettingBonusEntity> GetEventBonusBySettingID(int eventSettingID)
        {
            var query = EventSettingBonusRepository.GetBySettingID(eventSettingID);
            return query;
        }

        public List<EventSettingAwardItemEntity> GetEventAwardBySettingID(int eventSettingID)
        {
            var query = EventSettingAwardItemRepository.GetBySettingID(eventSettingID);
            return query;
        }

        public List<EventSettingLotteryConditionsItemEntity> GetEventLotteryConditionsBySettingID(int eventSettingID)
        {
            var query = EventSettingLotteryConditionsItemRepository.GetBySettingID(eventSettingID);
            return query;
        }


        public CRUDResultModel UpdateEventRules(ModifyEventRuleRequestModel updateSetting)
        {            
            var rule_minCharges = updateSetting.RuleItems
                .Where(x => x.RuleType == (int)EventRuleTypeEnum.TotalDepositAmount || x.RuleType == (int)EventRuleTypeEnum.DepositAmount_FirstDeposit)
                .ToList();

            var valids = new List<CheckResultModel>
            {
                ValidateHelper.CheckIsNotZeroOrMinus(updateSetting.EventSettingID , TargetColumnEnum.Id),
                ValidateHelper.ChecIsNotEmptyOrNull(updateSetting.BonuItems , TargetColumnEnum.ListObject),
                ValidateHelper.ChecIsNotEmptyOrNull(updateSetting.RuleItems , TargetColumnEnum.ListObject),
                ValidateHelper.CheckIsNotMinus(updateSetting.BonuItems.Select(x=> x.Id) , TargetColumnEnum.Id),
                ValidateHelper.CheckIsNotMinus(updateSetting.RuleItems.Select(x=> x.EventSettingID) , TargetColumnEnum.FK_Id),
                ValidateHelper.CheckIsNotMinus(updateSetting.BonuItems.Select(z=> z.FlowRate) , TargetColumnEnum.Amount),
                ValidateHelper.CheckIsNotMinus(updateSetting.BonuItems.Select(z=> (long)z.Value) , TargetColumnEnum.Amount),
                ValidateHelper.CheckIsNotMinus(rule_minCharges.Select(z=> z.RuleValue) , TargetColumnEnum.Amount)
            };

            var checkResult = ValidateHelper.CheckAllSuccess(valids);

            if (!checkResult.IsSuccess)
                return ValidateHelper.ConvertToCRUDResult(checkResult);
            

            var queryEventSetting = EventSettingRepository.GetById(updateSetting.EventSettingID);

            if (queryEventSetting == null)
                return new CRUDResultModel("查无此资料");


            //根據eventType不同 客製一些檢查差異
            if(queryEventSetting.EventType == (int)ApplyEventTypeEnum.FirstChargeBonus)
            {
                long beforeValue = 9999999999999;
                foreach(var item in updateSetting.RuleItems)
                {
                    if(item.RuleValue > beforeValue)
                    {
                        return new CRUDResultModel("首存金额设置。排序后方的项目不得大于前面的项目");
                    }

                    beforeValue = item.RuleValue;
                }
            }

            //更新bonus
            UpdateBonus(updateSetting.BonuItems);

            //更新rules (刪除再重建)
            EventSettingRuleItemRepository.UpdateEventRules(updateSetting.EventSettingID , updateSetting.RuleItems);

            //更新修改時間
            EventSettingRepository.UpdateModifyTime(updateSetting.EventSettingID , updateSetting.UpdateBy);

            return new CRUDResultModel(true);
        }



        public void UpdateBonus(List<EvnetSettingBonusEntity> bonus)
        {
            List<int> ids = bonus.Select(z => z.Id).ToList();
            var queryExist = EventSettingBonusRepository.GetByIds(ids);
                       
            if (queryExist.IsEmptyOrNull())
                return;
            
            foreach(var exist in queryExist)
            {
                var match = bonus.FirstOrDefault(x => x.Id == exist.Id);

                //先寫成只更新部分欄位
                exist.Value = match.Value;
                exist.FlowRate = match.FlowRate;
                exist.MaxAmountLimit = match.MaxAmountLimit;


                //exist.BonusType = match.BonusType;
                //exist.Sequence = match.Sequence;
                //exist.DisplayName = match.DisplayName;
                //exist.Memo = match.Memo;

            }

            EventSettingBonusRepository.UpdateMany(queryExist);
        }


        public CRUDResultModel UpdateEventLuckyRouletteRules(ModifyEventLuckyRouletteRequestModel updateSetting)
        {
            var valids = new List<CheckResultModel>
            {
                ValidateHelper.CheckIsNotZeroOrMinus(updateSetting.EventSettingID , TargetColumnEnum.Id)
            };

            var checkResult = ValidateHelper.CheckAllSuccess(valids);

            if (!checkResult.IsSuccess)
                return ValidateHelper.ConvertToCRUDResult(checkResult);

            var queryEventSetting = EventSettingRepository.GetById(updateSetting.EventSettingID);

            if (queryEventSetting == null)
                return new CRUDResultModel("查无此资料");

            //更新EventSetting
            UpdateEventSetting(updateSetting.EventSettingID, updateSetting.Memo, updateSetting.Tips, updateSetting.UpdateBy);

            //更新Award
            UpdateAward(updateSetting.AwardItems);

            //更新Lottery
            if (updateSetting.LotteryItems != null)
                UpdateLottery(updateSetting.LotteryItems);

            //更新rules (刪除再重建)
            EventSettingRuleItemRepository.UpdateEventRules(updateSetting.EventSettingID, updateSetting.RuleItems);

            //更新修改時間
            EventSettingRepository.UpdateModifyTime(updateSetting.EventSettingID, updateSetting.UpdateBy);

            return new CRUDResultModel(true);
        }

        public CRUDResultModel UpdateAwardImage(ModifyEventLuckyRouletteRequestModel updateSetting)
        {
            var valids = new List<CheckResultModel>
            {
                ValidateHelper.CheckIsNotZeroOrMinus(updateSetting.EventSettingID , TargetColumnEnum.Id)
            };

            var checkResult = ValidateHelper.CheckAllSuccess(valids);

            if (!checkResult.IsSuccess)
                return ValidateHelper.ConvertToCRUDResult(checkResult);

            var queryEventSetting = EventSettingRepository.GetById(updateSetting.EventSettingID);

            if (queryEventSetting == null)
                return new CRUDResultModel("查无此资料");

            //更新Award
            var result = UpdateAwardImage(updateSetting.AwardItems[0]);

            return new CRUDResultModel(true);
        }

        public void UpdateEventSetting(int eventSettingID, string memo, string tips, string updateBy)
        {
            EventSettingRepository.UpdateMemoAndTips(eventSettingID, memo, tips, updateBy);
        }

        public void UpdateAward(List<EventSettingAwardItemEntity> awards)
        {
            List<int> ids = awards.Select(z => z.Id).ToList();
            var queryExist = EventSettingAwardItemRepository.GetByIds(ids);

            if (queryExist.IsEmptyOrNull())
                return;

            foreach (var exist in queryExist)
            {
                var match = awards.FirstOrDefault(x => x.Id == exist.Id);

                //先寫成只更新部分欄位
                exist.AwardName = match.AwardName;
                exist.ImageUrlDomain = match.ImageUrlDomain;
                exist.ImageUrl = match.ImageUrl;
                exist.AwardType = match.AwardType;
                exist.AwardAmount = match.AwardAmount;
                exist.AwardRate = match.AwardRate;
                exist.AwardLimit = match.AwardLimit;
            }

            EventSettingAwardItemRepository.UpdateMany(queryExist);
        }

        public CRUDResultModel UpdateAwardImage(EventSettingAwardItemEntity model)
        {
            EventSettingAwardItemRepository.UpdateImageUrl(model);
            return new CRUDResultModel
            {
                IsSuccess = true,
                Message = "新增成功",
            };
        }

        public void UpdateLottery(List<EventSettingLotteryConditionsItemEntity> lottery)
        {
            List<int> ids = lottery.Select(z => z.Id).ToList();
            var queryExist = EventSettingLotteryConditionsItemRepository.GetByIds(ids);

            if (queryExist.IsEmptyOrNull())
                return;

            foreach (var exist in queryExist)
            {
                var match = lottery.FirstOrDefault(x => x.Id == exist.Id);

                //先寫成只更新部分欄位
                exist.LotteryAmount = match.LotteryAmount;
                exist.LotteryCount = match.LotteryCount;
            }

            EventSettingLotteryConditionsItemRepository.UpdateMany(queryExist);
        }

        public CRUDResultModel AddAwardRow(EventSettingAwardItemEntity model)
        {
            var resultId = EventSettingAwardItemRepository.Create(model);
            return new CRUDResultModel
            {
                ResultId = resultId,
                IsSuccess = true,
                Message = "新增成功",
            };
        }

        public CRUDResultModel DeleteAwardRow(int awardID)
        {
            EventSettingAwardItemRepository.Delete(awardID);
            return new CRUDResultModel
            {
                IsSuccess = true,
                Message = "删除成功",
            };
        }

        public CRUDResultModel AddLotteryRow(EventSettingLotteryConditionsItemEntity model)
        {
            var resultId = EventSettingLotteryConditionsItemRepository.Create(model);
            return new CRUDResultModel
            {
                ResultId = resultId,
                IsSuccess = true,
                Message = "新增成功",
            };
        }
        public CRUDResultModel DeleteLotteryRow(int lotteryID)
        {
            EventSettingLotteryConditionsItemRepository.Delete(lotteryID);
            return new CRUDResultModel
            {
                IsSuccess = true,
                Message = "删除成功",
            };
        }

        
    }
}
