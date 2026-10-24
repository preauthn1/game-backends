
using static game.Entity.Game.SuperToolTodayUser;

namespace game.Entity.Game
{
    public class SuperToolFishSystemRecord
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
        public string Name { get; set; }
        /// <summary>
        /// 一般金额
        /// </summary>
        [MoneyInt]
        public long Amount { get; set; }
        /// <summary>
        /// 放分金额
        /// </summary>
        [MoneyInt]
        public long RewardAmount { get; set; }
    }
}
