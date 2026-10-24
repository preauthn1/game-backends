
using System;

namespace game.Entity.Game
{
    public class PlatformGameServer
    {
        public int Id
        {
            get;
            set;
        }
        /// <summary>
        /// 游戏ID
        /// </summary>
        public int GameID
        {
            get;
            set;
        }

        /// <summary>
        /// 服务器名
        /// </summary>
        public string ServerName
        {
            get;
            set;
        }
        /// <summary>
        /// 显示名称
        /// </summary>
        public string ShowName
        {
            get;
            set;
        }

        /// <summary>
        /// 客户端访问地址
        /// </summary>        
        public string ClientAddr
        {
            get;
            set;
        }

        /// <summary>
        /// 通知地址
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
        /// 入场金币数
        /// </summary>
        [MoneyInt]
        public Int64 Money
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

        public virtual Game Game
        {
            get;
            set;
        }

        public int PlatformID
        {
            get;
            set;
        }

        public string PlatformName
        {
            get;
            set;
        }
    }
}
