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
using target.Models.CoreLibs;

namespace target.API.Job.Controllers
{
    [JobApiMagicPasswordFilter]
    public class CallbackRetrySWController : BaseController
    {
        public ISingleWalletCallRetryJobQueueService BetRecordStatusChangeNotifyService { get; set; }

        /// <summary>
        /// 補發callback 未結算/取消 的注單
        /// </summary>
        [Route("bet-record/retry/queue")]
        [HttpGet]
        [ApiConfig(IsHeaderLogEnable = false)]
        public void BetReocrdNotifyQueueRetryProcess()
        {
            BetRecordStatusChangeNotifyService.BetReocrdNotifyQueueRetryProcess();
        }

        /// <summary>
        /// (手動執行) 觸發單一單號 未結算/未通知 的注單
        /// </summary>
        [Route("bet-record/retry/{specificBetRecordNoPGS}/queue")]
        [HttpGet]
        public void BetReocrdNotifyQueueRetryProcess_SpecificOrderNo(string specificBetRecordNoPGS)
        {
            BetRecordStatusChangeNotifyService.BetReocrdNotifyQueueRetryProcess_SpecificOrderNo(specificBetRecordNoPGS);
        }


        /// <summary>
        /// (手動觸發) 將EazyTemp資料 轉成NotifyQueue
        /// </summary>
        [Route("bet-record/eazyTempValue-convert-to-notifyQueue")]
        [HttpPost]
        public CRUDResultModel ConvertEazyTempValueToNotifyQueue([FromBody]EazyTempValueConvertNotifyQueueRequestModel model)
        {
            var result = BetRecordStatusChangeNotifyService.ConvertEazyTempValueToNotifyQueue(model);
            return result;
        }

    }
}
