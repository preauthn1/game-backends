using System;
using static game.Entity.Game.SuperToolTodayUser;

namespace game.Entity.Game
{
    public class SuperToolFishSystemReward
    {
        /// <summary>
        /// ID
        /// </summary>
        public FlagType FlagID { get; set; }
        /// <summary>
        /// 魚ID
        /// </summary>
        public int FishID { get; set; }
        /// <summary>
        /// 魚名稱
        /// </summary>
        public string FishName { get; set; }
        /// <summary>
        /// 放分金额
        /// </summary>
        [MoneyInt]
        public long Amount { get; set; }
        /// <summary>
        /// 放分胜率
        /// </summary>
        public int Rate { get; set; }
        /// <summary>
        /// 完成金額
        /// </summary>
        [MoneyInt]
        public long FinishAmount { get; set; }
        /// <summary>
        /// 是否完成
        /// </summary>
        public int Finish { get; set; }
        /// <summary>
        /// 狀態 1 開始 0 暫停
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 修改人
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 完成時間
        /// </summary>
        public DateTime? FinishTime { get; set; }

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

        private string _strFinish;
        public string StrFinish
        {
            get { return _strFinish; }
            set { _strFinish = Finish == 0 ? "否" : "是"; }
        }
    }
}
