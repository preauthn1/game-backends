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
    public class MessageAssignRepository : FormatIntegerPrimaryKeyRepository<MessageAssignEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MessageAssign]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "MessageAssignID"; }
        }

        public List<MessageAssignEntity> GetByFilter(QueryMessageAssignRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<MessageAssignEntity>(sql, filter);
            return query;
        }


        public PagedList<MessageAssignEntity> GetPagingByFilter(QueryMessageAssignRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<MessageAssignEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMessageAssignRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
