using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Attributes;
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
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace target.DA.Repository
{
    public class EventBetCounterRepository : FormatIntegerPrimaryKeyRepository<EventBetCounterEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EventBetCounter]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public void ConsumeEventPoint(int playerID , int eventID , int gameID , decimal consumePoint , decimal maxUsagePoint)
        {
            string sql = @"UPDATE [PGS_Master].[dbo].[EventBetCounter] set UsagePoint = UsagePoint + @consumePoint
                           WHERE TotalPoint >= (UsagePoint + @consumePoint) AND  PlayerID = @playerID and GameID = @gameID AND EventID = @eventID AND UsagePoint < @maxUsagePoint ";

            var param = new
            {
                consumePoint = consumePoint,
                playerID = playerID,
                eventID = eventID,
                gameID = gameID,
                maxUsagePoint = maxUsagePoint,
            };

            var updateResult = this.UpdateAndSelectRowCount(sql, param);

            if (updateResult.UpdateRowCount < 1)
            {
                throw new TerproException(ApiErrorCode.Forbidden_InsufficientPointOrReachMaxTimes);
            }
        }

        public EventCounterValuePlusSPResultModel EventCounterValuePlus(EventCounterValuePlusSPParameter model)
        {
            string spName = "PrWeb_Event_EventCounterValuePlus";

            var para = new DynamicParameters();
            para.Add("playerID", model.PlayerID);
            para.Add("gameID", model.GameID);
            para.Add("eventID", model.EventID);
            para.Add("betAmount", model.BetAmount);
            para.Add("payoutAmount", model.PayoutAmount);
            para.Add("point", model.EventPoint);
            para.Add("eventStatDate", model.EventStatDate);

            para.Add(name: "afterTotalRound", dbType: DbType.Int32, direction: ParameterDirection.Output);
            para.Add(name: "afterTotalBetAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output, precision: 18, scale: 4);
            para.Add(name: "afterTotalPayoutAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output, precision: 18, scale: 4);
            para.Add(name: "afterTotalWinLossAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output, precision: 18, scale: 4);
            para.Add(name: "afterMaxPayoutAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output, precision: 18, scale: 4);
            para.Add(name: "afterMaxPayoutRate", dbType: DbType.Decimal, direction: ParameterDirection.Output, precision: 18, scale: 4);
            para.Add(name: "afterTotalPoint", dbType: DbType.Decimal, direction: ParameterDirection.Output, precision: 18, scale: 4);

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Execute(spName, param: para, commandType: CommandType.StoredProcedure);

                int afterTotalRound = para.GetOutputVariable<int>("afterTotalRound");
                decimal afterTotalBetAmount = para.GetOutputVariable<decimal>("afterTotalBetAmount");
                decimal afterTotalPayoutAmount = para.GetOutputVariable<decimal>("afterTotalPayoutAmount");
                decimal afterTotalWinLossAmount = para.GetOutputVariable<decimal>("afterTotalWinLossAmount");
                decimal afterTotalPoint = para.GetOutputVariable<decimal>("afterTotalPoint");

                return new EventCounterValuePlusSPResultModel
                {
                    AfterTotalRound = afterTotalRound,
                    AfterTotalBetAmount = afterTotalBetAmount,
                    AfterTotalPayoutAmount = afterTotalPayoutAmount,
                    AfterTotalWinLossAmount = afterTotalWinLossAmount,
                    AfterTotalPoint = afterTotalPoint
                };
            }
        }

        public EventBetCounterEntity GetSingleByPlayer(int playerID , int eventID , int gameID)
        {
            var query = GetByFilter(new QueryEventBetCounterRequestViewModel
            {
                EventIDs = eventID.ToListCollection(),
                PlayerIDs = playerID.ToListCollection(),
                GameIDs = gameID.ToListCollection()
            });

            return query.FirstOrDefault();
        }


        public List<EventBetCounterEntity> GetByFilter(QueryEventBetCounterRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<EventBetCounterEntity>(sql, filter);
            return query;
        }


        public List<EventBetCounterEntity> GetTopRankingPlayers(int eventID , int maxSize, EventCounterOrderByEnum orderBy)
        {
            string orderByColumn = orderBy.GetAttributeFromEnum<ExtraAttributeValue>()?.Value ?? "TotalBetAmount";

            return GetPagingByFilter(new QueryEventBetCounterRequestViewModel
            {
                PageSize = maxSize,
                EventIDs = eventID.ToListCollection(),
                OrderByColumn = orderByColumn,
            }).Items;
        }



        public PagedList<EventBetCounterEntity> GetPagingByFilter(QueryEventBetCounterRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<EventBetCounterEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEventBetCounterRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.EventIDs.IsNotEmptyNull())
                sql += " AND EventID in @EventIDs ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            return sql;
        }

    }


}
