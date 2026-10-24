using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.API.AdminSys;

namespace target.API.Vendor.Controllers
{
    [Route("v{version:int}")]
    public class GameLaunchController : BaseController
    {
        public IGameLinkService GameLinkService { get; set; }

        /// <summary>
        /// 2.1.1 Get game launch url (取得游戏启动连结) (TransferMode)
        /// </summary>
        [SwaggerApiDescriptionEN("Get the link of a game.")]
        [SwaggerApiDescriptionCN("取得游戏网址")]
        [Route("game/link")]
        [ApiExplorerSettings(GroupName = "tfMode")]
        [HttpGet]
        public GameLinkGenerateResultViewModel GenerateGameUrl([FromQuery] GameLaunchVendorApiTransferModeRequestModel model)
        {
            var result = GameLinkService.GenerateTransferModeLaunchLink(model);
            return result;
        }

        /// <summary>
        /// 2.1.2 Get game launch url (取得游戏启动连结) (SeamlessMode)
        /// </summary>
        [SwaggerApiDescriptionEN("Get the link of a game.<BR>`*NOTE` Before you call launch API. You need implement 5.1.1 API first (wallet API).")]
        [SwaggerApiDescriptionCN("取得游戏网址<BR>`*NOTE`在调用启动游戏API之前，您需要先实现5.1.1 API（wallet API）")]
        [Route("game/launch-link")]
        [ApiExplorerSettings(GroupName = "swMode")]
        [HttpGet]
        public GameLinkGenerateResultViewModel GenerateSingleWalletGameLaunchUrl([FromQuery] GameLaunchVendorApiSingleWalletModeRequestModel model)
        {
            var result = GameLinkService.GenerateSingleWalletLaunchLink(model);
            return result;
        }
    }
}
