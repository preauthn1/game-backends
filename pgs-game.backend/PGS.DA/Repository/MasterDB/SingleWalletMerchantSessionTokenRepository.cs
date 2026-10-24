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
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.DA.Repository
{
    public class SingleWalletMerchantSessionTokenRepository : FormatIntegerPrimaryKeyRepository<SingleWalletMerchantSessionTokenEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[SingleWalletMerchantSessionToken]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public SingleWalletMerchantSessionTokenEntity GetCacheValueBySessionTokenPGS(string pgsToken)
        {
            if (string.IsNullOrWhiteSpace(pgsToken))
                throw new TerproException(ApiErrorCode.InvalidParameter_TextEmpty, new ErrorDetailModel("pgsToken"));

            string cacheKey = CacheKey.DbEntity_SingleWalletMerchantSessionToken(pgsToken);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var query = GetByFilter(new QuerySingleWalletMerchantSessionTokenRequestViewModel
                   {
                       SessionTokenPGS = pgsToken
                   });

                   return query.FirstOrDefault();

               }, 5);

            return queryCache;
        }

        public SingleWalletMerchantSessionTokenEntity GetPlayerLastSessionToken(int playerID , string selectColumns = "*")
        {
            string sql = $@"SELECT top 1 {selectColumns} 
                           FROM [dbo].[SingleWalletMerchantSessionToken] with (NOLOCK)
                           WHERE PlayerID = @playerID 
                           ORDER BY Id desc ";

            var param = new
            {
                playerID = playerID
            };

            var query = QueryByDapper<SingleWalletMerchantSessionTokenEntity>(sql, param);
            return query.FirstOrDefault();
        }

        public List<SingleWalletMerchantSessionTokenEntity> GetByFilter(QuerySingleWalletMerchantSessionTokenRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<SingleWalletMerchantSessionTokenEntity>(sql, filter);
            return query;
        }


        public PagedList<SingleWalletMerchantSessionTokenEntity> GetPagingByFilter(QuerySingleWalletMerchantSessionTokenRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<SingleWalletMerchantSessionTokenEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QuerySingleWalletMerchantSessionTokenRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND ID in @IDs ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            if (filter.SessionTokenPGS.IsNotEmptyTextNotNull())
                sql += " AND SessionTokenPGS = @SessionTokenPGS ";

            if (filter.SessionTokenRaw.IsNotEmptyTextNotNull())
                sql += " AND SessionTokenRaw = @SessionTokenRaw ";

            return sql;
        }

    }


}
