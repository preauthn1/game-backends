using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using FW.Core.Helper;
using System.Transactions;

namespace FW.Core.Repository
{
    public abstract class IDapperRepository<T> where T: class
    {
        public abstract string ConnectionString { get; }

        public abstract string TableName { get; }

        public virtual string DefaultSortColumn
        {
            get
            {
                return this.PrimaryKeyColumnName;
            }
        }

        public virtual string PrimaryKeyColumnName
        { 
            get
            {
                return "Id";
            }
        }


        public Type _type = null;


        public IDapperRepository()
        {
            _type = typeof(T);
        }
        

        protected string GetConnectionStringByDbName(string dbName)
        {
            var setion = DBConfigHelper.GetDBConnByName(dbName);

            if (setion != null)
                return setion.ConnectionString;

            var conn = System.Configuration.ConfigurationManager.ConnectionStrings[dbName];

            if (conn == null)
                throw new Exception("not found conn" + dbName);

            return conn.ConnectionString;
        }
        

        public int Create(T model , string skipColumn = "Id")
        {
            string insertSQL = SQLHelper.GetInsertCommand<T>(TableName , new List<string> { skipColumn });
            insertSQL += " SELECT CAST(SCOPE_IDENTITY() as int) ";

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Query<int?>(insertSQL, model);

                if (result == null)
                    return 0;

                int? resultID = result.FirstOrDefault();

                return resultID.HasValue ? resultID.Value : 0;
            }
        }

        public void CreateMany(IEnumerable<T> models, string skipColumn = "Id")
        {
            if (models == null || models.Count() == 0)
                return;

            string insertSQL = SQLHelper.GetInsertCommand<T>(TableName, new List<string> { skipColumn });

            using (var conn = new SqlConnection(ConnectionString))
            {
                var resultId = conn.Execute(insertSQL, models);
                return;
            }
        }

        protected void CreateOrUpdate(IEnumerable<T> models , List<string> matchColumns)
        {
            string sql = SQLHelper.GetInsertOrUpdateCommandByIgnoreId<T>(TableName , matchColumns);

            using (var tran = new TransactionScope())
            {
                using (var conn = new SqlConnection(ConnectionString))
                {
                    conn.Execute(sql, models);
                    tran.Complete();
                    return;
                }
            }
        }


        public bool Update(T model)
        {
            var sql = SQLHelper.GetUpdateCommand<T>(TableName);
            using (var conn = new SqlConnection(ConnectionString))
            {
                return conn.Execute(sql, model) == 1;
            }
        }

        public bool UpdateMany(IEnumerable<T> models)
        {
            if (models == null || models.Count() == 0)
                return true;

            var sql = SQLHelper.GetUpdateCommand<T>(TableName);
            using (var conn = new SqlConnection(ConnectionString))
            {
                return conn.Execute(sql, models) == 1;
            }
        }

        public void Delete(long id)
        {
            string deleteSql = $"DELETE {TableName} WHERE {PrimaryKeyColumnName} = @id ";
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute(deleteSql, new { id = id });
            }
        }

        public List<T> GetAll()
        {
            string sql = $@"SELECT * FROM {TableName}";
            return QueryByDapper<T>(sql, new {  }).ToList();
        }


        public T GetById(int id)
        {
            string PKColumnName = GetKeyAttributeName();
            string sql = $@"SELECT * FROM {TableName} 
                            WHERE {PKColumnName} = @id ";

            return QueryByDapper<T>(sql , new  { id }).FirstOrDefault();
        }


        public List<T> GetByIds(List<int> ids)
        {
            ids = ids.Distinct().ToList();

            if (ids == null || ids.Count() == 0)
                return new List<T>();

            if (ids.Count() >= 2100)
                return GetByLargeIDs(ids);

            string PKColumnName = GetKeyAttributeName();
            string sql = $@"SELECT * FROM {TableName} 
                            WHERE {PKColumnName} in @ids ";

            return QueryByDapper<T>(sql , new { ids });
        }


        public List<T> GetByLargeIDs(List<int> ids)
        {
            if (ids == null || ids.Count() == 0)
                return new List<T>();

            string PKColumnName = GetKeyAttributeName();
            string sql = $@"SELECT * FROM {TableName} 
                            WHERE {PKColumnName} in ( SELECT Id FROM #tempIDs ) ";

            return ExecuteQueryWithLargeIDs<T>(sql, ids);
        }
        
        /// <summary>
        /// in query限制最多2100筆，因此使用tempTable的機制查詢大量IDs
        /// </summary>
        protected List<T> ExecuteQueryWithLargeIDs<T>(string sql, List<int> ids) where T : class
        {
            using (var db = new SqlConnection(ConnectionString))
            {
                var idCoverts = ids.Distinct().Select(x => new { Id = x }).ToList();
                db.Open();
                db.Execute("CREATE TABLE #tempIDs(Id int not null primary key);");
                db.Execute("INSERT INTO #tempIDs VALUES(@Id);", idCoverts);
                return db.Query<T>(sql).ToList();
            }
        }
        

        protected List<T> QueryByDapper<T>(string sql , object parameters) where T: class
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                var query = conn.Query<T>(sql, parameters);
                return query.ToList();
            }
        }

        protected int ExecuteByDapper(string sql, object parameters)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Execute(sql, parameters);
                return result;
            }
        }


        protected int ExecuteSPByDapper(string spName, object parameters)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Execute(spName, parameters, commandType: CommandType.StoredProcedure);
                return result;
            }
        }


        protected List<S> ExecuteSPResultByDapper<S>(string spName, object parameters)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Query<S>(spName, parameters, commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
        }



        protected string GenerateColumnEqualWhereQuery(string columnsName) 
        {
            string sql = $@" SELECT * FROM {TableName} 
                             WHERE {columnsName} = @{columnsName} ";

            return sql;
        }


        protected string GenerateColumnInWhereQuery(string columnsName) 
        {
            string sql = $@" SELECT * FROM {TableName} 
                             WHERE {columnsName} in @{columnsName} ";

            return sql;
        }


        private string GetKeyAttributeName()
        {
            foreach(PropertyInfo item in _type.GetProperties())
            {
                var matchAttr = item.GetCustomAttributes<KeyAttribute>(true);

                var matchAttr_notAuto = item.GetCustomAttributes<NoneAutoKey>(true);

                if(matchAttr_notAuto != null && matchAttr_notAuto.Count() > 0)
                    return item.Name;

                if (matchAttr != null && matchAttr.Count() > 0)
                    return item.Name;

                continue;
            }

            //如果沒設定key 改吃 PrimaryKeyColumnName
            return PrimaryKeyColumnName;
        }


        /// <summary>
        /// 分頁
        /// </summary>
        protected PagedList<S> GetPageingEntitiesBySQLScript<S>(string qrySQL, PaginationParameter pager, object param = null) where S : class
        {
            string sortDirection = pager.IsASC ? "asc" : "desc";
            string[] strArr = qrySQL.Split(new string[] { "FROM", "from", "From" }, StringSplitOptions.None);
            
            if (string.IsNullOrWhiteSpace(pager.OrderByColumn))
                pager.OrderByColumn = DefaultSortColumn;

            string str1 = strArr[0] + $",(ROW_NUMBER() OVER (ORDER BY {pager.OrderByColumn} {sortDirection} )) as RowNum ";

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

            return GetPageingEntitiesBySQLScript<S>(qrySQL, countSQL, pager.PageIndex, pager.PageSize, param);
        }


        /// <summary>
        /// 依條件查詢多筆資料
        /// </summary>
        private PagedList<S> GetPageingEntitiesBySQLScript<S>(string sqlScript, string countSQL, int currentPage, int pageSize, object param = null) where S : class
        {
            PagedList<S> pageResult = new PagedList<S>();
            int total = GetScalarBySQLScript<int>(countSQL, param);
            pageResult = new PagedList<S>
            {
                PageIndex = currentPage,
                PageSize = pageSize,
                TotalCount = GetScalarBySQLScript<int>(countSQL, param),
                TotalPage = CalculateTotalPage(total, pageSize)
            };
            pageResult.PageData = GetPageingEntities<S>(sqlScript, currentPage, pageSize, param);
            return pageResult;
        }


        private int CalculateTotalPage(int totalCount , int pageSize)
        {
            int totalPage = ((int)totalCount / pageSize);

            if (totalCount % pageSize == 0)
                return totalPage;

            return totalPage + 1;
        }


        /// <summary>
        /// 依條件查詢第一筆第一個欄位資料
        /// </summary>
        private S GetScalarBySQLScript<S>(string _SQLScript, object param = null)
        {
            using (SqlConnection _SqlConnection = new SqlConnection(ConnectionString))
            {
                return _SqlConnection.ExecuteScalar<S>(_SQLScript, param);
            }
        }


        /// <summary>
        /// 依條件查詢多筆資料(分頁用)
        /// </summary>
        private List<S> GetPageingEntities<S>(string sqlLScript, int currentPage, int pageSize, object param = null)
        {
            int startRowNum = (currentPage <= 1) ? 1 : 1 + (currentPage - 1) * pageSize;
            int endRowNum = (startRowNum - 1) + pageSize;
            string paggingSQL = @"
                        SELECT *
                        FROM ( " + sqlLScript + @" ) as pageData
                        WHERE RowNum >= " + startRowNum + @" AND RowNum <= " + endRowNum;
            
            return GetEntitiesBySQLScript<S>(paggingSQL, param);
        }



        /// <summary>
        /// 依條件查詢多筆資料
        /// </summary>
        private List<S> GetEntitiesBySQLScript<S>(string _SQLScript, object param = null)
        {
            using (SqlConnection _SqlConnection = new SqlConnection(ConnectionString))
            {
                return _SqlConnection.Query<S>(_SQLScript, param).ToList();
            }
        }


    }


}
