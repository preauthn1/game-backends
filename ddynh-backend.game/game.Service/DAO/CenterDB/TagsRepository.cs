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

    public class TagsRepository : IDapperRepository<TagsRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Users].[Tags]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "TagID"; }
        }

        public List<TagsRawEntity> GetByFilter(QueryTagsRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<TagsRawEntity>(sql, filter);
            return query;
        }

        public PagedList<TagsRawEntity> GetPagingByFilter(QueryTagsRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<TagsRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryTagsRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.TagIDs.IsNotEmptyNull())
                sql += " AND TagID in @TagIDs ";

            return sql;
        }

    }


}
