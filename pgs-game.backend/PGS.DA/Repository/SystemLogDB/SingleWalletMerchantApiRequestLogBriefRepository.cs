using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.DA.Repository
{
    public class SingleWalletMerchantApiRequestLogBriefRepository : FormatLongPrimaryKeyRepository<SingleWalletMerchantApiRequestLogBriefEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.SystemLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[SingleWalletMerchantApiRequestLogBrief]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "UniqueID"; }
        }

        protected override string DefaultSortColumn 
        {
            get { return "CreateTime"; }
        }


        public List<SingleWalletMerchantApiRequestLogEntity> GetByFilter(QuerySingleWalletMerchantApiRequestLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<SingleWalletMerchantApiRequestLogEntity>(sql, filter, filter.TimeoutSeconds);
            return query;
        }


        public PagedList<SingleWalletMerchantApiRequestLogEntity> GetPagingByFilter(QuerySingleWalletMerchantApiRequestLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<SingleWalletMerchantApiRequestLogEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QuerySingleWalletMerchantApiRequestLogRequestViewModel filter)
        {
            string topSize = filter.TopSize.HasValue ? $" TOP {filter.TopSize.Value}" : "";

            string sql = $@"SELECT {topSize} * FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.UniqueID.IsNotEmptyTextNotNull())
                sql += " AND UniqueID = @UniqueID ";

            if (filter.MerchantID.HasValue)
                sql += " AND MerchantID = @MerchantID ";

            //if (filter.RequestID.IsNotEmptyTextNotNull())
            //    sql += " AND RequestID = @RequestID ";

            if (filter.Category.IsNotEmptyTextNotNull())
                sql += " AND Category = @Category ";

            if (filter.SubCategory.IsNotEmptyTextNotNull())
                sql += " AND SubCategory = @SubCategory ";

            if (filter.CreateTimeBegin.HasValue)
                sql += " AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += " AND CreateTime < @CreateTimeEnd ";

            if (filter.ResponseTimeLargeThan.HasValue)
                sql += " AND ResponseTime >= @ResponseTimeLargeThan ";

            if (filter.ResponseStatusCode.HasValue)
            {
                if (filter.ResponseStatusCode < 0)
                {
                    filter.ResponseStatusCode = filter.ResponseStatusCode.Value.ToAbs();
                    sql += " AND ResponseStatusCode != @ResponseStatusCode ";
                }
                else
                {
                    sql += " AND ResponseStatusCode = @ResponseStatusCode ";
                }
            }

            if (filter.ReferenceTransactionNo.IsNotEmptyTextNotNull())
                sql += " AND ReferenceTransactionNo = @ReferenceTransactionNo ";

            if (filter.RequestContentKeyword.IsNotEmptyTextNotNull())
                sql += $" AND {GenerateLikeQuery("RequestContent", "@RequestContentKeyword")} ";

            if (filter.ResponseTextKeyword.IsNotEmptyTextNotNull())
                sql += $" AND {GenerateLikeQuery("ResponseText", "@ResponseTextKeyword")} ";

            if (filter.DebugInfo2.IsNotEmptyTextNotNull())
                sql += $" AND DebugInfo2 = @DebugInfo2 ";

            sql += $" {filter.ExtraSpecificOrderBy} ";
            return sql;
        }

    }


}
