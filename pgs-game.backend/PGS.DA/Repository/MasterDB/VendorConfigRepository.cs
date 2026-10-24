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
using Lck.Utility.Extensions.Helper;
using target.Models.Models;
using target.Models.Enums;
using target.Utility.Helper;

namespace target.DA.Repository
{

    public class VendorConfigRepository : FormatIntegerPrimaryKeyRepository<VendorConfigEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[VendorConfig]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "VendorConfigID"; }
        }

        public VendorConfigEntity GetSingleByCache(int vendorConfigID)
        {
            string cacheKey = CacheKey.DbEntity_VendorConfig(vendorConfigID);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var query = GetById(vendorConfigID);
                   return query;
               }, 15);

            return queryCache;
        }

        public VendorConfigEntity GetCacheByVendorTypeAndMerchant(int merchantID , int gameVendorType)
        {
            string cacheKey = CacheKey.DbEntity_VendorConfig_By_Merchant_GameVendor(merchantID , gameVendorType);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var query = GetByFilter(new QueryVendorConfigRequestViewModel
                {
                    VendorIDs = gameVendorType.ToListCollection(),
                    MerchantIDs = merchantID.ToListCollection()
                });

                return query.FirstOrDefault();
            }, 15);

            return queryCache;
        }

        public List<VendorConfigMerchantInfoModel> GetByVendorTypeAndMerchants(int gameVendorType , List<int> merchantIDs)
        {
            var queryColumns = GenerateInQuery_Or_LargeIDsQuery(" @merchantIDs ", merchantIDs);

            string sql = $@" SELECT c.VendorID , c.VendorConfigID , c.Name as VendorConfigName, c.IsDefault , mapping.MerchantID 
                            FROM [ThirdParty].[VendorConfig] as c
                            JOIN [ThirdParty].[VendorConfigMerchantMapping] as mapping on c.VendorConfigID = mapping.VendorConfigID
                            WHERE c.VendorID = @gameVendorType AND mapping.MerchantID in {queryColumns.WhereSQL} ";

            var para = new
            {
                merchantIDs = merchantIDs,
                gameVendorType = gameVendorType,
            };

            var config = new DbQueryConfig
            {
                ConnectTimeoutSeconds = null,
                TempIDsAppendSQL = queryColumns.TempIDsAppendSQL
            };

            var query = QueryByDapper<VendorConfigMerchantInfoModel>(sql, para , config);
            return query;
        }

        public int GetCountByFilter(QueryVendorConfigRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter, " count(*) ");
            var query = QueryByDapper<int>(sql, filter);
            return query.FirstOrDefault();
        }


        public List<VendorConfigEntity> GetByFilter(QueryVendorConfigRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<VendorConfigEntity>(sql, filter);
            return query;
        }


        public PagedList<VendorConfigEntity> GetPagingByFilter(QueryVendorConfigRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<VendorConfigEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryVendorConfigRequestViewModel filter , string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK) 
                           WHERE 1=1 ";

            if(!filter.IsIncludeDeleteData)
                sql += $" AND [Status] != {NormalActiveStatus.PermanentDelete.ToInteger()} ";

            if (filter.VendorConfigIDs.IsNotEmptyNull())
                sql += " AND VendorConfigID in @VendorConfigIDs ";

            if (filter.VendorIDs.IsNotEmptyNull())
                sql += " AND VendorID in @VendorIDs ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND VendorConfigID in (SELECT VendorConfigID from [ThirdParty].[VendorConfigMerchantMapping] WHERE MerchantID in @MerchantIDs) ";

            if (filter.FilterMerchantIDsByMultipleHierarchyMerchant.IsNotEmptyNull())
            {
                string myWhereSQL = AppHelper.GenerateMultipleHierarchyMerchantFilterWhereSQL_V2026("MerchantID" , "");
                sql += $" AND VendorConfigID in (SELECT VendorConfigID from [ThirdParty].[VendorConfigMerchantMapping] WHERE {myWhereSQL} ) ";
            }

            if (filter.IsTestEnvironment.HasValue)
                sql += " AND IsTestEnvironment = @IsTestEnvironment ";

            if (!string.IsNullOrWhiteSpace(filter.SupportCurrency))
                sql += " AND SupportCurrency = @SupportCurrency ";

            if (filter.IsDefault.HasValue)
                sql += " AND IsDefault = @IsDefault ";

            if (filter.IsFetchFlagOnly)
                sql += " AND IsSkipFetchSummary = 0 ";

            if (filter.Keyword.IsNotEmptyTextNotNull())
            {
                //$"{GenerateLikeQuery("Memo", "@Keyword")} OR " +


            sql += @$" AND ( 
                    {GenerateLikeQuery("Name", "@Keyword")} OR 
                    {GenerateLikeQuery("cast(VendorConfigID as nvarchar(10))", "@Keyword")} OR
                    {GenerateLikeQuery("Memo", "@Keyword")}   
                   ) ";
            }

            return sql;
        }

    }


}
