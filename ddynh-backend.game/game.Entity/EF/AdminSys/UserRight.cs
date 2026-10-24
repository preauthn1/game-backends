using FW.Core.Helper;
using FW.Core.Repository;
using game.Infrastructure;

namespace game.Entity.AdminSys
{
    [Database(DBNameConfig.DBName_AdminSys)]
    public class UserRight : BaseEntity
    {
        public int UserID
        {
            get;
            set;
        }

        public int RightID
        {
            get;
            set;
        }

        public virtual AdminUser User
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
