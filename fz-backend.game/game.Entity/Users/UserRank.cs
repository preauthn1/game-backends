
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

    /// <summary>
    /// 1 日赛排行, 2累积赛排行
    /// </summary>
    public enum RankType : byte
    {
        /// <summary>
        /// 日賽排行
        /// </summary>
        [EnumDescription("日赛排行")]
        DayEvent = 1,
        /// <summary>
        /// 累积赛排行
        /// </summary>
        [EnumDescription("累积赛排行")]
        Cumulative = 2,
    }


    /// <summary>
    /// 是否完成結算 0未完成, 1完成
    /// </summary>
    public enum LastCalcRankType : byte
    {
        /// <summary>
        /// 基本信息
        /// </summary>
        [EnumDescription("未完成")]
        undone = 0,
        /// <summary>
        /// 用户名
        /// </summary>
        [EnumDescription("完成")]
        done = 1,
    }
}
