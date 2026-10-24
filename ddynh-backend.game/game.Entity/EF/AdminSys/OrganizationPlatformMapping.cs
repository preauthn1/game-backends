using FW.Core.Helper;
using FW.Core.Repository;
using game.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.AdminSys
{
    [Database(DBNameConfig.DBName_AdminSys)]
    public class OrganizationPlatformMapping : BaseEntity
    {

        public int OrgID { get; set; }


        public int PlatformID { get; set; }



    }
}
