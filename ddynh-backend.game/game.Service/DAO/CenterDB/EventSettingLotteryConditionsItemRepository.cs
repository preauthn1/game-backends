using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Event;
using game.Infrastructure;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using game.Infrastructure.Extensions;

namespace game.Service.DAO
{
    public class EventSettingLotteryConditionsItemRepository : IDapperRepository<EventSettingLotteryConditionsItemEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Event].[EventSettingLotteryConditionsItem]"; }
        }

        public List<EventSettingLotteryConditionsItemEntity> GetBySettingID(int settingID)
        {
            var sql = GenerateColumnEqualWhereQuery("EventSettingID");
            var query = QueryByDapper<EventSettingLotteryConditionsItemEntity>(sql, new { EventSettingID = settingID });
            return query;
        }

        public void Delete(int id)
        {
            string deleteSQL = $@" DELETE FROM {TableName} WHERE Id = @id ";

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute(deleteSQL, new { id = id });
            }
        }
    }
}
