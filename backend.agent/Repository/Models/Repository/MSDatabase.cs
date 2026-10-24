using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using Repository.Models.Interface;

namespace Repository.Models.Repository
{
    public class MSDatabase : IDatabase
    {
        public abstract class BaseEntity
        {
            /// <summary>
            /// 主键Id
            /// </summary>
            [Key]
            public int Id { get; set; }
        }
        private static List<Type> _simpleTypes = new List<Type>() {
            typeof(Int32),
            typeof(Int64),
            typeof(Int16),
            typeof(byte),
            typeof(bool),
            typeof(string),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(DateTime),
            typeof(Guid)
            };
        private DbContext _context
        {
            get;
            set;
        }

        public MSDatabase(string name)
        {
            var connStr = ConfigurationManager.ConnectionStrings[name].ConnectionString;
            this._context = new AdminSysContext(connStr);
        }

        public IEnumerable<TObject> ExecuteSqlQuery<TObject>(string sql, params DbParameter[] parameters)
        {
            return _context.Database.SqlQuery<TObject>(sql, parameters);
        }

        public int ExecuteCommand(string sql, params DbParameter[] parameters)
        {
            try
            {
                return _context.Database.ExecuteSqlCommand(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<TObject> ExecuteStoreProcedureForDataList<TObject>(string sql, params DbParameter[] parameters)
        {
            try
            {
                var comm = _context.Database.Connection.CreateCommand();
                comm.CommandType = CommandType.StoredProcedure;
                comm.CommandText = sql;

                DbParameter paramReturn = comm.CreateParameter();
                paramReturn.ParameterName = "@return";
                paramReturn.Direction = ParameterDirection.ReturnValue;
                paramReturn.DbType = DbType.Int32;
                comm.Parameters.Add(paramReturn);

                Array.ForEach(parameters, p =>
                {
                    comm.Parameters.Add(p);
                });

                if (comm.Connection.State != ConnectionState.Open)
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
                catch (Exception ex)
                {
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
            catch (Exception ex)
            {
                throw;
            }
        }

        public int ExecuteStoreProcedureForResult(string sql, params DbParameter[] parameters)
        {
            try
            {
                var comm = _context.Database.Connection.CreateCommand();
                comm.CommandType = CommandType.StoredProcedure;
                comm.CommandText = sql;

                DbParameter paramReturn = comm.CreateParameter();
                paramReturn.ParameterName = "@return";
                paramReturn.Direction = ParameterDirection.ReturnValue;
                paramReturn.DbType = DbType.Int32;
                comm.Parameters.Add(paramReturn);

                Array.ForEach(parameters, p =>
                {
                    comm.Parameters.Add(p);
                });

                if (comm.Connection.State != ConnectionState.Open)
                {
                    comm.Connection.Open();
                }

                try
                {
                    var ret = comm.ExecuteNonQuery();
                }
                catch (Exception ex)
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
            catch (Exception ex)
            {
                throw;
            }
        }


        private object getTobjectFromDataRow(DataRow dr, Type objType, string prefix)
        {
            if (IsSimpleType(objType))
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

                if (typeof(BaseEntity).IsAssignableFrom(ptype) || !IsSimpleType(ptype))
                {
                    p.SetValue(obj, getTobjectFromDataRow(dr, ptype, prefix + p.Name + "."));
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
                        if (typeof(DBNull) == dr[columnName].GetType())
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
            return obj;
        }

        public static bool IsSimpleType(Type type)
        {
            return _simpleTypes.Where(t => t == type).FirstOrDefault() != null || type.IsEnum;
        }
    }
}
