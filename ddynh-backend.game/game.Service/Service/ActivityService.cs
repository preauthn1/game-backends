using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.Event;
using game.Entity.Users;
using game.Infrastructure;
using game.Service.DAO;
using game.Infrastructure.Extensions;

namespace game.Service.Service
{
    public class ActivityService : BaseService , IActivityService
    {

        private IRepositoryFactory _repoFactory;

        private IDatabaseFactory _dbFactory;

        private EventSettingRepository _eventSettingRepository { get; set; }

        /// <summary>
        /// 某些類型的eventType 允許建立多個獎勵活動 (ApplyEventTypeEnum.cs)
        /// ps. 也可以以table的方式 管理此資料
        /// </summary>
        private List<int> _allowMultipleEventTypes = new List<int>
        {
            ApplyEventTypeEnum.Roulette_Recharge.ToInteger(),
            ApplyEventTypeEnum.Roulette_FlowAmount.ToInteger()
        };


        public ActivityService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }

        public PagedList<EventSettingEntity> GetEventSettingList(QueryEventSettingRequestViewModel filter)
        {
            var query = _eventSettingRepository.GetPagingByFilter(filter);
            return query;
        }

        public EventSettingEntity GetEventSettingById(int id)
        {
            var query = _eventSettingRepository.GetById(id);
            return query;
        }

        public CRUDResultModel CreateEventSetting(EventSettingEntity model)
        {
            List<CheckResultModel> valids = new List<CheckResultModel>
            {
                ValidateHelper.CheckIsNotZeroOrMinus(model.PlatformID , TargetColumnEnum.PlatformID),
                ValidateHelper.CheckIsNotZeroOrMinus(model.EventType , TargetColumnEnum.Type),
                ValidateHelper.CheckDate_IsNotEmptyDate(model.StartTime , TargetColumnEnum.BeginDate),
                ValidateHelper.CheckDate_IsNotEmptyDate(model.EndTime , TargetColumnEnum.EndDate),
                ValidateHelper.CheckTextLength_IsNotEmpty_IsValidLength(model.EventName , TargetColumnEnum.Name),
            };

            var validResult = ValidateHelper.CheckAllSuccess(valids);

            if (!validResult.IsSuccess)
                return ValidateHelper.ConvertToCRUDResult(validResult);

            var queryExist = _eventSettingRepository.GetByPlatformAndType(model.PlatformID, model.EventType);

            //如果重複資料 且又是不允許重複建立的類型
            if (queryExist.IsNotEmptyNull() && !_allowMultipleEventTypes.Contains(model.EventType))
            {
                return new CRUDResultModel("不允许重复建立此类型的活动");
            }

            model.UpdateTime = DateTime.Now;
            var resultID = _eventSettingRepository.Create(model);

            return new CRUDResultModel
            {
                IsSuccess = true,
                ResultId = resultID,
                Message = "储存成功"
            };
        }
        

        public CRUDResultModel ModifyEvent(EventSettingEntity model)
        {
            var queryExist = _eventSettingRepository.GetById(model.ID);

            if (queryExist == null)
            {
                return new CRUDResultModel("查无资料");
            }

            queryExist.EventName = model.EventName;
            queryExist.Status = model.Status;
            queryExist.EndTime = model.EndTime;
            queryExist.StartTime = model.StartTime;
            queryExist.Updater = model.Updater;
            queryExist.UpdateTime = DateTime.Now;
            queryExist.Memo = model.Memo;
            queryExist.Tips = model.Tips;

            _eventSettingRepository.Update(queryExist);
            return new CRUDResultModel(true , "修改成功");
        }

        public CRUDResultModel UpdateStatus(CommonUpdateStatusRequestModel model)
        {
            List<CheckResultModel> valids = new List<CheckResultModel>
            {
                ValidateHelper.CheckIsNotZeroOrMinus(model.TargetId , TargetColumnEnum.Id),
                ValidateHelper.CheckIsNotMinus(model.TargetStatus , TargetColumnEnum.Status),
            };

            var validResult = ValidateHelper.CheckAllSuccess(valids);

            if (!validResult.IsSuccess)
                return ValidateHelper.ConvertToCRUDResult(validResult);

            var queryExist = _eventSettingRepository.GetById(model.TargetId);

            if (queryExist == null)
            {
                return new CRUDResultModel("查无资料");
            }

            queryExist.Status = model.TargetStatus;
            queryExist.Updater = model.UpdateBy;
            queryExist.UpdateTime = DateTime.Now;

            _eventSettingRepository.Update(queryExist);
            return new CRUDResultModel(true, "修改成功");
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
    }
}
