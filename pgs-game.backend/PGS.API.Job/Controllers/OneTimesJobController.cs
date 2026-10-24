using game.Entity.Game;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using target.BL.Interface;
using target.BL.Services;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Web.Modules.Filter;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Threading.Tasks;

namespace target.API.Job.Controllers
{
    [JobApiMagicPasswordFilter]
    public class OneTimesJobController : BaseController
    {
        public IOneTimesJobService OneTimesJobService { get; set; }


        /// <summary>
        /// 檢查是否有執行時間到的一次性排程要執行
        /// </summary>
        [Route("jobs/one-times")]
        [HttpGet]
        [ApiConfig(IsHeaderLogEnable = false)]
        public string CheckOneTimesJobToExecute()
        {
            _ = OneTimesJobService.CheckOneTimesJobToExecute();
            return "排程已非同步觸發";
        }

    }
}
