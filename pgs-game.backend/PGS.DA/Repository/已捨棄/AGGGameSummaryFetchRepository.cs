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
    public class AGGGameSummaryFetchRepository : FormatIntegerPrimaryKeyRepository<AGGGameSummaryFetchEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[AGGGameSummaryFetch]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public DateTime GetLastFetchDate()
        {
            string sql = @"SELECT top 1 StatDate from [WagerFetch].[AGGGameSummaryFetch] with (NOLOCK) 
                           order by StatDate desc";

            var queryByDate = QueryByDapper<DateTime>(sql , null);
            return queryByDate.FirstOrDefault();
        }


        public int GetCountByFilter(QueryAGGGameSummaryFetchRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter, " count(*) ");
            var query = QueryByDapper<int>(sql, filter);
            return query.FirstOrDefault();
        }


        public List<AGGGameSummaryFetchEntity> GetByFilter(QueryAGGGameSummaryFetchRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<AGGGameSummaryFetchEntity>(sql, filter);
            return query;
        }


        public PagedList<AGGGameSummaryFetchEntity> GetPagingByFilter(QueryAGGGameSummaryFetchRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<AGGGameSummaryFetchEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryAGGGameSummaryFetchRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
