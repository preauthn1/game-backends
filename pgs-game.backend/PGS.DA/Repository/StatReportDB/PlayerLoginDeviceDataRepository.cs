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
    public class PlayerLoginDeviceDataRepository : FormatIntegerPrimaryKeyRepository<PlayerLoginDeviceDataEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.StatReportDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[PlayerLoginDeviceData]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "PlayerID"; }
        }

        public List<PlayerLoginDeviceModel> GetEachPlayerLoginDeviceCount(int merchantID , DateTime begin , DateTime end)
        {
            string sql = @" SELECT PlayerID , HDType , SUM(LoginCount) as LoginCount 
                            FROM [dbo].[PlayerLoginDeviceData] WITH (NOLOCK)
                            WHERE StatTime >= @begin AND StatTime < @end AND MerchantID = @merchantID
                            GROUP BY PlayerID , HDType";

            var param = new
            {
                begin = begin,
                end = end,
                merchantID = merchantID
            };

            var query = QueryByDapper<PlayerLoginDeviceModel>(sql, param);
            return query;
        }


        public List<PlayerLoginDeviceDataEntity> GetByFilter(QueryPlayerLoginDeviceDataRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<PlayerLoginDeviceDataEntity>(sql, filter);
            return query;
        }


        public PagedList<PlayerLoginDeviceDataEntity> GetPagingByFilter(QueryPlayerLoginDeviceDataRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<PlayerLoginDeviceDataEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPlayerLoginDeviceDataRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
