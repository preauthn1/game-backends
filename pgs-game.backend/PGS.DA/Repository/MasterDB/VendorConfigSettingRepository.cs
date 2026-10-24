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
using target.Models.Models;
using target.Models.Enums;
using target.Models.Attributes;
using target.Utility.Helper;

namespace target.DA.Repository
{

    public class VendorConfigSettingRepository : FormatIntegerPrimaryKeyRepository<VendorConfigSettingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[VendorConfigSetting]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        /// <summary>
        /// 取得所有AGG 正式環境的 IntergrateID (e.g. slot88-idr)
        /// </summary>
        public List<VendorConfigSettingBriefModel> GetAllProductionAggIntergratorIDs(GameVendorTypeEnum gameVendorType = GameVendorTypeEnum.AGG)
        {
            int targetSettingKey = VendorConfigSettingEnum.MainAssignVendorID.ToInteger();
            int gameVendor = gameVendorType.ToInteger();

            string sql = @" SELECT s.VendorConfigID , s.SettingKey ,  s.Value from [ThirdParty].[VendorConfigSetting] as s
                            join [ThirdParty].[VendorConfig] as v on s.VendorConfigID = v.VendorConfigID
                            WHERE SettingKey = @targetSettingKey AND v.VendorID = @gameVendor AND IsTestEnvironment = 0";

            var para = new
            {
                targetSettingKey = targetSettingKey,
                gameVendor = gameVendor,
            };

            var query = QueryByDapper<VendorConfigSettingBriefModel>(sql, para);
            return query;
        }

        public VendorConfigSettingBriefModel GetByVendorMainAssignVendorIDSettingValue(GameVendorTypeEnum gameVendorType, string settingValue)
        {
            int targetSettingKey = VendorConfigSettingEnum.MainAssignVendorID.ToInteger();
            int gameVendor = gameVendorType.ToInteger();

            string sql = @" SELECT s.VendorConfigID , s.SettingKey ,  s.Value from [ThirdParty].[VendorConfigSetting] as s
                            join [ThirdParty].[VendorConfig] as v on s.VendorConfigID = v.VendorConfigID
                            WHERE SettingKey = @targetSettingKey AND v.VendorID = @gameVendor AND IsTestEnvironment = 0 AND Value = @settingValue";

            var para = new
            {
                targetSettingKey = targetSettingKey,
                gameVendor = gameVendor,
                settingValue = settingValue,
            };

            var query = QueryByDapper<VendorConfigSettingBriefModel>(sql, para);
            return query.FirstOrDefault();
        }


        /// <summary>
        /// 取得指定的 merchantID + vendorID的 所有設置
        /// </summary>
        public SystemConfigSettingParseModel GetParseModelByUniqueKey(int vendorConfigID, VendorConfigSettingEnum setting)
        {
            int settingKey = setting.ToInteger();

            var querySetting = GetByFilter(new QueryVendorConfigSettingRequestViewModel
                {
                    SettingKeys = settingKey.ToListCollection(),
                    VendorConfigIDs = vendorConfigID.ToListCollection(),
                })
                .FirstOrDefault();

            if(querySetting == null)
            {
                string defaultValue = setting.GetAttributeFromEnum<SettingDefaultValue>()?.Value;
                return new SystemConfigSettingParseModel(settingKey, defaultValue);
            }

            return new SystemConfigSettingParseModel(settingKey, querySetting?.Value);
        }


        /// <summary>
        /// 取得指定的 merchantID + vendorID的 所有設置
        /// </summary>
        public List<SystemConfigSettingParseModel> GetTargetMerchantVendorSettings(int merchantID, GameVendorTypeEnum vendorID)
        {
            string sql = @" SELECT mapping.VendorConfigID as OwnerID , [SettingKey] ,[Value] as SettingValue
                            FROM [ThirdParty].[VendorConfigMerchantMapping] as mapping
                            INNER JOIN [ThirdParty].[VendorConfigSetting] as setting on mapping.VendorConfigID = setting.VendorConfigID
                            WHERE MerchantID = @merchantID AND [VendorID] = @vendorID";

            var para = new { merchantID = merchantID, vendorID = vendorID.ToInteger() };
            var query = QueryByDapper<SystemConfigSettingParseModel>(sql, para);
            return query;
        }


        public List<VendorConfigGameVendorSettingModel> GetAllGameVendorSettingByMerchat(int merchantID , VendorConfigSettingEnum settingKey)
        {
            string sql = @" SELECT s.VendorConfigID , s.SettingKey , s.Value as SettingValue , v.VendorID 
                            FROM [ThirdParty].[VendorConfigSetting] as s 
                            LEFT JOIN [ThirdParty].[VendorConfig] as v on s.VendorConfigID = v.VendorConfigID
                            WHERE s.SettingKey = @settingKey AND s.VendorConfigID in
                            (
	                            SELECT VendorConfigID FROM [ThirdParty].[VendorConfigMerchantMapping] as v
	                            WHERE MerchantId = @merchantID
                            )";

            var param = new
            {
                merchantID = merchantID,
                settingKey = settingKey.ToInteger(),
            };

            var query = QueryByDapper<VendorConfigGameVendorSettingModel>(sql , param);
            return query.ToList();
        }

        public VendorConfigSettingEntity GetAssignVendorIdByVendorConfigID(int vendorConfigID)
        {
            var query = GetByFilter(new QueryVendorConfigSettingRequestViewModel
            {
                VendorConfigIDs = vendorConfigID.ToListCollection(),
                SettingKeys = VendorConfigSettingEnum.MainAssignVendorID.ToInteger().ToListCollection()
            });

            return query.FirstOrDefault();
        }

        public VendorConfigSettingEntity GetSpecificVendorConfigSetting(int vendorConfigID, VendorConfigSettingEnum settingKey)
        {
            var query = GetByFilter(new QueryVendorConfigSettingRequestViewModel
            {
                VendorConfigIDs = vendorConfigID.ToListCollection(),
                SettingKeys = settingKey.ToInteger().ToListCollection()
            });

            return query.FirstOrDefault();
        }

        /// <summary>
        /// 如果不存在 就塞setting
        /// </summary>
        public void CreateOrUpdate(VendorConfigSettingEntity model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.SettingKey).Rule_NotZeroNotMinus();

            var queryExistData = GetByFilter(new QueryVendorConfigSettingRequestViewModel
                {
                    VendorConfigIDs = model.VendorConfigID.ToListCollection(),
                    SettingKeys = model.SettingKey.ToListCollection()
                })
                .FirstOrDefault();

            if (queryExistData == null)
            {
                Create(model);
            }
            else
            {
                queryExistData.Value = model.Value;
                queryExistData.UpdateTime = DateTimeHelper.GetCurrentTime();

                Update(queryExistData);
            }
        }


        public List<VendorConfigSettingEntity> GetByFilter(QueryVendorConfigSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<VendorConfigSettingEntity>(sql, filter);
            return query;
        }


        public PagedList<VendorConfigSettingEntity> GetPagingByFilter(QueryVendorConfigSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<VendorConfigSettingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryVendorConfigSettingRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.VendorConfigIDs.IsNotEmptyNull())
                sql += " AND VendorConfigID in @VendorConfigIDS ";

            if (filter.SettingKeys.IsNotEmptyNull())
                sql += " AND SettingKey in @SettingKeys ";

            if (filter.MatchSettingValues.IsNotEmptyNull())
                sql += " AND Value in @MatchSettingValues ";

            if (filter.MatchAssignVendorID.IsNotEmptyTextNotNull())
            {
                int key = VendorConfigSettingEnum.MainAssignVendorID.ToInteger();
                sql += $" AND VendorConfigID in ( SELECT  VendorConfigID FROM  {TableName} WITH (NOLOCK) WHERE SettingKey = {key} AND Value = @MatchAssignVendorID ) ";
            }

            if (filter.MatchOtherSettingValue3.IsNotEmptyTextNotNull())
            {
                int key = VendorConfigSettingEnum.OtherKeySettingValue3.ToInteger();
                sql += $" AND VendorConfigID in ( SELECT  VendorConfigID FROM  {TableName} WITH (NOLOCK) WHERE SettingKey = {key} AND Value = @MatchOtherSettingValue3 ) ";
            }

            if (filter.AssignMerchantID.HasValue)
            {
                sql += $" AND VendorConfigID in ( SELECT VendorConfigID FROM [PGS_Master].[ThirdParty].[VendorConfigMerchantMapping] WITH (NOLOCK) WHERE MerchantID = @AssignMerchantID ) ";
            }

            if (filter.GameVendorID.HasValue)
            {
                sql += $" AND VendorConfigID in ( SELECT VendorConfigID FROM [ThirdParty].[VendorConfig] WITH (NOLOCK) WHERE [VendorID] = @GameVendorID ) ";
            }

            return sql;
        }

    }


}
