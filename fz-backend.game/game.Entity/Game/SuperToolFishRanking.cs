using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    public class FishRanking
    {
        public List<SuperToolFishRanking> TodayRanking;
        public List<SuperToolFishRanking> TotalRanking;
        public List<SuperToolFishRanking> YestodayRanking;
    }

    public class SuperToolFishRanking
    {
        /// <summary>
        /// 玩家ID    
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// 今日輸贏
        /// </summary>
        [MoneyInt]
        public long TodayUserWinLose { get; set; }
        /// <summary>
        /// 總輸贏
        /// </summary>
        [MoneyInt]
        public long TotalUserWinLose { get; set; }
        /// <summary>
        /// 放分金額
        /// </summary>
        [MoneyInt]
        public long SumAssignAmount { get; set; }
        /// <summary>
        /// 最後上線時間
        /// </summary>
        public DateTime? LoginTime { get; set; }

        private string _strLoginTime;
        public string StrLoginTime
        {
            get { return _strLoginTime; }
            set { _strLoginTime = LoginTime.HasValue ?  LoginTime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "超过两星期未上线"; }
        }
    }

}
