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
    /// 游戏记录
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class GameLog : BaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID
        {
            get;
            set;
        }

        /// <summary>
        /// 服务器ID
        /// </summary>
        public int ServerID
        {
            get;
            set;
        }

        /// <summary>
        /// 进游戏金币数
        /// </summary>
        [MoneyInt]
        public Int64 LoginMoney
        {
            get;
            set;
        }

        /// <summary>
        /// 金额
        /// </summary>
        [MoneyInt]
        public Int64 Amount
        {
            get;
            set;
        }

        /// <summary>
        /// 税收
        /// </summary>
        [MoneyInt]
        public Int64 TaxAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 赢
        /// </summary>
        public int WinCount
        {
            get;
            set;
        }

        /// <summary>
        /// 输
        /// </summary>
        public int LoseCount
        {
            get;
            set;
        }

        /// <summary>
        /// 平
        /// </summary>
        public int DrawCount
        {
            get;
            set;
        }


        /// <summary>
        /// 游戏时间
        /// </summary>
        public int GameTime
        {
            get;
            set;
        }

        /// <summary>
        /// IP
        /// </summary>
        public string IPAddress
        {
            get;
            set;
        }

        /// <summary>
        /// 硬件唯一码
        /// </summary>        
        public string HDCode
        {
            get;
            set;
        } 

        /// <summary>
        /// 硬件类型
        /// </summary>
        public game.Entity.Users.HDType HDType
        {
            get;
            set;
        }
     
        /// <summary>
        /// 进入房间时间
        /// </summary>
        public DateTime LoginTime
        {
            get;
            set;
        }

        /// <summary>
        /// 登出时间
        /// </summary>
        public DateTime LogoutTime
        {
            get;
            set;
        }


        public virtual Users.UserInfo User
        {
            get;
            set;
        }

        public virtual GameServer GameServer
        {
            get;
            set;
        }
    }
}
