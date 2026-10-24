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
    public class RiskControlConfigRepository : FormatIntegerPrimaryKeyRepository<RiskControlConfigEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[RiskControlConfig]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "RiskControlConfigID"; }
        }

        public List<RiskControlConfigEntity> GetByFilter(QueryRiskControlConfigRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<RiskControlConfigEntity>(sql, filter);
            return query;
        }


        public PagedList<RiskControlConfigEntity> GetPagingByFilter(QueryRiskControlConfigRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<RiskControlConfigEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryRiskControlConfigRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND (MerchantID in @MerchantIDs OR IsAllMerchants = 1)";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND (GameID in @GameIDs  OR  IsAllGames = 1) ";

            if(filter.Status.HasValue)
                sql += " AND Status = @Status ";

            return sql;
        }

    }


}
