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

namespace target.API.Public.Controllers
{
    [Route("v{version:int}")]
    public class PlayerBehaviourController : BaseController
    {

        public ILobbyPlayerTagService LobbyPlayerTagService { get; set; }


        [SwaggerDocumentAccessTokenRequired]
        [LobbyTokenAuthorizeFilter]
        [Route("lobby/player-tag")]
        [HttpPost]
        public void AddPlayerTag([FromBody] LazyCommonRequestModel model)
        {
            int playerID = GetLobbyTokenPlayerID();
            LobbyPlayerTagService.AddPlayerTagFromLobby(playerID, model);
            return;
        }

    }
}
