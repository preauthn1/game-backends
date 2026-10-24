using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Stat
{
    /// <summary>
    /// 总金币记录
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class SumMoneyLog : BaseEntity
    {
        /// <summary>
        /// 金币
        /// </summary>
        [MoneyInt]
        public Int64 CashAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 保險箱
        /// </summary>
        [MoneyInt]
        public Int64 BankAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 流入
        /// </summary>
        [MoneyInt]
        public Int64 SumIn { get; set; }

        /// <summary>
        /// 流出
        /// </summary>
        [MoneyInt]
        public Int64 SumOut { get; set; }

        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTime StatTime
        {
            get;
            set;
        }

        /// <summary>
        /// 用戶金币
        /// </summary>
        public Int64 UserCashAmount { get; set; }

        /// <summary>
        /// 用戶保險箱
        /// </summary>
        public Int64 UserBankAmount { get; set; }
    }
}
