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
    public class AnnouncementReadRepository : FormatIntegerPrimaryKeyRepository<AnnouncementReadEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[AnnouncementRead]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "AnnouncementID"; }
        }


        public bool IsReadAlready(int playerID , int announcementID)
        {
            var query = GetByFilter(new QueryAnnouncementReadRequestViewModel
            {
                PlayerIDs = playerID.ToListCollection(),
                AnnouncementIDs = announcementID.ToListCollection(),
            });

            return query.IsNotEmptyNull();
        }


        public List<AnnouncementReadEntity> GetByFilter(QueryAnnouncementReadRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<AnnouncementReadEntity>(sql, filter);
            return query;
        }


        public PagedList<AnnouncementReadEntity> GetPagingByFilter(QueryAnnouncementReadRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<AnnouncementReadEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryAnnouncementReadRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.AnnouncementIDs.IsNotEmptyNull())
                sql += " AND AnnouncementID in @AnnouncementIDs ";

            return sql;
        }

    }


}
