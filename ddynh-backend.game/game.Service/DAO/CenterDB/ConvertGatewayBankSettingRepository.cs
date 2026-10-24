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
    public class ConvertGatewayBankSettingRepository : IDapperRepository<ConvertGatewayBankSettingEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Pay].[ConvertGatewayBankSetting]"; }
        }


        public List<ConvertGatewayBankSettingEntity> GetByFilter(QueryConvertGatewayBankSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<ConvertGatewayBankSettingEntity>(sql, filter);
            return query;
        }

        public PagedList<ConvertGatewayBankSettingEntity> GetPagingByFilter(QueryConvertGatewayBankSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = GetPageingEntitiesBySQLScript<ConvertGatewayBankSettingEntity>(sql, filter, filter);
            return query;
        }
        

        public void DeleteAndAddNew(int GWId, List<ConvertGatewayBankSettingEntity> items)
        {
            string deleteSQL = $@" DELETE FROM {TableName} 
                           WHERE ConvertGatewayID = @GWId ";

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute(deleteSQL, new { GWId = GWId });
                CreateMany(items);
            }
        }


        private string GenerateWhereSQL(QueryConvertGatewayBankSettingRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            filter.GWIDs = filter.GWIDsText.SeperateToListIntOrDefaultValue(filter.GWIDs);

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND Id in @IDs ";

            if (filter.GWIDs.IsNotEmptyNull())
                sql += " AND ConvertGatewayID in @GWIDs ";

            if (filter.Status.HasValue)
                sql += " AND Status = @Status ";

            return sql;
        }


    }


}
