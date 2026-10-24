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
    public class CommonDailyRoutineJobController : BaseController
    {
        public ICommonRoutineJobService CommonRoutineJobService { get; set; }


        /// <summary>
        /// 一天跑一次的雜項排程 (凌晨時段)
        /// </summary>
        [Route("common-jobs/daily-routine/early-morning")]
        [HttpGet , HttpPost]
        public string DailyRoutineJobs()
        {
            var result = CommonRoutineJobService.RoutineJobsEarlyMorning_Every24Hours();
            return result;
        }


        /// <summary>
        /// 8小時跑一次的雜項排程
        /// </summary>
        [Route("common-jobs/8-hours-routine")]
        [HttpGet, HttpPost]
        public string RoutineJobsEach8Hours()
        {
            var result = CommonRoutineJobService.RoutineJobs_Every8Hours();
            return result;
        }

        /// <summary>
        /// 30分鐘跑一次的雜項排程
        /// </summary>
        [Route("common-jobs/30-minutes-routine")]
        [HttpGet, HttpPost]
        public string RoutineJobs_Every30Minutes()
        {
            var result = CommonRoutineJobService.RoutineJobs_Every30Minutes();
            return result;
        }

    }
}
