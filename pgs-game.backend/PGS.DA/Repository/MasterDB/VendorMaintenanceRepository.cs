using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.DA.Repository
{
    public class VendorMaintenanceRepository : FormatIntegerPrimaryKeyRepository<VendorMaintenanceEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[VendorMaintenance]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        /// <summary>
        /// 檢查玩家可以略過維護的兩種條件之一 (符合其一即可)
        /// case 1. 被標記特定Tag
        /// case 2. 商戶進階設置有配置
        /// </summary>
        public bool IsPlayerCanSkipMaintainByTagOrSetting(int playerID)
        {
            bool isSkipMaintainTag = IsPlayerHasSkipMaintainExistTagByCache(playerID);

            if (isSkipMaintainTag)
                return true;

            int settingKey = MerchantCustomizedSettingKeyEnum.IsInternalTestMerchant.ToInteger();

            string sql = @"SELECT count(1) FROM [dbo].[MerchantCustomizedSetting] WITH (NOLOCK)
                           WHERE CustomizedSettingKey = @SettingKey AND SettingValue = 1 AND MerchantID in
                            (
                                SELECT MerchantID 
                                FROM [dbo].[Player] WITH(NOLOCK)
                                WHERE PlayerID = @PlayerID
                            )";

            var param = new
            {
                SettingKey = settingKey,
                PlayerID = playerID
            };

            var queryCount = QueryByDapper<int>(sql, param).FirstOrDefault();

            if (queryCount >= 1)
                return true;

            return false;
        }



        public void CheckVendorMaintenanceAndSkipTagThenThrowException(int vendorID, int playerID)
        {
            vendorID = AppHelper.ConvertSlot88ToPgsVendorID(vendorID);

            var queryMaintainByCache = GetVendorMaintenanceByCache(vendorID);

            if (queryMaintainByCache == null)            
                return;            

            ApiErrorCode targetErrorCode = ApiErrorCode.GameUnderMaintain;

            bool isPlayerCanSkipMaintain = IsPlayerHasSkipMaintainExistTagByCache(playerID);

            if (isPlayerCanSkipMaintain)
                return;

            //有兩種設法 (排除商戶 或 僅限特定商戶)
            if (queryMaintainByCache.IncludeMerchantTemplateID.NotNullNotZeroNotMinus())
            {
                if (IsMaintainEffectOnPlayerByIncludeMerchantSetting(queryMaintainByCache.IncludeMerchantTemplateID, playerID))
                {
                    throw new TerproException(targetErrorCode);
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (IsPlayerCanSkipMaintainByExcludeMerchantSetting(queryMaintainByCache.ExcludeMerchantTemplateID, playerID))
                {
                    return;
                }
                else
                {
                    throw new TerproException(targetErrorCode);
                }
            }
        }


        /// <summary>
        /// 若有設置維護可以排除特定商戶、透過playerID來檢查是否可以忽略維護
        /// </summary>
        public bool IsPlayerCanSkipMaintainByExcludeMerchantSetting(int? templateID, int playerID)
        {
            if (!templateID.HasValue)
                return false;

            int type = MerchantSelectMappingItemTypeEnum.IncludeMerchantID.ToInteger();
            string sql = $@"SELECT MerchantID from [dbo].[MerchantSelectMappingItem] with (NOLOCK)
                            WHERE TemplateID = @TemplateID  AND Type = @Type AND MerchantID in
                            (
                                SELECT MerchantID from[dbo].[Player] with(NOLOCK)
                                WHERE PlayerID = @PlayerID
                            )";

            var param = new
            {
                Type = type,
                PlayerID = playerID,
                TemplateID = templateID
            };

            var queryCanSkipMerchants = QueryByDapper<int>(sql, param);
            return queryCanSkipMerchants.IsNotEmptyNull();
        }

        /// <summary>
        /// 若有設置維護 僅限特定商戶(include) 透過playerID來檢查是否可以忽略維護
        /// </summary>
        public bool IsMaintainEffectOnPlayerByIncludeMerchantSetting(int? includeTemplateID, int playerID)
        {
            if (!includeTemplateID.HasValue)
                return false;

            int type = MerchantSelectMappingItemTypeEnum.IncludeMerchantID.ToInteger();
            string sql = $@"SELECT MerchantID from [dbo].[MerchantSelectMappingItem] with (NOLOCK)
                            WHERE TemplateID = @TemplateID  AND Type = @Type AND MerchantID in
                            (
                                SELECT MerchantID from[dbo].[Player] with(NOLOCK)
                                WHERE PlayerID = @PlayerID
                            )";

            var param = new
            {
                Type = type,
                PlayerID = playerID,
                TemplateID = includeTemplateID
            };

            var queryCanSkipMerchants = QueryByDapper<int>(sql, param);
            return queryCanSkipMerchants.IsNotEmptyNull();
        }


        private bool IsPlayerHasSkipMaintainExistTagByCache(int playerID)
        {
            return IsPlayerTagExist(playerID, SystemTagEnum.Player_AllowSkipMaintain);
        }

        /// <summary>
        /// 建議呼叫 CheckVendorMaintenanceAndThrowException 而不要直接呼叫這個
        /// </summary>
        private bool IsPlayerTagExist(int playerID, SystemTagEnum tag)
        {
            int tagID = tag.ToInteger();
            string cacheKey = CacheKey.FlagExistValue_PlayerTag(playerID, tagID);

            var queryCache = CacheHelper.GetCacheOrQuery<string>(cacheKey, () =>
            {
                string sql = @"SELECT * FROM [dbo].[PlayerTagMapping] 
                               WHERE playerID = @playerID AND tagID = @tagID ";

                var query = QueryByDapper<PlayerTagMappingEntity>(sql, new { playerID, tagID });
                return query.IsNotEmptyNull() ? SystemConstant.FlagExist_Yes : SystemConstant.FlagExist_No;

            }, 15);

            return queryCache == SystemConstant.FlagExist_Yes;
        }


        public VendorMaintenanceEntity GetVendorMaintenanceByCache(int vendorID)
        {
            string cacheKey = CacheKey.DbEntity_VendorMaintain_VendorID(vendorID);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var query = GetVendorMaintenance_OnProgressingOrReadyToStart(vendorID);

                //因為null不會存cache。因此返回一個預設空物件
                if (query.IsEmptyOrNull())
                {
                    return new List<VendorMaintenanceEntity>();
                }

                return query;

            }, 15);

            if (queryCache.IsEmptyOrNull())
                return null;

            var now = DateTimeHelper.GetCurrentTime();

            //cache可能是15分鐘前。而實際設置的EndTime已經結束了 因此要在外層再檢查一次
            var matchFirst = queryCache
                .Where(x => now >= x.StartTime && now < x.EndTime)
                .FirstOrDefault();

            return matchFirst;
        }

        public List<VendorMaintenanceEntity> GetVendorMaintenance_OnProgressingOrReadyToStart(int vendorID)
        {
            string selectColumns = " Id , VendorID , Status , StartTime , EndTime , ExcludeMerchantTemplateID , IncludeMerchantTemplateID ";

            var query = GetByFilter(new QueryVendorMaintenanceRequestViewModel
            {
                VendorIDs = vendorID.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger(),
                IsOnProgressingOrReadyToStart = true,  //包含即將開始的 呼叫端要再自己額外做判斷
            }, selectColumns);

            return query;
        }

     
        public List<VendorMaintenanceEntity> GetByFilter(QueryVendorMaintenanceRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<VendorMaintenanceEntity>(sql, filter);
            return query;

        }

        public PagedList<VendorMaintenanceEntity> GetPagingByFilter(QueryVendorMaintenanceRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);

            var query = GetPagingEntitiesBySQLScript<VendorMaintenanceEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryVendorMaintenanceRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK) 
                            WHERE 1=1 ";

            if (filter.VendorIDs.IsNotEmptyNull())
                sql += " AND VendorID in @VendorIDs ";

            if (!filter.IsIncludeDelete)
                sql += $" AND Status != {NormalActiveStatus.PermanentDelete.ToInteger()} ";

            if (filter.IsOnProgressing)
                sql += $" AND (GETUTCDATE() >= StartTime AND EndTime > GETUTCDATE()) ";

            if (filter.IsOnProgressingOrReadyToStart)
            {
                sql += @$" AND 
                        ( 
                            (GETUTCDATE() >= StartTime AND EndTime > GETUTCDATE()) 
                                OR
                            (DATEADD(hour , 1 , GETUTCDATE()) >= StartTime AND EndTime > GETUTCDATE())
                        )";

            }

            if (filter.Status.HasValue)
                sql += $" AND Status = @Status ";

            if (filter.EndTimeBegin.HasValue)
                sql += $" AND EndTime >= @EndTimeBegin ";

            if (filter.EndTimeEnd.HasValue)
                sql += $" AND EndTime < @EndTimeEnd ";

            return sql;
        }

    }


}
