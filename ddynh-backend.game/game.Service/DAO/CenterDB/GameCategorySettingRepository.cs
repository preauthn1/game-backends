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
    public class GameCategorySettingRepository : IDapperRepository<GameCategorySettingEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[ServerInfo].[GameCategorySetting]"; }
        }


        public List<GameCategorySettingEntity> GetByFilter(QueryGameCategorySettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<GameCategorySettingEntity>(sql, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryGameCategorySettingRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND Id in @IDs ";

            return sql;
        }



        public void DeleteAndAddNew(List<GameCategorySettingEntity> items)
        {
            string deleteSQL = $@" DELETE FROM {TableName} ";

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute(deleteSQL);
                CreateMany(items);
            }
        }


    }


}
