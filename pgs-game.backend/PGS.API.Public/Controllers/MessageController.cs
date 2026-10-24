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
    [LobbyTokenAuthorizeFilter]
    [Route("v{version:int}")]
    public class MessageController : BaseController
    {

        public IMessageService MessageService { get; set; }


        /// <summary>
        /// 發送給玩家的訊息
        /// </summary>
        [Route("message/player-messages")]
        [SwaggerDocumentAccessTokenRequired]
        [HttpGet]
        public MessageViewModel GetNotReadPlayerMessages()
        {
            int playerID = GetLobbyTokenPlayerID();

            SetLanguageByLobbyToken();

            var query = MessageService.GetPlayerMessages(playerID);
            return query;
        }


        /// <summary>
        /// 訊息已讀
        /// </summary>
        [Route("message/player-messages/read")]
        [SwaggerDocumentAccessTokenRequired]
        [HttpPost]
        public void SetMessageRead([FromBody] MessageReadRequestModel model)
        {
            int playerID = GetLobbyTokenPlayerID();
            MessageService.SetMessageRead(playerID , model);
            return;
        }


        /// <summary>
        /// 針對特定訊息提示(type) 設定已讀
        /// </summary>
        [Route("message/messages-hint/{type}/read")]
        [SwaggerDocumentAccessTokenRequired]
        [HttpPost]
        public void SetMessageHintRead(int type)
        {
            int playerID = GetLobbyTokenPlayerID();
            MessageService.SetMessageHintRead(playerID, type);
            return;
        }

    }



}
