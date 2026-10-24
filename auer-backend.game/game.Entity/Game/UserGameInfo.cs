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
    /// 用户游戏信息
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class UserGameInfo : BaseEntity
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
        /// 游戏ID
        /// </summary>
        public int GameID
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
        /// 积分
        /// </summary>
        public Int64 Score
        {
            get;
            set;
        }

        /// <summary>
        /// 游戏时间(单位秒)
        /// </summary>
        public Int64 GameTime
        {
            get;
            set;
        }

        public virtual game.Entity.Users.UserInfo UserInfo
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
}
