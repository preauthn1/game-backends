using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models;
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
    public class PlayerTagMappingRepository : FormatMultiplePrimaryKeyRepository<PlayerTagMappingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[PlayerTagMapping]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "PlayerID"; }
        }

        /// <summary>
        /// 玩家重要事件紀錄
        /// </summary>
        public void CreatePlayerBehaviorEvent(int playerID, string memo)
        {
            var insert = new PlayerTagMappingEntity
            {
                TagID = SystemTagEnum.Player_PlayerBehaviorEvent.ToInteger(),
                SettingValue = memo.Truncate(100),
                CreateTime = DateTimeHelper.GetCurrentTime(),
                SubCategory = DateTimeHelper.GetCurrentTimeTimestamp().ToString(),
                CreateBy = "system",
                PlayerID = playerID
            };

            try
            {
                this.Create(insert);
            }
            catch
            {

            }
        }


        public void PlayerRiskScorePlus(int playerID, decimal plusValue = 10.0M)
        {
            var tagID = SystemTagEnum.Player_RiskScore.ToInteger();

            var queryExist = GetPlayerTag(playerID, tagID, null);

            bool isDataExist = false;

            if (queryExist == null)
            {
                queryExist = new PlayerTagMappingEntity
                {
                    PlayerID = playerID,
                    TagID = tagID,
                    SettingValue = "1",
                };
            }
            else
            {
                isDataExist = true;
                queryExist.SettingValue = (decimal.Parse(queryExist.SettingValue) + plusValue).ToString();
            }

            CreateOrUpdate(queryExist, isDataExist);
        }


        public void CreateOrUpdate(PlayerTagMappingEntity model, bool isDataExist = false)
        {
            model.RuleFor(x => x.TagID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();

            if (!isDataExist)
            {
                var queryExist = GetPlayerTag(model.PlayerID, model.TagID, model.SubCategory);

                if (queryExist != null)
                {
                    isDataExist = true;
                }
            }

            if (!isDataExist)
            {
                this.Create(model);
            }
            else
            {
                model.SettingValue = model.SettingValue;
                this.Update(model);
            }
        }

        public override void CreateMany(IEnumerable<PlayerTagMappingEntity> models)
        {
            foreach (var item in models)
            {
                this.SetDefaultValueBeforeInsert(item);
            }

            base.CreateMany(models);
        }

        public void CreatePlayerTag(int playerID, SystemTagEnum tagID, string subCategory = "", string settingValue = null)
        {
            this.Create(new PlayerTagMappingEntity
            {
                PlayerID = playerID,
                TagID = tagID.ToInteger(),
                SubCategory = subCategory,
                SettingValue = settingValue
            });
        }

        public override void Create(PlayerTagMappingEntity model)
        {
            this.SetDefaultValueBeforeInsert(model);
            base.Create(model);
        }

        private void SetDefaultValueBeforeInsert(PlayerTagMappingEntity model)
        {
            if (model.CreateTime == SystemConstant.EmptyTime)
            {
                model.CreateTime = DateTimeHelper.GetCurrentTime();
            }

            if (string.IsNullOrWhiteSpace(model.CreateBy))
            {
                model.CreateBy = "system";
            }

            if (string.IsNullOrWhiteSpace(model.SubCategory))
            {
                model.SubCategory = "";
            }
        }


        public bool IsPlayerTagExistByCache(int playerID, SystemTagEnum tagID, string subCategory = null)
        {
            return IsPlayerTagExistByCache(playerID, (int)tagID, subCategory);
        }


        public bool IsPlayerTagExistByCache(int playerID, int tagID, string subCategory = null)
        {
            string cacheKey = CacheKey.FlagExistValue_PlayerTag(playerID, tagID);

            string queryCache = CacheHelper.GetCacheOrQuery<string>(cacheKey, () =>
               {
                   var query = GetPlayerTag(playerID, tagID);
                   return query != null ? SystemConstant.FlagExist_Yes : SystemConstant.FlagExist_No;

               }, 15);

            return queryCache == SystemConstant.FlagExist_Yes;
        }

        
        public decimal GetPlayerRiskScore(int playerID)
        {
            var queryTag = GetPlayerTag(playerID,  SystemTagEnum.Player_RiskScore);

            if(queryTag == null || queryTag.SettingValue.IsEmptyOrNull())
            {
                return 0;
            }

            return decimal.Parse(queryTag.SettingValue);
        }


        public PlayerTagMappingEntity GetPlayerTag(int playerID, SystemTagEnum tag, string subCategory = null)
        {
            return GetPlayerTag(playerID, tag.ToInteger(), subCategory);
        }


        public PlayerTagMappingEntity GetPlayerTag(int playerID, int tagID, string subCategory = null)
        {
            var query = GetByFilter(new QueryPlayerTagMappingRequestViewModel
            {
                TagIDs = tagID.ToListCollection(),
                PlayerIDs = playerID.ToListCollection(),
                SubCategory = subCategory,
                IsLockQuery = true, //因為是臨時改的。保險起見預設值設為true
            });

            return query.FirstOrDefault();
        }

        public int GetCountByFilter(QueryPlayerTagMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter, " count(*) ");
            var query = QueryByDapper<int>(sql, filter);
            return query.FirstOrDefault();
        }

        public List<PlayerTagMappingEntity> GetByFilter(QueryPlayerTagMappingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<PlayerTagMappingEntity>(sql, filter);
            return query;
        }


        public List<PlayerTagDisplayModel> GetPlayerTagsAndName(int playerID)
        {
            var query = GetByFilter(new QueryPlayerTagMappingRequestViewModel
            {
                PlayerIDs = playerID.ToListCollection()
            });

            return query.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<PlayerTagDisplayModel>();
                    convert.TagName = MultiLanguageTextHelper.GetEnumsCultureText((SystemTagEnum)x.TagID);

                    return convert;
                })
                .ToList();
        }


        public PagedList<PlayerTagMappingEntity> GetPagingByFilter(QueryPlayerTagMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = GetPagingEntitiesBySQLScript<PlayerTagMappingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPlayerTagMappingRequestViewModel filter, string selectColumns = "*")
        {
            string lockQuery = filter.IsLockQuery ? "" : " WITH (NOLOCK) ";
            string sql = $@"SELECT {selectColumns} FROM {TableName} {lockQuery}
                           WHERE 1=1 ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.TagIDs.IsNotEmptyNull())
                sql += " AND TagID in @TagIDs ";

            if (filter.SubCategory.IsNotEmptyTextNotNull())
                sql += " AND SubCategory = @SubCategory ";

            return sql;
        }

    }


}
