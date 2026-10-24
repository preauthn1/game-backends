using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using target.Core.Helper;
using target.Models.Attributes;
using target.Models.CoreLibs;
using target.Models.Models;
using target.Models.Paging;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;
using target.Utility.Utility.ThirdParty;

namespace target.Core.Repository
{
    public abstract class BaseDapperRepository<T> where T : class
    {
        protected abstract string ConnectionString { get; }

        protected virtual string ConnectionStringRepli
        {
            get
            {
                return this.ConnectionString;
            }
        }

        protected abstract string TableName { get; }


        protected virtual string DefaultSortColumn
        {
            get
            {
                return this.PrimaryKeyColumnName;
            }
        }

        protected virtual string PrimaryKeyColumnName
        {
            get
            {
                return "Id";
            }
        }

        protected virtual bool IsConvertToDbValueType
        {
            get
            {
                return false;
            }
        }

        public void CreateTryCatch(T model, bool isNotifyWhenException = false)
        {
            string insertSQL = SQLHelper.GetInsertCommand<T>(TableName);
            insertSQL += " SELECT CAST(SCOPE_IDENTITY() as int) ";

            try
            {
                CreateBySQL(insertSQL, model);
            }
            catch (Exception ex)
            {
                string msg = $"{TableName} CreateTryCatch fail , {ex.Message}";

                if (isNotifyWhenException)
                {
                    MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                    {
                        Channel = SlackChannelEnum.n_error_pgs_common,
                        Message = msg
                    });
                }
            }
        }


        public virtual void CreateAsync(T model, bool isNotifyWhenError = true)
        {
            string insertSQL = SQLHelper.GetInsertCommand<T>(TableName);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    CreateBySQL(insertSQL, model);
                }
                catch (Exception ex)
                {
                    if (isNotifyWhenError && !SettingHelper.IsMSTest)
                    {
                        string msg = $"{TableName} Create fail , {GetPrimaryKeyColumnValues(model)} , {ex.Message}";
                        MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                        {
                            Channel = SlackChannelEnum.n_error_pgs_common,
                            Message = msg
                        });
                    }
                }
            });
        }


        /// <summary>
        /// 若create失敗 (e.g. DB負荷太嚴重) 會嘗試用queue補救 (ps. 背後未必是queue)
        /// </summary>
        public virtual void CreateOrQueueRetry(T model, EazyQueueTypeEnum queueType = EazyQueueTypeEnum.None)
        {
            string insertSQL = SQLHelper.GetInsertCommand<T>(TableName);

            try
            {
                CreateBySQL(insertSQL, model);
            }
            catch (Exception ex)
            {
                if (ex.IsSqlUniqueException())
                    return;

                if (SettingHelper.IsMSTest)
                    return;

                EazyQueueHelper.Append(new MyEazyQueueModel
                {
                    JsonValue = model.ConvertToJsonString(),
                    Type = queueType
                });

                string msg = $"{TableName} Create fail ，有搭配 EazyQueue 補救 , {GetPrimaryKeyColumnValues(model)} , {ex.Message}";
                MySlackUtility.SendMessageToChannelNotAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_error_pgs_common,
                    Message = msg
                });
            }
        }


        protected void CreateBySQL(string insertSQL, object model)
        {
            try
            {
                using (var conn = InstanceSQLConn())
                {
                    if (IsConvertToDbValueType)
                    {
                        conn.Execute(insertSQL, ConvertClassToDapperDynamicParameters(model));
                    }
                    else
                    {
                        conn.Execute(insertSQL, model);
                    }
                }
            }
            catch (Exception)
            {
                HeaderValueHelper.AppendExtraNotifyMsg("資料表：" + TableName);
                throw;
            }
        }


        protected string GetPrimaryKeyColumnValues(T entity)
        {
            return DbEntityRepositoryHelper.GetPrimaryKeyColumnValueForDebug(entity);
        }



        protected string ReplaceConnectTimeoutInConnString(string connText, int newTimeoutSeconds = 30)
        {
            //滿廢的寫法
            connText = connText.Replace("Connection Timeout=60", $"Connection Timeout={newTimeoutSeconds}");
            connText = connText.Replace("Connection Timeout=45", $"Connection Timeout={newTimeoutSeconds}");
            connText = connText.Replace("Connection Timeout=30", $"Connection Timeout={newTimeoutSeconds}");
            return connText;
        }

        protected string GetConnectionStringByDbName(string dbName, bool isReplicationDB = false)
        {
            if (isReplicationDB)
            {
                dbName = dbName + "_Repli"; //複寫DB固定用此命名
            }

            string settingName = $"ConnectionStrings:{dbName}";

            var conn = BaseSettingHelper.GetSettingValue(settingName);

            if (string.IsNullOrWhiteSpace(conn))
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel
                {
                    ColumnName = settingName,
                    IsAlwaysNotify = true
                });
            }

            return conn;
        }

        protected SqlConnection InstanceSQLConn(int? timeoutSeconds = null)
        {
            return MyInstanceSQLConn(ConnectionString, new DbQueryConfig
            {
                ConnectTimeoutSeconds = timeoutSeconds
            });
        }

        protected SqlConnection InstanceSQLConn(string connectString, DbQueryConfig config = null)
        {
            return MyInstanceSQLConn(connectString, config);
        }

        protected SqlConnection InstanceSQLConn(DbQueryConfig config)
        {
            return MyInstanceSQLConn(ConnectionString, config);
        }

        private SqlConnection MyInstanceSQLConn(string connectString, DbQueryConfig config)
        {
            string myConn = connectString;

            config = config ?? new DbQueryConfig();

            if (config.IsQueryByReplicationDB)
            {
                myConn = ConnectionStringRepli;
            }
            else if (config.IsQueryByOldDataStorageDB)
            {
                myConn = GetConnectionStringByDbName(DBConnType.OldDataStorageDB);
            }
            else if (config.IsQueryByLogDB)
            {
                myConn = GetConnectionStringByDbName(DBConnType.SystemLogDB);
            }


            if (config.ConnectTimeoutSeconds.NotNullNotZeroNotMinus())
            {
                myConn = ReplaceConnectTimeoutInConnString(myConn, config.ConnectTimeoutSeconds.Value);
            }

            return new SqlConnection(myConn);
        }


        public DapperSqlQueue GenerateSQL_Create(T model)
        {
            string insertSQL = SQLHelper.GetInsertCommand<T>(TableName);

            return new DapperSqlQueue
            {
                Sql = insertSQL,
                Parameters = model
            };
        }

        public void ExecuteTransactionSQLs(params DapperSqlQueue[] sqlCommands)
        {
            ExecuteTransactionSQLs(sqlCommands?.ToList());
        }


        public void ExecuteTransactionSQLs(List<DapperSqlQueue> sqlCommands)
        {
            using (var conn = InstanceSQLConn())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    foreach (var item in sqlCommands)
                    {
                        var resultId = conn.Execute(item.Sql, item.Parameters, trans);
                    }

                    trans.Commit();
                }
            }
        }



        public virtual void CreateMany(IEnumerable<T> models)
        {
            if (models == null || models.Count() == 0)
                return;

            string insertSQL = SQLHelper.GetInsertCommand<T>(TableName);

            using (var conn = InstanceSQLConn())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    var resultId = conn.Execute(insertSQL, models, trans);
                    trans.Commit();
                    return;
                }
            }
        }

        public void BulkInsert<MyType>(List<MyType> items, System.Transactions.IsolationLevel level = System.Transactions.IsolationLevel.ReadCommitted) where MyType : class
        {
            BulkInsert(this.TableName, items, level);
        }


        public void BulkInsert<MyType>(string tableName, List<MyType> items, System.Transactions.IsolationLevel level) where MyType : class
        {
            if (items.IsEmptyOrNull())
                return;

            var dt = new DataTable();

            var insertColumns = SQLHelper.GetFieldListCollection<MyType>(null);

            //如果是nullable的欄位 會出現nullable型別的錯誤。需強制轉成string寫入
            List<string> forceToStringColumns = new List<string>();

            foreach (var item in typeof(MyType).GetProperties())
            {
                bool isRequired = insertColumns.Contains(item.Name);

                if (isRequired)
                {
                    if (item.PropertyType.IsNullableType())
                    {
                        dt.Columns.Add(item.Name, typeof(string));
                        forceToStringColumns.Add(item.Name);
                    }
                    else
                    {
                        dt.Columns.Add(item.Name, item.PropertyType);
                    }
                }
            }

            foreach (var item in items)
            {
                var row = dt.NewRow();
                var type = item.GetType();

                foreach (DataColumn column in dt.Columns)
                {
                    var value = type.GetProperty(column.ColumnName).GetValue(item, null);

                    if (forceToStringColumns.Contains(column.ColumnName) && value != null)
                    {
                        value = value.ToString();
                    }

                    row[column.ColumnName] = value;
                }

                dt.Rows.Add(row);
            }

            using (var tran = AppHelper.InstanceTransactionScopeByLevel(level))
            {
                using (var conn = InstanceSQLConn())
                {
                    conn.Open();

                    using (var bk = new SqlBulkCopy(conn))
                    {

                        foreach (DataColumn column in dt.Columns)
                        {
                            SqlBulkCopyColumnMapping mapID = new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName);
                            bk.ColumnMappings.Add(mapID);
                        }

                        bk.BatchSize = 5000;
                        bk.BulkCopyTimeout = 60;
                        bk.DestinationTableName = tableName;
                        bk.WriteToServer(dt);
                    }
                }
                tran.Complete();
            }
        }


        public virtual bool Update(T model)
        {
            var sql = SQLHelper.GetUpdateCommand<T>(TableName).FullSQL;
            using (var conn = InstanceSQLConn())
            {
                return conn.Execute(sql, model) == 1;
            }
        }



        public bool Update(long id, object dynamicObject)
        {
            var allPropertiesOfClass = typeof(T).GetProperties().Select(x => x.Name);

            List<string> updateColumns = new List<string>();

            //先檢查傳入的參數 是否為class定義的db raw entity欄位
            foreach (var item in dynamicObject.GetType().GetProperties())
            {
                string columnName = item.Name;
                bool isExist = allPropertiesOfClass.Any(z => z == columnName);

                if (!isExist)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_NotValidDefineValue);
                }

                updateColumns.Add(columnName);
            }

            string updateColumnsSQL = string.Join(" , ", updateColumns.Select(x => $"[{x}] = @{x}"));
            string updateSQL = $" UPDATE {TableName} SET {updateColumnsSQL} WHERE {PrimaryKeyColumnName} = {id}";

            int result = ExecuteSqlByDapper(updateSQL, dynamicObject);
            return result == 1;
        }

        public SQLUpdateRowCountResult UpdateByCustomizedWhere(T model, string whereCondition, bool isConvertParamByAttribute = false)
        {
            if (string.IsNullOrWhiteSpace(whereCondition))
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("whereCondition"));

            var updateSQL = SQLHelper.GetUpdateCommandWithoutWhere<T>(TableName);
            updateSQL += whereCondition;

            object sqlParam = isConvertParamByAttribute ? ConvertClassToDapperDynamicParameters(model, updateSQL) : (object)model;

            var result = UpdateAndSelectRowCount(updateSQL, sqlParam);
            return result;
        }


        public SQLUpdateRowCountResult UpdateAndSelectRowCount(string updateSQL, object parameters)
        {
            updateSQL += "  SELECT @@ROWCOUNT as UpdateRowCount ";

            using (var conn = InstanceSQLConn())
            {
                var result = conn.QueryFirst<SQLUpdateRowCountResult>(updateSQL, parameters);
                return result;
            }
        }


        public bool UpdateMany(IEnumerable<T> models)
        {
            if (models == null || models.Count() == 0)
                return true;

            var sql = SQLHelper.GetUpdateCommand<T>(TableName).FullSQL;
            using (var conn = InstanceSQLConn())
            {
                return conn.Execute(sql, models) == 1;
            }
        }

        public void DeleteByModel(object model)
        {
            var properties = model.GetType().GetProperties();

            if (properties.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.InvalidParameter_CollectionIsEmptyNull, new ErrorDetailModel("properties"));

            string deleteSql = $"DELETE {TableName} WHERE ";

            List<string> columns = new List<string>();

            foreach (var prop in properties)
            {
                string columnName = prop.Name;
                columns.Add($" {columnName} = @{columnName} ");
            }

            deleteSql += string.Join(" AND ", columns);

            using (var conn = InstanceSQLConn())
            {
                conn.Execute(deleteSql, model);
            }
        }

        public void DeleteByKeyValues(Dictionary<string, object> keyValues)
        {
            string deleteSql = $"DELETE {TableName} WHERE ";

            List<string> columns = new List<string>();

            foreach (var eachValue in keyValues)
            {
                string columnName = eachValue.Key;
                columns.Add($" {columnName} = @{columnName} ");
            }

            deleteSql += string.Join(" AND ", columns);

            using (var conn = InstanceSQLConn())
            {
                conn.Execute(deleteSql, keyValues);
            }
        }


        protected void DeleteAndInsertNewData(string columnName, object keyValue, List<T> models)
        {
            string deleteSql = $"DELETE {TableName} WHERE {columnName} = @keyValue ";
            string insertSQL = SQLHelper.GetInsertCommand<T>(TableName);

            using (var conn = InstanceSQLConn())
            {
                conn.Execute(deleteSql, new { keyValue = keyValue });

                if (models.IsNotEmptyNull())
                {
                    conn.Execute(insertSQL, models);
                }
            }
        }


        public List<T> GetAll()
        {
            string sql = $@"SELECT * FROM {TableName}";
            return QueryByDapper<T>(sql, new { }).ToList();
        }


        public List<T> GetByLargeAmountIDs(List<long> ids, string selectColumns = "*")
        {
            if (ids == null || ids.Count() == 0)
                return new List<T>();

            string PKColumnName = GetKeyAttributeName();
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                            WHERE {PKColumnName} in ( SELECT Id FROM #tempIDs ) ";

            return ExecuteQueryWithLargeIDs<T>(sql, ids);
        }

        public List<T> GetByLargeAmountIDs(List<int> ids, string selectColumns = "*")
        {
            if (ids == null || ids.Count() == 0)
                return new List<T>();

            string PKColumnName = GetKeyAttributeName();
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                            WHERE {PKColumnName} in ( SELECT Id FROM #tempIDs ) ";

            return ExecuteQueryWithLargeIDs<T>(sql, ids);
        }


        public string GetTableName()
        {
            return this.TableName;
        }

        protected string GenerateTopSizeQuerySQL(int? topSize = null)
        {
            if (!topSize.HasValue)
                return string.Empty;

            if (topSize.Value <= 0)
                return string.Empty;

            return $" top {topSize.Value} ";
        }

        /// <summary>
        /// in query限制最多2100筆，因此使用tempTable的機制查詢大量IDs
        /// </summary>
        private List<T2> ExecuteQueryWithLargeIDs<T2>(string sql, List<long> ids) where T2 : class
        {
            string tempTableSQL = GenerateLargeIDTempSQL(null, ids);
            using (var db = InstanceSQLConn())
            {
                db.Open();
                db.Execute(tempTableSQL);
                var query = db.Query<T2>(sql).ToList();
                return query;
            }
        }

        private List<T2> ExecuteQueryWithLargeIDs<T2>(string sql, List<int> ids) where T2 : class
        {
            string tempTableSQL = GenerateLargeIDTempSQL(ids);
            using (var db = InstanceSQLConn())
            {
                db.Open();
                db.Execute(tempTableSQL);
                return db.Query<T2>(sql).ToList();
            }
        }

        protected List<T2> QueryByDapper<T2>(string sql, object parameters, int? timeoutSeconds = null)
        {
            var config = new DbQueryConfig
            {
                ConnectTimeoutSeconds = timeoutSeconds
            };

            return QueryByDapper<T2>(sql, parameters, config);
        }


        protected List<T2> QueryByDapper<T2>(string sql, object parameters, DbQueryConfig extraConfig)
        {
            extraConfig = extraConfig ?? new DbQueryConfig();

            bool isConvert = IsConvertToDbValueType || extraConfig.IsForceConvertToDbValueType;

            object searchParams = isConvert ? ConvertClassToDapperDynamicParameters(parameters, sql) : parameters;

            try
            {
                if (!string.IsNullOrWhiteSpace(extraConfig.TempIDsAppendSQL))
                {
                    //語法可以參考 GetCustomizedSettingMappingOneToManyItems
                    using (var conn = InstanceSQLConn(ConnectionString, extraConfig))
                    {
                        conn.Open();
                        conn.Execute(extraConfig.TempIDsAppendSQL);
                        var query = conn.Query<T2>(sql, parameters);
                        return query.ToList();
                    }
                }
                else
                {
                    using (var conn = InstanceSQLConn(ConnectionString, extraConfig))
                    {
                        var query = conn.Query<T2>(sql, searchParams, commandTimeout: TimeoutOrDefault(extraConfig.ConnectTimeoutSeconds));
                        return query.ToList();
                    }
                }
            }
            catch(Exception ex)
            {
                string debug1 = TableName;
                string debug2 = sql;
                string debug3 = ex.GetMessageAndErrorLine();

                //斷點設這邊
                throw;
            }

        }

       
        private int TimeoutOrDefault(int? timeoutSeconds)
        {
            if (timeoutSeconds.HasValue)
                return timeoutSeconds.Value;

            return 30;
        }



        private bool IsInt32()
        {
            return typeof(T).GetProperty(this.PrimaryKeyColumnName).PropertyType == typeof(int);
        }


        private string GenerateLargeIDTempSQL(List<int> myIDs, List<long> longIDs = null)
        {
            myIDs = myIDs ?? new List<int>();
            longIDs = longIDs ?? new List<long>();

            string dataFormat = "int";

            if (longIDs.CountOrZero() > 0)
            {
                if (IsInt32())
                {
                    dataFormat = "int";
                }
                else
                {
                    dataFormat = "bigint";
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append($"CREATE TABLE #tempIDs(Id {dataFormat} not null primary key) ");

            if (myIDs.IsNotEmptyNull())
            {
                var splitArray = myIDs.SplitListByArraySize(999);

                foreach (var eachArray in splitArray)
                {
                    string mySQL = "INSERT INTO #tempIDs VALUES " + string.Join(",", eachArray.Select(x => $"({x})")) + " ;";
                    sb.Append(mySQL);
                }
            }

            if (longIDs.IsNotEmptyNull())
            {
                var splitArray = longIDs.SplitListByArraySize(999);

                foreach (var eachArray in splitArray)
                {
                    string mySQL = "INSERT INTO #tempIDs VALUES " + string.Join(",", eachArray.Select(x => $"({x})")) + " ;";
                    sb.Append(mySQL);
                }
            }

            return sb.ToString();
        }


        protected int ExecuteSqlByDapper(string sql, object parameters, int? timeoutSeconds = null)
        {
            using (var conn = InstanceSQLConn(timeoutSeconds))
            {
                var result = conn.Execute(sql, parameters);
                return result;
            }
        }


        protected int ExecuteStoreProcedureByDapper(string spName, object parameters)
        {
            using (var conn = InstanceSQLConn())
            {
                var result = conn.Execute(spName, parameters, commandType: CommandType.StoredProcedure);
                return result;
            }
        }


        protected List<S> ExecuteStoreProcedureResultByDapper<S>(string spName, object parameters)
        {
            using (var conn = InstanceSQLConn())
            {
                var result = conn.Query<S>(spName, parameters, commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
        }

   
        public GenerateWhereSQLResult GenerateInQuery_Or_LargeIDsQuery(string basicWhereIn, List<int> listIDs)
        {
            if (listIDs.CountOrZero() >= 2050)
            {
                return new GenerateWhereSQLResult
                {
                    WhereSQL = "  (SELECT Id FROM #tempIDs) ",
                    TempIDsAppendSQL = GenerateLargeIDTempSQL(listIDs)
                };
            }
            else
            {
                return new GenerateWhereSQLResult
                {
                    WhereSQL = $" {basicWhereIn} ",
                };
            }
        }


        protected string GetKeyAttributeName()
        {
            List<string> keyAttr = new List<string>();

            foreach (PropertyInfo item in typeof(T).GetProperties())
            {
                var matchAttr = item.GetCustomAttributes<KeyAttribute>(true);

                if (matchAttr == null || matchAttr.Count() == 0)
                    continue;

                keyAttr.Add(item.Name);
            }

            if (keyAttr.IsEmptyOrNull())
                throw new Exception("not define key columns (PK) in class. ");

            //多key的table禁止使用此function
            if (keyAttr.Count() >= 2)
                throw new Exception("multiple  key columns (PK) define , forbidden. ");

            return keyAttr.FirstOrDefault();
        }



        /// <summary>
        /// 分頁
        /// </summary>
        protected PagedList<S> GetPagingEntitiesBySQLScript<S>(string qrySQL, PaginationParameter pager, object param = null, string prefixSQL = "") where S : class
        {
            string sortDirection = pager.IsASC ? "asc" : "desc";
            string[] strArr = qrySQL.Split(new string[] { "FROM", "from", "From" }, StringSplitOptions.None);

            if (string.IsNullOrWhiteSpace(pager.OrderByColumn))
            {
                pager.OrderByColumn = DefaultSortColumn;
            }

            var splitSortColumns = pager.OrderByColumn.Split(',');

            foreach (var eachColumn in splitSortColumns)
            {
                //姑且防範下可能的 SQL injection 所以設字數限制
                if (!pager.IsSkipCheckOrderByInjection && eachColumn != DefaultSortColumn)
                {
                    if (eachColumn.CountOrZero() >= 25 || RegexRuleHelper.IsMatchRequestBodySqlInjectionKeyword(eachColumn))
                    {
                        throw new TerproException(ApiErrorCode.InvalidParameter_OrderByColumn, new ErrorDetailModel
                        {
                            IsAlwaysNotify = true
                        });
                    }
                }
            }

            string sortBySQL = $" {pager.OrderByColumn} {sortDirection} ";

            //如果是多欄位排序
            if (splitSortColumns.Count() > 1)
            {
                sortBySQL = $" {pager.OrderByColumn} ";
            }

            string str1 = strArr[0] + $",(ROW_NUMBER() OVER (ORDER BY {sortBySQL} )) as RowNum ";

            StringBuilder sb = new StringBuilder();
            //跳過第一個
            for (int i = 1; i < strArr.Length; i++)
            {
                sb.Append(@" FROM" + strArr[i]);
            }

            string str2 = sb.ToString();
            qrySQL = str1 + str2;

            string countSQL = $@"
                              SELECT COUNT(0) as cnt 
                              FROM ({qrySQL}) as DataCount";

            string topQuerySQL = $@"
                              SELECT * 
                              FROM ({qrySQL}) as DataCount";

            return GetPagingEntitiesBySQLScript<S>(qrySQL, countSQL, pager, param, prefixSQL, topQuerySQL);
        }


        /// <summary>
        /// 依條件查詢多筆資料
        /// </summary>
        private PagedList<S> GetPagingEntitiesBySQLScript<S>(string sqlScript, string countSQL, PaginationParameter pager, object param = null, string prefixSQL = "", string queryNoPagingSQL = "") where S : class
        {
            pager.PageIndex = pager.PageIndex <= 0 ? 1 : pager.PageIndex;
            pager.PageSize = pager.PageSize <= 0 ? 5 : pager.PageSize;

            PagedList<S> pageResult = new PagedList<S>();
            pageResult.Items = new List<S>();

            countSQL = prefixSQL + " " + countSQL;

            var queryConfig = pager.DbQueryExtraConfig ?? new DbQueryConfig();

            int totalCount = GetScalarBySQLScript<int>(countSQL, param, queryConfig);

            pageResult = new PagedList<S>
            {
                PageIndex = pager.PageIndex,
                PageSize = pager.PageSize,
                TotalCount = totalCount,
                TotalPage = CalculateTotalPage(totalCount, pager.PageSize)
            };

            if (totalCount > 0 && totalCount <= pager.PageSize && pager.PageIndex == 1)
            {
                //以players為例。千萬筆資料。只要筆數小於pageSize 查詢就會變超級超級慢...
                pageResult.Items = GetEntitiesBySQLScript<S>(prefixSQL + queryNoPagingSQL, param, queryConfig);
            }
            else if (totalCount > 0)
            {
                //如果要追SQL很慢的問題 請確認此
                var queryResult = GetPageingEntities<S>(sqlScript, pager, param, queryConfig, prefixSQL);
                pageResult.Items = queryResult.Items;

                if (!SettingHelper.IsProduction)
                {
                    pageResult.DebugInfo = new
                    {
                        CountSQL = Regex.Replace(countSQL, @"\r\n?|\n", ""),
                        PagingSQL = Regex.Replace(queryResult.SQL, @"\r\n?|\n", "")
                    };
                }
            }

            return pageResult;
        }


        protected int CalculateTotalPage(int totalCount, int pageSize)
        {
            return AppHelper.CalculateTotalPage(totalCount, pageSize);
        }


        /// <summary>
        /// 依條件查詢第一筆第一個欄位資料
        /// </summary>
        protected S GetScalarBySQLScript<S>(string _SQLScript, object param, DbQueryConfig config)
        {
            using (SqlConnection conn = InstanceSQLConn(config))
            {
                return conn.ExecuteScalar<S>(_SQLScript, param);
            }
        }


        /// <summary>
        /// 依條件查詢多筆資料(分頁用)
        /// </summary>
        private (List<S> Items, string SQL) GetPageingEntities<S>(string sqlLScript, PaginationParameter pager, object param, DbQueryConfig config, string prefixSQL = "")
        {
            int startRowNum = (pager.PageIndex <= 1) ? 1 : 1 + (pager.PageIndex - 1) * pager.PageSize;
            int endRowNum = (startRowNum - 1) + pager.PageSize;
            string paggingSQL = $@"
                        {prefixSQL}
                        SELECT *
                        FROM ( " + sqlLScript + @" ) as pageData
                        WHERE RowNum >= " + startRowNum + @" AND RowNum <= " + endRowNum;

            return (GetEntitiesBySQLScript<S>(paggingSQL, param, config), paggingSQL);
        }



        /// <summary>
        /// 依條件查詢多筆資料
        /// </summary>
        protected List<S> GetEntitiesBySQLScript<S>(string _SQLScript, object param, DbQueryConfig config)
        {
            using (SqlConnection conn = InstanceSQLConn(config))
            {
                return conn.Query<S>(_SQLScript, param).ToList();
            }
        }

        /// <summary>
        /// Not Like 不包含
        /// </summary>
        protected string GenerateNotLikeQuery(string dbColumnName, string dapperParameterName)
        {
            return $" ({dbColumnName} NOT LIKE CONCAT('%',{dapperParameterName} ,'%')) ";
        }

        protected string GenerateLikeQuery(string dbColumnName, string dapperParameterName)
        {
            return $" ({dbColumnName} LIKE CONCAT('%',{dapperParameterName} ,'%')) ";
        }

        protected string GetLockQuerySQL(bool isLockQuery)
        {
            return isLockQuery ? "" : " WITH (NOLOCK) ";
        }

        protected DynamicParameters InstanceDynamicParameterWithReturnValue()
        {
            var para = new DynamicParameters();

            //似乎這邊add的順序 有可能導致output參數轉型出問題(?) 感覺是dapper的bug
            para.Add(name: "@RetVal", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
            return para;
        }

        /// <summary>
        /// 追蹤程式碼是在哪邊呼叫的
        /// </summary>
        protected string GenerateDebugSourceInfo(string sourceMethod = "")
        {
            if (string.IsNullOrWhiteSpace(sourceMethod))
            {
                return @$" -- {SettingHelper.WebSiteName} {sourceMethod} 
                      {Environment.NewLine}";
            }

            return @$" -- {sourceMethod} {SettingHelper.WebSiteName}
                      {Environment.NewLine}";
        }


        public DynamicParameters ConvertClassToDapperDynamicParameters(object param, string executeSQL = null)
        {
            if (param == null)
                return null;

            var parameters = new DynamicParameters();

            List<string> availableParameters = new List<string>();

            //如果有傳SQL進來。只擷取有使用到的參數 (e.g. SQL裡面只有用到 @ID 欄位、則最終只會返回一個欄位)
            if (!string.IsNullOrWhiteSpace(executeSQL))
            {
                // 使用正則表達式匹配包含 @ 的關鍵字
                Regex regex = new Regex(@"@\w+");

                MatchCollection matches = regex.Matches(executeSQL);

                foreach (Match match in matches)
                {
                    availableParameters.Add(match.Value.Replace("@", ""));
                }
            }

            foreach (PropertyInfo prop in param.GetType().GetProperties())
            {
                var columnName = prop.Name;
                var dbStringAttr = prop.GetTargetAttribute<ReferDbStringType>();
                var dbDecimalAttr = prop.GetTargetAttribute<ReferDbDecimalType>();
                var myValue = prop.GetValue(param);
                bool isDateTime = prop.PropertyType == typeof(DateTime);
                bool isDateTimeNULL = prop.PropertyType == typeof(DateTime?);


                //若陣列有值，會過濾不必要的欄位
                if (availableParameters.Count() >= 1)
                {
                    if (!availableParameters.Any(t => t == columnName))
                    {
                        continue;
                    }
                }

                if (dbStringAttr != null)
                {
                    string finalStringValue = null;

                    if (myValue != null)
                    {
                        finalStringValue = myValue.ToString();
                    }

                    if (dbStringAttr.IsNVarchar)
                    {
                        parameters.Add(columnName, ToNVarchar(finalStringValue, dbStringAttr.TextLength));
                    }
                    else
                    {
                        parameters.Add(columnName, ToVarchar(finalStringValue, dbStringAttr.TextLength));
                    }
                }
                else if (dbDecimalAttr != null)
                {
                    parameters.Add(columnName, value: myValue, dbType: DbType.Decimal, precision: dbDecimalAttr.Precision, scale: dbDecimalAttr.Scale);
                }
                else if (isDateTime || isDateTimeNULL)
                {
                    parameters.Add(columnName, value: myValue, dbType: DbType.DateTime);
                }
                else
                {
                    parameters.Add(columnName, myValue);
                }
            }


            return parameters;
        }

        public DbString ToNVarchar(string textValue, int length = 50)
        {
            return new DbString { Value = textValue, IsAnsi = false, Length = length };
        }


        public DbString ToVarchar(string textValue, int length = 50)
        {
            return new DbString { Value = textValue, IsAnsi = true, Length = length };
        }

        private string ReplaceToNoneChangeLineSQL(string sql)
        {
            return Regex.Replace(sql, @"\r\n?|\n|\t", "").Replace("  ", " ");
        }

    }


}
