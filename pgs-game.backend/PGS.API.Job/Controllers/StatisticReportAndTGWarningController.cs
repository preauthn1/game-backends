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
using target.BL.Services;
using target.Models.Enums;
using target.Models.ViewModels.GameVendor;
using target.Models;

namespace target.API.Job.Controllers
{
    [JobApiMagicPasswordFilter]
    public class StatisticReportAndTGWarningController : BaseController
    {
        public ITelegramStatisticReportNotifyService TelegramStatisticReportNotifyService { get; set; }


        [Route("TG/statistic-notify/W88")]
        [HttpGet , HttpPost]
        public string SendFishGameReportNotify()
        {
            TelegramStatisticReportNotifyService.SendFishGameReportNotify();
            return "OK";
        }


        /// <summary>
        /// 檢查RTP報表是否有異常
        /// </summary>
        [Route("TG/statistic-notify/game-vendor-rtp-unexpected")]
        [HttpGet, HttpPost]
        public string SendGameVendorRTPUnexprectedNotify()
        {
            TelegramStatisticReportNotifyService.SendGameVendorRTPUnexprectedNotify();
            return "OK";
        }


    }
}
