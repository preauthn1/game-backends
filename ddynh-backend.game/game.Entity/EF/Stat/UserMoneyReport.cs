using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Stat
{
    /// <summary>
    /// 用户金币报表
    /// </summary>
    public class UserMoneyReport
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID
        {
            get;
            set;
        }

        /// <summary>
        /// 金币
        /// </summary>
        [MoneyInt]
        public Int64? CashAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 银行
        /// </summary>
        [MoneyInt]
        public Int64? BankAmount
        {
            get;
            set;
        }

        public UserInfo User
        {
            get;
            set;
        }
    }
}
