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
    public class MessageCustomizedSettingRepository : FormatIntegerPrimaryKeyRepository<MessageCustomizedSettingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MessageCustomizedSetting]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "[MessageCustomizedSettingID]"; }
        }

        public List<MessageCustomizedSettingEntity> GetByFilter(QueryMessageCustomizedSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<MessageCustomizedSettingEntity>(sql, filter);
            return query;
        }

        public PagedList<MessageCustomizedSettingEntity> GetPagingByFilter(QueryMessageCustomizedSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<MessageCustomizedSettingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMessageCustomizedSettingRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.MessageIDs.IsNotEmptyNull())
                sql += " AND MessageID in @MessageIDs ";

            if (filter.SettingKeys.IsNotEmptyNull())
                sql += " AND SettingKey in @SettingKeys ";

            return sql;
        }

    }


}
