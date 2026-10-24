using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Data.Common;
using System.Data;
using FW.Core.Helper;
using FW.Core.Helper.Reflection;


namespace FW.Core.Repository.EF
{
    public class EFDatabase : IDatabase
    {
        private DbContext _dbContext;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbContext"></param>
        public EFDatabase(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<TObject> ExecuteSqlQuery<TObject>(string sql, params System.Data.Common.DbParameter[] parameters)
        {
            try
            {
                return _dbContext.Database.SqlQuery<TObject>(sql, parameters);
            }
            catch (System.Exception ex)
            {
                throw new RepositoryException("执行Sql查询出错," + ex.Message, ex);
            }
        }

        public int ExecuteCommand(string sql, params System.Data.Common.DbParameter[] parameters)
        {
            try
            {
                return _dbContext.Database.ExecuteSqlCommand(sql, parameters);
            }
            catch (System.Exception ex)
            {
                throw new RepositoryException("执行数据库操作出错," + ex.Message, ex);
            }
        }

        public IEnumerable<TObject> ExecuteStoreProcedureWithIdsForDataList<TObject>(string sql, params System.Data.Common.DbParameter[] parameters)
            where TObject : BaseEntity
        {
            var ids = ExecuteStoreProcedureForDataList<int>(sql, parameters);

            var rets = new List<TObject>();
            if (ids != null && ids.Count() > 0)
            {
                ids.ToList().ForEach(id => {
                     rets.Add(_dbContext.Set<TObject>().Where(t => t.Id == id).FirstOrDefault());
                });
            }

            return rets;
        }

        public IEnumerable<TObject> ExecuteStoreProcedureForDataList<TObject>(string sql, params System.Data.Common.DbParameter[] parameters)
        {
            string debugMsg = string.Empty;

            try
            {
                var comm = _dbContext.Database.Connection.CreateCommand();
                comm.CommandType = System.Data.CommandType.StoredProcedure;
                comm.CommandText = sql;

                Array.ForEach(parameters, p =>
                {
                    comm.Parameters.Add(p);
                });

                if (comm.Connection.State != System.Data.ConnectionState.Open)
                {
                    comm.Connection.Open();
                }

                DbDataReader reader = null;
                try
                {
                    reader = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    List<TObject> rets = new List<TObject>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        rets.Add((TObject)getTobjectFromDataRow(dr, typeof(TObject), ""));
                    }

                    return rets;

                }
                catch (System.Exception ex)
                {
                    debugMsg = ex.Message;
                    throw ex;
                }
                finally
                {
                    reader.Close();
                    if (comm.Connection.State != ConnectionState.Closed)
                    {
                        try
                        {
                            comm.Connection.Close();
                        }
                        catch { }
                    }
                }

            }
            catch (System.Exception ex)
            {
                string spName = sql;
                throw new RepositoryException($"执行存储过程出错 (EF), {spName} , {debugMsg} , {ex.Message}", ex);
            }
        }

        public DataTable ExecuteStoreProcedureDataTable(string sql, params DbParameter[] parameters)
        {
            try
            {
                var comm = _dbContext.Database.Connection.CreateCommand();
                comm.CommandType = System.Data.CommandType.StoredProcedure;
                comm.CommandText = sql;

                Array.ForEach(parameters, p =>
                {
                    comm.Parameters.Add(p);
                });

                if (comm.Connection.State != System.Data.ConnectionState.Open)
                {
                    comm.Connection.Open();
                }

                DbDataReader reader = null;
                try
                {
                    reader = comm.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    return dt;
                }
                finally
                {
                    reader.Close();
                    if (comm.Connection.State != ConnectionState.Closed)
                    {
                        try
                        {
                            comm.Connection.Close();
                        }
                        catch { }
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new RepositoryException("执行存储过程出错," + ex.Message, ex);
            }
        }

        public DataSet ExecuteStoreProcedureDataSet(string sql, params DbParameter[] parameters)
        {
            try
            {
                var comm = _dbContext.Database.Connection.CreateCommand();
                comm.CommandType = System.Data.CommandType.StoredProcedure;
                comm.CommandText = sql;

                Array.ForEach(parameters, p =>
                {
                    comm.Parameters.Add(p);
                });

                if (comm.Connection.State != System.Data.ConnectionState.Open)
                {
                    comm.Connection.Open();
                }

                DbDataReader reader = null;
                try
                {
                    reader = comm.ExecuteReader();
                    DataSet ds = new DataSet();
                    ds.Load(reader, LoadOption.PreserveChanges, new string[]{"fw0", "fw1", "fw2" });
                    return ds;
                }
                finally
                {
                    reader.Close();
                    if (comm.Connection.State != ConnectionState.Closed)
                    {
                        try
                        {
                            comm.Connection.Close();
                        }
                        catch { }
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new RepositoryException("执行存储过程出错," + ex.Message, ex);
            }
        }

        private object getTobjectFromDataRow(DataRow dr, Type objType, string prefix)
        {
            if (TypeHelper.IsSimpleType(objType))
            {
                return Convert.ChangeType(dr[0], objType);
            }

            Object obj = Activator.CreateInstance(objType);

            var ps = objType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            foreach (var p in ps)
            {
                var ptype = p.PropertyType;
                if (p.PropertyType.IsGenericType && typeof(Nullable<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition()))
                {
                    ptype = p.PropertyType.GetGenericArguments()[0];
                }

                if (typeof(BaseEntity).IsAssignableFrom(ptype) || !TypeHelper.IsSimpleType(ptype))
                {
                    p.SetValue(obj, getTobjectFromDataRow(dr, ptype,prefix +  p.Name + "."));
                }
                else
                {
                    string columnName = String.Format("{0}{1}", prefix, p.Name);
                    if (!dr.Table.Columns.Contains(columnName))
                    {
                        columnName = p.Name;
                    }

                    if (dr.Table.Columns.Contains(columnName))
                    {
                        if( typeof(DBNull) == dr[columnName].GetType())
                        {
                            continue;
                        }

                        if (!ptype.IsEnum)
                        {
                            p.SetValue(obj, Convert.ChangeType(dr[columnName], ptype));
                        }
                        else
                        {
                            var evalue = Convert.ToByte(dr[columnName]);

                            var ename = Enum.GetName(ptype, evalue);
                            if (!String.IsNullOrWhiteSpace(ename))
                            {
                                var enm = Enum.Parse(ptype, ename);

                                p.SetValue(obj, enm);
                            }
                        }

                    }

                }
            }

            //var fs = objType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            //foreach(var f in fs)
            //{
            //    var ftype = f.FieldType;

            //    if (typeof(BaseEntity).IsAssignableFrom(ftype))
            //    {
            //        f.SetValue(obj, getTobjectFromDataRow(dr, ftype, prefix +  f.Name + "."));
            //    }
            //    else
            //    {
            //        string columnName = String.Format("{0}{1}", prefix, f.Name);
            //        if (!dr.Table.Columns.Contains(columnName))
            //        {
            //            columnName = f.Name;
            //        }

            //        if (dr.Table.Columns.Contains(columnName))
            //        {
            //            if (!ftype.IsEnum)
            //            {
            //                f.SetValue(obj, Convert.ChangeType(dr[columnName], ftype));
            //            }
            //            else
            //            {
            //                f.SetValue(obj, ftype.GetEnumValues().GetValue(Convert.ToByte(dr[columnName])));
            //            }

            //        }

            //    }
            //}

            return obj;
        }

        public int ExecuteStoreProcedureForResult(string sql, params System.Data.Common.DbParameter[] parameters)
        {
            try
            {
                var comm = _dbContext.Database.Connection.CreateCommand();
                comm.CommandType = System.Data.CommandType.StoredProcedure;
                comm.CommandText = sql;

                DbParameter paramReturn = comm.CreateParameter();
                paramReturn.ParameterName = "@return";
                paramReturn.Direction = System.Data.ParameterDirection.ReturnValue;
                paramReturn.DbType = System.Data.DbType.Int32;
                comm.Parameters.Add(paramReturn);

                Array.ForEach(parameters, p =>
                {
                    comm.Parameters.Add(p);
                });

                if (comm.Connection.State != System.Data.ConnectionState.Open)
                {
                    comm.Connection.Open();
                }

                try
                {
                    var ret = comm.ExecuteNonQuery();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (comm.Connection.State != ConnectionState.Closed)
                    {
                        try
                        {
                            comm.Connection.Close();
                        }
                        catch { }
                    }
                }
                return Convert.ToInt32(comm.Parameters["@return"].Value);
            }
            catch (System.Exception ex)
            {
                throw new RepositoryException("执行存储过程出错," + ex.Message, ex);
            }
        }

        public System.Data.Common.DbConnection Connection
        {
            get { return _dbContext.Database.Connection; }
        }





    }
}
