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
    public class GameCategoryRepository : IDapperRepository<GameCategoryEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[ServerInfo].[GameCategory]"; }
        }


        public List<GameCategoryEntity> GetByFilter(QueryGameCategoryRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<GameCategoryEntity>(sql, filter);
            return query;
        }


        public PagedList<GameCategoryEntity> GetPagingByFilter(QueryGameCategoryRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<GameCategoryEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryGameCategoryRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND Id in @IDs ";

            return sql;
        }




    }


}
