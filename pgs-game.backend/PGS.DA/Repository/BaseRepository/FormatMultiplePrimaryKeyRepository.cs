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
    public abstract class FormatMultiplePrimaryKeyRepository<T> : BaseDapperRepository<T> where T: class
    {     

        public virtual void Create(T model)
        {
            string insertSQL = SQLHelper.GetInsertCommand<T>(TableName);
            base.CreateBySQL(insertSQL, model);
        }

    }

}
