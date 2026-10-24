using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
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
using Dapper;

namespace game.Service.DAO
{
    public class ApiRequestLogRepository : IDapperRepository<ApiRequestLog>
    {
        public override string ConnectionString
        {
            get {return GetConnectionStringByDbName(DBNameConfig.DBName_LogDB); }
        }

        public override string TableName
        {
            get { return "[dbo].[ApiRequestLog]"; }
        }

        public void CreateToTargetTable(string tableName , ApiRequestLog table)
        {
            string insertSQL = SQLHelper.GetInsertCommand<ApiRequestLog>(tableName);

            using(var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute(insertSQL, table);
            }
        }
    }


}
