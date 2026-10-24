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
    public class BetRecordRawWagerTagMappingRepository : FormatIntegerPrimaryKeyRepository<BetRecordRawWagerTagMappingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[BetRecordRawWagerTagMapping]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BetRecordNo"; }
        }


        public List<BetRecordRawWagerTagMappingEntity> GetByFilter(QueryBetRecordRawWagerTagMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<BetRecordRawWagerTagMappingEntity>(sql, filter);
            return query;
        }


        public PagedList<BetRecordRawWagerTagMappingEntity> GetPagingByFilter(QueryBetRecordRawWagerTagMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<BetRecordRawWagerTagMappingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryBetRecordRawWagerTagMappingRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.GameVendorID.HasValue)
                sql += " AND GameVendorID = @GameVendorID ";

            if (filter.TagID.HasValue)
                sql += " AND TagID = @TagID ";

            if (filter.BetRecordNo.IsNotEmptyTextNotNull())
                sql += " AND BetRecordNo = @BetRecordNo ";

            if (filter.BetRecordNoList.IsNotEmptyNull())
                sql += " AND BetRecordNo in @BetRecordNoList ";

            return sql;
        }

    }


}
