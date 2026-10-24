using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    public class SuperToolTodayRanking
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// 今日遊戲總輸贏
        /// </summary>
        public long TodayUserWinLose { get; set; }
        /// <summary>
        /// 今日局數
        /// </summary>
        public int TodayAll { get; set; }
        /// <summary>
        /// 今日勝局
        /// </summary>
        public int TodayWin { get; set; }
        /// <summary>
        /// 總勝率
        /// </summary>
        public int TotalWinRate { get; set; }
        /// <summary>
        /// 今日勝率
        /// </summary>
        public int TodayWinRate { get; set; }
        /// <summary>
        /// 上線時間
        /// </summary>
        public DateTime LoginTime { get; set; }
        /// <summary>
        /// 下線時間
        /// </summary>
        public DateTime LogoutTime { get; set; }
        /// <summary>
        /// 監控UserID
        /// </summary>
        public int VIPUserID { get; set; }
        /// <summary>
        /// 是否在線
        /// </summary>
        public int Online { get; set; }

        private string _strOnline;
        public string StrOnline
        {
            get { return _strOnline; }
            set { _strOnline = Online == 1 ? "在线" : "离线"; }
        }

        private string _strLoginTime;
        public string StrLoginTime
        {
            get { return _strLoginTime; }
            set { _strLoginTime = LoginTime.ToString("yyyy/MM/dd HH:mm:ss"); }
        }

        private string _strLogoutTime;
        public string StrLogoutTime
        {
            get { return _strLogoutTime; }
            set { _strLogoutTime = LogoutTime.ToString("yyyy/MM/dd HH:mm:ss"); }
        }

    }
}
