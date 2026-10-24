using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;


namespace FW.Core.Repository
{
    public interface IDatabase
    {
        int ExecuteCommand(string sql, params DbParameter[] parameters);


        int ExecuteStoreProcedureForResult(string sql, params DbParameter[] parameters);


        IEnumerable<TObject> ExecuteSqlQuery<TObject>(string sql, params DbParameter[] parameters);


        IEnumerable<TObject> ExecuteStoreProcedureForDataList<TObject>(string sql, params DbParameter[] parameters);

        DataTable ExecuteStoreProcedureDataTable(string sql, params DbParameter[] parameters);

        DataSet ExecuteStoreProcedureDataSet(string sql, params DbParameter[] parameters);


        DbConnection Connection { get; }
    }


}
