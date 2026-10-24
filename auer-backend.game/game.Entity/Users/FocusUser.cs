using FW.Core.Helper;
using FW.Core.Repository;

namespace game.Entity.Users
{
    /// <summary>
    /// VIP用户
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class FocusUser : BaseEntity
    {
        public virtual UserInfo User
        {
            get;
            set;
        }
    }
}