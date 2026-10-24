using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.Enums;
using target.API.AdminSys;
using target.Web.Modules.Filter;

namespace target.API.Vendor.Controllers
{
    [Route("v{version:int}")]
    public class GamesController : BaseController
    {
        public IGamesService GameInfoService { get; set; }



        /// <summary>
        /// 4.6.1 Get game list (取得游戏列表)
        /// </summary>
        [SwaggerApiDescriptionEN("Get enable game list.<BR>This API has limited access times. Up to `30 times` per hour. <BR>You can put locale:`en-us` `zh-cn` in Http Header. then get English or Chinese game name.")]
        [SwaggerApiDescriptionCN("取得运营商已开放的游戏清单(仅返回状态设为开启的游戏)<BR>此API有限制存取次數。每小時最多請求 `30次`<BR>您可以在HttpHeader放入locale:`en-us` `zh-cn`。得到英文或中文游戏名称 ")]
        [Route("games")]
        [ApiConfig(IsResponseLogEnable = false)]
        [HttpGet]
        public List<GameInfoViewModel> GetMerchantEnableAllGames([FromQuery]GameInfoRequestModel model)
        {
            var query = GameInfoService.GetMerchantEnableGames(model);
            return query;
        }

    }
}
