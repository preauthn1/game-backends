
namespace game.Entity.Users
{
    public class EventBonus
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
        /// 充值到達金額
        /// </summary>
        [MoneyInt]
        public long OrderReach { get; set; }

        /// <summary>
        /// 免費次數
        /// </summary>
        public int FreeTicket { get; set; }
    }
}
