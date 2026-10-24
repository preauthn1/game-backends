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
    public class EventPlayerTagRepository : FormatMultiplePrimaryKeyRepository<EventPlayerTagEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EventPlayerTag]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "EventID"; }
        }



        public void CreateOrUpdate(EventPlayerTagEntity entity)
        {
            var query = GetByFilter(new QueryEventPlayerTagRequestViewModel
                {
                    EventIDs = entity.EventID.ToListCollection(),
                    PlayerIDs = entity.PlayerID.ToListCollection(),
                    TagIDs = entity.TagID.ToListCollection(),
                })
                .FirstOrDefault();

            if(query == null)
            {
                Create(entity);
            }
            else
            {
                entity.SettingValue = entity.SettingValue;
                Update(entity);
            }
        }


        public EventPlayerTagEntity GetByUniqueKey(int eventID , int playerID , EventPlayerTagEnum tag)
        {
            var query = GetByFilter(new QueryEventPlayerTagRequestViewModel
            {
                EventIDs = eventID.ToListCollection(),
                PlayerIDs = playerID.ToListCollection(),
                TagIDs = tag.ToInteger().ToListCollection()
            });

            return query.FirstOrDefault();
        }


        public List<EventPlayerTagEntity> GetByFilter(QueryEventPlayerTagRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<EventPlayerTagEntity>(sql, filter);
            return query;
        }


        public PagedList<EventPlayerTagEntity> GetPagingByFilter(QueryEventPlayerTagRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<EventPlayerTagEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEventPlayerTagRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.EventIDs.IsNotEmptyNull())
                sql += " AND EventID in @EventIDs ";

            if (filter.TagIDs.IsNotEmptyNull())
                sql += " AND TagID in @TagIDs ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            return sql;
        }

    }


}
