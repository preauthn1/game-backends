using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{
    public class UserGameInfo
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
        /// 游戏名称
        /// </summary>
        public string GameName
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

        public int LastWinLose
        {
            get;
            set;
        }
    }
}
