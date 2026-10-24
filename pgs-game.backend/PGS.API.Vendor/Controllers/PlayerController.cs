using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.API.Vendor.Controllers;
using target.API.AdminSys;
using target.Web.Modules.Filter;

namespace target.API.Vendor.Controllers
{
    [Route("v{version:int}")]
    public class PlayerController : BaseController
    {
        public IPlayerVendorService PlayerVendorService { get; set; }


        /// <summary>
        /// 2.2.1 Check player exist (检查用户是否存在)
        /// </summary>
        [SwaggerApiDescriptionEN("Check player account exist in game provider's system.")]
        [SwaggerApiDescriptionCN("检查运营商之特定玩家账号、是否已存在于游戏商")]
        [Route("player/exist")]
        [ApiExplorerSettings(GroupName = "tfMode")]
        [HttpGet]
        public PlayerVendorViewModel CheckIsPlayerAccountExist([FromQuery]QueryPlayerAccountExistRequestModel model)
        {
            var query = PlayerVendorService.IsAccountExist(model);
            return query;
        }


        /// <summary>
        /// 2.2.2 Create player account (建立玩家帐户)
        /// </summary>
        [SwaggerApiDescriptionEN("Create player game account.")]
        [SwaggerApiDescriptionCN("运营商可透过此接口建立玩家账号")]
        [Route("player")]
        [ApiExplorerSettings(GroupName = "tfMode")]
        [HttpPost]
        public PlayerCreateResultVenodrApiViewModel CreateNewPlayer([FromBody]CreatePlayerAccountExistRequestModel model)
        {
            var result = PlayerVendorService.CreateNewPlayer(model);
            return result;
        }


        /// <summary>
        /// 4.9.9 強制登出玩家 (Force logout player)
        /// </summary>
        [SwaggerApiDescriptionEN("Force player's token expired. if player already in the game. player will see error message in next spin (next betting)")]
        [SwaggerApiDescriptionCN("将玩家身分令牌(Token)强制失效。若玩家已于游戏房内，下一次Spin或投注后、將弹出错误讯息并踢出游戏房间。(部分游戏无法生效)")]
        [Route("player/logout")]
        [HttpPost]
        public PlayerLogoutResultViewModel ForcePlayerLogout([FromBody] PlayerLogoutRequestModel model)
        {
            var query = PlayerVendorService.ForcePlayerLogout(model);
            return query;
        }

    }
}
