using FW.Core.Helper;
using FW.Core.Repository;
using game.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.AdminSys
{
    [Database(DBNameConfig.DBName_AdminSys)]
    public class Role : BaseEntity
    {
        public int RoleID
        {
            get { return Id; }
            set { Id = value; }
        }

        [Required]
        [StringLength(32)]
        public string RoleName
        {
            get;
            set;
        }

        public string Memo { get; set; }

        public int OrgID { get; set; }

        public bool IsSystemDefault { get; set; }

        public bool IsTemplateForCopy { get; set; }

        public DateTime CreateTime { get; set; }

        public int? ParentTemplateID { get; set; }

    }
}
