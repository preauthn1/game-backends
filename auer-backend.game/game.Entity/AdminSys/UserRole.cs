using FW.Core.Helper;
using FW.Core.Repository;
using game.Infrastructure;

namespace game.Entity.AdminSys
{
    [Database(DBNameConfig.DBName_AdminSys)]
    public class UserRole : BaseEntity
    {
        public int UserID
        {
            get;
            set;
        }

        public int RoleID
        {
            get;
            set;
        }

        public virtual AdminUser User
        {
            get;
            set;
        }

        public virtual Role Role
        {
            get;
            set;
        }

    }
}
