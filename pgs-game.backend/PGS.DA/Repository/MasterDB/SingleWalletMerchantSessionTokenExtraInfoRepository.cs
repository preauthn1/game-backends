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
    public class SingleWalletMerchantSessionTokenExtraInfoRepository : FormatIntegerPrimaryKeyRepository<SingleWalletMerchantSessionTokenExtraInfoEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[SingleWalletMerchantSessionTokenExtraInfo]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public SingleWalletMerchantSessionTokenExtraInfoEntity GetBySessionTokenPGS(string sessionTokenPGS)
        {
            string sql = $"SELECT top 1 * FROM {TableName} with (NOLOCK) where SessionTokenPGS = @SessionTokenPGS order by {PrimaryKeyColumnName} desc";
            var query = QueryByDapper<SingleWalletMerchantSessionTokenExtraInfoEntity>(sql, new { SessionTokenPGS  = sessionTokenPGS });
            return query.FirstOrDefault();
        }

        public SingleWalletMerchantSessionTokenExtraInfoEntity GetLastTokenByPlayerID(int playerID)
        {
            string sql = $"SELECT top 1 * FROM {TableName} with (NOLOCK) where PlayerID = @playerID order by {PrimaryKeyColumnName} desc";
            var query = QueryByDapper<SingleWalletMerchantSessionTokenExtraInfoEntity>(sql, new { playerID = playerID });
            return query.FirstOrDefault();
        }

        public SingleWalletMerchantSessionTokenExtraInfoEntity GetByW88Token(string w88Token)
        {
            string sql = $"SELECT top 1 * FROM {TableName} with (NOLOCK) where Info1 = @Info1 order by {PrimaryKeyColumnName} desc";
            var query = QueryByDapper<SingleWalletMerchantSessionTokenExtraInfoEntity>(sql, new { Info1 = w88Token });
            return query.FirstOrDefault();
        }


        public List<SingleWalletMerchantSessionTokenExtraInfoEntity> GetByFilter(QuerySingleWalletMerchantSessionTokenExtraInfoRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<SingleWalletMerchantSessionTokenExtraInfoEntity>(sql, filter);
            return query;
        }


        public PagedList<SingleWalletMerchantSessionTokenExtraInfoEntity> GetPagingByFilter(QuerySingleWalletMerchantSessionTokenExtraInfoRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<SingleWalletMerchantSessionTokenExtraInfoEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QuerySingleWalletMerchantSessionTokenExtraInfoRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
