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
using target.Models.Models;

namespace target.API.Public.Controllers
{
    [Route("v{version:int}")]
    public class ScreenshotController : BaseController
    {

        public IAdminSysComplexService AdminSysComplexService { get; set; }


        public IEazyTokenValidateService IEazyTokenValidateService { get; set; }


        /// <summary>
        /// AGGLive遊戲的詳細記錄
        /// </summary>
        [Route("bet-records/screenshot/aggLive")]
        [Route("bet-records/screenshot")]
        [HttpGet]
        public PublicApiBetRecordReplayInfoViewModel GetGameRecordReplayInfo([FromQuery] LazyCommonRequestModel model)
        {
            //toekn合法性檢查
            IEazyTokenValidateService.CheckIsValidToken(model.Token, new EazyTokenValidateRule
            {
                ValidateValueMatch = model.TransactionNo,
                CheckExpiredHourBeforeQueryDB = SystemConstant.ReplayHistoryTokenExpiredHour * 2
            });

            //透過aggLive後台API去查資料
            var query = AdminSysComplexService.GetBetRecordReplayInfoByGameVendor(model);
            return query;
        }

    }
}
