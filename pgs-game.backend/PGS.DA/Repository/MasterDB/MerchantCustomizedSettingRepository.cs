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
using target.Utility.Helper;
using target.Utility;

namespace target.DA.Repository
{

    public class MerchantCustomizedSettingRepository : FormatMultiplePrimaryKeyRepository<MerchantCustomizedSettingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MerchantCustomizedSetting]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "MerchantID"; }
        }


        public void CreateOrUpdate(int merchantID , int settingKey , string settingValue , bool isTriggerRemoveCache = true)
        {
            var queryExistSetting = GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
                CustomizedSettingKeyIDs = settingKey.ToListCollection()

            }).FirstOrDefault();

            if(queryExistSetting == null)
            {
                Create(new MerchantCustomizedSettingEntity
                {
                    MerchantID = merchantID,
                    SettingValue = settingValue,
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    CustomizedSettingKey = settingKey,
                });
            }
            else
            {
                queryExistSetting.SettingValue = settingValue;
                Update(queryExistSetting);
            }

            if(isTriggerRemoveCache)
            {
                CacheRemoveHelper.RemoveCache_WhenUpdateMerchantSetting(merchantID, settingKey);
                CacheRemoveHelper.RemoveCache_WhenUpdateMerchantSetting_WhenAllDone(merchantID);
            }
        }


        public void UpdateMultiple(List<int> merchantIDs , int settingKey, string settingValue)
        {
            if (merchantIDs.IsEmptyOrNull())
                return;

            var splitArray = merchantIDs.SplitListByArraySize(1000);

            foreach(var smallSizeIDs in splitArray)
            {
                List<int> myMerchantIDs = smallSizeIDs;

                string sql = @"UPDATE [dbo].[MerchantCustomizedSetting] SET SettingValue = @settingValue
                           WHERE CustomizedSettingKey = @settingKey AND MerchantID in @MerchantIDs ";

                var param = new
                {
                    settingValue = settingValue,
                    settingKey = settingKey,
                    MerchantIDs = smallSizeIDs
                };

                ExecuteSqlByDapper(sql, param);
            }
        }


        public List<int> GetInternalTestMerchantIDs()
        {
            var filter = new QueryMerchantCustomizedSettingRequestViewModel
            {
                CustomizedSettingKeyIDs = MerchantCustomizedSettingKeyEnum.IsInternalTestMerchant.ToInteger().ToListCollection()
            };

            var query = GetByFilter(filter);

            return query
                .Select(z => z.ReturnInjecteObject<MerchantCustomizedSettingModel>())
                .Where(z => z.SettingValueOfBoolean == true)
                .Select(z=> z.MerchantID)
                .ToList();
        }

        public bool IsYesValue(int merchantID, MerchantCustomizedSettingKeyEnum key)
        {
            var valueType = key.GetAttributeFromEnum<ReferType>();

            if(valueType == null || valueType.Type != typeof(NormalYesOrNoEnum))
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny);
            }

            var settingValue = GetMerchantSettingByCache(merchantID, key);
            return settingValue.SettingValueOfInt == NormalYesOrNoEnum.Yes.ToInteger();
        }


        public void MultipleDelete(List<int> merchantIDs, MerchantCustomizedSettingKeyEnum key)
        {
            var split2000 = merchantIDs.SplitListByArraySize(2000);

            foreach(var myMerchantIDs in split2000)
            {
                if (myMerchantIDs.IsEmptyOrNull())
                    continue;

                string sql = @"DELETE FROM [dbo].[MerchantCustomizedSetting]
                           WHERE MerchantID in @MerchantIDs AND [CustomizedSettingKey] = @SettingKey";

                var param = new
                {
                    MerchantIDs = myMerchantIDs,
                    SettingKey = key
                };

                ExecuteSqlByDapper(sql, param);
            }
        }


        public List<MerchantCustomizedSettingModel> GetMultipleMerchantSettings(List<int> merchantIDs, MerchantCustomizedSettingKeyEnum key)
        {
            if (merchantIDs.IsEmptyOrNull())
                return new List<MerchantCustomizedSettingModel>();

            var filter = new QueryMerchantCustomizedSettingRequestViewModel
            {
                MerchantIDs = merchantIDs,
                CustomizedSettingKeyIDs = key.ToInteger().ToListCollection()
            };

            var query = GetByFilter(filter);
            return query.Select(x=> x.ReturnInjecteObject<MerchantCustomizedSettingModel>()).ToList();
        }

        public bool IsInternalTestMerchant(int merchantID)
        {
            var query = GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.IsInternalTestMerchant);

            if (query == null)
                return false;

            return query.SettingValueOfBoolean;
        }

        public decimal GetMerchantGamePointConvertRateWihtoutCache(int merchantID, MerchantCustomizedSettingKeyEnum key)
        {
            var querySetting = GetMerchantSettingKeyWithoutCache(merchantID, key);

            if (querySetting == null || string.IsNullOrWhiteSpace(querySetting.SettingValue))
                return 1;

            return querySetting.SettingValueOfDecimal;
        }


        public decimal GetMerchantGamePointConvertRateByCache(int merchantID, MerchantCustomizedSettingKeyEnum key)
        {
            var querySetting = GetMerchantSettingByCache(merchantID , key);

            if (querySetting == null || string.IsNullOrWhiteSpace(querySetting.SettingValue))
                return 1;

            return querySetting.SettingValueOfDecimal;
        }


        /// <summary>
        /// 若查無資料 會返回預設empty model
        /// </summary>
        public List<MerchantCustomizedSettingModel> GetMultipleMerchantSettingByCache(int merchantID, List<MerchantCustomizedSettingKeyEnum> keys)
        {
            if (merchantID == 0 && SettingHelper.IsMSTest)
            {
                //ps. 商戶ID 不用檢查負號 因為有測試案例用到
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("MerchantID: 0"));
            }

            string cacheKey = CacheKey.DbEntity_ParseModel_Multiple_MerchantCustomizedSetting(merchantID, keys.Select(x=> (int)x).ToList());

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                List<MerchantCustomizedSettingModel> listOfResult = new List<MerchantCustomizedSettingModel>();

                foreach (var settingKey in keys)
                {
                    var setting = GetMerchantSettingByCache(merchantID, settingKey);
                    listOfResult.Add(setting);
                }

                return listOfResult;

            }, expiredMinute: 30);

            queryCache = queryCache ?? new List<MerchantCustomizedSettingModel>();

            //還是再防呆下好了 (如果cache沒清成功 似乎還是會有null的可能
            foreach (var key in keys)
            {
                var isSettingExist = queryCache.Any(x => x.CustomizedSettingKey == (int)key);

                if(!isSettingExist)
                {
                    queryCache.Add(InstanceDefaultSettingModel(merchantID, key));
                }
            }

            return queryCache;
        }


        /// <summary>
        /// 若撈到DB無資料的配置會返回 預設值Model (不返回NULL)
        /// 請勿改掉此邏輯
        /// </summary>
        public MerchantCustomizedSettingModel GetMerchantSettingByCache(int merchantID, MerchantCustomizedSettingKeyEnum key)
        {
            if(merchantID == 0 && SettingHelper.IsMSTest)
            {
                //ps. 商戶ID 不用檢查負號 因為有測試案例用到
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("MerchantID: 0"));
            }

            string cacheKey = CacheKey.DbEntity_ParseModel_MerchantCustomizedSetting(merchantID , key.ToInteger());

            var queryCachae = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var querySingle = GetMerchantSettingKeyWithoutCache(merchantID , key);

                //如果沒配置 就塞預設值 (回null物件 code寫起來比較醜、很麻煩)
                if(querySingle == null)
                {
                    return InstanceDefaultSettingModel(merchantID, key);
                }

                return querySingle;

            }, expiredMinute: 15);


            //致命的防呆 (如果不小心update錯DB?)
            if(key == MerchantCustomizedSettingKeyEnum.GameVendor_ConvertRate_Transfer)
            {
                if(queryCachae.SettingValueOfDecimal == 0)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel
                    {
                        ColumnName = "GameVendor_ConvertRate_Transfer is 0",
                        IsAlwaysNotify = true
                    });
                }
            }

            return queryCachae;
        }


        private MerchantCustomizedSettingModel InstanceDefaultSettingModel(int merchantID , MerchantCustomizedSettingKeyEnum settingKey)
        {
            var defaultValue = settingKey.GetAttributeFromEnum<SettingDefaultValue>();

            return new MerchantCustomizedSettingModel
            {
                SettingValue = defaultValue?.Value,
                MerchantID = merchantID,
                CustomizedSettingKey = settingKey.ToInteger()
            };
        }


        public MerchantCustomizedSettingModel GetMerchantSettingKeyWithoutCache(int merchantID , MerchantCustomizedSettingKeyEnum key , bool isReturnNull = true)
        {
            var filter = new QueryMerchantCustomizedSettingRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
                CustomizedSettingKeyIDs = key.ToInteger().ToListCollection()
            };

            var query = GetByFilter(filter).FirstOrDefault();

            if (query == null)
            {
                if(isReturnNull)
                {
                    return null;
                }
                else
                {
                    return InstanceDefaultSettingModel(merchantID ,  key);
                }
            }

            return query.ReturnInjecteObject<MerchantCustomizedSettingModel>();
        }


        public List<MerchantCustomizedSettingModel> GetMerchantMultipleSettings(int merchantID, List<MerchantCustomizedSettingKeyEnum> keys)
        {
            var filter = new QueryMerchantCustomizedSettingRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
                CustomizedSettingKeyIDs = keys.Select(x=> x.ToInteger()).ToList()
            };

            var query = GetByFilter(filter);

            if (query.IsEmptyOrNull())
                return new List<MerchantCustomizedSettingModel>();

            return query.Select(x => x.ReturnInjecteObject<MerchantCustomizedSettingModel>()).ToList(); ;
        }


        public int GetCountByFilter(QueryMerchantCustomizedSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter, " count(*) ");
            var query = QueryByDapper<int>(sql, filter);
            return query.FirstOrDefault();
        }

        public MerchantCustomizedSettingEntity GetByUniqueKey(int merchantID , MerchantCustomizedSettingKeyEnum settingKey)
        {
            var query = GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                CustomizedSettingKeyIDs = settingKey.ToInteger().ToListCollection(),
                MerchantIDs = merchantID.ToListCollection()
            });

            return query.FirstOrDefault();
        }


        public List<MerchantCustomizedSettingEntity> GetBriefByFilter(QueryMerchantCustomizedSettingRequestViewModel filter)
        {
            string selectColumns = "  MerchantID , CustomizedSettingKey , SettingValue ";
            return GetByFilter(filter, selectColumns);
        }


        public List<MerchantCustomizedSettingEntity> GetByFilter(QueryMerchantCustomizedSettingRequestViewModel filter , string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter , selectColumns);
           
            var query = QueryByDapper<MerchantCustomizedSettingEntity>(sql, filter);

            if(filter.FilterMerchantIDsByCSharp.IsNotEmptyNull())
            {
                query = query
                    .Where(x => filter.FilterMerchantIDsByCSharp.Contains(x.MerchantID))
                    .ToList();
            }

            return query;
        }


        public PagedList<MerchantCustomizedSettingEntity> GetPagingByFilter(QueryMerchantCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter , selectColumns);
            var query = GetPagingEntitiesBySQLScript<MerchantCustomizedSettingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMerchantCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.MerchantIDs.IsNotEmptyNull())
            {
                if(filter.MerchantIDs.Count() >= 2000)
                {
                    filter.FilterMerchantIDsByCSharp = filter.MerchantIDs;
                    filter.MerchantIDs = new List<int>();
                }
                else
                {
                    sql += " AND MerchantID in @MerchantIDs ";
                }
            }

            if (filter.MatchMerchantByLogicType.HasValue)
            {
                var type = MerchantCustomizedSettingKeyEnum.GameSetting_SingleWalletApiLogicMode.ToInteger();
                sql += $@" AND MerchantID in 
                ( 
                    SELECT MerchantID from [dbo].[MerchantCustomizedSetting] WITH (NOLOCK)
                    WHERE CustomizedSettingKey = {type} AND SettingValue = '{filter.MatchMerchantByLogicType}'
                )";
            }               

            if (filter.CustomizedSettingKeyIDs.IsNotEmptyNull())
                sql += " AND CustomizedSettingKey in @CustomizedSettingKeyIDs ";

            if (filter.SettingValueMatch.IsNotEmptyTextNotNull())
                sql += " AND SettingValue = @SettingValueMatch ";

            if (filter.SettingValueLikeQuery.IsNotEmptyTextNotNull())
                sql += $" AND ( {GenerateLikeQuery("SettingValue", "@SettingValueLikeQuery")} ) ";

            if (filter.SettingValueNotEqual.IsNotEmptyTextNotNull())
                sql += " AND SettingValue != @SettingValueNotEqual ";

            if (filter.MultipleSettingValueMatch.IsNotEmptyNull())
                sql += " AND (SettingValue in @MultipleSettingValueMatch) ";

            if (filter.IsNotDeleteMerchant)
                sql += " AND MerchantID not in  ( SELECT MerchantID FROM [dbo].[Merchant] WITH (NOLOCK) WHERE Status = 999 )";

            if(filter.IsSettingValueEmptyNullMatch.HasValue)
            {
                if(filter.IsSettingValueEmptyNullMatch == true)
                {
                    sql += " AND (SettingValue is NULL OR SettingValue = '') ";
                }
                else
                {
                    sql += " AND (SettingValue is not NULL AND SettingValue != '') ";
                }
            }

            return sql;
        }

    }


}
