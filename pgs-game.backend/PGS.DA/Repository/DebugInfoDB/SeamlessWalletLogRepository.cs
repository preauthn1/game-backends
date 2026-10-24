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
    public class SeamlessWalletLogRepository : FormatIntegerPrimaryKeyRepository<SeamlessWalletLogEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.DebugInfoDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[SeamlessWalletLog]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<SeamlessWalletLogEntity> GetByFilter(QuerySeamlessWalletLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<SeamlessWalletLogEntity>(sql, filter);
            return query;
        }


        public PagedList<SeamlessWalletLogEntity> GetPagingByFilter(QuerySeamlessWalletLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<SeamlessWalletLogEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QuerySeamlessWalletLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
