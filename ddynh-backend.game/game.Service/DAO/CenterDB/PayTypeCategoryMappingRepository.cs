using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Payment;
using game.Entity.SystemLog;
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
    public class PayTypeCategoryMappingRepository : IDapperRepository<PayTypeCategoryMapping>
    {
        public override string ConnectionString
        {
            get {return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Payment].[PayTypeCategoryMapping]"; }
        }
    }

}
