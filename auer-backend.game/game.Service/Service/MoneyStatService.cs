using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Repository;
using game.Entity.Stat;
using game.Infrastructure;

namespace game.Service
{
    public class MoneyStatService : IMoneyStatService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;
        public MoneyStatService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }

        public List<MoneySourceLog> GetMoneySourceStatLog(int operatorID, int? platformids, int? areaID, int? sourceTypeGroup, int? sourceTypeID, Entity.Money.MoneySourceChangeType? changeType, DateTime? beginTime, DateTime? endTime)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID", operatorID),
                                    new SqlParameter("@intPlatformID", platformids),
                                    new SqlParameter("@intAreaID",areaID),
                                    new SqlParameter("@intSourceGroup",sourceTypeGroup),
                                    new SqlParameter("@intSourceType",sourceTypeID),
                                    new SqlParameter("@intChangeType",changeType),
                                    new SqlParameter("@dtBeginTime",beginTime.Value.ToString("yyyy-MM-dd")),
                                    new SqlParameter("@dtEndTime",endTime.Value.ToString("yyyy-MM-dd"))
                                   };
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<MoneySourceLog>("dbo.PrWeb_Stat_GetMoneySourceLog_New", paras).ToList();

            return _list ?? new List<MoneySourceLog>();
        }

        /// <summary>
        /// 總金幣統計
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<SumMoneyLog> GetSumMoneyLog(int operatorID, DateTime? beginTime, DateTime? endTime)
        {

            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID", operatorID),
                                    new SqlParameter("@intPlatformID",DBNull.Value),
                                    new SqlParameter("@intAreaID",DBNull.Value),
                                    new SqlParameter("@dtStatTime",beginTime),
                                    new SqlParameter("@dtEndTime",endTime)
                                   };
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SumMoneyLog>("dbo.PrWeb_Stat_GetDailySumMoney", paras).ToList();

            return _list ?? new List<SumMoneyLog>();
        }

        public List<UserMoneyReport> GetUserTopMoneyReport(Int64? amount, int count)
        {

            //用户存储过程
            SqlParameter[] paras = {
                                    new SqlParameter("@longAmount",amount),
                                    new SqlParameter("@intCount",count)
                                   };
            List<UserMoneyReport> list = _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<UserMoneyReport>("dbo.PrWeb_Stat_GetUserTopMoneyReport", paras).ToList();

            return list ?? new List<UserMoneyReport>();
        }

        /// <summary>
        /// 统计游戏税收记录
        /// </summary>
        /// <param name="platformids"></param>
        /// <param name="areaID"></param>
        /// <param name="gameID"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<GameTaxLog> GetGameTaxLog(int operatorID, int? platformids, int? areaID, int? gameID, DateTime? beginTime, DateTime? endTime)
        {

            SqlParameter[] paras = {
                                   new SqlParameter("@intAdminID", operatorID),
                                   new SqlParameter("@intPlatformID", platformids),
                                   new SqlParameter("@intAreaID",areaID),
                                   new SqlParameter("@intGameID",gameID),
                                   new SqlParameter("@dtBeginTime",beginTime),
                                   new SqlParameter("@dtEndTime",endTime)
                                   };
            List<GameTaxLog> list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<GameTaxLog>("dbo.PrWeb_Stat_GetGameTaxLog", paras).ToList();

            return list ?? new List<GameTaxLog>();
        }

        /// <summary>
        /// 游戏房间税收统计
        /// </summary>
        /// <param name="platformids"></param>
        /// <param name="areaID"></param>
        /// <param name="gameID"></param>
        /// <param name="serverID"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<GameServerTaxLog> GetGameServerTaxLog(int operatorID, int? platformids, int? areaID, int? gameID, int? serverID, DateTime? beginTime, DateTime? endTime)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@intAdminID", operatorID),
                                   new SqlParameter("@intPlatformID", platformids),
                                   new SqlParameter("@intAreaID",areaID),
                                   new SqlParameter("@intGameID",gameID),
                                   new SqlParameter("@intServerID",serverID),
                                   new SqlParameter("@dtBeginTime",beginTime),
                                   new SqlParameter("@dtEndTime",endTime)
                                   };
            List<GameServerTaxLog> list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<GameServerTaxLog>("dbo.PrWeb_Stat_GetGameServerTaxLog", paras).ToList();

            return list ?? new List<GameServerTaxLog>();
        }

        public List<SystemGainLog> GetSystemGainLog(int operatorID, DateTime? beginTime, DateTime? endTime)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@dtbeginTime",beginTime),
                                   new SqlParameter("@dtendTime",endTime)
                                   };
            List<SystemGainLog> list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SystemGainLog>("dbo.PrWeb_Stat_GetSystemGain", paras).ToList();

            return list ?? new List<SystemGainLog>();
        }

        /// <summary>
        /// 查询游戏总局数前N名玩家
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="topN"></param>
        /// <returns></returns>
        public List<UserGameTotalRank> GetGameTotalRankByGameID(int gameID, int topN)
        {
            //用户存储过程
            SqlParameter[] paras = {
                                   new SqlParameter("@intGameID",gameID),
                                   new SqlParameter("@intTopN",topN)
                                   };
            List<UserGameTotalRank> list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<UserGameTotalRank>("dbo.PrWeb_Stat_GetGameTotalRank", paras).ToList();

            return list;
        }

        public List<BankSourceLog> GetBankSourceStatLog(int operatorID, int? platformids, int? areaID, int? sourceTypeID, Entity.Money.BankSourceChangeType? changeType, DateTime? beginTime, DateTime? endTime)
        {
            SqlParameter[] paras = {
                                   new SqlParameter("@intAdminID", operatorID),
                                   new SqlParameter("@intPlatformID", platformids),
                                   new SqlParameter("@intAreaID",areaID),
                                   new SqlParameter("@intSourceType",sourceTypeID),
                                   new SqlParameter("@intChangeType",changeType),
                                   new SqlParameter("@dtbeginTime",beginTime),
                                   new SqlParameter("@dtendTime",endTime)
                                   };
            List<BankSourceLog> list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<BankSourceLog>("dbo.PrWeb_Stat_GetBankSourceLog_New", paras).ToList();

            return list ?? new List<BankSourceLog>();
        }
    }
}
