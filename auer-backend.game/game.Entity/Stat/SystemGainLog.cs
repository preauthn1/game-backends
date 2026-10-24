using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Stat
{
    /// <summary>
    /// 平台收益记录
    /// </summary>    
    public class SystemGainLog
    {
        /// <summary>
        /// 充值金额
        /// </summary>
        [MoneyInt]
        public Int64 ChargeAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 兑换金额
        /// </summary>
        [MoneyInt]
        public Int64 ConvertAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 兑换税收
        /// </summary>
        [MoneyInt]
        public Int64 ConvertTax
        {
            get;
            set;
        }

        /// <summary>
        /// 兑换真实税收
        /// </summary>
        [MoneyInt]
        public Int64 ConvertRealTax
        {
            get;
            set;
        }

        /// <summary>
        /// 用户税收
        /// </summary>
        [MoneyInt]
        public Int64 TaxAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 机器人税收
        /// </summary>
        [MoneyInt]
        public Int64 RobotTaxAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 赠送金额
        /// </summary>
        [MoneyInt]
        public Int64 GiveAmount
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
    }
}
