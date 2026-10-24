using System.Collections.Generic;
using System.Data.Common;

namespace Repository.Models.Interface
{
    public interface IDatabase
    {
        int ExecuteStoreProcedureForResult(string sql, params DbParameter[] parameters);

        IEnumerable<TObject> ExecuteSqlQuery<TObject>(string sql, params DbParameter[] parameters);

        IEnumerable<TObject> ExecuteStoreProcedureForDataList<TObject>(string storedProcedureName, params DbParameter[] parameters);

        int ExecuteCommand(string sql, params DbParameter[] parameters);
    }
}
