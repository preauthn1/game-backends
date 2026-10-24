using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.Event;
using game.Entity.Money;
using game.Infrastructure;
using game.Infrastructure.Extensions;

namespace game.Service.DAO
{
    public class EventDetailLogRepository : IDapperRepository<EventDetailLogEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Users].[EventDetailLog]"; }
        }


        public PagedList<EventDetailLogEntity> GetPagingByFilter(QueryEventSettingRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName}
                            WHERE 1=1 ";

            filter.PlatformIDs = filter.PlatformIDsText.SeperateToListIntOrDefaultValue(filter.PlatformIDs);

            if (filter.StartTimeBegin.HasValue)
                sql += " AND StartTime >= @StartTimeBegin ";

            if (filter.StartTimeEnd.HasValue)
                sql += " AND StartTime <= @StartTimeEnd ";

            if (filter.PlatformIDs.IsNotEmptyNull())
                sql += " AND PlatformID in @PlatformIDs ";

            if (filter.EventType.HasValue)
                sql += " AND EventType = @EventType ";

            if (filter.Status.HasValue)
                sql += " AND Status = @Status ";

            if (!filter.IsIncludeDeleteData)
                sql += $" AND Status != {(int)NormalActiveStatusEnum.PermanentInvisible} ";

            if (filter.UserID.HasValue)
                sql += " AND UserID = @UserID ";

            if (filter.EventSettingID.HasValue)
                sql += " AND EventSettingID = @EventSettingID ";

            var query = GetPageingEntitiesBySQLScript<EventDetailLogEntity>(sql, filter, filter);
            return query;
        }


    }



}
