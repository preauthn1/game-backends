using Dapper;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.Event;
using game.Entity.Game;
using game.Entity.Money;
using game.Infrastructure;
using game.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace game.Service.DAO
{
    public class EventSettingRepository : IDapperRepository<EventSettingEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Users].[EventSetting]"; }
        }


        public List<EventSettingEntity> GetByPlatformAndType(int platformID , int eventType)
        {
            var filter = new QueryEventSettingRequestViewModel
            {
                PlatformIDs = platformID.ToListCollection(),
                EventType = eventType,
                PageSize = 9999
            };

            var query = GetPagingByFilter(filter);
            return query.PageData;
        }


        public void UpdateModifyTime(int settingID, string updateBy)
        {
            string sql = @" Update [Users].[EventSetting]
                            SET [Updater] = @updateBy , UpdateTime = getdate()
                            WHERE ID = @settingID";

            QueryByDapper<string>(sql, new { settingID = settingID, updateBy = updateBy });
        }

        public void UpdateMemoAndTips(int settingID, string memo, string tips, string updateBy)
        {
            string sql = @" Update [Users].[EventSetting]
                            SET [Memo] = @memo , Tips = @tips, UpdateTime = getdate()
                            WHERE ID = @settingID";

            QueryByDapper<string>(sql, new { settingID = settingID, memo = memo, tips = tips, updateBy = updateBy });
        }

        public PagedList<EventSettingEntity> GetPagingByFilter(QueryEventSettingRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} " +
                            "WHERE 1=1 ";

            filter.PlatformIDs = filter.PlatformIDsText.SeperateToListIntOrDefaultValue(filter.PlatformIDs);

            if (filter.StartTimeBegin.HasValue)
                sql += " AND StartTime >= @StartTimeBegin ";

            if (filter.StartTimeEnd.HasValue)
                sql += " AND StartTime <= @StartTimeEnd ";

            if (filter.EndTimeBegin.HasValue)
                sql += " AND EndTime >= @EndTimeBegin ";

            if (filter.EndTimeEnd.HasValue)
                sql += " AND EndTime >= @EndTimeEnd ";

            if (filter.PlatformIDs.IsNotEmptyNull())
                sql += " AND PlatformID in @PlatformIDs ";

            if (filter.EventType.HasValue)
                sql += " AND EventType = @EventType ";

            if (filter.EventTypes.IsNotEmptyNull())
                sql += " AND EventType in @EventTypes ";

            if (filter.Status.HasValue)
                sql += " AND Status = @Status ";

            if (!filter.IsIncludeDeleteData)
                sql += $" AND Status != {(int)NormalActiveStatusEnum.PermanentInvisible} ";

            var query = GetPageingEntitiesBySQLScript<EventSettingEntity>(sql, filter, filter);
            return query;
        }


        public int GetLotteryCount(int userID, int platformID, int eventID)
        {
            var para = new
            {
                intUserID = userID,
                intPlatformID = platformID,
                intEventID = eventID
            };

            var _params = new DynamicParameters(para);
            _params.Add(name: "@intLotteryCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            _params.Add(name: "@RetVal", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Execute(sql: "PrPs_Event_GetLotteryCount", param: _params, commandType: CommandType.StoredProcedure);
                int lotteryCount = _params.Get<int>("@intLotteryCount");
                return lotteryCount;
            }
        }
        
        public PromotionEventLotteryResultModel GetLottery(int userID, int platformID, int eventID)
        {
            string spName = "PrPs_Event_GetLottery";

            var para = new DynamicParameters();
            para.Add("intUserID", userID);
            para.Add("intPlatformID", platformID);
            para.Add("intEventID", eventID);
            para.Add("intLotteryID", dbType: DbType.Int32, direction: ParameterDirection.Output);
            para.Add("intLotteryRemaining", dbType: DbType.Int32, direction: ParameterDirection.Output);
            para.Add("chvRtnMsg", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Query(spName, para, commandType: CommandType.StoredProcedure);
                var resultModel = new PromotionEventLotteryResultModel();
                resultModel.EventID = para.Get<int>("intLotteryID");
                resultModel.LotteryRemaining = para.Get<int>("intLotteryRemaining");
                resultModel.EventMsg = para.Get<string>("chvRtnMsg");
                return resultModel;
            }
        }
    }
}
