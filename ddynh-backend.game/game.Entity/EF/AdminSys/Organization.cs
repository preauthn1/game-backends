using FW.Core.Helper;
using FW.Core.Repository;
using game.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.AdminSys
{
    [Database(DBNameConfig.DBName_AdminSys)]
    public class Organization : BaseEntity
    {

        [Key]
        public int OrgID
        {
            get { return Id; }
            set { Id = value; }
        }

        public string Name { get; set; }

        public string Memo { get; set; }

        public DateTime CreateTime { get; set; }

        public string ShortName { get; set; }


    }
}
