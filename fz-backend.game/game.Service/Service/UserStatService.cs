using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Repository;
using game.Entity.Stat;
using game.Infrastructure;

namespace game.Service
{
    public class UserStatService : IUserStatService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;
        public UserStatService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }



        public List<RegUserLog> GetRegUserLog(int operatorID, int? platformID, DateTime? beginTime, DateTime? endTime)
        {
            //return _repoFactory.CreateRepository<RegUserLog>()
            //    .Entities
            //    .Where(m => (platformID == null || platformID == m.PlatformID) &&
            //                (beginTime == null || m.StatTime >= beginTime) &&
            //                (endTime == null || m.StatTime <= endTime)
            //    )
            //    .OrderByDescending(m => m.StatTime)
            //    .ToList();

            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID", operatorID),
                                    new SqlParameter("@intPlatformID",platformID),
                                    new SqlParameter("@dtStartTime",beginTime),
                                    new SqlParameter("@dtEndTime",endTime)
                                   };
            var _result = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<RegUserLog>("[dbo].[PrWeb_Stat_GetRegUserLog]", paras).ToList();
            return _result;



        }

        public List<LogonUserLog> GetLogonUserLog(int operatorID, int? platformID, DateTime? beginTime, DateTime? endTime)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID", operatorID),
                                    new SqlParameter("@intPlatformID",platformID),
                                    new SqlParameter("@dtStartTime",beginTime),
                                    new SqlParameter("@dtEndTime",endTime)
                                   };
            var _result = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<LogonUserLog>("[dbo].[PrWeb_Stat_GetLogonUserLog]", paras).ToList();
            return _result;
        }


        public FW.Core.Helper.PagedList<Entity.Users.LogonLog> GetUserLogonLog(DateTime? beginTime, DateTime? endTime, int? userID, int pageIndex, int pageSize)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                     new SqlParameter("@intUserID",userID),
                                     new SqlParameter("@intBeginTime",beginTime),
                                     new SqlParameter("@intEndTime",endTime),
                                     new SqlParameter("@intPageIndex",pageIndex),
                                     new SqlParameter("@intPageSize",pageSize),
                                     new SqlParameter("@intPageCount",pageCount),
                                   };
            paras.LastOrDefault().Direction = System.Data.ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<Entity.Users.LogonLog>("dbo.[PrWeb_Game_GetHallLog]", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);
            return new FW.Core.Helper.PagedList<Entity.Users.LogonLog>(list, pageIndex, pageSize, pageCount);
        }

        /// <summary>
        /// 留存率
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<KeepLog> GetKeepLog(int adminId, int? platformID, DateTime? beginTime, DateTime? endTime)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID",adminId),
                                    new SqlParameter("@intPlatformID",platformID),
                                    new SqlParameter("@intAreaID", null),
                                    new SqlParameter("@dtStartTime",beginTime),
                                    new SqlParameter("@dtEndTime",endTime)
                                   };
            var _result = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<KeepLog>("[dbo].[PrWeb_Stat_GetKeepLog]", paras).ToList();
            return _result;
        }

        public List<ChargeKeepLog> GetChargeKeepLog(int? hdType)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intHDType",hdType)
                                   };
            var _result = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<ChargeKeepLog>("[dbo].[PrWeb_Stat_GetChargeKeepLog]", paras).ToList();
            return _result;
        }

        public List<ActiveUserKeepLog> GetActiveUserKeepLog()
        {
            var _result = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<ActiveUserKeepLog>("[dbo].[PrWeb_Stat_GetActiveUserKeepLog]").ToList();
            return _result;
        }

        public Dictionary<string, string> SearchUserStatInfo(byte searchType, int count)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@tnySearchType",searchType),
                                    new SqlParameter("@intCount",count)
                                   };
            var dt = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureDataTable("dbo.PrWeb_User_UserStatInfo", paras);

            Dictionary<string, string> list = new Dictionary<string, string>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(dr[0].ToString(), dr[1].ToString());
                }
            }
            return list;
        }

        public List<GradeSetting> GetUserGradeCount()
        {
            SqlParameter[] paras = { };
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<GradeSetting>("dbo.[PrWeb_Stat_GetGradeUserCount]", paras).ToList();
            return list;

        }

        public List<GradeSetting> GetGradeSetting()
        {
            return _repoFactory.CreateRepository<GradeSetting>().Entities.OrderBy(m => m.Id).ToList();
        }
    }
}
