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
    public class StatControlTableRepository : FormatIntegerPrimaryKeyRepository<StatControlTableEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.StatReportDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[StatControlTable]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "TableName"; }
        }


        public int GetCountByFilter(QueryStatControlTableRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter, " count(*) ");
            var query = QueryByDapper<int>(sql, filter);
            return query.FirstOrDefault();
        }


        public List<StatControlTableEntity> GetByFilter(QueryStatControlTableRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<StatControlTableEntity>(sql, filter);
            return query;
        }


        public PagedList<StatControlTableEntity> GetPagingByFilter(QueryStatControlTableRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<StatControlTableEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryStatControlTableRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} 
                           WHERE 1=1 ";

            return sql;
        }

    }


}
