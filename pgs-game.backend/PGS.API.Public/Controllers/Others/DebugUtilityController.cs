using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using Microsoft.AspNetCore.Cors;
using target.Utility.Extensions;
using example.invalid;
using example.invalid.Http;
using Microsoft.AspNetCore.Mvc;
using target.Web.Modules.Filter;
using target.Utility.Helper;
using target.Models;

namespace target.API.Public.Controllers
{
    [Route("v{version:int}")]
    public class DebugUtilityController : BaseController
    {

        public IDebugUtilityService DebugUtilityService { get; set; }

       
        [Route("debug/speed-test-info")]
        [HttpGet]
        public SppedTestDebugInfo GetSeeedTestInfo()
        {
            int playerID = TryGetLobbyTokenPlayerID();
            var query = DebugUtilityService.GetSeeedTestInfo(playerID);
            return query;
        }

    }
}
