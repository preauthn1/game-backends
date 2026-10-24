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
    public class UserCustomizedSettingRepository : FormatMultiplePrimaryKeyRepository<UserCustomizedSettingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.OperationDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[UserCustomizedSetting]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "UserID"; }
        }


        public (bool IsCreate , decimal AfterValue) SettingValuePlus(int userID , UserCustomizedSettingEnum settingKeyEnum , decimal plusValue)
        {
            int settingKey = settingKeyEnum.ToInteger();

            var now = DateTimeHelper.GetCurrentTime();

            var queryExist = GetByUniqueKey(userID, settingKeyEnum);

            if(queryExist == null)
            {
                Create(new UserCustomizedSettingEntity
                {
                    SettingKey = settingKey,
                    UserID = userID,
                    SettingValue = plusValue.ToString(),
                    UpdateTime = now
                });

                return (true, plusValue);
            }
            else
            {
                decimal originalValue = decimal.Parse(queryExist.SettingValue);
                decimal afterValue = (originalValue + plusValue);
                queryExist.SettingValue = afterValue.ToString();

                Update(queryExist);

                return (false, afterValue);
            }
        }


        public SystemConfigSettingParseModel GetParseModelByUniqueKey(int userID, UserCustomizedSettingEnum settingKeyEnum)
        {
            var queryExist = GetByUniqueKey(userID, settingKeyEnum);

            if (queryExist == null)
                return new SystemConfigSettingParseModel();

            return new SystemConfigSettingParseModel(settingKeyEnum.ToInteger(), queryExist.SettingValue); ;
        }


        public UserCustomizedSettingEntity GetByUniqueKey(int userID , UserCustomizedSettingEnum settingKeyEnum)
        {
            var queryExist = GetByFilter(new QueryUserCustomizedSettingRequestViewModel
                {
                    SettingKeys = settingKeyEnum.ToInteger().ToListCollection(),
                    UserIDs = userID.ToListCollection(),
                    IsLockQuery = true,
                })
               .FirstOrDefault();

            return queryExist;
        }


        public List<UserCustomizedSettingEntity> GetByFilter(QueryUserCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<UserCustomizedSettingEntity>(sql, filter);
            return query;
        }


        public PagedList<UserCustomizedSettingEntity> GetPagingByFilter(QueryUserCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<UserCustomizedSettingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryUserCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string noLock = filter.IsLockQuery ? "" : " WITH (NOLOCK) ";
            string sql = $@"SELECT {selectColumns} FROM {TableName} {noLock}
                           WHERE 1=1 ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND UserID in @UserIDs ";

            if (filter.SettingKeys.IsNotEmptyNull())
                sql += " AND SettingKey in @SettingKeys ";

            return sql;
        }

    }


}
