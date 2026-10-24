using Microsoft.AspNetCore.Mvc;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
using target.Models.Enums;
using target.Models.Table;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Helper;
using target.Web.Modules.Filter;
using System;
using System.Collections.Generic;

namespace target.API.Job.Controllers
{
    [CompanyIPOrDevEnvAuthorizeFilter]
    public class TestDebugController : BaseController
    {
        public IGameVendorRoutineJobService GameVendorRoutineJobService { get; set; }


        [Route("Debug/TestCase/Test001")]
        [HttpGet, HttpPost]
        public object Test001(string param1 = "", string param2 = "")
        {
            return new 
            {
                msg = "Hello",
            };

        }

        [Route("Debug/TestCase/AnalyzeMemory")]
        [HttpGet, HttpPost]
        public object GetTopMemoryObjects(string password = "")
        {
            if (SettingHelper.IsProduction && password != SystemConstant.PublicAPI_EazyToken_MagicToken)
                return "";

            return ApplicationMemoryHelper.GetTopMemoryObjects();
        }


    }
}
