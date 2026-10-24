using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.Paging;
using target.Web.Modules.Filter;
using target.API.AdminSys;

namespace target.API.Vendor.Controllers
{
    [Route("v{version:int}")]
    public class EventRecordController : BaseController
    {

        public IPromotionRecordService PromotionRecordService { get; set; }


        /// <summary>
        /// 4.8.1 Get event award records (取得活动派奖记录)
        /// </summary>
        [SwaggerApiDescriptionEN("Get event award records. <BR>Some event rewards will have different creation time  and Receive time. <BR>Our accounting report will be based on the `Receive time` .<BR>Data older than 90 days will be deleted.")]
        [SwaggerApiDescriptionCN("取得活动派奖记录 <BR>部分活动奖励会分成资料建立时间（CreateTime)与领取时间 (ReceiveTime)。<BR>我方帐务计算会以 `领取时间` 为基准<BR>超过90天的资料将会被系统删除")]
        [Route("event/prize-records")]
        [ApiConfig(IsResponseLogEnable = false)]
        [HttpGet]
        public PagedList<EventPrizeRecordVendorApiViewModel> GetPrizeRecords([FromQuery] PromotionRecordRequestModel filter)
        {
            var query = PromotionRecordService.GetPrizeRecords(filter);
            return query;
        }

    }
}
