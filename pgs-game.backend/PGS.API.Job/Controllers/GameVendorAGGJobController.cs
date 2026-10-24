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
using System.Threading.Tasks;

namespace target.API.Job.Controllers
{
    [JobApiMagicPasswordFilter]
    public class GameVendorAGGJobController : BaseController
    {
        public IAGGBetRecordJobService AGGBetRecordJobService { get; set; }



        /// <summary>
        /// 強制AGG完成結算通知
        /// </summary>
        [Route("bet-record/agg/trigger-settle")]
        [HttpGet]
        public string CheckPendingRecordAndTriggerSettle_GET([FromQuery]QueryPendingRecordJobParameter param , [FromQuery] bool isAsync = true)
        {
            var result = ExecuteCheckPendingRecordAndTriggerSettle(param , isAsync);
            return result;
        }

        /// <summary>
        /// 強制AGG完成結算通知
        /// </summary>
        [Route("bet-record/agg/trigger-settle")]
        [HttpPost]
        public string CheckPendingRecordAndTriggerSettle_POST([FromBody] QueryPendingRecordJobParameter param, [FromQuery] bool isAsync = true)
        {
            var result = ExecuteCheckPendingRecordAndTriggerSettle(param, isAsync);
            return result;
        }

        /// <summary>
        /// AGG處理異常注單
        /// </summary>
        [Route("bet-record/agg/exceptional-wagers")]
        [HttpGet, HttpPost]
        public string CleanUpAGGExceptionalWagers(bool isAsync = true)
        {
            if (isAsync)
            {
                AsyncAndTryCatch(() =>
                {
                    string result = AGGBetRecordJobService.CleanUpAGGExceptionalWagers();
                });

                return "isAsync";
            }
            else
            {
                string result = AGGBetRecordJobService.CleanUpAGGExceptionalWagers();
                return result;
            }
        }


        /// <summary>
        /// 強制特定某一筆注單 AGG完成結算通知
        /// </summary>
        [Route("bet-record/agg/trigger-settle/by-orderNo")]
        [HttpGet, HttpPost]
        public string ForceSingleRecordTriggerAGGSettle(string betRecordNoPGS)
        {
            string response = AGGBetRecordJobService.ForceSingleRecordTriggerAGGSettle(betRecordNoPGS);
            return response;
        }


        private string ExecuteCheckPendingRecordAndTriggerSettle(QueryPendingRecordJobParameter param, [FromQuery] bool isAsync = true)
        {
            param = param ?? new QueryPendingRecordJobParameter();

            if (isAsync)
            {
                AsyncAndTryCatch(() =>
                {
                    string result = AGGBetRecordJobService.CheckPendingRecordAndTriggerSettle(param);
                });

                return "isAsync";
            }
            else
            {
                string result = AGGBetRecordJobService.CheckPendingRecordAndTriggerSettle(param);
                return result;
            }
        }


    }
}
