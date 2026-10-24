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
    public abstract class FormatStringPrimaryKeyRepository<T> : BaseDapperRepository<T> where T: class
    {     

        /// <summary>
        /// 如果PK欄位是char型別，指定不適當的型別 查詢速度可能會有差距
        /// </summary>
        protected virtual DbType PK_Type
        {
            get
            {
                return DbType.String;  //其他可能: DbType.AnsiString (如果是 char)
            }
        }

        /// <summary>
        /// 如果PK欄位有固定長度(e.g. 32) 指定的話 可能可以增加查詢速度
        /// </summary>
        protected virtual int? PK_Text_Length
        {
            get
            {
                return null;
            }
        }


        public virtual void Create(T model)
        {
            string insertSQL = SQLHelper.GetInsertCommand<T>(TableName);

            CreateBySQL(insertSQL, model);
        }


        public bool IsExist(string id)
        {
            string sql = $"SELECT {PrimaryKeyColumnName} FROM {TableName} WITH (NOLOCK) WHERE {PrimaryKeyColumnName} = @id";
            var param = InstancePrimaryKeyParameterObject(id);

            using (var conn = InstanceSQLConn())
            {
                var resultId = conn.Query<int>(sql, param).FirstOrDefault();

                if (resultId != 0)
                    return true;

                return false;
            }
        }

        public bool IsAllExist(List<string> ids)
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


        public void IsAllExistOrException(List<string> ids)
        {
            if (ids.IsEmptyOrNull())
                return;

            bool isAllExist = IsAllExist(ids);

            if(!isAllExist)
            {
                throw new TerproException(ApiErrorCode.NotFoundData);
            }
        }


        public void Delete(string id)
        {
            string deleteSql = $"DELETE {TableName} WHERE {PrimaryKeyColumnName} = @id ";
            var param = InstancePrimaryKeyParameterObject(id);

            using (var conn = InstanceSQLConn())
            {
                conn.Execute(deleteSql, param);
            }
        }


        public void Delete(List<string> ids)
        {
            string deleteSql = $"DELETE {TableName} WHERE {PrimaryKeyColumnName} in @ids ";

            using (var conn = InstanceSQLConn())
            {
                conn.Execute(deleteSql, new { ids = ids });
            }
        }


        public T GetById(string id , bool isLockQuery = true)
        {
            string noLock = isLockQuery ? "" : "WITH (NOLOCK) ";
            string PKColumnName = GetKeyAttributeName();
            string sql = $@"SELECT * FROM {TableName} {noLock}
                            WHERE {PKColumnName} = @id ";

            var param = InstancePrimaryKeyParameterObject(id);
            return QueryByDapper<T>(sql , param).FirstOrDefault();
        }

        public T GetByIdWithNolock(string id)
        {
            string PKColumnName = GetKeyAttributeName();
            string sql = $@"SELECT * FROM {TableName} WITH (NOLOCK)
                            WHERE {PKColumnName} = @id ";

            var param = InstancePrimaryKeyParameterObject(id);
            return QueryByDapper<T>(sql, param).FirstOrDefault();
        }


        private DynamicParameters InstancePrimaryKeyParameterObject(string value)
        {
            var param = new DynamicParameters();

            //如果是資料量很大的table 根據參數型別指定差異 會有速度上的落差(可能會有案例是差到5秒以上)
            param.Add("@id", value, dbType: PK_Type, size: PK_Text_Length);

            return param;
        }


    }

}
