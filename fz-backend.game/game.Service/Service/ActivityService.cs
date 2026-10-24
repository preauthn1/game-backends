using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Users;
using game.Infrastructure;

namespace game.Service.Service
{
    public class ActivityService : IActivityService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;
        public ActivityService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }

        public List<EventSetting> GetEventList(DateTime? beginTime, DateTime? endTime, int? status, string eventName)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@chvEventName",eventName),
                    new SqlParameter("@dtStatTime",beginTime),
                    new SqlParameter("@dtEndTime",endTime),
                    new SqlParameter("@tnyStatus",status)
                    };

            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<EventSetting>("dbo.[PrWeb_Event_EventSettingGet]", paras).ToList();
            return _list;
        }

        public bool ModifyEvent(EventSetting model, ref string msg)
        {
            SqlParameter[] _paras = {
                                    new SqlParameter("@sintEventID",model.EventID),
                                    new SqlParameter("@chvEventName",model.EventName),
                                    new SqlParameter("@dtStatTime",model.StartTime),
                                    new SqlParameter("@dtEndTime",model.EndTime),
                                    new SqlParameter("@tnyStatus",model.Status),
                                    new SqlParameter("@lngGiftCash",model.GiftCash),
                                    new SqlParameter("@updater",model.Updater),
                                    new SqlParameter("@intRtnCode", 0),
                                    new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
                                   };
            _paras[7].Direction = ParameterDirection.Output;
            _paras.LastOrDefault().Direction = ParameterDirection.Output;

            var _result = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Event_EventSetting", _paras);
            var _returnValue = Convert.ToInt32(_paras[7].Value);
            msg = Convert.ToString(_paras.LastOrDefault().Value);

            return _returnValue == 1;
        }

        public List<EventLog> GetUserEventAccessList(DateTime? beginTime, DateTime? endTime, int? userID, string loginName = null)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@sintEventID", null),
                    new SqlParameter("@dtStatTime",beginTime),
                    new SqlParameter("@dtEndTime",endTime),
                    new SqlParameter("@intUserID",userID),
                    new SqlParameter("@chvLoginName",loginName)
                    };

            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<EventLog>("dbo.[PrWeb_Event_UserEventAccessGet]", paras).ToList();
            return _list;
        }

        public PagedList<EventDetailLog> GetUserEventAccessDetail(DateTime? beginTime, DateTime? endTime, int? userID, int pageIndex, int pageSize)
        {
            long pageCount = 0;
            SqlParameter[] paras = {
                    new SqlParameter("@sintEventID", null),
                    new SqlParameter("@dtStatTime",beginTime),
                    new SqlParameter("@dtEndTime",endTime),
                    new SqlParameter("@intUserID",userID),
                    new SqlParameter("@intPageIndex",pageIndex),
                    new SqlParameter("@intPageSize",pageSize),
                    new SqlParameter("@intPageCount",pageCount)
                    };

            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<EventDetailLog>("dbo.[PrWeb_Event_UserEventAccessDetail]", paras).ToList();
            pageCount = Convert.ToInt64(paras.LastOrDefault().Value);
            if (pageCount > int.MaxValue) pageCount = int.MaxValue;

            var pageList = new FW.Core.Helper.PagedList<EventDetailLog>();
            pageList.PageData = _list;
            pageList.PageIndex = pageIndex;
            pageList.PageSize = pageSize;
            pageList.TotalCount = Convert.ToInt32(pageCount);
            pageList.TotalPage = Common.GetTotalPage(pageSize, Convert.ToInt32(pageCount));

            return pageList;
        }

        public bool ResetGrantTime(int eventID, int userID, string updater, int resetTime, ref string msg)
        {
            SqlParameter[] _paras = {
                                    new SqlParameter("@sintEventID", eventID),
                                    new SqlParameter("@intUserID", userID),
                                    new SqlParameter("@tnyLoginTimes", resetTime),
                                    new SqlParameter("@updater", updater),
                                    new SqlParameter("@intRtnCode", 0),
                                    new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
                                   };
            _paras[4].Direction = ParameterDirection.Output;
            _paras.LastOrDefault().Direction = ParameterDirection.Output;

            var _result = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Event_UserEventAccess", _paras);
            var _returnValue = Convert.ToInt32(_paras[4].Value);
            msg = Convert.ToString(_paras.LastOrDefault().Value);

            return _returnValue == 1;
        }


        public List<EventContest> GetGamesEventMGMT(int? contestID, DateTime? beginTime, DateTime? endTime)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intContestID",contestID),
                    new SqlParameter("@dtStartTime",beginTime),
                    new SqlParameter("@dtEndTime",endTime)
                    };

            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<EventContest>("dbo.[PrWeb_Event_GetContest]", paras).ToList();
            return _list;
        }

        public List<EventBonus> GetEventBonus(int? contestID)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intContestID",contestID)
                    };
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<EventBonus>("dbo.[PrWeb_Event_GetBonusConfig]", paras).ToList();
            return _list;
        }

        public List<EventReward> GetEventReward(int? contestID)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intContestID",contestID)
                    };
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<EventReward>("dbo.[PrWeb_Event_GetRewardConfig]", paras).ToList();
            return _list;
        }

        public List<EventRank> GetContestRank(int? contestID, DateTime? rankDate, int rankType)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intContestID",contestID),
                    new SqlParameter("@dtRankDate",rankDate),
                    new SqlParameter("@intRankType",rankType)
                    };
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<EventRank>("dbo.[PrWeb_Event_GetContestRank]", paras).ToList();
            return _list;
        }

        public bool UpdateEventContest(EventContest mode, ref int contestID, ref string message)
        {
            var rtnCode = string.Empty;
            int number = 0;
            var chvRtnMsg = new SqlParameter
            {
                ParameterName = "@chvRtnMsg",
                Value = message,
                Direction = ParameterDirection.Output,
                Size = 32
            };

            var chvRtnCode = new SqlParameter
            {
                ParameterName = "@intRtnCode",
                Value = rtnCode,
                Direction = ParameterDirection.Output,
                Size = 32
            };

            var intRtnContestID = new SqlParameter
            {
                ParameterName = "@intRtnContestID",
                Value = contestID,
                Direction = ParameterDirection.Output,
                Size = 32
            };

            SqlParameter[] paras = {
                                    new SqlParameter("@intContestID", mode.ID),
                                    new SqlParameter("@chvName", mode.Name),
                                    new SqlParameter("@intGameID", mode.GameID),
                                    new SqlParameter("@intRankInterval", mode.RankInterval),
                                    new SqlParameter("@dtStartTime", mode.StartTime),
                                    new SqlParameter("@dtEndTime", mode.EndTime),
                                    new SqlParameter("@tnyStatus", mode.Status),
                                    new SqlParameter("@tnyShow", mode.Show),
                                    new SqlParameter("@dtEnterTime", mode.Entertime),
                                    new SqlParameter("@dtDeadline", mode.Deadline),
                                    new SqlParameter("@dtPeriodEnterTime", mode.PeriodEntertime),
                                    new SqlParameter("@dtPeriodDeadline", mode.PeriodDeadline),
                                    new SqlParameter("@intRewardAfterMin", mode.RewardAfterMin),
                                    new SqlParameter("@lngCharge", mode.Charge),
                                    new SqlParameter("@intPlayTime", mode.PlayTime),
                                    new SqlParameter("@lngCoin", mode.Coin),
                                    intRtnContestID,
                                    chvRtnMsg,
                                    chvRtnCode
                                   };

            _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_Event_AddContest]", paras);
            message = chvRtnMsg.Value.ToString();
            int.TryParse(intRtnContestID.Value.ToString(), out number);
            contestID = number;
            var _returnValue = Convert.ToInt32(chvRtnCode.Value);
            return _returnValue == 1;
        }

        public bool UpdateEventBonus(EventBonus mode, ref string message)
        {
            var rtnCode = string.Empty;
            var chvRtnMsg = new SqlParameter
            {
                ParameterName = "@chvRtnMsg",
                Value = message,
                Direction = ParameterDirection.Output,
                Size = 32
            };

            var chvRtnCode = new SqlParameter
            {
                ParameterName = "@intRtnCode",
                Value = rtnCode,
                Direction = ParameterDirection.Output,
                Size = 32
            };

            SqlParameter[] paras = {
                                    new SqlParameter("@intID", mode.ID),
                                    new SqlParameter("@intContestID", mode.ContestID),
                                    new SqlParameter("@lngOrderReach", mode.OrderReach),
                                    new SqlParameter("@intFreeTicket", mode.FreeTicket),
                                    chvRtnMsg,
                                    chvRtnCode
                                   };

            _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_Event_AddBonusConfig]", paras);
            message = chvRtnMsg.Value.ToString();
            var _returnValue = Convert.ToInt32(chvRtnCode.Value);
            return _returnValue == 1;
        }

        public bool UpdateEventReward(EventReward mode, ref string message)
        {
            var rtnCode = string.Empty;
            var chvRtnMsg = new SqlParameter
            {
                ParameterName = "@chvRtnMsg",
                Value = message,
                Direction = ParameterDirection.Output,
                Size = 32
            };

            var chvRtnCode = new SqlParameter
            {
                ParameterName = "@intRtnCode",
                Value = rtnCode,
                Direction = ParameterDirection.Output,
                Size = 32
            };

            SqlParameter[] paras = {
                                    new SqlParameter("@intID", mode.ID),
                                    new SqlParameter("@intContestID", mode.ContestID),
                                    new SqlParameter("@intRank", mode.Rank),
                                    new SqlParameter("@tnyType", mode.Type),
                                    new SqlParameter("@lngMoney", mode.Money),
                                    chvRtnMsg,
                                    chvRtnCode
                                   };

            _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_Event_AddRewardConfig]", paras);
            message = chvRtnMsg.Value.ToString();
            var _returnValue = Convert.ToInt32(chvRtnCode.Value);
            return _returnValue == 1;
        }

        public bool DeleteEventBonus(int id, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intID", id)
                                   };
            _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_Event_DelBonusConfig]", paras);
            message = "操作成功";
            return true;
        }

        public bool DeleteEventReward(int id, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intID", id)
                                   };
            _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_Event_DelRewardConfig]", paras);
            message = "操作成功";
            return true;
        }

        public List<SevenDaysLoginBonus> GetSevenDaysLoginBonus(int? platformID, int? areaID)
        {
            SqlParameter[] paras = {
                    new SqlParameter("@intPlatformID",platformID),
                    new SqlParameter("@intAreaID",areaID)
                    };

            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SevenDaysLoginBonus>("dbo.[PrWeb_Event_SevenDaysLoginBonusGet]", paras).ToList();
            return _list;
        }


        public bool AddSevenDaysLoginBonus(SevenDaysLoginBonus model, ref string msg)
        {
            //*******下方程式內容還沒改

            SqlParameter[] _paras = {
                                    new SqlParameter("@intPlatformID",model.PlatformID),
                                    new SqlParameter("@intAreaID",model.AreaID),
                                    new SqlParameter("@intRtnCode", 0),
                                    new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
                                   };
            _paras[2].Direction = ParameterDirection.Output;
            _paras.LastOrDefault().Direction = ParameterDirection.Output;

            var _result = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_Event_SevenDaysLoginBonusDel]", _paras);
            var _returnValue = Convert.ToInt32(_paras[2].Value);
            msg = Convert.ToString(_paras.LastOrDefault().Value);

            return _returnValue == 1;
        }


        public bool DelSevenDaysLoginBonus(SevenDaysLoginBonus model, ref string msg)
        {
            SqlParameter[] _paras = {
                                    new SqlParameter("@intPlatformID",model.PlatformID),
                                    new SqlParameter("@intAreaID",model.AreaID),
                                    new SqlParameter("@intRtnCode", 0),
                                    new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
                                   };
            _paras[2].Direction = ParameterDirection.Output;
            _paras.LastOrDefault().Direction = ParameterDirection.Output;

            var _result = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_Event_SevenDaysLoginBonusDel]", _paras);
            var _returnValue = Convert.ToInt32(_paras[2].Value);
            msg = Convert.ToString(_paras.LastOrDefault().Value);

            return _returnValue == 1;
        }
    }
}
