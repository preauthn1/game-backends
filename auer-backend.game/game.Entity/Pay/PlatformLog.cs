using System;
using System.Collections.Generic;

namespace game.Entity.Pay
{

    /// <summary>
    /// 订单
    public class PlatformLog
    {
        /// <summary>
        /// 支付渠道
        /// </summary>
        public string TypeName
        {
            get;
            set;
        }

        /// <summary>
        /// 支付平台
        /// </summary>

        public string GatewayName
        {
            get;
            set;
        }

        /// <summary>
        /// 订单数
        /// </summary>
        ///
        public int TotalCount
        {
            get;
            set;
        }

        /// <summary>
        /// 成功数
        /// </summary>
        ///
        public int SuccessCount
        {
            get;
            set;
        }

        /// <summary>
        /// 成功率
        /// </summary>
        public double SuccessRate
        {
            get;
            set;
        }

        public string TransData { get; set; }

        /// <summary>
        /// 支付金額
        /// </summary>
        [MoneyInt]
        public Int64 TotalAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 充值金幣數
        /// </summary>
        [MoneyInt]
        public Int64 TotalPointAmount
        {
            get;
            set;
        }
    }

}
