using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using Microsoft.AspNetCore.Cors;
using target.Web.Modules.Filter;
using target.Utility.Helper;
using target.Utility;

namespace target.API.Public.Controllers
{
    [Route("v{version:int}")]
    public class LobbyController : BaseController
    {
        public IGameLaunchService GameLaunchService { get; set; }

        public IInternalUtilityService InternalUtilityService { get; set; }

        public ILobbyService ILobbyService { get; set; }


        [SwaggerDocumentAccessTokenRequired]
        [LobbyTokenAuthorizeFilter]
        [Route("lobby/balance")]
        [HttpGet]
        public LobbyApiGetGamesInfoViewModel GetPlayerBalanceOnly([FromQuery] LobbyApiGetGamesRequestModel model)
        {
            var query = ILobbyService.GetPlayerBalanceOnly(model);
            return query;
        }


        [SwaggerDocumentAccessTokenRequired]
        [LobbyTokenAuthorizeFilter]
        [Route("lobby/settings")]
        [HttpGet]
        public LobbyApiGetGamesInfoViewModel GetBalanceAndLobbySettings([FromQuery] LobbyApiGetGamesRequestModel model)
        {
            var query = ILobbyService.GetLobbySettings(model);
            return query;
        }


        [SwaggerDocumentAccessTokenRequired]
        [LobbyTokenAuthorizeFilter]
        [Route("lobby/games")]
        [HttpGet]
        public LobbyApiGetGamesInfoViewModel GetLobbyGames([FromQuery] LobbyApiGetGamesRequestModel model)
        {
            var query = ILobbyService.GetLobbyGamesAndSettings(model);
            return query;
        }


        [SwaggerDocumentAccessTokenRequired]
        [LobbyTokenAuthorizeFilter]
        [Route("lobby/game-link")]
        [HttpGet]
        public LobbyGenerateLinkResultModel GenerateGameLinkForLobby([FromQuery] LobbyApiGenerateLinkParameterModel model)
        {
            var query = GameLaunchService.GenerateGameLinkForLobby(model);
            return query;
        }


        [SwaggerDocumentAccessTokenRequired]
        [LobbyTokenAuthorizeFilter]
        [Route("iframe/game-link")]
        [HttpGet]
        public LobbyGenerateLinkResultModel GenerateGameLinkForIframe([FromQuery] LobbyApiGenerateLinkParameterModel model)
        {
            var query = GameLaunchService.GenerateGameLinkForIframe(model);
            return query;
        }


        /// <summary>
        /// 用來測試是否打得通 (e.g. Cloudflare IP區域限制阻擋)
        /// 網址不要亂改。cloudflare有用關鍵字設定規則
        /// </summary>
        [Route("test/test-my-ip")]
        [ApiConfig(IsLogApiEnable = false)]
        [HttpPost]
        public object TestClientIP()
        {
            return new 
            {
                MyIP = IPHelper.GetIP(),
            };
        }

    }
}
