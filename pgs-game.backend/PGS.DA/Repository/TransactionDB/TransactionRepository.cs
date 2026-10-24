using Dapper;
using game.Entity;
using target.Core.Repository;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using target.Utility.Extensions;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using target.Models.Models.StoreProcedures;
using target.Utility.Helper;
using target.Models.Models;

namespace target.DA.Repository
{

    public class TransactionRepository : FormatLongPrimaryKeyRepository<TransactionEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.TransactionDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[Transaction]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "TransactionID"; }
        }

        protected override string DefaultSortColumn
        {
            get
            {
                return "CreateTime";  //用partition key排序會比PK Id快
            }
        }

        public override long Create(TransactionEntity model, bool isTransactionScope = false)
        {
            throw new TerproException(ApiErrorCode.BusinessLogicDeny, "請統一讓Insert行為只在SP處理 PrWeb_AddTransaction");
        }


        public List<TransactionEntity> GetByFilter(QueryTransactionRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<TransactionEntity>(sql, filter);
            return query;
        }


        public PagedList<TransactionEntity> GetPagingByFilter(QueryTransactionRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);

            var query = GetPagingEntitiesBySQLScript<TransactionEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryTransactionRecordRequestViewModel filter, string selectColumns)
        {
            string targetTableName = TableName;

            if (!string.IsNullOrWhiteSpace(filter.TargetTableName))
            {
                targetTableName = filter.TargetTableName;
            }

            string sql = $@"SELECT {selectColumns} FROM {targetTableName} WITH (NOLOCK) " +
                           "WHERE 1=1 ";

            //*這個的順序要在merchantIDs之前
            if (filter.FilterMerchantIDByHierarchyMerchant.HasValue)
            {
                filter.MerchantIDs = AppHelper.SetListToEmptyWhenLargeFilter(filter.MerchantIDs);
                sql += AppHelper.GenerateMerchantHierarchyWhereSQL(filter.FilterMerchantIDByHierarchyMerchant.Value);
            }
            else if (filter.FilterMerchantIDsByMultipleHierarchyMerchant.IsNotEmptyNull())
            {
                filter.MerchantIDs = AppHelper.SetListToEmptyWhenLargeFilter(filter.MerchantIDs);

                string sql1 = $" {AppHelper.GenerateMultipleHierarchyMerchantFilterWhereSQL("MerchantID", "")} ";
                string sql2 = " MerchantID in @FilterMerchantIDsByMultipleHierarchyMerchant ";
                sql += $" AND ( {sql1} OR {sql2} )";
            }

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += "AND MerchantID in @MerchantIDs ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += "AND PlayerID in @PlayerIDs ";

            if (filter.GameID.HasValue)
                sql += "AND GameID = @GameID ";

            if (filter.TransactionType.HasValue)
                sql += "AND TransactionType = @TransactionType ";

            if (filter.TransactionTypeList.IsNotEmptyNull())
                sql += "AND TransactionType in @TransactionTypeList ";

            if (filter.IPAddress.IsNotEmptyNull())
                sql += "AND IPAddress = @IPAddress ";

            if (filter.CreateTimeBegin.HasValue)
                sql += "AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += "AND CreateTime <= @CreateTimeEnd ";

            if (filter.AmountMatch.HasValue)
                sql += "AND Amount = @AmountMatch ";

            if (filter.ReferenceOrderNo.IsNotEmptyTextNotNull())
                sql += "AND ReferenceOrderNo = @ReferenceOrderNo ";

            if (filter.ReferenceOrderNoList.IsNotEmptyNull())
                sql += "AND ReferenceOrderNo in @ReferenceOrderNoList ";

            return sql;
        }
    }
}
