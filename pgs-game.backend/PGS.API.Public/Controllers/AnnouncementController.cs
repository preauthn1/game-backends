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
    public class AnnouncementController : BaseController
    {

        public IAnnouncementService AnnouncementService { get; set; }

        /// <summary>
        /// 取得未讀公告
        /// </summary>
        [Route("message/announcement")]
        [SwaggerDocumentAccessTokenRequired]
        [HttpGet]
        public AnnouncementViewModel GetNotReadAnnoucementList()
        {
            int playerID = GetLobbyTokenPlayerID();

            SetLanguageByLobbyToken();

            var query = AnnouncementService.GetNotReadAnnoucementList(playerID);
            return query;
        }


        /// <summary>
        /// 訊息已讀
        /// </summary>
        [Route("message/announcement/read")]
        [SwaggerDocumentAccessTokenRequired]
        [HttpPost]
        public void RedEnvelopeLuckyDraw([FromBody] AnnouncementReadRequestModel model)
        {
            int playerID = GetLobbyTokenPlayerID();
            AnnouncementService.SetMessageRead(playerID , model);
            return;
        }

    }
}
