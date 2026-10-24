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
    public class MerchantGameSettingSelfConfigRepository : FormatMultiplePrimaryKeyRepository<MerchantGameSettingSelfConfigEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MerchantGameSettingSelfConfig]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "GameID"; }
        }



        public void DeleteByIDs(List<int> merchantID, List<int> gameIDs)
        {
            //保險起見 不允許傳入空陣列
            if (merchantID.IsEmptyOrNull() || gameIDs.IsEmptyOrNull())
                return;

            var para = new
            {
                MerchantIDs = merchantID,
                GameIDs = gameIDs
            };

            string sql = $" DELETE FROM {TableName} WHERE MerchantID in @MerchantIDs AND GameID in @GameIDs";
            ExecuteSqlByDapper(sql, para);
        }

        public List<MerchantGameSettingSelfConfigEntity> GetByMerchantID(int merchantID)
        {
            var query = GetByFilter(new QueryMerchantGameSettingSelfConfigRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection()
            });

            return query;
        }


        public List<MerchantGameSettingSelfConfigEntity> GetByFilter(QueryMerchantGameSettingSelfConfigRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<MerchantGameSettingSelfConfigEntity>(sql, filter);
            return query;
        }


        public PagedList<MerchantGameSettingSelfConfigEntity> GetPagingByFilter(QueryMerchantGameSettingSelfConfigRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<MerchantGameSettingSelfConfigEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMerchantGameSettingSelfConfigRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            return sql;
        }

    }


}
