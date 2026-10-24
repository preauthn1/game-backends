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
using target.Models.Enums;
using target.Models.Models;
using Lck.Utility.Extensions.Helper;
using target.Models.Attributes;
using target.Utility;

namespace target.DA.Repository
{

    public class SystemConfigSettingRepository : FormatIntegerPrimaryKeyRepository<SystemConfigSettingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[SystemConfigSetting]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public bool IsSystemMaintain()
        {
            var query = GetParseModelByCache(SystemConfigSettingEnum.SystemMaintain_IsSwtichToMaintain);
            return IsSystemMaintain(query);
        }


        public bool IsSystemMaintain(SystemConfigSettingParseModel query)
        {
            if (query == null)
                return false;

            return query.SettingValueOfInt != (int)SystemMaintainStatusEnum.NormalStatus;
        }


        public void CheckSystemMaintainAndThrowException()
        {
            bool isSystemMaintain = IsSystemMaintain();

            if (isSystemMaintain)
                throw new TerproException(ApiErrorCode.SystemMaintain);

            return;
        }

        /// <summary>
        /// 檢查報表開關是否開啟 (若發生問題、臨時不開放查詢可以先關閉)
        /// </summary>
        public void CheckReportPageEnableOrThrowException()
        {
            var query = GetParseModelByCache(SystemConfigSettingEnum.SystemMaintain_MerchantReportEnableFlag);

            if (query.SettingValueOfInt == NormalActiveStatus.Enable.ToInteger())
                return;

            throw new TerproException(ApiErrorCode.DatabaseBusyAndFeatureUnderMaintain);
        }


        public bool IsAdminSysForbiddenIPs(string myIPAddress)
        {
            if (string.IsNullOrWhiteSpace(myIPAddress))
                return false;

            var forbiddenIPs = GetParseModelByCache(SystemConfigSettingEnum.SystemParameter_FrobiddenLoginIPAddress).SettingValueOfListString;

            if (forbiddenIPs.IsEmptyOrNull())
                return false;

            bool isMatchForbiddenIP = false;

            foreach (string ipKeyword in forbiddenIPs)
            {
                if(ipKeyword.Contains("@"))
                {
                    //有@符號 代表需要完全match
                    string fullMatchIP = ipKeyword.Replace("@", "");

                    if(myIPAddress == fullMatchIP)
                    {
                        isMatchForbiddenIP = true;
                    }
                }
                else if (myIPAddress.Contains(ipKeyword))
                {
                    isMatchForbiddenIP = true;
                }
            }

            return isMatchForbiddenIP;
        }



        /// <summary>
        /// 若配置不存在 會返回 預設值物件
        /// </summary>
        public SystemConfigSettingParseModel GetParseModelByCache(SystemConfigSettingEnum value)
        {
            bool isLocalTestProductionDB = SettingHelper.IsLocalTestProductionDb;

            //本地端測試可能導致全域cache出問題。因此刻意調低cache時數。
            var cacheConfig = new CacheQueryConfig
            {
                ExpiredTime = new TimeSpan(0, 15, 0)
            };

            if(isLocalTestProductionDB)
            {
                cacheConfig.ExpiredTime = new TimeSpan(0, 0, 10);
            }

            string cacheKey2 = CacheKey.DbEntity_ParseModel_SystemConfigSetting(value.ToInteger());

            var cacheValue = CacheHelper.GetCacheOrQuery(cacheKey2, () =>
               {
                   //裡面的邏輯 如果配置不存在 會返回defaultValue 而非null物件
                   var query = GetParseModelByEnum(value);
                   return query;

               }, cacheConfig);

            return cacheValue;
        }


        /// <summary>
        /// 查無資料 會返回 空model
        /// </summary>
        public SystemConfigSettingParseModel GetParseModelByEnum(SystemConfigSettingEnum value)
        {
            var query = GetById(value.ToInteger());

            if (query == null)
            {
                return new SystemConfigSettingParseModel
                {
                    SettingKey = value.ToInteger(),
                    SettingValue = value.GetAttributeFromEnum<SettingDefaultValue>()?.Value
                };
            }

            return new SystemConfigSettingParseModel
            {
                SettingKey = query.ID,
                SettingValue = query.SettingValue
            };
        }


        public SystemConfigSettingEntity GetEntityByEnum(SystemConfigSettingEnum value)
        {
            var query = GetById(value.ToInteger());
            return query;
        }

        
        public List<SystemConfigSettingEntity> GetByFilter(QuerySystemConfigSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<SystemConfigSettingEntity>(sql, filter);
            return query;
        }


        public PagedList<SystemConfigSettingEntity> GetPagingByFilter(QuerySystemConfigSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<SystemConfigSettingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QuerySystemConfigSettingRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND ID in @IDs ";

            return sql;
        }

    }


}
