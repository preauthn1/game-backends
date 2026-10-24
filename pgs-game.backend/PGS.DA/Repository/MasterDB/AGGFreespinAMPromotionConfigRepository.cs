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
    public class AGGFreespinAMPromotionConfigRepository : FormatIntegerPrimaryKeyRepository<AGGFreespinAMPromotionConfigEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[AGGFreespinAMPromotionConfig]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<AGGFreespinAMPromotionConfigEntity> GetByFilter(QueryAGGFreespinAMPromotionConfigRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<AGGFreespinAMPromotionConfigEntity>(sql, filter);
            return query;
        }


        public PagedList<AGGFreespinAMPromotionConfigEntity> GetPagingByFilter(QueryAGGFreespinAMPromotionConfigRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<AGGFreespinAMPromotionConfigEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryAGGFreespinAMPromotionConfigRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND ID in @IDs ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            if (filter.Type.HasValue)
                sql += " AND Type = @Type ";

            return sql;
        }

    }


}
