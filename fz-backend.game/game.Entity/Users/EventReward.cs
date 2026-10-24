
namespace game.Entity.Users
{
    public class EventReward
    {
        /// <summary>
        /// 自動編號
        /// </summary>
        public int? ID { get; set; }

        /// <summary>
        /// 場次ID
        /// </summary>
        public int ContestID { get; set; }

        /// <summary>
        /// 排名
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 贈送金幣
        /// </summary>
        [MoneyInt]
        public long Money { get; set; }

        /// <summary>
        /// 日賽(1)或週賽(2)
        /// </summary>
        public byte Type { get; set; }
    }
}
