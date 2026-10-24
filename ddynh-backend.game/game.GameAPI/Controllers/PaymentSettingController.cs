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

namespace game.GameAPI.Controllers
{
    public class PaymentSettingController : BaseAPIController
    {
        private IMerchantInfoService _merchantInfoService;
        

        public PaymentSettingController(
            IMerchantInfoService merchantInfoService)
        {
            _merchantInfoService = merchantInfoService;
        }


        [HttpGet]
        public MerchantInfoEntity GetMerchantInfoBy(int id)
        {
            var query = _merchantInfoService.GetByID(id);
            return query;
        }

        [HttpGet]
        public List<MerchantInfoEntity> GetMerchantInfoByFilter([FromUri]QueryMerchantInfoRequestViewModel filter)
        {
            var query = _merchantInfoService.GetByFilter(filter);
            return query;
        }

    }
}