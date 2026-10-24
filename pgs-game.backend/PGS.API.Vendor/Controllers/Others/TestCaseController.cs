using game.Entity.Game;
using Lck.Utility.Extensions.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using target.BL.Services;
using target.DA.Repository;
using target.Models.Models;
using target.Models.Table;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;
using target.Web.Modules.Filter;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;

namespace target.API.Vendor.Controllers
{
    [CompanyIPOrDevEnvAuthorizeFilter]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TestCaseController : BaseController
    {
        [Route("testcase/test001")]
        [HttpGet]
        public object TestCase001(string cacheKey = "Test")
        {
            return "OK";
        }


    }
}
