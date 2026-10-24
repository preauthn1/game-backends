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
    public class EventGameMappingRepository : FormatIntegerPrimaryKeyRepository<EventGameMappingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EventGameMapping]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        public void DeleteAndInsertNew(int eventID, List<int> gameIDs)
        {
            DeleteByModel(new { EventID = eventID });

            if (gameIDs.IsEmptyOrNull())
                return;

            var insertModels = gameIDs.Select(z => new EventGameMappingEntity
                {
                    EventID = eventID,
                    GameID = z
                })
                .ToList();

            CreateMany(insertModels);
            return;
        }


        public List<EventGameMappingEntity> GetByFilter(QueryEventGameMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<EventGameMappingEntity>(sql, filter);
            return query;
        }


        public PagedList<EventGameMappingEntity> GetPagingByFilter(QueryEventGameMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<EventGameMappingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEventGameMappingRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            if (filter.EventIDs.IsNotEmptyNull())
                sql += " AND EventID in @EventIDs ";

            return sql;
        }

    }


}
