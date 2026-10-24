using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FW.Core.Helper;
using game.Entity.AdminSys;
using game.Entity.Stat;
using game.Service;

namespace game.GameAPI.Controllers
{
    public class AreaStatController : BaseAPIController
    {
        private IAreaStatService _asService;


        public AreaStatController(IAreaStatService asService)
        {
            _asService = asService;
        }

        [HttpGet]
        public PagedList<AreaDataDay> GetAreaDataDay(int operatorID, int platformID, int areaID, DateTime? beginTime = null, DateTime? endTime = null, int pageIndex = 1, int pageSize = 20, SortName? sortName = SortName.ChargeAmount, SortLifting? sortLifting = SortLifting.DESC)
        {
            return _asService.GetAreaDataDay(operatorID, platformID, areaID, beginTime, endTime, pageIndex, pageSize, sortName, sortLifting);
        }

        [HttpGet]
        public List<AreaDataDaySummary> GetAreaDataDaySummary(int? platformID, int? areaID, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _asService.GetAreaDataDaySummary(platformID, areaID, beginTime, endTime);
        }

        [HttpGet]
        public PagedList<AreaDataMonth> GetAreaDataMonth(int platformID, int areaID, DateTime? beginTime = null, DateTime? endTime = null, int pageIndex = 1, int pageSize = 20, SortName? sortName = SortName.ChargeAmount, SortLifting? sortLifting = SortLifting.DESC)
        {
            return _asService.GetAreaDataMonth(platformID, areaID, beginTime, endTime, pageIndex, pageSize, sortName, sortLifting);
        }

        [HttpGet]
        public List<DailyMoneyLog> GetDailyMoneyLog(int operatorID, int? platformID, int? areaID, string beginTime, string endTime)
        {
            return _asService.GetDailyMoneyLog(operatorID, platformID, areaID, beginTime, endTime);
        }
    }
}