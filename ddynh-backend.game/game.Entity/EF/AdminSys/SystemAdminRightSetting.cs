using FW.Core.Helper;
using FW.Core.Repository;
using game.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.AdminSys
{
    [Database(DBNameConfig.DBName_AdminSys)]
    public class SystemAdminRightSetting : BaseEntity
    {

        public int RightID { get; set; }


    }
}
