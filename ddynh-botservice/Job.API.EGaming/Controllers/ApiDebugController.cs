using game.Entity.ViewModels;
using game.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using example.invalid.Http;
using System.Web.Http;

namespace Job.API.EGaming.Controllers
{
    public class ApiDebugController : BaseController
    {
        public IEGamingJobTaskService JobTaskService { get; set; }


        [Route("debug/fetch-records/{platformID}")]
        [HttpPost, HttpGet]
        public object GetBetRecordApiResult(int platformID , DateTime? date = null)
        {
            if (!date.HasValue)
                return "缺少參數 date";

            DateTime targetDate_TW = date.Value;
            var queryApi = JobTaskService.GetBetRecordApiResult(platformID , targetDate_TW);
            return queryApi;
        }


    }
}
