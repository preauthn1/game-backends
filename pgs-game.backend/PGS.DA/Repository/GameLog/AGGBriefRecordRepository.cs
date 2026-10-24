using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.DA.Repository
{
    public class AGGBriefRecordRepository : FormatIntegerPrimaryKeyRepository<AGGBriefRecordEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[WagerFetch].[AGGBriefRecord]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "WagerID"; }
        }


        public int GetCountByFilter(QueryAGGBriefRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter, " count(*) ");
            var query = QueryByDapper<int>(sql, filter);
            return query.FirstOrDefault();
        }


        public List<AGGBriefRecordEntity> GetByFilter(QueryAGGBriefRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<AGGBriefRecordEntity>(sql, filter);
            return query;
        }


        public PagedList<AGGBriefRecordEntity> GetPagingByFilter(QueryAGGBriefRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<AGGBriefRecordEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryAGGBriefRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
