using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Payment;
using game.Entity.SystemLog;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using game.Infrastructure.Extensions;

namespace game.Service.DAO
{
    public class PayGatewayRepository : IDapperRepository<PayGatewayEntity>
    {
        public override string ConnectionString
        {
            get {return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Payment].[PayGateway]"; }
        }

        public List<PayGatewayEntity> GetByFilter(QueryPayGatewayRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<PayGatewayEntity>(sql, filter);
            return query;
        }


        public PagedList<PayGatewayEntity> GetPagingByFilter(QueryPayGatewayRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<PayGatewayEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPayGatewayRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND Id in @IDs ";

            return sql;
        }

    }


}
