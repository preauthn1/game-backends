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
    public class SystemMonitorController : BaseController
    {
        public ISystemMonitorService SystemMonitorService { get; set; }


        /// <summary>
        /// 檢查資料庫硬碟空間
        /// </summary>
        [Route("system-monitor/disk-size")]
        [HttpGet , HttpPost]
        public string CheckDatabaseDiskSize()
        {
            string result = SystemMonitorService.CheckDatabaseDiskSize();
            return result;
        }



        /// <summary>
        /// 檢查報表排程是否有 停止/異常
        /// </summary>
        [Route("system-monitor/report-job/healthy")]
        [HttpGet, HttpPost]
        public string CheckReportJobStatusHealthy()
        {
            string result = SystemMonitorService.CheckReportJobStatusHealthy();
            return result;
        }


    }
}
