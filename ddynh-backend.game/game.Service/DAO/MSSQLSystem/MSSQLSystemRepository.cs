using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.SQL;
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
    public class MSSQLSystemRepository : IDapperRepository<NothingModel>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "This_Is_MSSQL_System"; }
        }
        
        public List<QuerySystemJobResultModel> GetAllJobs()
        {
            string sql = @" SELECT name, enabled as Enable , [job_id] as Job_ID
                            FROM msdb.dbo.sysjobs";

            var query = QueryByDapper<QuerySystemJobResultModel>(sql, null);
            return query;   
        }


        public void ChangeJobEnableFlag(string jobName , bool isEnable)
        {
            int enableValue = isEnable ? 1 : 0;
            string sql = $@" EXEC msdb.dbo.sp_update_job @job_name= '{jobName}', @enabled = {enableValue}";

            using(var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute(sql);
            }
        }

    }






}
