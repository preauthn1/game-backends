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
    public class PerGameDetailLogMerchantJoinRepository : FormatMultiplePrimaryKeyRepository<PerGameDetailLogMerchantJoinEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[PerGameDetailLogMerchantJoin]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "PerGameDetailLogID"; }
        }


        public void UpdateAndReplaceMerchantID(int originalMerchantID , PerGameDeleteProcess_UpdateMerchantPlayerParameter param)
        {
            param.RuleFor(x => x.Id).Rule_NotZeroNotMinus();
            param.RuleFor(x => x.GameTime).Rule_NotEmptyDateTime();
            param.RuleFor(x => x.NewPlayerID).Rule_NotZeroNotMinus();
            param.RuleFor(x => x.NewMerchantID).Rule_NotZeroNotMinus();

            string sql = $@" UPDATE {TableName} SET 
                                MerchantID = @NewMerchantID 
                             WHERE PerGameDetailLogID = @Id AND GameTime = @GameTime AND MerchantID = {originalMerchantID} ";

            ExecuteSqlByDapper(sql, param);
        }


        public List<PerGameDetailLogMerchantJoinEntity> GetByFilter(QueryDetailLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<PerGameDetailLogMerchantJoinEntity>(sql, filter);
            return query;
        }


        public PagedList<PerGameDetailLogMerchantJoinEntity> GetPagingByFilter(QueryDetailLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<PerGameDetailLogMerchantJoinEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryDetailLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.MerchantID.HasValue)
                sql += " AND MerchantID = @MerchantID ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            if (filter.GameStartTimeBegin.HasValue)
                sql += " AND GameTime >= @GameStartTimeBegin ";

            if (filter.GameStartTimeEnd.HasValue)
                sql += " AND GameTime < @GameStartTimeEnd ";

            return sql;
        }

    }


}
