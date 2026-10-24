
using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Users
{
    public class UserRank
    {
        /// <summary>
        /// 活动名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 1 日赛排行, 2累积赛排行
        /// </summary>
        public RankType Type { get; set; }

        /// <summary>
        /// 排行
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 分数
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 是否完成结算
        /// </summary>
        public LastCalcRankType LastCalcRank { get; set; }

        /// <summary>
        /// 排行时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 奖励金额
        /// </summary>
        public int Reward { get; set; }
    }

}
