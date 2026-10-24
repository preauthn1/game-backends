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

namespace game.GameAPI.Controllers
{
    public class ConvertGatewayBankSettingController : BaseAPIController
    {
        private IConvertGatewayBankSettingService _service;


        public ConvertGatewayBankSettingController(IConvertGatewayBankSettingService service)
        {
            _service = service;
        }


        [HttpGet]
        public List<ConvertGatewayBankSettingEntity> GetByFilter([FromUri]QueryConvertGatewayBankSettingRequestViewModel filter)
        {
            var query = _service.GetByFilter(filter);
            return query;
        }


        [HttpGet]
        public PagedList<ConvertGatewayBankSettingEntity> GetPagingByFilter([FromUri]QueryConvertGatewayBankSettingRequestViewModel filter)
        {
            var query = _service.GetPagingByFilter(filter);
            return query;
        }


        [HttpPost, HttpPut]
        public CRUDResultModel Edit([FromBody]ConvertGatewayBankSettingViewModel model)
        {
            var query = _service.Edit(model);
            return query;
        }

    }
}