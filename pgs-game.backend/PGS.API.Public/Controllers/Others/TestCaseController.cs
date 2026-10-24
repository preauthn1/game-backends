using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.Web.Modules.Filter;
using target.Utility.Helper.HttpContext;
using Lck.Utility.Extensions.Helper;
using target.Utility;
using target.DA.Repository;
using target.Utility.Helper;
using target.BL.Services;

namespace target.API.Public.Controllers
{
    [CompanyIPOrDevEnvAuthorizeFilter]
    [DoNotCheckCertificate]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TestCaseController : BaseController
    {

        /// <summary>
        /// 測試redis是否有寫入
        /// </summary>
        [Route("internal/testCase/test001")]
        [HttpGet]
        public object TestCase001()
        {
            var testService = new PromotionEventService();
            testService.LuckyWheelStartDraw(1000000, new Models.ViewModels.EventLuckyDrawRequestModel
            {
            });

            return "XXXXXXXXXXX";
        }


        [Route("internal/test/GetOutboundIPAddress")]
        [HttpGet]
        public object GetOutboundIPAddress()
        {
            var myValue = AppHelper.GetOutboundIPAddress();
            return myValue;
        }


    }
}
