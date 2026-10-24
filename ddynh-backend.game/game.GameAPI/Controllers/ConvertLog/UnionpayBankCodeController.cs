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
    public class UnionpayBankCodeController : BaseAPIController
    {
        private IUnionpayBankCodeService _service;


        public UnionpayBankCodeController(IUnionpayBankCodeService service)
        {
            _service = service;
        }


        [HttpGet]
        public UnionpayBankCodeEntity GetByID(int id)
        {
            var query = _service.GetByID(id);
            return query;
        }


        [HttpGet]
        public List<UnionpayBankCodeEntity> GetByIDs(string idsText)
        {
            List<int> ids = idsText.SeperateToListIntOrDefaultValue();
            var query = _service.GetByIDs(ids);
            return query;
        }


        [HttpGet]
        public List<UnionpayBankCodeEntity> GetByFilter([FromUri]QueryUnionpayBankCodeRequestViewModel filter)
        {
            var query = _service.GetByFilter(filter);
            return query;
        }
        

        [HttpGet]
        public PagedList<UnionpayBankCodeEntity> GetPagingByFilter([FromUri]QueryUnionpayBankCodeRequestViewModel filter)
        {
            var query = _service.GetPagingByFilter(filter);
            return query;
        }


        [HttpPost]
        public CRUDResultModel Create([FromBody]UnionpayBankCodeEntity model)
        {
            var query = _service.Create(model);
            return query;
        }



        [HttpPost , HttpPut]
        public CRUDResultModel Edit([FromBody]UnionpayBankCodeEntity model)
        {
            var query = _service.Edit(model);
            return query;
        }

    }
}