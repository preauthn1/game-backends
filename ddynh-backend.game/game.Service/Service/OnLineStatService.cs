using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Repository;
using game.Entity.Stat;

namespace game.Service
{
    public class OnLineStatService : IOnLineStatService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;
        public OnLineStatService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }

        public List<ServerOnLineUserLog> GetServerOnLineUserLog(int operatorID, int platformID, int? gameID, int? serverID, DateTime? beginTime, DateTime? endTime)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@intAdminID",operatorID),
                                   new SqlParameter("@intPlatformID",platformID),
                                   new SqlParameter("@intGameID", gameID),
                                   new SqlParameter("@intServerID", serverID),
                                   new SqlParameter("@dtStartTime",beginTime),
                                   new SqlParameter("@dtEndTime",endTime)
                                   };
            return _dbFactory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<ServerOnLineUserLog>("dbo.PrWeb_Stat_GetServerOnLineUserLog", paras).ToList();
        }

        public List<DayMaxServerOnLineUserLog> GetMaxServerOnLineUserLog(int operatorID, int platformID, int? gameID, int? serverID, DateTime? beginTime, DateTime? endTime)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@intAdminID",operatorID),
                                   new SqlParameter("@intPlatformID",platformID),
                                   new SqlParameter("@intGameID", gameID),
                                   new SqlParameter("@intServerID", serverID),
                                   new SqlParameter("@dtStartTime",beginTime),
                                   new SqlParameter("@dtEndTime",endTime)
                                   };
            return _dbFactory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<DayMaxServerOnLineUserLog>("dbo.PrWeb_Stat_GetMaxServerOnlineUserLog", paras).ToList();
        }

        public List<GameOnLineUserLog> GetGameOnLineUserLog(int operatorID, int platformID, int? gameID, DateTime? beginTime, DateTime? endTime)
        {

            SqlParameter[] paras = {
                                   new SqlParameter("@intAdminID",operatorID),
                                   new SqlParameter("@intPlatformID",platformID),
                                   new SqlParameter("@intGameID", gameID),
                                   new SqlParameter("@dtStartTime",beginTime),
                                   new SqlParameter("@dtEndTime",endTime)
                                   };
            return _dbFactory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<GameOnLineUserLog>("dbo.PrWeb_Stat_GetGameOnlineUserLog", paras).ToList();
        }

        public List<DayMaxGameOnLineUserLog> GetMaxGameOnLineUserLog(int operatorID, int platformID, int? gameID, DateTime? beginTime, DateTime? endTime)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@intAdminID",operatorID),
                                   new SqlParameter("@intPlatformID",platformID),
                                   new SqlParameter("@intGameID", gameID),
                                   new SqlParameter("@dtStartTime",beginTime),
                                   new SqlParameter("@dtEndTime",endTime)
                                   };
            return _dbFactory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<DayMaxGameOnLineUserLog>("dbo.PrWeb_Stat_GetDayMaxGameOnLineUserLog", paras).ToList();

        }

        public List<HallOnLineUserLog> GetHallOnLineUserLog(int operatorID, int platformID, DateTime? beginTime, DateTime? endTime)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID",operatorID),
                                   new SqlParameter("@intPlatformID",platformID),
                                   new SqlParameter("@dtStartTime",beginTime),
                                   new SqlParameter("@dtEndTime",endTime)
                                   };
            return _dbFactory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<HallOnLineUserLog>("dbo.PrWeb_Stat_GetHallOnlineUserLog", paras).ToList();
        }

        public List<DayMaxHallOnLineUserLog> GetMaxHallOnLineUserLog(int platformID, DateTime? beginTime, DateTime? endTime, int? AdminID = 1)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@intAdminID",AdminID),
                                   new SqlParameter("@intPlatformID",platformID),
                                   new SqlParameter("@dtStartTime",beginTime),
                                   new SqlParameter("@dtEndTime",endTime)
                                   };
            return _dbFactory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<DayMaxHallOnLineUserLog>("dbo.PrWeb_Stat_GetDayMaxHallOnLineUserLog", paras).ToList();
        }


        public List<TotalGameOnLineLog> GetTotalGameOnLineLog(int platformID, int areaID, DateTime? beginTime, DateTime? endTime, int? AdminID = 1)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@intAdminID",AdminID),
                                   new SqlParameter("@intPlatformID",platformID),
                                   new SqlParameter("@intAreaID ",areaID),
                                   new SqlParameter("@dtStartTime",beginTime),
                                   new SqlParameter("@dtEndTime",endTime)
                                   };
            return _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<TotalGameOnLineLog>("dbo.PrWeb_Stat_GetOnlineUserCount", paras).ToList();
        }


        public List<DayMaxTotalGameOnLineLog> GetDayMaxTotalGameOnLineLog(int platformID, int areaID, DateTime? beginTime, DateTime? endTime, int? AdminID = 1)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@intAdminID",AdminID),
                                   new SqlParameter("@intPlatformID",platformID),
                                   new SqlParameter("@intAreaID ",areaID),
                                   new SqlParameter("@dtStartTime",beginTime),
                                   new SqlParameter("@dtEndTime",endTime)
                                   };
            return _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<DayMaxTotalGameOnLineLog>("dbo.PrWeb_Stat_GetMaxOnlineUserCount", paras).ToList();
        }

        public List<GameOnlineChatData> GetGameOnlineChatData(int platformID, int? gameID, int dataType, DateTime? beginTime, DateTime? endTime)
        {
            //用户存储过程
            SqlParameter[] paras = {
                                   new SqlParameter("@intPlatformID",platformID),
                                   new SqlParameter("@intGameID",gameID),
                                   new SqlParameter("@tintDatType",dataType),
                                   new SqlParameter("@dtBeginTime",beginTime),
                                   new SqlParameter("@dtEndTime",endTime)
                                   };
            List<GameOnlineChatData> list = _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<GameOnlineChatData>("dbo.PrWeb_Stat_GetGameOnlineChatData", paras).ToList();

            return list;
        }


        public List<TotalGameOnLineLogArea> GetTotalGameOnLineLogArea(int areaID, DateTime? beginTime, DateTime? endTime)
        {
            return _repoFactory.CreateRepository<TotalGameOnLineLogArea>()
               .Entities
               .Where(m => (beginTime == null || m.StatTime >= beginTime) &&
                           (endTime == null || m.StatTime <= endTime) &&
                           areaID == m.AreaID
                           )
               .OrderByDescending(m => m.StatTime)
               .ToList();
        }
    }
}
