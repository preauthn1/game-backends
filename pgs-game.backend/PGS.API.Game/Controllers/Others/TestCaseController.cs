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

namespace target.API.Game.Controllers
{
    [DoNotCheckCertificate]
    [CompanyIPOrDevEnvAuthorizeFilter]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TestCaseController : BaseController
    {

      

    }
}
