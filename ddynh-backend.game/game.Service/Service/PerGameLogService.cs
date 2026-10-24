using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.PerLog;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace game.Service.Service
{
    public class PerGameLogService : IPerGameLogService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;

        public PerGameLogService(IRepositoryFactory repoFactory, IDatabaseFactory dbfactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbfactory;
        }

        public PagedList<Entity.PerLog.DetailLog> GetDetailLog(QueryDetailLogRequestModel filter)
        {
            DateTime? begin = string.IsNullOrWhiteSpace(filter.BeginTime) ? null : (DateTime?)DateTime.Parse(filter.BeginTime);
            DateTime? end = string.IsNullOrWhiteSpace(filter.EndTime) ? null : (DateTime?)DateTime.Parse(filter.EndTime);

            //如果userid與牌局參數有值, 可忽略時間區間, 直接進行記錄查詢
            if (filter.UserID.HasValue && filter.DealNo.HasValue)
            {
                //do nothing
            }
            //如果牌局号有值, 可忽略時間區間, 直接進行記錄查詢
            else if (filter.DealNo.HasValue)
            {
                //do nothing
            }
            //如果牌局号為空, 檢查時間區間
            else if (!filter.DealNo.HasValue)
            {
                //如果沒有帶任何值, 預設查詢1hr範圍
                if (begin == null && filter.EndTime == null)
                {
                    end  = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    begin = Convert.ToDateTime(((DateTime)end).AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss"));
                }
                //如果開始時間沒有帶值,結束時間有帶值,預設查詢1hr範圍
                else if (begin == null && end != null)
                {
                    begin = Convert.ToDateTime(((DateTime)end).AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss"));
                }
                //如果結束時間沒有帶值,預設查詢1hr範圍
                else if (end == null)
                {
                    end = Convert.ToDateTime(((DateTime)begin).AddMinutes(10).ToString("yyyy-MM-dd HH:mm:ss"));
                }
            }

            long pageCount = 0;
            SqlParameter[] paras = {
                                    new SqlParameter("@intUserID", filter.UserID),
                                    new SqlParameter("@intGameID", filter.GameID),
                                    new SqlParameter("@intServerID", filter.ServerID),
                                    new SqlParameter("@longDealNo", filter.DealNo),
                                    new SqlParameter("@dtBeginTime", begin),
                                    new SqlParameter("@dtEndTime", end),
                                    new SqlParameter("@intAdminID", filter.AdminID),
                                    new SqlParameter("@intPageIndex", filter.PageIndex),
                                    new SqlParameter("@intPageSize", filter.PageSize),
                                    new SqlParameter("@intPageCount",pageCount)
                                   };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<Entity.PerLog.DetailLog>("[dbo].[PrWeb_Game_PerLog_SearchPerGameDetail]", paras).ToList();

            pageCount = Convert.ToInt64(paras.LastOrDefault().Value);
            if (pageCount > int.MaxValue) pageCount = int.MaxValue;

            var pageList = new PagedList<Entity.PerLog.DetailLog>();
            pageList.PageData = list;
            pageList.PageIndex = filter.PageIndex;
            pageList.PageSize = filter.PageSize;
            pageList.TotalCount = Convert.ToInt32(pageCount);
            pageList.TotalPage = Common.GetTotalPage(filter.PageSize, Convert.ToInt32(pageCount));

            return pageList;
        }


        public List<DetailLog> GetDetailLogByLogID(string logID, DateTime gametime)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@chvLogID",logID),
                                     new SqlParameter("@dtGameTime",gametime)
                                   };
            return _dbFactory.GetDatabase(DBNameConfig.DBName_PerGameLog).ExecuteStoreProcedureForDataList<DetailLog>("[dbo].[PrWeb_PerLog_GetDetailLogByLogID]", paras).ToList();
        }

        public PerGameLog GetPerGameLogByLogID(string logID, DateTime gametime)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@chvLogID",logID),
                                    new SqlParameter("@dtGameTime",gametime)
                                   };
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_PerGameLog).ExecuteStoreProcedureForDataList<PerGameLog>("[dbo].[PrWeb_PerLog_GetPerGameLogByLogID]", paras).ToList();

            if (list.Count > 0) return list[0];
            return null;

        }


        public List<string> GetPerGameLogByTradeID(string tradeID)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@lngTradeID",tradeID)
                                   };
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_PerGameLog).ExecuteStoreProcedureForDataList<string>("[dbo].[PrWeb_PerGameLogByTradeID]", paras).ToList();

            if (list.Count > 0) return list;
            return null;
        }

        public List<DetailLog> GetGameLogByTradeID(long tradeID, int userID)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@lngTradeID",tradeID),
                                    new SqlParameter("@intUserID", userID)
                                   };
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_PerGameLog).ExecuteStoreProcedureForDataList<DetailLog>("[dbo].[PrWeb_PerGameLogByTradeID]", paras).ToList();

            return list;
        }


        public PagedList<OnlineUser> GetOnlineUser(int operatorID, int? gameID, int? serverID, int pageIndex, int pageSize, int? sortName, int? sortLifting)
        {
            int pageCount = 0, totalOnlineCount = 0, totalOnlineiOSCount = 0, totalOnlineAndroidCount = 0, totalOnlineHallCount = 0, totalOnlineGameRoomCount = 0, totalOnlineRegCount = 0,
             totalOnlineRegWeeklyCount = 0, totalOnlineAvgCount = 0, totalOnlineAvgAndRoidCount = 0, totalOnlineAvgiOSCount = 0, totalOnlineMaxCount = 0, totalOnlineMaxiOSCount = 0,
             totalOnlineMaxAndRoidCount = 0;

            var PageCount = new SqlParameter
            {
                ParameterName = "@intPageCount",
                Value = pageCount,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineCount = new SqlParameter
            {
                ParameterName = "@TotalOnlineCount",
                Value = totalOnlineCount,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineiOSCount = new SqlParameter
            {
                ParameterName = "@TotalOnlineiOSCount",
                Value = totalOnlineiOSCount,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineAndroidCount = new SqlParameter
            {
                ParameterName = "@TotalOnlineAndroidCount",
                Value = totalOnlineAndroidCount,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineHallCount = new SqlParameter
            {
                ParameterName = "@TotalOnlineHallCount",
                Value = totalOnlineHallCount,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineGameRoomCount = new SqlParameter
            {
                ParameterName = "@TotalOnlineGameRoomCount",
                Value = totalOnlineGameRoomCount,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineRegCount = new SqlParameter
            {
                ParameterName = "@TotalOnlineRegCount",
                Value = totalOnlineRegCount,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineRegWeeklyCount = new SqlParameter
            {
                ParameterName = "@TotalOnlineRegWeeklyCount",
                Value = totalOnlineRegWeeklyCount,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineAvgCount = new SqlParameter
            {
                ParameterName = "@TotalOnlineAvgCount",
                Value = totalOnlineAvgCount,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineAvgAndRoidCount = new SqlParameter
            {
                ParameterName = "@TotalOnlineAvgAndRoidCount",
                Value = totalOnlineAvgAndRoidCount,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineAvgiOSCount = new SqlParameter
            {
                ParameterName = "@TotalOnlineAvgiOSCount",
                Value = totalOnlineAvgiOSCount,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineMaxCount = new SqlParameter
            {
                ParameterName = "@TotalOnlineMaxCount",
                Value = totalOnlineMaxCount,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineMaxiOSCount = new SqlParameter
            {
                ParameterName = "@TotalOnlineMaxiOSCount",
                Value = totalOnlineMaxiOSCount,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineMaxAndRoidCount = new SqlParameter
            {
                ParameterName = "@TotalOnlineMaxAndRoidCount",
                Value = totalOnlineMaxAndRoidCount,
                Direction = ParameterDirection.Output
            };


            SqlParameter[] paras = {
                                     new SqlParameter("@intAdminID",operatorID),
                                     new SqlParameter("@intGameID",gameID),
                                     new SqlParameter("@intServerID",serverID),
                                     new SqlParameter("@intPageIndex",pageIndex),
                                     new SqlParameter("@intPageSize",pageSize),
                                     new SqlParameter("@tnyColumns",sortName),
                                     new SqlParameter("@tnySort",sortLifting),
                                     PageCount,
                                     TotalOnlineCount,
                                     TotalOnlineiOSCount,
                                     TotalOnlineAndroidCount,
                                     TotalOnlineHallCount,
                                     TotalOnlineGameRoomCount,
                                     TotalOnlineRegCount,
                                     TotalOnlineRegWeeklyCount,
                                     TotalOnlineAvgCount,
                                     TotalOnlineAvgAndRoidCount,
                                     TotalOnlineAvgiOSCount,
                                     TotalOnlineMaxCount,
                                     TotalOnlineMaxiOSCount,
                                     TotalOnlineMaxAndRoidCount
                                   };

            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<OnlineUser>("[dbo].[PrWeb_GetOnlineSummary]", paras).ToList();

            pageCount = Convert.ToInt32(PageCount.Value);
            totalOnlineCount = Convert.ToInt32(TotalOnlineCount.Value);
            totalOnlineiOSCount = Convert.ToInt32(TotalOnlineiOSCount.Value);
            totalOnlineAndroidCount = Convert.ToInt32(TotalOnlineAndroidCount.Value);
            totalOnlineHallCount = Convert.ToInt32(TotalOnlineHallCount.Value);
            totalOnlineGameRoomCount = Convert.ToInt32(TotalOnlineGameRoomCount.Value);
            totalOnlineRegCount = Convert.ToInt32(TotalOnlineRegCount.Value);
            totalOnlineRegWeeklyCount = Convert.ToInt32(TotalOnlineRegWeeklyCount.Value);
            totalOnlineAvgCount = Convert.ToInt32(TotalOnlineAvgCount.Value);
            totalOnlineAvgAndRoidCount = Convert.ToInt32(TotalOnlineAvgAndRoidCount.Value);
            totalOnlineAvgiOSCount = Convert.ToInt32(TotalOnlineAvgiOSCount.Value);
            totalOnlineMaxCount = Convert.ToInt32(TotalOnlineMaxCount.Value);
            totalOnlineMaxiOSCount = Convert.ToInt32(TotalOnlineMaxiOSCount.Value);
            totalOnlineMaxAndRoidCount = Convert.ToInt32(TotalOnlineMaxAndRoidCount.Value);
          

            return new PagedList<OnlineUser>(list, pageIndex, pageSize, pageCount, new
            {
                totalOnlineCount,
                totalOnlineiOSCount,
                totalOnlineAndroidCount,
                totalOnlineHallCount,
                totalOnlineGameRoomCount,
                totalOnlineRegCount,
                totalOnlineRegWeeklyCount,
                totalOnlineAvgCount,
                totalOnlineAvgAndRoidCount,
                totalOnlineAvgiOSCount,
                totalOnlineMaxCount,
                totalOnlineMaxiOSCount,
                totalOnlineMaxAndRoidCount
            });
        }
    }
}
