using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Money
{
    /// <summary>
    /// 现金
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class Cash : BaseEntity
    {
        /// <summary>
        /// 金额
        /// </summary>
        [MoneyInt]
        public Int64 Amount
        {
            get;
            set;
        }
        public long CouponAmount
        {
            get;
            set;
        }
        public virtual UserInfo User
        {
            get;
            set;
        }
    }
}
