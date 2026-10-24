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
    public class EventCustomizedSettingRepository : FormatIntegerPrimaryKeyRepository<EventCustomizedSettingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EventCustomizedSetting]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        /// <summary>
        /// 若查無資料 會返回預設Model
        /// </summary>
        public SystemConfigSettingParseModel GetEventSettingByCache(int eventID, EventCustomizedSettingKeyEnum key , bool isReturnEmptyModel = true)
        {
            string cacheKey = CacheKey.DbEntity_ParseModel_EventCustomizedSetting(eventID, key.ToInteger());

            var queryCachae = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var querySingle = GetByFilter(new QueryEventCustomizedSettingRequestViewModel
                {
                    EventIDs = eventID.ToListCollection(),
                    SettingKeys = key.ToInteger().ToListCollection()
                })
                .FirstOrDefault();

                return querySingle;

            }, expiredMinute: 5);

            if (queryCachae == null)
            {
                if(isReturnEmptyModel)
                {
                    return new SystemConfigSettingParseModel(key.ToInteger(), "");
                }
                else
                {
                    return null;
                }
            }               

            return new SystemConfigSettingParseModel(queryCachae.SettingKey , queryCachae.SettingValue);
        }

        /// <summary>
        /// 如果不存在 就塞setting
        /// </summary>
        public void AddSettingTagIfNotExist(int eventID , EventCustomizedSettingKeyEnum settingKey)
        {
            var query = GetByFilter(new QueryEventCustomizedSettingRequestViewModel
            {
                EventIDs = eventID.ToListCollection(),
                SettingKeys = settingKey.ToInteger().ToListCollection()
            });

            if(query.IsNotEmptyNull())
            {
                return;
            }

            Create(new EventCustomizedSettingEntity
            {
                EventID = eventID,
                SettingKey = settingKey.ToInteger(),
                SettingValue = "",
            });

            return;
        }


        /// <summary>
        /// 如果不存在 就塞setting
        /// </summary>
        public void CreateOrUpdate(EventCustomizedSettingEntity model)
        {
            model.RuleFor(x => x.EventID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.SettingKey).Rule_NotZeroNotMinus();

            var query = GetByFilter(new QueryEventCustomizedSettingRequestViewModel
            {
                EventIDs = model.EventID.ToListCollection(),
                SettingKeys = model.SettingKey.ToListCollection()
            });

            if(query.IsEmptyOrNull())
            {
                Create(model);
            }
            else
            {
                var existData = query.FirstOrDefault();

                existData.SettingValue = model.SettingValue;
                Update(existData);
            }
        }



        public List<EventCustomizedSettingEntity> GetByFilter(QueryEventCustomizedSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<EventCustomizedSettingEntity>(sql, filter);
            return query;
        }


        public PagedList<EventCustomizedSettingEntity> GetPagingByFilter(QueryEventCustomizedSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<EventCustomizedSettingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEventCustomizedSettingRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.EventIDs.IsNotEmptyNull())
                sql += " AND EventID in @EventIDs ";

            if (filter.SettingKeys.IsNotEmptyNull())
                sql += " AND SettingKey in @SettingKeys ";

            return sql;
        }

    }


}
