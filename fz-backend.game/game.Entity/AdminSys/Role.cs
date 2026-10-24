using FW.Core.Helper;
using FW.Core.Repository;
using game.Infrastructure;
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
    }
}
