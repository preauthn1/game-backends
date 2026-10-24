using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.Event;
using game.Entity.Payment;
using game.Entity.Users;
using game.Infrastructure;
using game.Service.DAO;
using game.Infrastructure.Extensions;
using game.Entity.Game;
using game.Entity.Money;

namespace game.Service.Service
{
    public class PromotionEventService : BaseService, IPromotionEventService
    {

        public PromotionEventRepository PromotionEventRepository { get; set; }

        public PromotionEventItemRepository PromotionEventItemRepository { get; set; }

        public PromotionEventAreaMappingRepository PromotionEventAreaMappingRepository { get; set; }


        public List<PromotionEventEntity> GetAll()
        {
            var query = PromotionEventRepository.GetAll();
            return query;
        }

        public PromotionEventEntity GetById(int eventId)
        {
            var query = PromotionEventRepository.GetById(eventId);
            return query;
        }

        public PromotionEventItemEntity GetItemByUniqueKey(int eventId , PromotionEventPositionTypeEnum type)
        {
            var query = PromotionEventItemRepository.GetByEventIds(eventId.ToListCollection());
            return query.FirstOrDefault(x=> x.Type == (int)type);
        }

       
        public CRUDResultModel CreatePromotionEventAndDefaultItems(PromotionEventEntity model)
        {
            List<CheckResultModel> valid = new List<CheckResultModel>
            {
                ValidateHelper.CheckTextLength_IsNotEmpty_IsValidLength(model.Title , TargetColumnEnum.Title , 50),
                ValidateHelper.CheckTextLength_IsValid(model.Memo, TargetColumnEnum.Memo  , 200),
                ValidateHelper.CheckBeginEndIsValid(model.EventStartTime , model.EventEndTime),
                ValidateHelper.CheckIsNotZeroOrMinus(model.PlatformID , TargetColumnEnum.PlatformID),
            };

            var result = ValidateHelper.CheckAllSuccessToCRUDResult(valid);

            if (!result.IsSuccess)
                return result;

            model.CreateTime = DateTime.Now;
            model.UpdateTime = DateTime.Now;

            //step 1. 建立event
            int resultEventId = PromotionEventRepository.Create(model);           

            var items =  InstanceDefaultItem(resultEventId);

            //step 2. 建立eventItem (预设的)
            PromotionEventItemRepository.CreateMany(items);

            return new CRUDResultModel
            {
                IsSuccess = true,
                Message = "成功",
                ResultId = resultEventId,
            };
        }



        public CRUDResultModel UpdatePromotionEvent(PromotionEventEntity model)
        {
            List<CheckResultModel> valid = new List<CheckResultModel>
            {
                ValidateHelper.CheckIsNotZeroOrMinus(model.EventId , TargetColumnEnum.Id),
                ValidateHelper.CheckTextLength_IsNotEmpty_IsValidLength(model.Title , TargetColumnEnum.Title , 50),
                ValidateHelper.CheckTextLength_IsValid(model.Memo, TargetColumnEnum.Memo  , 200),
                ValidateHelper.CheckBeginEndIsValid(model.EventStartTime , model.EventEndTime),
            };

            var result = ValidateHelper.CheckAllSuccessToCRUDResult(valid);

            if (!result.IsSuccess)
                return result;

            var queryExist = PromotionEventRepository.GetById(model.EventId);

            if (queryExist == null)
                return new CRUDResultModel("查无资料");

            queryExist.Title = model.Title;
            queryExist.Memo = model.Memo;
            queryExist.EventStartTime = model.EventStartTime;
            queryExist.EventEndTime = model.EventEndTime;
            queryExist.Status = model.Status;
            queryExist.UpdateBy = model.UpdateBy;
            queryExist.UpdateTime = DateTime.Now;

            PromotionEventRepository.Update(queryExist);

            return new CRUDResultModel(true);
        }


        public CRUDResultModel UpdateStatus(List<int> eventIds , NormalActiveStatusEnum status)
        {
            if (eventIds.IsEmptyOrNull())
                return new CRUDResultModel("查无资料 (0)");

            var queryByIDs = PromotionEventRepository.GetByIds(eventIds);

            foreach (var id in eventIds)
            {
                var queryExist = queryByIDs.FirstOrDefault(x => x.EventId == id);

                if (queryExist == null)
                    return new CRUDResultModel("查无资料");

                queryExist.Status = (int)status;
                queryExist.UpdateTime = DateTime.Now;

                PromotionEventRepository.Update(queryExist);
            }

            return new CRUDResultModel(true);
        }



        public CRUDResultModel UpdateEditor(UpdateEditorRequestModel model)
        {          
            var queryExist = PromotionEventRepository.GetById(model.TargetId);

            if (queryExist == null)
                return new CRUDResultModel("查无资料");

            queryExist.UpdateBy = model.UpdateBy;
            queryExist.UpdateTime = DateTime.Now;

            PromotionEventRepository.Update(queryExist);

            return new CRUDResultModel(true);
        }

        public CRUDResultModel CopyEvent(CopyEventRequestModel model)
        {
            if(model.TemplateEventIDs.IsEmptyOrNull())
                return new CRUDResultModel("参数 TemplateEventIDs 是必填的");

            if (model.TargetPlatformIDs.IsEmptyOrNull())
                return new CRUDResultModel("参数 TargetPlatformIDs 是必填的");

            if(string.IsNullOrWhiteSpace(model.UpdateBy))
                return new CRUDResultModel("参数 UpdateBy 是必填的");

            PromotionEventRepository.CopyEventBySP(model.TemplateEventIDs , model.TargetPlatformIDs, model.UpdateBy);
            return new CRUDResultModel(true);
        }
        

        public CRUDResultModel UpdateEventItem(PromotionEventItemEntity model)
        {
            List<CheckResultModel> valid = new List<CheckResultModel>
            {
                ValidateHelper.CheckIsNotZeroOrMinus(model.Id , TargetColumnEnum.Id),
                ValidateHelper.CheckIsNotMinus(model.Status , TargetColumnEnum.Status),
                ValidateHelper.CheckIsNotMinus(model.BehaviorType , TargetColumnEnum.Type),
            };

            var result = ValidateHelper.CheckAllSuccessToCRUDResult(valid);

            if (!result.IsSuccess)
                return result;

            var queryExist = PromotionEventItemRepository.GetById(model.Id);

            if (queryExist == null)
                return new CRUDResultModel("查无资料");


            //如果这两个栏位没填数值 不更新
            if(!string.IsNullOrWhiteSpace(model.ImageUrl) && 
               !string.IsNullOrWhiteSpace(model.ImageUrlDomain))
            {
                queryExist.ImageUrlDomain = model.ImageUrlDomain;
                queryExist.ImageUrl = model.ImageUrl;
            }

            queryExist.Tags = model.Tags;
            queryExist.Sort = model.Sort;
            queryExist.Status = model.Status;
            queryExist.BehaviorType = model.BehaviorType;
            queryExist.BehaviorSettingValue = model.BehaviorSettingValue;
            queryExist.DisplayMode = model.DisplayMode;
            queryExist.Content = model.Content;

            PromotionEventItemRepository.Update(queryExist);
            return new CRUDResultModel(true);
        }

        public List<PromotionEventAreaMappingEntity> GetAreaMappingsByEventID(int eventID)
        {
            var query = PromotionEventAreaMappingRepository.GetByEventID(eventID);
            return query;
        }

   

        public CRUDResultModel UpdateAreaMappings(UpdatePromotionEventAreaMappingRequestModel model)
        {
            List<CheckResultModel> valid = new List<CheckResultModel>
            {
                ValidateHelper.CheckIsNotZeroOrMinus(model.EventID , TargetColumnEnum.Id)
            };

            var result = ValidateHelper.CheckAllSuccessToCRUDResult(valid);

            if (!result.IsSuccess)
                return result;

            PromotionEventAreaMappingRepository.DeleteAndInsertMultiple(model.EventID , model.AreaIDs);

            return new CRUDResultModel(true);
        }


        private List<PromotionEventItemEntity> InstanceDefaultItem(int eventId)
        {
            var insertItems = new List<PromotionEventItemEntity>();
            insertItems.Add(new PromotionEventItemEntity
            {
                ImageUrl = string.Empty,
                ImageUrlDomain = string.Empty,
                BehaviorType = Behavior.NoAction.ToInteger(),
                Sort = 999,
                EventId = eventId,
                Status = NormalActiveStatusEnum.Disable.ToInteger(),
                Type = PromotionEventPositionTypeEnum.HomePage.ToInteger(),
                BehaviorSettingValue = string.Empty,
            });

            insertItems.Add(new PromotionEventItemEntity
            {
                ImageUrl = string.Empty,
                ImageUrlDomain = string.Empty,
                BehaviorType = Behavior.NoAction.ToInteger(),
                Sort = 999,
                EventId = eventId,
                Status = NormalActiveStatusEnum.Disable.ToInteger(),
                Type = PromotionEventPositionTypeEnum.MailPage.ToInteger(),
                BehaviorSettingValue = string.Empty,
            });

            return insertItems;
        }


        public PagedList<PromotionEventEntity> GetPagingByFilter(QueryPromotionEventRequestViewModel filter)
        {
            var query = PromotionEventRepository.GetPagingByFilter(filter);
            return query;
        }


        public PagedList<PromotionEventFullInfoViewModel> GetFullInfoPagingByFilter(QueryPromotionEventRequestViewModel filter)
        {
            var query = PromotionEventRepository.GetPagingByFilter(filter);

            var resultPaging = query.ReturnInjecteObject<PagedList<PromotionEventFullInfoViewModel>>();
            resultPaging.PageData = query.PageData.Select(x => x.ReturnInjecteObject<PromotionEventFullInfoViewModel>()).ToList();


            if (resultPaging.PageData.IsEmptyOrNull())
                return resultPaging;

            List<int> eventIds = query.PageData.Select(x => x.EventId).ToList();

            var queryItems = PromotionEventItemRepository.GetByEventIds(eventIds);
            var toalAreaCount = PromotionEventAreaMappingRepository.GetTotalAreaCount(eventIds);

            foreach (var item in resultPaging.PageData)
            {
                item.EventItems = queryItems.Where(x => x.EventId == item.EventId).ToList();
                item.TotalAreaCount = toalAreaCount.FirstOrDefault(x => x.EventID == item.EventId).TotalAreaCount;
            }

            return resultPaging;
        }


    }

}
