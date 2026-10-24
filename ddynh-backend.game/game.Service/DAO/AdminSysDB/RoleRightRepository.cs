using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Game;
using game.Entity.RawEntity;
using game.Infrastructure;
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
    public class RoleRightRepository : IDapperRepository<RoleRightEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_AdminSys); }
        }

        public override string TableName
        {
            get { return "[Admin].[RoleRight]"; }
        }

     
        public List<RoleRightEntity> GetByRightID(List<int> rightIDs)
        {
            string sql = GenerateColumnInWhereQuery("RightID");
            var query = QueryByDapper<RoleRightEntity>(sql, new { RightID = rightIDs });
            return query;
        }

    }


}
