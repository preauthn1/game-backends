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
using target.Utility;
using target.Utility.Helper;
using target.Web.Modules.Filter;

namespace target.API.Game.Controllers
{
    [DoNotCheckCertificate]
    public class HomeController : BaseController
    {

        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiConfig(IsLogApiEnable = false)]
        [Route("")]
        [HttpGet]
        public object HomePage(string password = "")
        {
            bool isPasswordIncorrect = password != SystemConstant.PublicAPI_SignKey_MagicPassword;

            if (isPasswordIncorrect && !IPHelper.IsCompanyIP())
            {
                return RuntimeHelper.GetHideInformationForProduction();
            }

            return RuntimeHelper.GetHomePageDebugInformation();
        }              

    }
}
