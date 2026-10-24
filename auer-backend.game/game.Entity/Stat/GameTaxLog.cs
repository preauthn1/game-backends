using System;

namespace game.Entity.Stat
{
    /// <summary>
    /// 游戏税收记录
    /// </summary>    
    public class GameTaxLog
    {
        /// <summary>
        /// 游戏ID
        /// </summary>
        public int GameID
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

        [MoneyInt]
        public Int64 RobotAmount
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
        /// 流水
        /// </summary>
        [MoneyInt]
        public Int64 FlowAmount
        {
            get;
            set;
        }

        [MoneyInt]
        public Int64 RobotTaxAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 奖池金额
        /// </summary>
        [MoneyInt]
        public Int64 PoolAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTime StatTime
        {
            get;
            set;
        }

        public virtual Game.Game Game
        {
            get;
            set;
        }

    }
}
