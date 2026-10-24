using Dapper;
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
    public class IMChannelCustomizedSettingRepository : FormatMultiplePrimaryKeyRepository<IMChannelCustomizedSettingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[IMChannelCustomizedSetting]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "IMChannelID"; }
        }


        public void CreateOrUpdate(int imChannelID , IMChannelCustomizedSettingKeyEnum settingKey , string settingValue)
        {
            var queryExist = GetByFilter(new QueryIMChannelCustomizedSettingRequestViewModel
            {
                IMChannelIDs = imChannelID.ToListCollection(),
                SettingKeys = settingKey.ToInteger().ToListCollection()
            });

            if(queryExist.IsNotEmptyNull())
            {
                var match = queryExist.FirstOrDefault();
                match.SettingValue = settingValue;

                Update(match);
            }
            else 
            {
                Create(new IMChannelCustomizedSettingEntity
                {
                    SettingKey = settingKey.ToInteger(),
                    SettingValue = settingValue,
                    IMChannelID = imChannelID,
                    UpdateTime = DateTimeHelper.GetCurrentTime()
                });
            }

            return;
        }

        public SystemConfigSettingParseModel GetSingleParseModel(int imChannelID, IMChannelCustomizedSettingKeyEnum settingKey)
        {
            var query = GetByFilter(new QueryIMChannelCustomizedSettingRequestViewModel
                {
                    SettingKeys = settingKey.ToInteger().ToListCollection(),
                    IMChannelIDs = imChannelID.ToListCollection()
                })
                .FirstOrDefault();

            if(query == null)
            {
                var defaultValue = settingKey.GetAttributeFromEnum<SettingDefaultValue>();
                return new SystemConfigSettingParseModel(settingKey.ToInteger(), defaultValue?.Value);
            }

            return new SystemConfigSettingParseModel(settingKey.ToInteger(), query.SettingValue);
        }

        public List<LanguageEnum> GetIMChannelPreferLanguages(string thirdChatID , IMChannelTypeEnum type)
        {
            var query = GetByFilter(new QueryIMChannelCustomizedSettingRequestViewModel
                {
                    SettingKeys = IMChannelCustomizedSettingKeyEnum.PreferLanguages.ToInteger().ToListCollection(),
                    IMChannelType = type.ToInteger(),
                    ThirdPartyChatID = thirdChatID
                })
                .FirstOrDefault();

            if(query == null)
            {
                return new List<LanguageEnum>
                {
                    LanguageEnum.zh_cn,
                    LanguageEnum.en_us,
                };
            }

            var langs = new List<LanguageEnum>();
            var split = query.SettingValue.SeperateToListString();

            foreach(var langText in split)
            {
                langs.Add(AppHelper.ConvertLangToEnum(langText));
            }

            langs = langs.Distinct().ToList();
            return langs;
        }


        public List<IMChannelCustomizedSettingEntity> GetByFilter(QueryIMChannelCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<IMChannelCustomizedSettingEntity>(sql, filter);
            return query;
        }


        public PagedList<IMChannelCustomizedSettingEntity> GetPagingByFilter(QueryIMChannelCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<IMChannelCustomizedSettingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryIMChannelCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.SettingKeys.IsNotEmptyNull())
                sql += " AND SettingKey in @SettingKeys ";

            if (filter.IMChannelIDs.IsNotEmptyNull())
                sql += " AND IMChannelID in @IMChannelIDs ";

            if (filter.SettinValueMatch.IsNotEmptyTextNotNull())
                sql += " AND SettingValue = @SettinValueMatch ";

            if (filter.SettinValueMatchList.IsNotEmptyNull())
                sql += " AND SettingValue in @SettinValueMatchList ";

            if (filter.ThirdPartyChatID.IsNotEmptyTextNotNull())
            {
                sql += @" AND IMChannelID in 
                (    
                    SELECT IMChannelID FROM [dbo].[IMChannel] WITH (NOLOCK)
                    WHERE [ThirdPartyChatID] = @ThirdPartyChatID
                )";
            }

            if (filter.IMChannelType.HasValue)
            {
                sql += @" AND IMChannelID in 
                (    
                    SELECT IMChannelID FROM [dbo].[IMChannel] WITH (NOLOCK)
                    WHERE [Type] = @IMChannelType
                )";
            }

            return sql;
        }

    }


}
