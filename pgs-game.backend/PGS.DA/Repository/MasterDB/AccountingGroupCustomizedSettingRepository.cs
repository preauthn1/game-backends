using Dapper;
using DocumentFormat.OpenXml;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Attributes;
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
    public class AccountingGroupCustomizedSettingRepository : FormatIntegerPrimaryKeyRepository<AccountingGroupCustomizedSettingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[AccountingGroupCustomizedSetting]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "AccountingGroupID"; }
        }

        public MerchantAccountingFeeRateModel GetMerchantAllGameVendorRate(int merchantID)
        {
            var resultModel = new MerchantAccountingFeeRateModel
            {
                AccountingGroupID = 0,
                GameVendorFeeRate = new Dictionary<int, decimal>(),
            };

            string sql_1 = $@"SELECT TOP 1 * FROM [dbo].[MerchantHierarchy] WITH (NOLOCK)
                              WHERE MerchantId = {merchantID}";

            List<int> referMerchantIds = new List<int>
            {
                merchantID
            };

            var queryHierarchy = QueryByDapper<MerchantHierarchyEntity>(sql_1 , null).FirstOrDefault();

            if(queryHierarchy != null)
            {
                referMerchantIds = AppHelper.GetHierarchyMerchantIDs(queryHierarchy.Hierarchy);
            }

            string sql_2 = @"SELECT TOP 1 SettingValue FROM[dbo].[MerchantCustomizedSetting] WITH(NOLOCK)
                             WHERE MerchantId in @referMerchantIds AND CustomizedSettingKey = 8851 AND LEN(SettingValue) >= 3";

            var param = new
            {
                referMerchantIds = referMerchantIds
            };

            var queryAccountingGroup = QueryByDapper<MerchantCustomizedSettingEntity>(sql_2, param).FirstOrDefault();

            if (queryAccountingGroup == null)
                return null;

            var parseValue = new SystemConfigSettingParseModel(0, queryAccountingGroup.SettingValue);

            int accountGroupID = parseValue.SettingValueOfInt;

            List<AccountingCustomizedSettingTypeEnum> settingKeys = ExtensionOfEnum.GetEnumAllValue<AccountingCustomizedSettingTypeEnum>()
                .Where(x =>
                {
                    var attr = x.GetAttributeFromEnum<DataCategoryAttribute>();

                    if (attr == null)
                        return false;

                    return attr.Value == DataCategoryEnum.MerchantFeeRate;
                })
                .ToList();

            resultModel.AccountingGroupID = accountGroupID;

            var querySettings = GetByFilter(new QueryAccountingGroupCustomizedSettingRequestViewModel
            {
                AccountingGroupIDs = accountGroupID.ToListCollection(),
                SettingKeys = settingKeys.Select(x => x.ToInteger()).ToList(),
            });

            foreach (var settingKey in settingKeys)
            {
                int settingKeyInt = (int)settingKey;
                var matchSetting = querySettings.FirstOrDefault(z => z.SettingKey == settingKeyInt);

                int gameVendorID = settingKeyInt % 10000000;

                if (matchSetting != null)
                {
                    resultModel.GameVendorFeeRate.Add(gameVendorID, decimal.Parse(matchSetting.SettingValue));
                }
                else
                {
                    var defaultValue = settingKey.GetAttributeFromEnum<SettingDefaultValue>();
                    string defaultValueText = (defaultValue.Value ?? "0");
                    resultModel.GameVendorFeeRate.Add(gameVendorID, decimal.Parse(defaultValueText));
                }
            }

            return resultModel;
        }


        public SystemConfigSettingParseModel GetParseModelByEnum(int accountingGroupID, AccountingCustomizedSettingTypeEnum settingKey)
        {
            var query = GetByFilter(new QueryAccountingGroupCustomizedSettingRequestViewModel
            {
                SettingKeys = settingKey.ToInteger().ToListCollection(),
                AccountingGroupIDs = accountingGroupID.ToListCollection()
            });

            var match = query.FirstOrDefault();

            if (match == null)
            {
                return new SystemConfigSettingParseModel
                {
                    SettingKey = settingKey.ToInteger(),
                    SettingValue = settingKey.GetAttributeFromEnum<SettingDefaultValue>()?.Value
                };
            }

            return new SystemConfigSettingParseModel
            {
                SettingKey = match.SettingKey,
                SettingValue = match.SettingValue
            };
        }


        public List<AccountingGroupCustomizedSettingEntity> GetByFilter(QueryAccountingGroupCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<AccountingGroupCustomizedSettingEntity>(sql, filter);
            return query;
        }


        public PagedList<AccountingGroupCustomizedSettingEntity> GetPagingByFilter(QueryAccountingGroupCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<AccountingGroupCustomizedSettingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryAccountingGroupCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.AccountingGroupIDs.IsNotEmptyNull())
                sql += " AND AccountingGroupID in @AccountingGroupIDs ";

            if (filter.SettingKeys.IsNotEmptyNull())
                sql += " AND SettingKey in @SettingKeys ";

            if (filter.SettingValueMatch.IsNotEmptyTextNotNull())
                sql += " AND SettingValue = @SettingValueMatch ";

            return sql;
        }

    }


}
