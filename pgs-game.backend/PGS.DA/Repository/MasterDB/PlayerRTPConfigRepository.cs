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
    public class PlayerRTPConfigRepository : FormatIntegerPrimaryKeyRepository<PlayerRTPConfigEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[RTP].[PlayerRTPConfig]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        /// <summary>
        /// 查出所有參加過活動 但RTP還沒還原的玩家
        /// </summary>
        public List<EventRTPBonusMustResetPlayersResultModel> QueryNotResetRTPPlayers(int eventID)
        {
            string sql = @" SELECT c.PlayerID , c.GameID , c.RTP from[RTP].[PlayerRTPConfig] as c
                            join [dbo].[EventPrizeRecord] as pr on c.PlayerID = pr.PlayerID AND c.GameID = pr.GameID
                            where pr.EventID = @EventID AND pr.EventPrizeType = @EventPrizeType";

            var para = new
            {
                EventID = eventID,
                EventPrizeType = EventPrizeTypeEnum.UpdatePlayerRTP.ToInteger()
            };

            var query = QueryByDapper<EventRTPBonusMustResetPlayersResultModel>(sql , para);
            return query;
        }


        public PlayerRTPConfigEntity GetByPlayerAndGameID(int playerID , int gameID)
        {
            var query = GetByFilter(new QueryPlayerRTPConfigRequestViewModel
            {
                PlayerIDs = playerID.ToListCollection(),
                GameIDs = gameID.ToListCollection()
            });

            return query.FirstOrDefault();
        }

        public List<PlayerRTPConfigEntity> GetByFilter(QueryPlayerRTPConfigRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<PlayerRTPConfigEntity>(sql, filter);
            return query;
        }


        public PagedList<PlayerRTPConfigEntity> GetPagingByFilter(QueryPlayerRTPConfigRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<PlayerRTPConfigEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPlayerRTPConfigRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            if(filter.RTPValue.HasValue)
                sql += " AND RTP = @RTPValue ";

            if (filter.IsRiskControl.HasValue)
                sql += " AND IsRiskControl = @IsRiskControl ";

            return sql;
        }

    }


}
