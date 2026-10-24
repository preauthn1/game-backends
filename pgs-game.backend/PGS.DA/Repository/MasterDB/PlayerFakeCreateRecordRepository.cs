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
    public class PlayerFakeCreateRecordRepository : FormatMultiplePrimaryKeyRepository<PlayerFakeCreateRecordEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[PlayerFakeCreateRecord]"; }
        }

        protected override string DefaultSortColumn 
        {
            get { return "CreateTime"; }
        }

        protected override bool IsConvertToDbValueType
        {
            get { return true; }
        }


        public PlayerFakeCreateRecordEntity GetByPlayerNameAndMerchantID(string playerName , int merchantID)
        {
            var query = GetByFilter(new QueryPlayerFakeCreateRecordRequestViewModel
            {
                PlayerName = playerName,
                MerchantID = merchantID
            });

            return query.FirstOrDefault();
        }


        public List<PlayerFakeCreateRecordEntity> GetByFilter(QueryPlayerFakeCreateRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<PlayerFakeCreateRecordEntity>(sql, filter);
            return query;
        }


        public PagedList<PlayerFakeCreateRecordEntity> GetPagingByFilter(QueryPlayerFakeCreateRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<PlayerFakeCreateRecordEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPlayerFakeCreateRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.MerchantID.HasValue)
                sql += " AND MerchantID = @MerchantID ";

            if (filter.PlayerName.IsNotEmptyTextNotNull())
                sql += " AND PlayerName = @PlayerName ";

            return sql;
        }

    }


}
