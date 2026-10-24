using Dapper;
using game.Entity;
using target.Core.Repository;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using target.Utility.Extensions;
using System.Linq;
using Lck.Utility.Extensions.Helper;
using target.Utility;
using target.Models.Models.StoreProcedures;
using System.Data.SqlClient;
using System.Data;
using target.Models.Enums;
using target.Utility.Helper;
using target.Models.Models;

namespace target.DA.Repository
{

    /// <summary>
    /// 純粹系統DB的SP
    /// </summary>
    public class SystemSPRepository
    {
        /// <summary>
        /// 取得lock超過二分鐘的SQL
        /// </summary>
        public int GetBlockSQLs(string dbConnType)
        {
            string sql = @$"SELECT spid
                            FROM sys.sysprocesses
                            WHERE blocked > 0 AND waittime > 120000
                            ORDER BY waittime DESC;";

            string dbConn = GetConnectionStringByDbName(dbConnType);

            dbConn = ReplaceConnectTimeoutInConnString(dbConn, newTimeoutSeconds: 300);

            using (var conn = new SqlConnection(dbConn))
            {
                var result = conn.Query<LockSQLResult>(sql).FirstOrDefault();

                if (result == null)
                    return 0;

                return result.spid;
            }
        }



        public List<Dictionary<string,object>> GetAllDataByTableName(string tableName , string whereSQL = "")
        {
            string sql = $" SELECT TOP 5000 * FROM {tableName} WITH (NOLOCK) {whereSQL}";

            string dbConn = GetConnectionStringByDbName(DBConnType.MasterDB);

            dbConn = ReplaceConnectTimeoutInConnString(dbConn, newTimeoutSeconds: 300);

            using (var conn = new SqlConnection(dbConn))
            {
                var result = conn.Query(sql).Cast<IDictionary<string, object>>();
                return result.Select(x => new Dictionary<string, object>(x)).ToList();
            }
        }


        public RoutineSPResult CalculateStatisticSummaryDataAgain(DateTime beginUTC)
        {            
            string spName = "PrWeb_Stat_CalculateStatisticSummaryAgain";

            var param = new
            {
                begin = beginUTC,
            };

            string dbConn = GetConnectionStringByDbName(DBConnType.StatReportDB);

            dbConn = ReplaceConnectTimeoutInConnString(dbConn, newTimeoutSeconds: 300);

            using (var conn = new SqlConnection(dbConn))
            {
                var result = conn.Query<RoutineSPResult>(spName, param: param, commandType: CommandType.StoredProcedure);
                return result.FirstOrDefault();
            }
        }


        public SP_PrTG_Stat_ForNotifyResult PrTG_Stat_ForNotify(int merchantID, DateTime dateTW)
        {
            dateTW = dateTW.Date;

            string dbConn = GetConnectionStringByDbName(DBConnType.StatReportDB);

            string spName = "PrTG_Stat_ForNotify";

            var param = new
            {
                MerchantId = merchantID,
                StatDate = dateTW
            };

            using (var conn = new SqlConnection(dbConn))
            {
                var result = conn.Query<SP_PrTG_Stat_ForNotifyResult>(spName, param: param, commandType: CommandType.StoredProcedure);
                return result.FirstOrDefault();
            }
        }

        public RoutineSPResult CheckVIPMerchantStatDBDataCorrect(DateTime begin, DateTime end)
        {
            string dbConn = GetConnectionStringByDbName(DBConnType.StatReportDB);

            var para = new DynamicParameters();
            para.Add(name: "begin", value: begin, dbType: DbType.DateTime);
            para.Add(name: "end", value: end, dbType: DbType.DateTime);
            para.Add(name: "result_isSuccess", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            para.Add(name: "result_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 128);

            string spName = "PrJob_Warning_CheckVIPMerchantStatDBDataCorrect";

            using (var conn = new SqlConnection(dbConn))
            {
                var result = conn.Execute(spName, param: para, commandType: CommandType.StoredProcedure);
                var result_isSuccess = para.GetOutputVariable<bool>("result_isSuccess");
                var result_msg = para.GetOutputVariable<string>("result_msg");

                return new RoutineSPResult
                {
                    IsSuccess = result_isSuccess,
                    ResultMsg = result_msg
                };
            }
        }



        public RoutineSPResult ExecuteTargetProcedureAndCheckSuccess(string dbCategoryName, string spName)
        {
            string dbConn = GetConnectionStringByDbName(dbCategoryName);

            var para = new DynamicParameters();
            para.Add(name: "result_isSuccess", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            para.Add(name: "result_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 128);

            using (var conn = new SqlConnection(dbConn))
            {
                var result = conn.Execute(spName, param: para, commandType: CommandType.StoredProcedure);
                var result_isSuccess = para.GetOutputVariable<bool>("result_isSuccess");
                var result_msg = para.GetOutputVariable<string>("result_msg");

                return new RoutineSPResult
                {
                    IsSuccess = result_isSuccess,
                    ResultMsg = result_msg
                };
            }
        }


        public int TriggerQAGenerateTestData(string text)
        {
            string dbConn = GetConnectionStringByDbName(DBConnType.MasterDB);
            var spName = "[dbo].[PrWeb_QAGenerateTestData]";

            var para = new
            {
                text = text
            };

            using (var conn = new SqlConnection(dbConn))
            {
                var result = conn.Execute(spName, para, commandType: CommandType.StoredProcedure);
                return result;
            }
        }

        public int TriggerQAClearData()
        {
            string dbConn = GetConnectionStringByDbName(DBConnType.MasterDB);
            var spName = "[dbo].[PrWeb_QAClearData]";

            using (var conn = new SqlConnection(dbConn))
            {
                var result = conn.Execute(spName, null, commandType: CommandType.StoredProcedure);
                return result;
            }
        }

        public List<Dictionary<string, object>> GetMasterDbDiskRemaningSize()
        {
            string dbConn = GetConnectionStringByDbName(DBConnType.MasterDB);
            var spName = "MASTER..xp_fixeddrives";
            using (var conn = new SqlConnection(dbConn))
            {
                var query = conn.Query(spName, null, commandType: CommandType.StoredProcedure);
                return query
                    .Cast<IDictionary<string, object>>()
                    .Select(it => it.ToDictionary(it => it.Key, it => it.Value))
                    .ToList();
            }
        }


        public List<Dictionary<string, object>> GetLogDBDiskRemaningSize()
        {
            string dbConn = GetConnectionStringByDbName(DBConnType.SystemLogDB);
            var spName = "MASTER..xp_fixeddrives";
            using (var conn = new SqlConnection(dbConn))
            {
                var query = conn.Query(spName, null, commandType: CommandType.StoredProcedure);
                return query
                    .Cast<IDictionary<string, object>>()
                    .Select(it => it.ToDictionary(it => it.Key, it => it.Value))
                    .ToList();
            }
        }


        protected int ExecuteSqlByDapper(string sql, object parameters , string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var result = conn.Execute(sql, parameters);
                return result;
            }
        }



        private string GetConnectionStringByDbName(string dbName)
        {
            var conn = BaseSettingHelper.GetSettingValue($"ConnectionStrings:{dbName}");
            return conn;
        }

        private string ReplaceConnectTimeoutInConnString(string connText, int newTimeoutSeconds = 30)
        {
            //滿廢的寫法
            connText = connText.Replace("Connection Timeout=60", $"Connection Timeout={newTimeoutSeconds}");
            connText = connText.Replace("Connection Timeout=45", $"Connection Timeout={newTimeoutSeconds}");
            connText = connText.Replace("Connection Timeout=30", $"Connection Timeout={newTimeoutSeconds}");
            return connText;
        }
    }


}
