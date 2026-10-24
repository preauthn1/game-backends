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
    public class PayGatewayConvertTypeMappingController : BaseAPIController
    {
        private IPayGatewayConvertTypeMappingService _service;


        public PayGatewayConvertTypeMappingController(IPayGatewayConvertTypeMappingService service)
        {
            _service = service;
        }


        [HttpGet]
        public PayGatewayConvertTypeMappingEntity GetByID(int id)
        {
            var query = _service.GetByID(id);
            return query;
        }


        [HttpGet]
        public List<PayGatewayConvertTypeMappingEntity> GetByIDs(string idsText)
        {
            List<int> ids = idsText.SeperateToListIntOrDefaultValue();
            var query = _service.GetByIDs(ids);
            return query;
        }


        [HttpGet]
        public List<PayGatewayConvertTypeMappingEntity> GetByFilter([FromUri]QueryPayGatewayConvertTypeMappingRequestViewModel filter)
        {
            var query = _service.GetByFilter(filter);
            return query;
        }
        

        [HttpPost, HttpPut]
        public CRUDResultModel EditAndDelete([FromBody]PayGatewayConvertTypeMappingUpdateRequestModel model)
        {
            var query = _service.EditAndDelete(model);
            return query;
        }

    }
}