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

namespace game.Service.DAO
{

    public class UserTagMappingRepository : IDapperRepository<UserTagMappingRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Users].[UserTagMapping]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<UserTagMappingRawEntity> GetByFilter(QueryUserTagMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<UserTagMappingRawEntity>(sql, filter);
            return query;
        }

        public PagedList<UserTagMappingRawEntity> GetPagingByFilter(QueryUserTagMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<UserTagMappingRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryUserTagMappingRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND UserID in @UserIDs ";

            if (filter.TagIDs.IsNotEmptyNull())
                sql += " AND TagID in @TagIDs ";

            return sql;
        }

    }


}
