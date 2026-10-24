using Dapper;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Area;
using game.Entity.Game;
using game.Entity.Payment;
using game.Entity.SystemLog;
using game.Infrastructure;
using game.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using game.Infrastructure.Extensions;
using game.Entity.ViewModels;

namespace game.Service.DAO
{
    public class GameServerListRepository : IDapperRepository<GameServerListEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[ServerInfo].[GameServerList]"; }
        }

        public override string PrimaryKeyColumnName 
        {
            get { return "ServerID"; }
        }


        public List<GameServerListEntity> GetBriefByFilter(QueryGameServerListRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter , " ServerID , GameID ");
            var query = QueryByDapper<GameServerListEntity>(sql, filter);
            return query;
        }



        public List<GameServerListEntity> GetByFilter(QueryGameServerListRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<GameServerListEntity>(sql, filter);
            return query;
        }


        public PagedList<GameServerListEntity> GetPagingByFilter(QueryGameServerListRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<GameServerListEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryGameServerListRequestViewModel filter , string queryColumns = "*")
        {
            string sql = $@"SELECT {queryColumns} FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND Id in @IDs ";

            if (filter.GameIDs.IsNotEmptyNull())
                sql += " AND GameID in @GameIDs ";

            return sql;
        }


    }


}
