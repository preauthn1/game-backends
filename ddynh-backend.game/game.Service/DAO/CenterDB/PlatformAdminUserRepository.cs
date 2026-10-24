using Dapper;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Area;
using game.Entity.Game;
using game.Entity.Payment;
using game.Entity.SystemLog;
using game.Infrastructure;
using game.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using game.Infrastructure.Extensions;
using game.Entity.ViewModels;

namespace game.Service.DAO
{
    public class PlatformAdminUserRepository : IDapperRepository<PlatformAdminUserEntity>
    {
        public override string ConnectionString
        {
            get {return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Area].[PlatformAdminUser]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }
        

        public List<PlatformAdminUserEntity> GetAvailablePlatformsByAdminUserID(int adminID)
        {
            var sql = GenerateColumnEqualWhereQuery("AdminID");
            var query = QueryByDapper<PlatformAdminUserEntity>(sql , new { AdminID  = adminID });
            return query.ToList();
        }


    }


}
