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
    public class IPAddressListTemplateRepository : FormatIntegerPrimaryKeyRepository<IPAddressListTemplateEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[IPAddressListTemplate]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        public IPAddressListTemplateBriefModel GetSingleBriefByCache(int id)
        {
            var cacheKey = CacheKey.DbEntity_Brief_IPAddressListTemplate_Id(id);

            var cache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   string selectColumns = " Id , DisplayName , IPAddressList  ";
                   var query = GetByFilter(new QueryIPAddressListTemplateRequestViewModel
                   {
                       IDs = id.ToListCollection()

                   }, selectColumns);

                   return query.FirstOrDefault();

               }, 5);

            if (cache == null)
                return null;

            return cache.ReturnInjecteObject<IPAddressListTemplateBriefModel>();
        }

        public void DeleteByNameKeyword_ForMSTest(string keyword)
        {
            string sql = $@"DELETE FROM {TableName}
                            WHERE {GenerateLikeQuery("DisplayName", "@keyword")} ";

            var param = new
            {
                keyword = keyword
            };

            ExecuteSqlByDapper(sql, param);
        }


        public List<IPAddressListTemplateEntity> GetByFilter(QueryIPAddressListTemplateRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<IPAddressListTemplateEntity>(sql, filter);
            return query;
        }


        public PagedList<IPAddressListTemplateEntity> GetPagingByFilter(QueryIPAddressListTemplateRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<IPAddressListTemplateEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryIPAddressListTemplateRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND ID in @IDs ";

            if (filter.Keyword.IsNotEmptyTextNotNull())
            {
                sql += @$" AND 
                       ( 
                         {GenerateLikeQuery("DisplayName", "@Keyword")} 
                         OR {GenerateLikeQuery("Memo", "@Keyword")} 
                         OR {GenerateLikeQuery("cast(Id as nvarchar(10))", "@Keyword")}
                       ) ";
            }

            if (filter.IPAddressNotContainsThisKeyword.IsNotEmptyTextNotNull())
            {
                string extraSQL = $@" AND Id not in
                ( 
                    SELECT Id FROM {TableName} WITH (NOLOCK)
                    WHERE {GenerateNotLikeQuery("IPAddressList", "@IPAddressNotContainsThisKeyword")} 
                        AND
                    LEN(IPAddressList) <= 5
                )  ";

                sql += extraSQL;
            }

            if (filter.imChannelMappingID.IsNotEmptyTextNotNull())
            {
                int imChanne = MerchantCustomizedSettingKeyEnum.Common_IMChannelID.ToInteger();
                int ipTemplate = MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting.ToInteger();

                //針對IM頻道ID篩選
                string extraSQL = $@"Id in
                            (
                                SELECT cast(ip_template.SettingValue as int) FROM [dbo].[MerchantCustomizedSetting] as ms
                                INNER JOIN [dbo].[MerchantCustomizedSetting] as ip_template on ms.MerchantID = ip_template.MerchantID and ip_template.CustomizedSettingKey = {ipTemplate}
                                WHERE ms.CustomizedSettingKey = {imChanne} AND ms.SettingValue = @imChannelMappingID
                            )";

                sql += $" AND ( {extraSQL} ) ";
            }

            if (filter.IPAddressKeyword.IsNotEmptyTextNotNull())
            {
                filter.IPAddressKeyword = filter.IPAddressKeyword.Trim();
                sql += $" AND ( {GenerateLikeQuery("IPAddressList", "@IPAddressKeyword")} ) ";
            }


            if (filter.FilterMerchantIDsByMultipleHierarchyMerchant.IsNotEmptyNull())
            {
                var settingKey = MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting.ToInteger();
                sql += @$" AND Id in 
                    (   
                        SELECT cast(SettingValue as int) 
                        FROM [dbo].[MerchantCustomizedSetting] WITH (NOLOCK)
                        WHERE CustomizedSettingKey = {settingKey} {AppHelper.GenerateMultipleHierarchyMerchantFilterWhereSQL()} 
                    ) ";
            }


            if (filter.AssignMerchantIDs.IsNotEmptyNull())
            {
                var settingKey = MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting.ToInteger();
                sql += @$" AND Id in 
                    (   
                        SELECT cast(SettingValue as int) 
                        FROM [dbo].[MerchantCustomizedSetting] WITH (NOLOCK)
                        WHERE CustomizedSettingKey = {settingKey} AND MerchantId in @AssignMerchantIDs
                    ) ";
            }

            if (filter.IsAssignMerchantOnly)
            {
                var settingKey = MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting.ToInteger();
                sql += @$" AND Id in 
                    (   
                        SELECT cast(SettingValue as int) 
                        FROM [dbo].[MerchantCustomizedSetting] WITH (NOLOCK)
                        WHERE CustomizedSettingKey = {settingKey} 
                    ) ";
            }

            if (filter.HierarchyOfTargerMerchant.HasValue)
            {
                var settingKey = MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting.ToInteger();
                string hierarchySQL = AppHelper.GenerateMerchantHierarchyWhereSQL(filter.HierarchyOfTargerMerchant.Value);
                sql += @$" AND Id in 
                    (   
                        SELECT cast(SettingValue as int) 
                        FROM [dbo].[MerchantCustomizedSetting] WITH (NOLOCK)
                        WHERE CustomizedSettingKey = {settingKey} {hierarchySQL}
                    ) ";
            }

            if (filter.ReferIMChannelID.HasValue)
            {
                sql += @$" AND ReferIMChannelID = @ReferIMChannelID ";
            }

            return sql;
        }

    }


}
