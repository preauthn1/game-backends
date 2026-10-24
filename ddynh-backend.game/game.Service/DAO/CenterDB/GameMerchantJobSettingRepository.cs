using Dapper;
using System;
using System.Collections.Generic;
using game.Infrastructure.Extensions;
using System.Linq;
using FW.Core.Repository;
using game.Infrastructure;
using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.RawEntity;
using game.Entity.ViewModels.Query;
using System.Data.SqlClient;

namespace game.Service.DAO
{

    public class GameMerchantJobSettingRepository : IDapperRepository<GameMerchantJobSettingRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[ThirdParty].[GameMerchantJobSetting]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "ID"; }
        }

        public void UpdateLastTimeAndNextTime(int ID , DateTime nextTime)
        {
            string sql = $@"UPDATE {TableName} SET 
                                LastExecutedTime = GETDATE() , 
                                JobNextStarttTime = @nextTime 
                            WHERE ID = @ID";

            var para = new
            {
                ID = ID,
                nextTime = nextTime
            };

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute(sql, para);
                return;
            }
        }
      
        public List<GameMerchantJobSettingRawEntity> GetByFilter(QueryPlatformGameMerchantJobSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<Entity.RawEntity.GameMerchantJobSettingRawEntity>(sql, filter);
            return query;
        }

        public PagedList<GameMerchantJobSettingRawEntity> GetPagingByFilter(QueryPlatformGameMerchantJobSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<Entity.RawEntity.GameMerchantJobSettingRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryPlatformGameMerchantJobSettingRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.PlatformIDs.IsNotEmptyNull())
                sql += " AND PlatformID in @PlatformIDs ";

            if (filter.JobTypes.IsNotEmptyNull())
                sql += " AND JobType in @JobTypes ";

            if (filter.JobCategories.IsNotEmptyNull())
                sql += " AND JobCategory in @JobCategories ";

            if (filter.GameMerchantIDs.IsNotEmptyNull())
                sql += " AND GameMerchantID in @GameMerchantIDs ";

            if (filter.IsEnableOnly)
                sql += " AND IsEnable = 1 ";


            return sql;
        }

    }


}
