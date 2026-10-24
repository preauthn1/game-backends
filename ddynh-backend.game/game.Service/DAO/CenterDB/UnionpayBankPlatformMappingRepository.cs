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
    public class UnionpayBankPlatformMappingRepository : IDapperRepository<UnionpayBankPlatformMappingEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Money].[UnionpayBankPlatformMapping]"; }
        }


        public List<UnionpayBankPlatformMappingEntity> GetByFilter(QueryUnionpayBankPlatformMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<UnionpayBankPlatformMappingEntity>(sql, filter);
            return query;
        }

        public PagedList<UnionpayBankPlatformMappingEntity> GetPagingByFilter(QueryUnionpayBankPlatformMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = GetPageingEntitiesBySQLScript<UnionpayBankPlatformMappingEntity>(sql, filter, filter);
            return query;
        }


        public void DeleteAndAddNew(int platformID , List<UnionpayBankPlatformMappingEntity> items)
        {
            string deleteSQL = $@" DELETE FROM {TableName} 
                           WHERE platformID = @platformID ";

            using(var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute(deleteSQL, new { platformID = platformID });
                CreateMany(items);
            }
        }

        private string GenerateWhereSQL(QueryUnionpayBankPlatformMappingRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            filter.PlatformIDs = filter.PlatformIDsText.SeperateToListIntOrDefaultValue(filter.PlatformIDs);

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND Id in @IDs ";

            if (filter.PlatformIDs.IsNotEmptyNull())
                sql += " AND PlatformID in @PlatformIDs ";

            return sql;
        }





    }


}
