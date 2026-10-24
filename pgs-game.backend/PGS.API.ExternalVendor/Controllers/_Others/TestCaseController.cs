using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using example.invalid;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using target.Models;
using target.Models.ViewModels.Base;
using target.Utility;
using target.Web.Modules.Filter;

namespace target.API.ExternalVendor.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [CompanyIPOrDevEnvAuthorizeFilter]
    public class TestCaseController : BaseController
    {
        [Route("Debug/TestCase/A001")]
        [HttpGet]
        public object TestCase001()
        {
            return new 
            {
                msg = "Hello"
            };
        }


        [Route("Debug/TestCase/RuntimeInfo")]
        [HttpGet]
        public object GetRuntimeInfo()
        {
            return new
            {
                WorkingSet = $"{(Environment.WorkingSet / 1024 / 1024)} MB",
                InstanceName = RuntimeHelper.GetK8sInstanceName(),
            };
        }

    }
}
