using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.AdminSys;
using game.Entity.Stat;

namespace game.Service
{
    public interface IAreaStatService
    {
        /// <summary>
        /// 查询渠道统计日报表相关数据
        /// </summary>
        /// <param name="platformID">平台ID</param>
        /// <param name="areaID">渠道ID</param>
        /// <param name="beginTime">开始时间范围</param>
        /// <param name="endTime">结束时间范围</param>
        /// <returns></returns>
        PagedList<AreaDataDay> GetAreaDataDay(int operatorID, int platformID, int areaID, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize, SortName? sortName, SortLifting? sortLifting);

        PagedList<AreaDataMonth> GetAreaDataMonth(int platformID, int areaID, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize, SortName? sortName, SortLifting? sortLifting);

        List<AreaDataDaySummary> GetAreaDataDaySummary(int? platformID, int? areaID, DateTime? beginTime, DateTime? endTime);

        /// <summary>
        /// 全平台金幣異動歷程
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<DailyMoneyLog> GetDailyMoneyLog(int operatorID, int? platformID, int? areaID, string beginTime, string endTime);
    }
}
