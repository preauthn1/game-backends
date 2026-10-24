using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.API.Public.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v{version:int}")]
    public class DemoSiteUtilityController : BaseController
    {
        public IInternalUtilityService InternalUtilityService { get; set; }

        public IGameLinkService GameLinkService { get; set; }


        /// <summary>
        /// 取得指定商戶的所有 開通遊戲&服務器
        /// 這隻API 基本上是for demo站使用
        /// </summary>
        [Route("internal-api/merchant/{merchantID}/servers")]
        [Route("demo-site/merchant/{merchantID}/servers")]
        [HttpGet]
        public List<DemoGameCategoryViewModels> GetServers(int merchantID)
        {
            var query = InternalUtilityService.GetMerchantServers(merchantID);
            return query;
        }


        [Route("demo-site/game/link")]
        [HttpGet]
        public GameLinkGenerateResultViewModel GenerateGameUrl([FromQuery] GameLaunchVendorApiTransferModeRequestModel model)
        {
            model.IsFromDemoSite = true;
            var result = GameLinkService.GenerateTransferModeLaunchLink(model);
            return result;
        }


        [Route("demo-site/game/launch-link")]
        [HttpGet]
        public GameLinkGenerateResultViewModel GenerateSingleWalletGameLaunchUrl([FromQuery] GameLaunchVendorApiSingleWalletModeRequestModel model)
        {
            var result = GameLinkService.GenerateSingleWalletLaunchLink(model);
            return result;
        }

    }
}
