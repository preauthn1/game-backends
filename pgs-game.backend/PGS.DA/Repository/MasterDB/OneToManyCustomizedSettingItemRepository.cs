using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Models;
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
    public class OneToManyCustomizedSettingItemRepository : FormatMultiplePrimaryKeyRepository<OneToManyCustomizedSettingItemEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[OneToManyCustomizedSettingItem]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "OneToManyCustomizedSettingID"; }
        }



        public List<VendorConfigDateMemoModel> GetVendorConfigReportDateMemo(OneToManyCustomizedSettingTypeEnum type, bool isExcludeEmptyValue = true)
        {
            string sql = @"SELECT item.* , v.VendorConfigID , v.VendorID, v.Name , v.SupportCurrency 
                            FROM [dbo].[OneToManyCustomizedSettingItem] as item 
                            INNER JOIN[dbo].[OneToManyCustomizedSetting] as o on o.OneToManyCustomizedSettingID = item.OneToManyCustomizedSettingID
                            INNER JOIN[ThirdParty].[VendorConfig] as v on o.OwnerID = v.VendorConfigID
                            WHERE o.Type = @type ";

            var param = new
            {
                type = type.ToInteger()
            };

            var query = QueryByDapper<VendorConfigDateMemoModel>(sql, param);

            if (isExcludeEmptyValue)
            {
                query = query
                    .Where(x => x.SettingValue.IsNotEmptyTextNotNull())
                    .ToList();
            }

            return query;
        }

        public List<OneToManyCustomizedSettingItemEntity> GetByMasterSettingType(OneToManyCustomizedSettingTypeEnum type, bool isExcludeEmptyValue = true)
        {
            var query = GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
            {
                OneToManyCustomizedSettingType = type.ToInteger()
            });

            if (isExcludeEmptyValue)
            {
                query = query
                    .Where(x => x.SettingValue.IsNotEmptyTextNotNull())
                    .ToList();
            }

            return query;
        }


        public OneToManyCustomizedSettingItemEntity GetByMasterSettingTypeAndSubCategory(OneToManyCustomizedSettingTypeEnum type, string subCategory)
        {
            var query = GetByMasterSettingType(type)
                .Where(x => x.SubCategory == subCategory)
                .FirstOrDefault();

            return query;
        }



        public SystemConfigSettingParseModel GetSingleMatchParseModel(QueryOneToManyCustomizedSettingItemRequestViewModel filter)
        {
            var query = GetByFilter(filter);
            var match = query.FirstOrDefault();

            if(match == null)
            {
                return new SystemConfigSettingParseModel
                {
                    SettingKey = 0,
                    SettingValue = ""
                };
            }

            return new SystemConfigSettingParseModel(match.SettingKey, match.SettingValue);
        }


        public void ResetDataForMSTest()
        {
            int targetType = OneToManyCustomizedSettingTypeEnum.GameUrlDoamin_BackupUrls.ToInteger();
            string sql = @" UPDATE [dbo].[OneToManyCustomizedSettingItem] set Memo = ''
                            WHERE OneToManyCustomizedSettingID in
                            (
	                            SELECT  OneToManyCustomizedSettingID FROM [dbo].[OneToManyCustomizedSetting]
	                            WHERE Type = @Type
                            )";

            var param = new
            {
                Type = targetType
            };

            ExecuteSqlByDapper(sql, param);
            return;
        }


        public List<OneToManyCustomizedSettingItemEntity> GetByFilter(QueryOneToManyCustomizedSettingItemRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<OneToManyCustomizedSettingItemEntity>(sql, filter);
            return query;
        }


        public PagedList<OneToManyCustomizedSettingItemEntity> GetPagingByFilter(QueryOneToManyCustomizedSettingItemRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<OneToManyCustomizedSettingItemEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryOneToManyCustomizedSettingItemRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";


            if (filter.OneToManyCustomizedSettingID.HasValue)
                sql += $@" AND OneToManyCustomizedSettingID = @OneToManyCustomizedSettingID ";

            if (filter.OneToManyCustomizedSettingType.HasValue)
            {
                sql += $@" AND OneToManyCustomizedSettingID in 
                            ( 
                                SELECT [OneToManyCustomizedSettingID] FROM [dbo].[OneToManyCustomizedSetting] 
                                WHERE Type = @OneToManyCustomizedSettingType 
                            )";
            }

            if (filter.Keyword.IsNotEmptyTextNotNull())
            {
                sql += $" AND (" +
                             $"{GenerateLikeQuery("SettingValue", "@Keyword")} OR " +
                             $"{GenerateLikeQuery("SubCategory", "@Keyword")} " +
                             ")";
            }

            if (filter.SubCategory.IsNotEmptyTextNotNull())
                sql += $@" AND SubCategory = @SubCategory ";

            if (filter.SettingKeys.IsNotEmptyNull())
                sql += $@" AND SettingKey in @SettingKeys ";

            if (filter.Memo2Match.IsNotEmptyTextNotNull())
                sql += $@" AND Memo2 = @Memo2Match ";

            if (filter.TagID.HasValue)
            {
                sql += $@" AND LEN(SubCategory) > 3 AND SubCategory in 
                (
                    SELECT TargetID FROM [dbo].[GeneralTagMapping] WITH (NOLOCK)
                    WHERE TagID = @TagID
                ) ";
            }

            if (filter.IsAssignProductionMerchantOnly)
            {
                sql += $@" AND LEN(SubCategory) > 3 AND SubCategory in 
                (
                    SELECT SettingValue FROM [dbo].[MerchantCustomizedSetting] WITH (NOLOCK)
                    WHERE LEN(SettingValue) > 3 AND CustomizedSettingKey = 3088 AND MerchantID in
                    (
	                    SELECT MerchantID FROM [dbo].[Merchant] WITH (NOLOCK)
	                    WHERE Type = 20 AND Status = 1
                    )
                ) ";
            }

            if (filter.NotAssignTagID.HasValue)
            {
                sql += $@" AND LEN(SubCategory) > 3 AND SubCategory not in 
                (
                    SELECT TargetID FROM [dbo].[GeneralTagMapping] WITH (NOLOCK)
                    WHERE TagID = @NotAssignTagID
                ) ";
            }

            if (filter.UniqueOnwerQuery != null)
            {
                sql += $@" AND OneToManyCustomizedSettingID in 
                            ( 
                                SELECT OneToManyCustomizedSettingID FROM [dbo].[OneToManyCustomizedSetting] 
                                WHERE OwnerID = {filter.UniqueOnwerQuery.OwnerID} AND Type = {(int)filter.UniqueOnwerQuery.Type}
                            ) ";

            }

            return sql;
        }

    }


}
