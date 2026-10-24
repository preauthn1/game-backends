using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    /// <summary>
    /// 广场在线玩家
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class HallUserOnLine : BaseEntity
    {
        /// <summary>
        /// 广场服务器编号
        /// </summary>
        public int ServerID
        {
            get;
            set;
        }

        /// <summary>
        /// 硬件类型
        /// </summary>
        public HDType HDType
        {
            get;
            set;
        }

       
        public virtual UserInfo User
        {
            get;
            set;
        }

        public virtual PlazaServer PlazaServer
        {
            get;
            set;
        }

    }
}
