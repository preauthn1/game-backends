using FW.Core.Helper;
using game.Entity;
using game.Entity.Event;
using game.Entity.Game;
using game.Entity.Users;
using game.Infrastructure;
using game.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using game.Infrastructure.Extensions;
using game.Entity.Payment;

namespace game.GameAPI.Controllers
{
    public class PayGatewayController : BaseAPIController
    {
        private IPayGatewayService _service;


        public PayGatewayController(IPayGatewayService service)
        {
            _service = service;
        }


        [HttpGet]
        public PayGatewayEntity GetByID(int id)
        {
            var query = _service.GetByID(id);
            return query;
        }


        [HttpGet]
        public List<PayGatewayEntity> GetByIDs(string idsText)
        {
            List<int> ids = idsText.SeperateToListIntOrDefaultValue();
            var query = _service.GetByIDs(ids);
            return query;
        }


        [HttpGet]
        public List<PayGatewayEntity> GetByFilter([FromUri]QueryPayGatewayRequestViewModel filter)
        {
            var query = _service.GetByFilter(filter);
            return query;
        }


        [HttpGet]
        public PagedList<PayGatewayEntity> GetPagingByFilter([FromUri]QueryPayGatewayRequestViewModel filter)
        {
            var query = _service.GetPagingByFilter(filter);
            return query;
        }



        [HttpPost]
        public CRUDResultModel Create([FromBody]PayGatewayEntity model)
        {
            var query = _service.Create(model);
            return query;
        }



        [HttpPost, HttpPut]
        public CRUDResultModel Edit([FromBody]PayGatewayEntity model)
        {
            var query = _service.Edit(model);
            return query;
        }

    }
}