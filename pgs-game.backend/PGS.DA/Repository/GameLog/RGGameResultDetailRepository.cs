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
    public class RGGameResultDetailRepository : FormatMultiplePrimaryKeyRepository<RGGameResultDetailEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogRawDB); }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[RGGameResultDetail]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BetRecordNoPGS"; }
        }

        public List<RGGameResultDetailEntity> GetByFilter(QueryRGGameResultDetailRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<RGGameResultDetailEntity>(sql, filter);
            return query;
        }


        public PagedList<RGGameResultDetailEntity> GetPagingByFilter(QueryRGGameResultDetailRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<RGGameResultDetailEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryRGGameResultDetailRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
