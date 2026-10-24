using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.DA.Repository
{
    public class BetRecordAGGLiveRepository : FormatBetRecordRepository<BetRecordAGGLiveEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[BetRecordAGGLive]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BetRecordNoPGS"; }
        }

        protected override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.AGGLive;
            }
        }

        public List<BetRecordAGGLiveEntity> GetBriefByRoundIDs(List<string> roundIDs, DateTime begin, DateTime end)
        {
            if (roundIDs.IsEmptyOrNull())
                return new List<BetRecordAGGLiveEntity>();

            string sql = @" SELECT BetRecordNoPGS ,  BetRecordNo , PlayerID , PlayerName , BetAmount ,  WinLossAmount , CreateTime , RoundID
                            FROM [ThirdParty].[BetRecordAGGLive] WITH (NOLOCK) 
                            WHERE RoundID in @roundIDs AND CreateTime >= begin AND CreateTime < @end
                            group by PlayerID , RoundID ";

            var param = new
            {
                roundIDs = roundIDs,
                begin = begin,
                end = end,
            };

            var query = QueryByDapper<BetRecordAGGLiveEntity>(sql, param);
            return query;
        }


        public List<SameRoundIdPlayersModel> GetSameRoundSummaryByPlayerID(int playerID, DateTime begin, DateTime end , GameVendorTypeEnum gameVendor)
        {
            var tableName = GameVendorDefinitionHelper.GetBetRecordTableName(gameVendor);

            string sql = $@"SELECT PlayerID , RoundID , sum(BetAmount) as TotalBetAmount , sum(WinLossAmount) as TotalWinLossAmount
                            FROM {tableName} WITH (NOLOCK) 
                            WHERE RoundID  in
                            (
	                            SELECT RoundID from {tableName} WITH (NOLOCK)
	                            WHERE PlayerID = @playerID AND CreateTime >= @begin AND CreateTime < @end
                            )
                            group by PlayerID , RoundID ";

            var param = new
            {
                playerID = playerID,
                begin = begin,
                end = end,
            };

            var connString = GetConnectionStringByDbName(GameVendorDefinitionHelper.GetDbConnType(gameVendor));

            using (var conn = InstanceSQLConn(connString))
            {
                var query = conn.Query<SameRoundIdPlayersModel>(sql, param);
                return query.ToList();
            }
        }


        public void UpdateToSettlementStatus(BetRecordAGGLiveEntity updateEntity)
        {
            this.ExecuteUpdateToSettlementStatus(updateEntity);
            return;
        }


    }


}
