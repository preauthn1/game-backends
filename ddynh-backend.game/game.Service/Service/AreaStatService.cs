using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.AdminSys;
using game.Entity.Stat;
using game.Infrastructure;

namespace game.Service
{
    public class AreaStatService : IAreaStatService
    {

        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;

        public AreaStatService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }
        public PagedList<AreaDataDay> GetAreaDataDay(int operatorID, int platformID, int areaID, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize, SortName? sortName, SortLifting? sortLifting)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                     new SqlParameter("@intAdminID",operatorID),
                                     new SqlParameter("@intPlatformID",platformID),
                                     new SqlParameter("@intAreaID  ",areaID),
                                     new SqlParameter("@dtStatTime",beginTime.HasValue ? (object)beginTime.Value.ToString("yyyy-MM-dd"):DBNull.Value ),
                                     new SqlParameter("@dtEndTime",endTime.HasValue ? (object)endTime.Value.ToString("yyyy-MM-dd"):DBNull.Value),
                                     new SqlParameter("@tnyColumns",(int)sortName),
                                     new SqlParameter("@tnySort",(int)sortLifting),
                                     new SqlParameter("@intPageIndex",pageIndex),
                                     new SqlParameter("@intPageSize",pageSize),
                                     new SqlParameter("@intPageCount",pageCount),
                                   };
            paras.LastOrDefault().Direction = System.Data.ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<AreaDataDay>("dbo.[PrWeb_Stat_GetAreaDataStat_Day]", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

            return new PagedList<AreaDataDay>(list, pageIndex, pageSize, pageCount);
        }


        public List<AreaDataDaySummary> GetAreaDataDaySummary(int operatorID, int? platformID, int? areaID, DateTime? beginTime, DateTime? endTime)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID",operatorID),
                                    new SqlParameter("@intPlatformID",platformID),
                                    new SqlParameter("@intAreaID  ",areaID),
                                    new SqlParameter("@dtStatTime",beginTime),
                                    new SqlParameter("@dtEndTime",endTime),
                                    new SqlParameter("@intPageCount",pageCount),
                                   };
            paras.LastOrDefault().Direction = System.Data.ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<AreaDataDaySummary>("dbo.[PrWeb_Stat_GetAreaDataSummary_Day]", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

            return new List<AreaDataDaySummary>(list);
        }

        public List<DailyMoneyLog> GetDailyMoneyLog(int operatorID, int? platformID, int? areaID, string beginTime, string endTime)
        {
            SqlParameter[] paras = {
                                     new SqlParameter("@intAdminID",operatorID),
                                     new SqlParameter("@intPlatformID",platformID),
                                     new SqlParameter("@intAreaID  ",areaID),
                                     new SqlParameter("@dtStartTime",beginTime),
                                     new SqlParameter("@dtEndTime",endTime),
                                   };
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<DailyMoneyLog>("dbo.[PrWeb_Stat_GetDailyMoneyLog]", paras);
            return new List<DailyMoneyLog>(list);
        }


        public PagedList<AreaDataMonth> GetAreaDataMonth(int operatorID, int platformID, int areaID, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize, SortName? sortName, SortLifting? sortLifting)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                     new SqlParameter("@intAdminID",operatorID),
                                     new SqlParameter("@intPlatformID",platformID),
                                     new SqlParameter("@intAreaID  ",areaID),
                                     new SqlParameter("@dtStatTime",beginTime),
                                     new SqlParameter("@dtEndTime",endTime),
                                     new SqlParameter("@tnyColumns",(int)sortName),
                                     new SqlParameter("@tnySort",(int)sortLifting),
                                     new SqlParameter("@intPageIndex",pageIndex),
                                     new SqlParameter("@intPageSize",pageSize),
                                     new SqlParameter("@intPageCount",pageCount),
                                   };
            paras.LastOrDefault().Direction = System.Data.ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<AreaDataMonth>("dbo.[PrWeb_Stat_GetAreaDataStat_Month]", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

            return new PagedList<AreaDataMonth>(list, pageIndex, pageSize, pageCount);
        }
    }
}
