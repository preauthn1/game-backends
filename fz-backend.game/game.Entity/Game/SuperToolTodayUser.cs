using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Game
{
    public class SuperToolTodayUser
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 玩家ID
        /// </summary>

        public FlagType FlagID { get; set; }
        /// <summary>
        /// 總充值
        /// </summary>
        public long TotalSumCharge { get; set; }
        /// <summary>
        /// 總兌換
        /// </summary>
        public long TotalSumConvert { get; set; }
        /// <summary>
        /// 總輸贏
        /// </summary>
        [MoneyInt]
        public long TotalUserWinLose { get; set; }
        /// <summary>
        /// 今日充值
        /// </summary>
        public long TodaySumCharge { get; set; }
        /// <summary>
        /// 今日兌換
        /// </summary>
        public long TodaySumConvert { get; set; }
        /// <summary>
        /// 今日總輸贏
        /// </summary>
        public long TodayUserWinLose { get; set; }
        /// <summary>
        /// 局數
        /// </summary>
        public int TotalAll { get; set; }
        /// <summary>
        /// 總勝局
        /// </summary>
        public int TotalWinCount { get; set; }
        /// <summary>
        /// 總負局
        /// </summary>
        public int TotalLoseCount { get; set; }
        /// <summary>
        /// 總勝率
        /// </summary>
        public int TotalWinRate { get; set; }
        /// <summary>
        /// 今日局數
        /// </summary>
        public int TodayAll { get; set; }
        /// <summary>
        /// 今日勝局
        /// </summary>
        public int TodayWin { get; set; }
        /// <summary>
        /// 今日勝率
        /// </summary>
        public int TodayWinRate { get; set; }
        /// <summary>
        /// 是否在線
        /// </summary>
        public int Online { get; set; }
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
        /// 用戶金幣
        /// </summary>
        [MoneyInt]
        public long UserCash { get; set; }

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


        /// <summary>
        /// 用户类型
        /// </summary>
        public enum FlagType : int
        {
            [EnumDescription("初级场")]
            level1 = 1,
            [EnumDescription("中级场")]
            level2 = 2,
            [EnumDescription("高级场")]
            level3 = 3,
            [EnumDescription("富豪场")]
            level4 = 4,
            [EnumDescription("皇家场")]
            level5 = 5
        }
    }
}
