using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace game.Infrastructure
{
    public static class DBHelper
    {
        public static SqlParameter GetProcedureErrorMsg()
        { 
            string message="";
            SqlParameter p = new SqlParameter("@chvErrMsg", message);
            p.Direction = ParameterDirection.Output;
            p.SqlDbType = SqlDbType.VarChar;
            p.Size = 128;
            return p;
        }
    }
}
