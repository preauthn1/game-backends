using game.Entity.Game;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using target.BL.Interface;
using target.BL.Services;
using target.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Web.Modules.Filter;
using System;
using System.Collections.Generic;
using example.invalid;
using example.invalid;
using example.invalid.Http;

namespace target.API.Public.Controllers
{

    [Route("v{version:int}")]
    public class ReverseAPIReditectController : BaseController
    {

        public IVendorCustomizedRequirementService VendorCustomizedRequirementService { get; set; }


        /// <summary>
        /// OPS 商戶客製的API (啟動遊戲)
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiConfig(ApiCategory = Models.Enums.ApiConfigCategoryEnum.VendorPlaformCustomizedAPI)]
        [Route("OPS/launcher")]
        [HttpGet]
        public ActionResult OPS_GetGameLaunchUrl([FromQuery] OPSPlatformGameLaunchRequestParameter model)
        {
            var result = VendorCustomizedRequirementService.GenerateLaunchLinkOPS(model);
            return Redirect(result.url);
        }


        /// <summary>
        /// OPS 商戶客製的API (回放)
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiConfig(ApiCategory = Models.Enums.ApiConfigCategoryEnum.VendorPlaformCustomizedAPI)]
        [Route("OPS/game_history")]
        [HttpGet]
        public ActionResult OPS_GetScreenshotReplayUrl([FromQuery] OPSPlatformScreenReplayRequestParameter model)
        {
            var result = VendorCustomizedRequirementService.GenerateScreenshotReplayLinkOPS(model);
            return Redirect(result.url);
        }


    }
}
