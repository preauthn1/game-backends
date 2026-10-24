using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity;
using game.Entity.Event;
using game.Entity.Money;
using game.Entity.Users;

namespace game.Service
{
    /// <summary>
    /// 活動相關
    /// </summary>
    public interface IPromotionEventService
    {

        PromotionEventEntity GetById(int eventId);


        List<PromotionEventEntity> GetAll();


        PromotionEventItemEntity GetItemByUniqueKey(int eventId, PromotionEventPositionTypeEnum type);

        List<PromotionEventAreaMappingEntity> GetAreaMappingsByEventID(int eventID);


        CRUDResultModel CreatePromotionEventAndDefaultItems(PromotionEventEntity model);


        CRUDResultModel UpdatePromotionEvent(PromotionEventEntity model);


        CRUDResultModel UpdateEditor(UpdateEditorRequestModel model);


        CRUDResultModel UpdateStatus(List<int> eventIds, NormalActiveStatusEnum status);


        CRUDResultModel UpdateEventItem(PromotionEventItemEntity model);


        CRUDResultModel UpdateAreaMappings(UpdatePromotionEventAreaMappingRequestModel model);


        CRUDResultModel CopyEvent(CopyEventRequestModel model);



        PagedList<PromotionEventEntity> GetPagingByFilter(QueryPromotionEventRequestViewModel filter); 

        
        /// <summary>
        /// 包含主表跟子表資料的query
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        PagedList<PromotionEventFullInfoViewModel> GetFullInfoPagingByFilter(QueryPromotionEventRequestViewModel filter);

    }
}
