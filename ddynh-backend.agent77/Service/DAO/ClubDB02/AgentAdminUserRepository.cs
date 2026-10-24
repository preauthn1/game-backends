using FW.Core.Helper;
using FW.Core.Repository;
using game.Infrastructure;
using Repository.DTOModels;
using Repository.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace game.Service.DAO
{
    public class AgentAdminUserRepository : IDapperRepository<AgentAdminUserEFModel>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_Club02); }
        }

        public override string TableName
        {
            get { return "[dbo].[AgentAdminUser]"; }
        }
        
        public override string PrimaryKeyColumnName
        {
            get { return "ID"; }
        }


        public AgentAdminUserEFModel GetByAccount(string account)
        {
            string sql = GenerateColumnEqualWhereQuery("Account");

            var query = QueryByDapper<AgentAdminUserEFModel>(sql, new { Account = account });
            return query.FirstOrDefault();
        }

    }


}
