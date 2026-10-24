using FW.Core.Repository;
using game.Entity.AdminSys;
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
    public class SystemAdminRightSettingRepository : IDapperRepository<SystemAdminRightSetting>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_AdminSys); }
        }

        public override string TableName
        {
            get { return "[Admin].[SystemAdminRightSetting]"; }
        }

    }


}
