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

    /// <summary>
    /// 臨時拿來 處理/補救/暫存 某些資料的表
    /// </summary>
    public class DebugTempTableRepository : FormatIntegerPrimaryKeyRepository<DebugTempTableEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[DebugTempTable]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<DebugTempTableEntity> GetByFilter(QueryDebugTempTableRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<DebugTempTableEntity>(sql, filter);
            return query;
        }


        public PagedList<DebugTempTableEntity> GetPagingByFilter(QueryDebugTempTableRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<DebugTempTableEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryDebugTempTableRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
