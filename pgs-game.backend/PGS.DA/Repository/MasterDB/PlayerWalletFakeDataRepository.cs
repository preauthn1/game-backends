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
    public class PlayerWalletFakeDataRepository : FormatMultiplePrimaryKeyRepository<PlayerWalletFakeDataEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[PlayerWalletFakeData]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "PlayerID"; }
        }

        public PlayerWalletFakeDataEntity GetByPlayerIDAndType(int playerID , FakeWalletTypeEnum type)
        {
            var query = GetByFilter(new QueryPlayerWalletFakeDataRequestViewModel
            {
                PlayerIDs = playerID.ToListCollection(),
                Types = type.ToInteger().ToListCollection(),
                IsLockQuery = true
            });

            return query.FirstOrDefault();
        }

        public void UpdateWalletAmount(int playerID , FakeWalletTypeEnum type , decimal plusOrMinusAmount)
        {
            string updateSQL = @$" Update {TableName} SET Amount = Amount + @plusOrMinusAmount
                             WHERE playerID = @playerID AND type = @type AND (Amount + @plusOrMinusAmount) >= 0";

            var param = new
            {
                playerID = playerID,
                type = type.ToInteger(),
                plusOrMinusAmount = plusOrMinusAmount
            };

            var result = UpdateAndSelectRowCount(updateSQL, param);
            
            if(result.UpdateRowCount <= 0)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DatabaseBusyAndUpdateFail);
            }
        }


        public List<PlayerWalletFakeDataEntity> GetByFilter(QueryPlayerWalletFakeDataRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<PlayerWalletFakeDataEntity>(sql, filter);
            return query;
        }


        public PagedList<PlayerWalletFakeDataEntity> GetPagingByFilter(QueryPlayerWalletFakeDataRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<PlayerWalletFakeDataEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPlayerWalletFakeDataRequestViewModel filter, string selectColumns = "*")
        {
            string lockSQL = filter.IsLockQuery ? "" : " WITH (NOLOCK) ";

            string sql = $@"SELECT {selectColumns} FROM {TableName} {lockSQL}
                           WHERE 1=1 ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.Types.IsNotEmptyNull())
                sql += " AND Type in @Types ";

            return sql;
        }

    }


}
