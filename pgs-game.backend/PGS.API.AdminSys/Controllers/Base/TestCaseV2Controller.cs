using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using example.invalid;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Lck.Utility.Extensions.Helper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using target.BL.Interface;
using target.Models.ViewModels;
using target.Models.ViewModels.Base;
using target.Utility;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;
using target.Utility.Extensions;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using target.Web.Modules.Filter;
using target.Models;
using target.BL.Services;
using target.Models.ViewModels.GameVendor;
using System.Security.Policy;
using target.Models.Models;
using target.DA.Repository;
using target.Utility.Extensions.Web;
using game.Entity.Game;
using example.invalid.Http;
using example.invalid;
using System.Text;

namespace target.API.AdminSys.Controllers
{
    /// <summary>
    /// 沒有繼承 BaseController
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [CompanyIPOrDevEnvAuthorizeFilter]
    [Produces("application/json")]
    [ApiController]
    public class TestCaseV2Controller : ControllerBase
    {
        [Consumes("application/json")]
        [Route("testcaseV2/test-post")]
        [HttpPost]
        public object TestPost_002()
        {
            return new
            {
                Text = "這隻API是 POST，請參考Log紀錄"
            };
        }


        [Consumes("application/x-www-form-urlencoded")]
        [Route("testcaseV2/test-post-form")]
        [HttpPost]
        public object TestPost_002_Form([FromForm] object myText)
        {
            return new
            {
                Text = "請參考Log紀錄"
            };
        }



    }

}