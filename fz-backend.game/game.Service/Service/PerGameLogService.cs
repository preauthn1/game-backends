using FW.Core.Helper;
using FW.Core.Repository;
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

        public PagedList<Entity.PerLog.DetailLog> GetDetailLog(int? userID, int? gameID, int? serverID, long? dealNo, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize, string deskCode)
        {

            //如果userid與牌局參數有值, 可忽略時間區間, 直接進行記錄查詢
            if (userID.HasValue && dealNo.HasValue)
            {
                //do nothing
            }
            //如果牌局号有值, 可忽略時間區間, 直接進行記錄查詢
            else if (dealNo.HasValue)
            {
                //do nothing
            }
            //如果牌局号為空, 檢查時間區間
            else if (!dealNo.HasValue)
            {
                //如果沒有帶任何值, 預設查詢1hr範圍
                if (beginTime == null && endTime == null)
                {
                    endTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    beginTime = Convert.ToDateTime(((DateTime)endTime).AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss"));
                }
                //如果開始時間沒有帶值,結束時間有帶值,預設查詢1hr範圍
                else if (beginTime == null && endTime != null)
                {
                    beginTime = Convert.ToDateTime(((DateTime)endTime).AddMinutes(-10).ToString("yyyy-MM-dd HH:mm:ss"));
                }
                //如果結束時間沒有帶值,預設查詢1hr範圍
                else if (endTime == null)
                {
                    endTime = Convert.ToDateTime(((DateTime)beginTime).AddMinutes(10).ToString("yyyy-MM-dd HH:mm:ss"));
                }
            }

            Int64 pageCount = 0;
            SqlParameter[] paras = {
                                    new SqlParameter("@intUserID",userID),
                                    new SqlParameter("@intGameID",gameID),
                                    new SqlParameter("@intServerID",serverID),
                                    new SqlParameter("@longDealNo",dealNo),
                                    new SqlParameter("@dtBeginTime",beginTime),
                                    new SqlParameter("@dtEndTime",endTime),
                                    new SqlParameter("@intPageIndex",pageIndex),
                                    new SqlParameter("@intPageSize",pageSize),
                                    new SqlParameter("@intPageCount",pageCount)
                                   };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<Entity.PerLog.DetailLog>("[dbo].[PrWeb_Game_PerLog_SearchPerGameDetail]", paras).ToList();

            pageCount = Convert.ToInt64(paras.LastOrDefault().Value);
            if (pageCount > int.MaxValue) pageCount = int.MaxValue;

            var pageList = new PagedList<Entity.PerLog.DetailLog>();
            pageList.PageData = list;
            pageList.PageIndex = pageIndex;
            pageList.PageSize = pageSize;
            pageList.TotalCount = Convert.ToInt32(pageCount);
            pageList.TotalPage = Common.GetTotalPage(pageSize, Convert.ToInt32(pageCount));

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
             totalOnlineMaxAndRoidCount = 0, totalOnlineGame1 = 0, totalOnlineGame2 = 0, totalOnlineGame3 = 0, totalOnlineGame4 = 0, totalOnlineGame5 = 0, totalOnlineGame6 = 0,
             totalOnlineGame7 = 0, totalOnlineGame8 = 0, totalOnlineGame9 = 0, totalOnlineGame10 = 0, totalOnlineGame11 = 0, totalOnlineGame12 = 0, totalOnlineGame13 = 0, totalOnlineGame14 = 0,
             totalOnlineGame15 = 0, totalOnlineGame16 = 0, totalOnlineGame18 = 0, totalOnlineGame19 = 0, totalOnlineGame20 = 0, totalOnlineGame21 = 0, totalOnlineGame22 = 0, totalOnlineGame23 = 0,
             totalOnlineGame24 = 0, totalOnlineGame25 = 0;

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
            var TotalOnlineGame1 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame1",
                Value = totalOnlineGame1,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineGame2 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame2",
                Value = totalOnlineGame2,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineGame3 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame3",
                Value = totalOnlineGame3,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineGame4 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame4",
                Value = totalOnlineGame4,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineGame5 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame5",
                Value = totalOnlineGame5,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineGame6 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame6",
                Value = totalOnlineGame6,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineGame7 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame7",
                Value = totalOnlineGame7,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineGame8 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame8",
                Value = totalOnlineGame8,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineGame9 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame9",
                Value = totalOnlineGame9,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineGame10 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame10",
                Value = totalOnlineGame10,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineGame11 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame11",
                Value = totalOnlineGame11,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineGame12 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame12",
                Value = totalOnlineGame12,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineGame13 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame13",
                Value = totalOnlineGame13,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineGame14 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame14",
                Value = totalOnlineGame14,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineGame15 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame15",
                Value = totalOnlineGame15,
                Direction = ParameterDirection.Output
            };
            var TotalOnlineGame16 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame16",
                Value = totalOnlineGame16,
                Direction = ParameterDirection.Output
            };

            var TotalOnlineGame18 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame18",
                Value = totalOnlineGame18,
                Direction = ParameterDirection.Output
            };

            var TotalOnlineGame19 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame19",
                Value = totalOnlineGame19,
                Direction = ParameterDirection.Output
            };


            var TotalOnlineGame20 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame20",
                Value = totalOnlineGame20,
                Direction = ParameterDirection.Output
            };

            var TotalOnlineGame21 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame21",
                Value = totalOnlineGame21,
                Direction = ParameterDirection.Output
            };

            var TotalOnlineGame22 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame22",
                Value = totalOnlineGame22,
                Direction = ParameterDirection.Output
            };

            var TotalOnlineGame23 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame23",
                Value = totalOnlineGame23,
                Direction = ParameterDirection.Output
            };

            var TotalOnlineGame24 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame24",
                Value = totalOnlineGame24,
                Direction = ParameterDirection.Output
            };

            var TotalOnlineGame25 = new SqlParameter
            {
                ParameterName = "@TotalOnlineGame25",
                Value = totalOnlineGame25,
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
                                     TotalOnlineMaxAndRoidCount,
                                     TotalOnlineGame1,
                                     TotalOnlineGame2,
                                     TotalOnlineGame3,
                                     TotalOnlineGame4,
                                     TotalOnlineGame5,
                                     TotalOnlineGame6,
                                     TotalOnlineGame7,
                                     TotalOnlineGame8,
                                     TotalOnlineGame9,
                                     TotalOnlineGame10,
                                     TotalOnlineGame11,
                                     TotalOnlineGame12,
                                     TotalOnlineGame13,
                                     TotalOnlineGame14,
                                     TotalOnlineGame15,
                                     TotalOnlineGame16,
                                     TotalOnlineGame18,
                                     TotalOnlineGame19,
                                     TotalOnlineGame20,
                                     TotalOnlineGame21,
                                     TotalOnlineGame22,
                                     TotalOnlineGame23,
                                     TotalOnlineGame24,
                                     TotalOnlineGame25
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
            totalOnlineGame1 = Convert.ToInt32(TotalOnlineGame1.Value);
            totalOnlineGame2 = Convert.ToInt32(TotalOnlineGame2.Value);
            totalOnlineGame3 = Convert.ToInt32(TotalOnlineGame3.Value);
            totalOnlineGame4 = Convert.ToInt32(TotalOnlineGame4.Value);
            totalOnlineGame5 = Convert.ToInt32(TotalOnlineGame5.Value);
            totalOnlineGame6 = Convert.ToInt32(TotalOnlineGame6.Value);
            totalOnlineGame7 = Convert.ToInt32(TotalOnlineGame7.Value);
            totalOnlineGame8 = Convert.ToInt32(TotalOnlineGame8.Value);
            totalOnlineGame9 = Convert.ToInt32(TotalOnlineGame9.Value);
            totalOnlineGame10 = Convert.ToInt32(TotalOnlineGame10.Value);
            totalOnlineGame11 = Convert.ToInt32(TotalOnlineGame11.Value);
            totalOnlineGame12 = Convert.ToInt32(TotalOnlineGame12.Value);
            totalOnlineGame13 = Convert.ToInt32(TotalOnlineGame13.Value);
            totalOnlineGame14 = Convert.ToInt32(TotalOnlineGame14.Value);
            totalOnlineGame15 = Convert.ToInt32(TotalOnlineGame15.Value);
            totalOnlineGame16 = Convert.ToInt32(TotalOnlineGame16.Value);
            totalOnlineGame18 = Convert.ToInt32(TotalOnlineGame18.Value);
            totalOnlineGame19 = Convert.ToInt32(TotalOnlineGame19.Value);
            totalOnlineGame20 = Convert.ToInt32(TotalOnlineGame20.Value);
            totalOnlineGame21 = Convert.ToInt32(TotalOnlineGame21.Value);
            totalOnlineGame22 = Convert.ToInt32(TotalOnlineGame22.Value);
            totalOnlineGame23 = Convert.ToInt32(TotalOnlineGame23.Value);
            totalOnlineGame24 = Convert.ToInt32(TotalOnlineGame24.Value);
            totalOnlineGame25 = Convert.ToInt32(TotalOnlineGame25.Value);

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
                totalOnlineMaxAndRoidCount,
                totalOnlineGame1,
                totalOnlineGame2,
                totalOnlineGame3,
                totalOnlineGame4,
                totalOnlineGame5,
                totalOnlineGame6,
                totalOnlineGame7,
                totalOnlineGame8,
                totalOnlineGame9,
                totalOnlineGame10,
                totalOnlineGame11,
                totalOnlineGame12,
                totalOnlineGame13,
                totalOnlineGame14,
                totalOnlineGame15,
                totalOnlineGame16,
                totalOnlineGame18,
                totalOnlineGame19,
                totalOnlineGame20,
                totalOnlineGame21,
                totalOnlineGame22,
                totalOnlineGame23,
                totalOnlineGame24,
                totalOnlineGame25
            });
        }
    }
}
