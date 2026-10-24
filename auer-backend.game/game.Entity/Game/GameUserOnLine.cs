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
    /// 游戏在线用户
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class GameUserOnLine : BaseEntity
    {
        /// <summary>
        /// 服务器ID
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

        public virtual GameServer GameServer
        {
            get;
            set;
        }

        public virtual UserInfo User
        {
            get;
            set;
        }
    }
}
