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
    public class LobbyTokenLogRepository : FormatLongPrimaryKeyRepository<LobbyTokenLogEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.TransactionDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[LobbyTokenLog]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public void ResetAllToken(ResetTokenParameter filter)
        {
            DateTime beginTime = DateTimeHelper.GetCurrentTime().AddDays(-7);
            DateTime emptyTime = new DateTime(1970, 1, 1);

            string extraWhere = string.Empty;

            //呼叫這個方法 比較完整(包含了清cache的邏輯)
            if (filter.IsFilterPlayerOnly)
            {
                ResetPlayerLobbyToken(filter.PlayerID.Value);
                return;
            }
            else if (filter.IsClearAll)
            {
                ExecuteSqlByDapper("TRUNCATE TABLE [PGS_Transaction].[dbo].[LobbyTokenLog]", null, 120);
                return;
            }

            if (filter.MerchantID.HasValue)
                extraWhere += " AND MerchantID  = @MerchantID ";

            if (filter.PlayerID.HasValue)
                extraWhere += " AND PlayerID  = @PlayerID ";

            string sql = @$" UPDATE {TableName} SET ExpiredTime = @emptyTime 
                             WHERE ExpiredTime > @beginTime {extraWhere}";

            var para = new
            {
                emptyTime = emptyTime,
                beginTime = beginTime,
                PlayerID = filter.PlayerID,
                MerchantID = filter.MerchantID
            };

            ExecuteSqlByDapper(sql, para, 120);
        }


        public void ResetPlayerLobbyToken(int playerID)
        {
            string columns = $" {nameof(LobbyTokenLogEntity.Id)} ";

            var query = GetByFilter(new QueryLobbyTokenLogRequestViewModel
            {
                PlayerIDs = playerID.ToListCollection(),
                IsNotExpired = true
            }, columns);

            if (query.IsEmptyOrNull())
                return;

            List<Guid> guidLsit = query.Where(z => z.IdentityGuid.HasValue).Select(z => z.IdentityGuid.Value).ToList();
            List<long> ids = query.Select(x => x.Id).ToList();

            //期望這種寫法會比較不容易發生lock...
            string sql = @$" UPDATE {TableName} SET ExpiredTime = @emptyTime 
                             WHERE Id in @ids ";

            DateTime emptyTime = new DateTime(1970, 1, 1);

            var sqlPara = new
            {
                emptyTime = emptyTime,
                ids = ids
            };

            //非同步處理
            AppHelper.ExecuteMethodAndRetryMaxTimes(2, (param) =>
               {
                   ExecuteSqlByDapper(sql, sqlPara);

                   foreach (Guid guid in guidLsit)
                   {
                       CacheRemoveHelper.RemoveCache_WhenLobbyTokenReset(guid);
                   }

                   return true;

               }, isAsync: true);

        }


        public LobbyTokenLogEntity GetByHash(string tokenHash)
        {
            var query = GetByFilter(new QueryLobbyTokenLogRequestViewModel
            {
                IdentityHash = tokenHash
            });

            return query.FirstOrDefault();
        }


        public LobbyTokenLogEntity GetByGuid(Guid uuid)
        {
            var query = GetByFilter(new QueryLobbyTokenLogRequestViewModel
            {
                IdentityGuid = uuid
            });

            return query.FirstOrDefault();
        }


        public List<LobbyTokenLogEntity> GetByFilter(QueryLobbyTokenLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<LobbyTokenLogEntity>(sql, filter);
            return query;
        }


        public PagedList<LobbyTokenLogEntity> GetPagingByFilter(QueryLobbyTokenLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<LobbyTokenLogEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryLobbyTokenLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK) 
                           WHERE 1=1 ";

            if (filter.IdentityHash.IsNotEmptyTextNotNull())
                sql += " AND IdentityHash = @IdentityHash ";

            if (filter.IdentityGuid.HasValue)
                sql += " AND IdentityGuid = @IdentityGuid ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.IsNotExpired)
                sql += " AND ExpiredTime > getUTCDate() ";

            if (filter.Types.IsNotEmptyNull())
                sql += " AND Type in @Types ";

            return sql;
        }

    }


}
