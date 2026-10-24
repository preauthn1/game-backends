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
using System.Data;
using System.Linq;

namespace target.DA.Repository
{
    public class MerchantCreditRepository : FormatIntegerPrimaryKeyRepository<MerchantCreditEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MerchantCredit]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public void UpdateCreditAmount(MerchantCreditUpdateSPParameter para)
        {
            string spName = "PrWeb_UpdateMerchantCredit";

            using (var conn = InstanceSQLConn(GetConnectionStringByDbName(DBConnType.TransactionDB)))
            {
                var result = conn.Execute(spName, param: para, commandType: CommandType.StoredProcedure);
            }
        }


        public List<MerchantCreditEntity> GetByFilter(QueryMerchantCreditRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<MerchantCreditEntity>(sql, filter);
            return query;
        }


        public PagedList<MerchantCreditEntity> GetPagingByFilter(QueryMerchantCreditRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<MerchantCreditEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMerchantCreditRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
