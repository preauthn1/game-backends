using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Web.Modules.Filter;

namespace target.API.Game.Controllers
{
   
    public class GameStatisticSummaryController : BaseController
    {
     
        public IBetSummaryInfoService BetSummaryInfoService { get; set; }


        /// <summary>
        /// 取得指定serverID 時間內的統計數據
        /// </summary>
        [Route("bet-summary/server")]
        [HttpPost]
        public BetSummaryRequestViewModel GetServerListAndSetting([FromBody] BetSummaryRequestModel model)
        {
            var result = BetSummaryInfoService.GetBetSummaryByServer(model);
            return result;
        }

    }
}
