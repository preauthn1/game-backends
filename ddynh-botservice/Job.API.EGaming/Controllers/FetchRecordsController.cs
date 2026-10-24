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
    public class FetchRecordsController : BaseController
    {
        public IEGamingJobTaskService JobTaskService { get; set; }


        [Route("fetch-records/{platformID}/yesterday")]
        [HttpPost, HttpGet]
        public object FetchByTaiwanYesterday(int platformID)
        {
            JobTaskService.FetchByTaiwanYesterday(platformID);
            return "OK";
        }



        [Route("fetch-records/{platformID}/hours/{hours_value}")]
        [HttpPost, HttpGet]
        public object FetchBetRecordsAgainPreviousHours(int platformID , int hours_value = 1)
        {
            JobTaskService.FetchBetRecordsSinglePlatform(new JobTriggerFetchBetRecordParameterModel
            {
                PlatformID = platformID,
                BeginTimeUTC = DateTime.UtcNow.AddHours(-1 * hours_value)
            });

            return "OK";
        }



        [Route("fetch-records/{platformID}/date-to-date")]
        [HttpPost, HttpGet]
        public object FetchByTaiwanTargetDate(int platformID, DateTime? begin = null, DateTime? end = null)
        {
            if (!begin.HasValue)
                return "缺少參數 begin";

            if (!end.HasValue)
                end = begin.Value.Date.AddDays(1);

            JobTaskService.FetchByTaiwanTimeFromDateToDate(begin.Value , end.Value , platformID);
            return "OK";
        }



        [Route("fetch-records/{platformID}/date")]
        [HttpPost, HttpGet]
        public object FetchByTaiwanTargetDate(int platformID , DateTime? date = null)
        {
            if (!date.HasValue)
                return "缺少參數 date";

            JobTaskService.FetchByTaiwanTargetDate(date.Value , platformID);
            return "OK";
        }


        [Route("fetch-records/{platformID}")]
        [HttpPost , HttpGet]
        public object FetchRecords(int platformID ,[FromUri] JobTriggerFetchBetRecordParameterModel model)
        {
            model = model ?? new JobTriggerFetchBetRecordParameterModel();
            model.PlatformID = platformID;

            JobTaskService.FetchBetRecordsSinglePlatform(model);
            return "OK";
        }


        [Route("daily-correct-check/{platformID}")]
        [HttpPost, HttpGet]
        public object CheckDataIsCorrect(int platformID)
        {
            JobTaskService.CheckYesterdayDataIsCorrect(platformID);
            return "OK";
        }

        
        [Route("correct-check/{platformID}")]
        [HttpPost, HttpGet]
        public object CheckDataIsCorrect(int platformID , DateTime? date = null)
        {
            if (!date.HasValue)
                return "缺少參數 date";

            JobTaskService.CheckTargetDataIsCorrect(platformID , date.Value);
            return "OK";
        }


    }
}
