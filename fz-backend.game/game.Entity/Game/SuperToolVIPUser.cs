using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    public class SuperToolVIPUser
    {

        /// <summary>
        /// 玩家ID
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// 放分金额
        /// </summary>
        [MoneyInt]
        public long AssignAmount { get; set; }
        /// <summary>
        /// 放分胜率
        /// </summary>
        public int AssignRate { get; set; }
        /// <summary>
        /// 完成金額
        /// </summary>
        [MoneyInt]
        public long CompleteAmount { get; set; }
        /// <summary>
        /// 是否完成
        /// </summary>
        public int Complete { get; set; }
        /// <summary>
        /// 狀態 1 開始 0 暫停
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 修改人
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 修改時間
        /// </summary>
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 完成時間
        /// </summary>
        public DateTime? FinishTime { get; set; }

        private string _strUpdateTime;
        public string StrUpdateTime
        {
            get { return _strUpdateTime; }
            set { _strUpdateTime = UpdateTime.ToString("yyyy/MM/dd HH:mm:ss"); }
        }

        private string _strCreateTime;
        public string StrCreateTime
        {
            get { return _strCreateTime; }
            set { _strCreateTime = CreateTime.ToString("yyyy/MM/dd HH:mm:ss"); }
        }

        private string _strFinishTime;
        public string StrFinishTime
        {
            get { return _strFinishTime; }
            set { _strFinishTime = FinishTime.HasValue ? FinishTime.Value.ToString("yyyy/MM/dd HH:mm:ss") : "-"; }
        }

        /// <summary>
        /// 總局數
        /// </summary>
        public int TotalAll { get; set; }
        /// <summary>
        /// 總輸贏
        /// </summary>
        [MoneyInt]
        public long TotalUserWinLose { get; set; }
        /// <summary>
        /// 今日輸贏
        /// </summary>
        public long TodayUserWinLose { get; set; }

        private string _strComplete;
        public string StrComplete
        {
            get { return _strComplete; }
            set { _strComplete = Complete == 0 ? "否" : "是"; }
        }
    }
}
