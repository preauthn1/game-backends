using System;
using System.Collections.Generic;
using System.Web.Http;
using game.Service;
using game.Infrastructure;
using game.Entity.Payment;
using FW.Core.Helper;
using Newtonsoft.Json;
using game.Entity.ViewModels;
using game.Entity;

namespace game.GameAPI.Controllers
{
    public class PaymentOthersController : BaseAPIController
    {
        private IPaymentPlatformSettingService _paymentSettingService;


        public PaymentOthersController(IPaymentPlatformSettingService paymentService)
        {
            _paymentSettingService = paymentService;
        }

    
        [HttpGet]
        public PlatformPayTypeSetting GetPlatformPayTypeSettingById(int id)
        {
            var query = _paymentSettingService.GetById(id);
            return query;
        }


        [HttpPost , HttpPut]
        public APIResult UpdatePlatformSettingPriority(UpdatePlatformSettingPriorityRequestModel model)
        {
            var result = _paymentSettingService.UpdatePlatformSettingPriority(model);
            return result;
        }


        [HttpGet]
        public List<PayTypeCategoryMapping> GetAllPayTypeCategoryMapping()
        {
            var query = _paymentSettingService.GetAllPayTypeCategoryMapping();
            return query;
        }



    }
}