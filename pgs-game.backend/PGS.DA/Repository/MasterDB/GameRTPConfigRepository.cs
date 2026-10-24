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
    public class GameRTPConfigRepository : FormatIntegerPrimaryKeyRepository<GameRTPConfigEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[RTP].[GameRTPConfig]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<GameRTPConfigEntity> GetByFilter(QueryGameRTPConfigRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<GameRTPConfigEntity>(sql, filter);
            return query;
        }


        public PagedList<GameRTPConfigEntity> GetPagingByFilter(QueryGameRTPConfigRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<GameRTPConfigEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryGameRTPConfigRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            if (filter.VendorConfigIDs.IsNotEmptyNull())
                sql += " AND VendorConfigID in @VendorConfigIDs ";

            if (filter.RTPValue.HasValue)
                sql += " AND RTP = @RTPValue ";

            return sql;
        }

    }


}
