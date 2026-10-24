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
    public class RiskControlRecordRepository : FormatIntegerPrimaryKeyRepository<RiskControlRecordEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.TransactionDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[RiskControlRecord]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public int GetCountByConfigID(int targetRiskControlConfigID , DateTime beginTime)
        {
            string sql = @$"SELECT count(*) FROM {TableName} WITH (NOLOCK)
                            WHERE TargetRiskControlConfigID = @targetRiskControlConfigID AND CreateTime >= @beginTime";

            var para = new
            {
                targetRiskControlConfigID = targetRiskControlConfigID,
                beginTime = beginTime
            };

            var query = QueryByDapper<int>(sql, para);
            return query.FirstOrDefault();
        }


        public List<RiskControlRecordEntity> GetByFilter(QueryRiskControlRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<RiskControlRecordEntity>(sql, filter);
            return query;
        }


        public PagedList<RiskControlRecordEntity> GetPagingByFilter(QueryRiskControlRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<RiskControlRecordEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryRiskControlRecordRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
