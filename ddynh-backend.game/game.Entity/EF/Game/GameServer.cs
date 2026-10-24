using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    /// <summary>
    /// 游戏服务器信息
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public partial class GameServer : BaseEntity
    {
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
        [Required]
        [StringLength(64)]
        public string ServerName
        {
            get;
            set;
        }
        /// <summary>
        /// 显示名称
        /// </summary>
        [StringLength(64)]
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

        [StringLength(128)]
        /// <summary>
        /// 通知地址
        /// </summary>
        public string NotifyAddr
        {
            get;
            set;
        }

        [StringLength(128)]
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

    }   
        
    /// <summary>
    /// default set
    /// </summary>
    public partial class GameServer
    {
        public GameServer()
        {

            Money = 0;
            LastUpdateTime = DateTime.Now;            
            ShowName = "";
        }
    }
}
