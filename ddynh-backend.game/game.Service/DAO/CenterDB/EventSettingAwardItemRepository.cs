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
    public class EventSettingAwardItemRepository : IDapperRepository<EventSettingAwardItemEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Event].[EventSettingAwardItem]"; }
        }

        public List<EventSettingAwardItemEntity> GetBySettingID(int settingID)
        {
            var sql = GenerateColumnEqualWhereQuery("EventSettingID");
            var query = QueryByDapper<EventSettingAwardItemEntity>(sql, new { EventSettingID = settingID });
            return query;
        }

        public void UpdateImageUrl(EventSettingAwardItemEntity model)
        {
            string deleteSQL = $@" UPDATE {TableName} " + 
                                "SET ImageUrlDomain = @ImageUrlDomain, ImageUrl = @ImageUrl " + 
                                "WHERE Id = @id ";

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute(deleteSQL, new { ImageUrlDomain = model.ImageUrlDomain, ImageUrl = model.ImageUrl, Id = model.Id });
            }
        }

        public void Delete(int id)
        {
            string deleteSQL = $@" DELETE FROM {TableName} WHERE Id = @id ";

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute(deleteSQL, new { Id = id });
            }
        }
    }
}
