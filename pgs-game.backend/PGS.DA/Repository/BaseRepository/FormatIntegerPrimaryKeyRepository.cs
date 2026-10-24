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
using target.Core.Helper;
using target.Models.Models;
using target.Models.Paging;
using target.Utility;
using target.Utility.Extensions;

namespace target.Core.Repository
{
    public abstract class FormatIntegerPrimaryKeyRepository<T> : BaseDapperRepository<T> where T: class
    {     

        public virtual int Create(T model)
        {
            string insertSQL = SQLHelper.GetInsertCommand<T>(TableName);
            insertSQL += " SELECT CAST(SCOPE_IDENTITY() as int) ";

            using (var conn = InstanceSQLConn())
            {
                var resultId = conn.Query<int?>(insertSQL, model);

                if (resultId.IsEmptyOrNull())
                    return 0;

                var myValue = resultId.FirstOrDefault();
                return myValue.HasValue ? myValue.Value : 0;
            }
        }



        public bool IsExist(int id)
        {
            string sql = $"SELECT {PrimaryKeyColumnName} FROM {TableName} WITH (NOLOCK) WHERE {PrimaryKeyColumnName} = @id";

            using (var conn = InstanceSQLConn())
            {
                var resultId = conn.Query<int>(sql, new { id  = id }).FirstOrDefault();

                if (resultId != 0)
                    return true;

                return false;
            }
        }

        public bool IsAllExist(List<int> ids)
        {
            ids = ids.Distinct().ToList();

            string sql = $"SELECT count(*) FROM {TableName} WITH (NOLOCK) WHERE {PrimaryKeyColumnName} in @ids";

            using (var conn = InstanceSQLConn())
            {
                var resultCount = conn.Query<int>(sql, new { ids = ids }).FirstOrDefault();

                if (resultCount == ids.Count())
                    return true;

                return false;
            }
        }

       
        public void IsAllExistOrException(List<int> ids)
        {
            if (ids.IsEmptyOrNull())
                return;

            bool isAllExist = IsAllExist(ids);

            if(!isAllExist)
            {
                throw new TerproException(ApiErrorCode.NotFoundData);
            }
        }

        public void Delete(int id)
        {
            string deleteSql = $"DELETE {TableName} WHERE {PrimaryKeyColumnName} = @id ";

            using (var conn = InstanceSQLConn())
            {
                conn.Execute(deleteSql, new { id = id });
            }
        }


        public void Delete(List<int> ids)
        {
            string deleteSql = $"DELETE {TableName} WHERE {PrimaryKeyColumnName} in @ids ";

            using (var conn = InstanceSQLConn())
            {
                conn.Execute(deleteSql, new { ids = ids });
            }
        }


        public T GetById(int id, bool isLockQuery = true)
        {
            string noLock = isLockQuery ? "" : " WITH (NOLOCK) ";
            string PKColumnName = GetKeyAttributeName();
            string sql = $@"SELECT * FROM {TableName} {noLock} 
                            WHERE {PKColumnName} = @id ";

            return QueryByDapper<T>(sql , new  { id }).FirstOrDefault();
        }

        public T GetByIdOrThrowException(int id , ApiErrorCode error = ApiErrorCode.NotFoundData)
        {
            T query = GetById(id);

            if (query == null)
            {
                string debugMsg = (SettingHelper.IsDev ? TableName + "、" : "" ) + id.ToString();
                throw new TerproException(error, new ErrorDetailModel(debugMsg));
            }

            return query;           
        }


        public List<T> GetByIds(List<int> ids , string selectColumns = "*")
        {
            ids = ids.Distinct().ToList();

            if (ids == null || ids.Count() == 0)
                return new List<T>();

            if (ids.Count() >= 2000)
                return GetByLargeAmountIDs(ids.Select(x=> (long)x).ToList() , selectColumns);

            string PKColumnName = GetKeyAttributeName();
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                            WHERE {PKColumnName} in @ids ";

            return QueryByDapper<T>(sql , new { ids });
        }


    
        protected List<MyType> ExecuteQueryWithLargeIDs<MyType>(string sql, List<int> ids , object myParam)
        {
            using (var db = InstanceSQLConn())
            {
                var idCoverts = ids.Distinct().Select(x => new { Id = x }).ToList();
                db.Open();
                db.Execute("CREATE TABLE #tempIDs(Id bigint not null primary key);");
                db.Execute("INSERT INTO #tempIDs VALUES(@Id);", idCoverts);
                return db.Query<MyType>(sql , myParam).ToList();
            }
        }


    }

}
