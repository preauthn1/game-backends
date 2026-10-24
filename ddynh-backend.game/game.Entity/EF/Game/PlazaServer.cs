using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    /// <summary>
    /// 广场服务器
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class PlazaServer : BaseEntity
    {
        /// <summary>
        /// 客户端地址
        /// </summary>
        public string ClientAddr
        {
            get;
            set;
        }

        /// <summary>
        /// 通知的地址
        /// </summary>
        public string NotifyAddr
        {
            get;
            set;
        }

        /// <summary>
        /// http地址
        /// </summary>
        public string HttpAddr
        {
            get;
            set;
        }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdateTime
        {
            get;
            set;
        }

    }
}
