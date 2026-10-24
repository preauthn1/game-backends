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
    public class WagerTransactionMappingFumaRepository : FormatIntegerPrimaryKeyRepository<WagerTransactionMappingFumaEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogRawDB); }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[WagerTransactionMappingFuma]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "TransactionID"; }
        }


        public int GetCountByFilter(QueryWagerTransactionMappingGeneralRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter, " count(*) ");
            var query = QueryByDapper<int>(sql, filter);
            return query.FirstOrDefault();
        }


        public List<WagerTransactionMappingVPlusEntity> GetByFilter(QueryWagerTransactionMappingGeneralRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<WagerTransactionMappingVPlusEntity>(sql, filter);
            return query;
        }


        public PagedList<WagerTransactionMappingVPlusEntity> GetPagingByFilter(QueryWagerTransactionMappingGeneralRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<WagerTransactionMappingVPlusEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryWagerTransactionMappingGeneralRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.BetRecordNoPGS.IsNotEmptyTextNotNull())
                sql += " AND BetRecordNoPGS = @BetRecordNoPGS ";

            return sql;
        }

    }


}
