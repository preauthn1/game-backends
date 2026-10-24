using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Users
{
    public class WinLostLeaderboard
    {
        /// <summary>
        /// 使用者ID
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// 充值数
        /// </summary>
        public long PayAmount { get; set; }
        /// <summary>
        /// 兌換數
        /// </summary>
        public long ConvertAmount { get; set; }
    }
}
