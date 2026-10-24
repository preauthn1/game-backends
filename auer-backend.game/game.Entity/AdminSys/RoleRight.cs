using FW.Core.Helper;
using FW.Core.Repository;
using game.Infrastructure;

namespace game.Entity.AdminSys
{
    [Database(DBNameConfig.DBName_AdminSys)]
    public class RoleRight : BaseEntity
    {
        public int RoleID
        {
            get;
            set;
        }

        public int RightID
        {
            get;
            set;
        }

        public virtual Role Role
        {
            get;
            set;
        }

        public virtual Right Right
        {
            get;
            set;
        }
    }
}
