using FW.Core.Helper;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Event;
using game.Entity.Money;
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
    public class PromotionEventController : BaseAPIController
    {
        private IPromotionEventService _promotionEventService;


        public PromotionEventController(IPromotionEventService service)
        {
            _promotionEventService = service;
        }


        [HttpGet]
        public PromotionEventEntity GetEventById(int id)
        {
            var query = _promotionEventService.GetById(id);
            return query;
        }

        
        [HttpPost]
        public CRUDResultModel Create(PromotionEventEntity model)
        {
            var result = _promotionEventService.CreatePromotionEventAndDefaultItems(model);
            return result;
        }


        [HttpPost , HttpPut]
        public CRUDResultModel Update(PromotionEventEntity model)
        {
            var result = _promotionEventService.UpdatePromotionEvent(model);
            return result;
        }



        [HttpPost, HttpPut]
        public CRUDResultModel SetInvisible(UpdateEditorRequestModel model)
        {
            var result = _promotionEventService.UpdateStatus(model.TargetIds, NormalActiveStatusEnum.PermanentInvisible);
            return result;
        }


        [HttpPost, HttpPut]
        public CRUDResultModel UpdateEditor(UpdateEditorRequestModel model)
        {
            var result = _promotionEventService.UpdateEditor(model);
            return result;
        }


        [HttpPost, HttpPut]
        public CRUDResultModel CopyEvents(CopyEventRequestModel model)
        {
            var result = _promotionEventService.CopyEvent(model);
            return result;
        }


        [HttpGet]
        public PagedList<PromotionEventEntity> GetPromotionEventPagingByFilter([FromUri]QueryPromotionEventRequestViewModel filter)
        {
            var query = _promotionEventService.GetPagingByFilter(filter);
            return query;
        }


        [HttpGet]
        public PagedList<PromotionEventFullInfoViewModel> GetFullInfoPagingByFilter([FromUri]QueryPromotionEventRequestViewModel filter)
        {
            var query = _promotionEventService.GetFullInfoPagingByFilter(filter);
            return query;
        }
        
        [HttpGet]
        public PromotionEventItemEntity GetItemByUniqueKey(int eventID , PromotionEventPositionTypeEnum type)
        {
            var query = _promotionEventService.GetItemByUniqueKey(eventID , type);
            return query;
        }

        [HttpPost , HttpPut]
        public CRUDResultModel UpdateEventItem(PromotionEventItemEntity model)
        {
            var query = _promotionEventService.UpdateEventItem(model);
            return query;
        }


        [HttpGet]
        public List<PromotionEventAreaMappingEntity> GetAreaMappingByEventID(int eventID)
        {
            var query = _promotionEventService.GetAreaMappingsByEventID(eventID);
            return query;
        }

        [HttpPost, HttpPut]
        public CRUDResultModel UpdateAreaMapping(UpdatePromotionEventAreaMappingRequestModel model)
        {
            var result = _promotionEventService.UpdateAreaMappings(model);
            return result;
        }


    }
}