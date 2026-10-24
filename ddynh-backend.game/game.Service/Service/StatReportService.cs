using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.Users;
using game.Infrastructure;

namespace game.Service.Service
{
    public class StatReportService : BaseService , IStatReportService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;

        public StatReportService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }

        public List<CompanyReportSPResultModel> GetCompanyReport(QueryCompanyReportRequestModel filter)
        {
            SqlParameter[] paras = 
                {
                    new SqlParameter("@intAdminID",filter.AdminID),
                    new SqlParameter("@intAreaID",filter.AreaID),
                    new SqlParameter("@intPlatformID",filter.PlatformID),
                    new SqlParameter("@dtStatTime",filter.BeginTime),
                    new SqlParameter("@dtEndTime",filter.EndTime),
                };

            string SP_Name = "dbo.[PrWeb_Stat_GetCompanyReport]";
            var query = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<CompanyReportSPResultModel>(SP_Name, paras).ToList();
            return query;
        }

    }
}
