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
    public class UnionpayBankPlatformMappingController : BaseAPIController
    {
        private IUnionpayBankPlatformMappingService _service;


        public UnionpayBankPlatformMappingController(IUnionpayBankPlatformMappingService service)
        {
            _service = service;
        }


        [HttpGet]
        public UnionpayBankPlatformMappingEntity GetByID(int id)
        {
            var query = _service.GetByID(id);
            return query;
        }


        [HttpGet]
        public List<UnionpayBankPlatformMappingEntity> GetByIDs(string idsText)
        {
            List<int> ids = idsText.SeperateToListIntOrDefaultValue();
            var query = _service.GetByIDs(ids);
            return query;
        }


        [HttpGet]
        public List<UnionpayBankPlatformMappingEntity> GetByFilter([FromUri]QueryUnionpayBankPlatformMappingRequestViewModel filter)
        {
            var query = _service.GetByFilter(filter);
            return query;
        }
        

        [HttpPost, HttpPut]
        public CRUDResultModel Edit( [FromBody]UnionpayBankPlatformUpdateViewModel model)
        {
            var query = _service.Edit(model);
            return query;
        }

    }
}