using FW.Core.Helper;

namespace game.Entity.Pay
{

    public class PayGateway : BaseEntity
    {
        /// <summary>
        /// 第三方厂商
        /// </summary>
        public int GWID
        {
            get;
            set;
        }

        /// <summary>
        /// 平台名称
        /// </summary>
        public string platformName
        {
            get;
            set;
        }
        
        public string GatewayName
        {
            get;
            set;
        }
        /// <summary>
        /// 第三方支付方式
        /// </summary>
        public int TypeID
        {
            get;
            set;
        }

        /// <summary>
        /// 第三方支付名称
        /// </summary>
        public string TypeName
        {
            get;
            set;
        }

        /// <summary>
        /// 第三方交易方式
        /// </summary>
        public string TradeCode
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string SourceTypeID
        {
            get;
            set;
        }

        /// <summary>
        /// 是否有范围
        /// </summary>
        public bool IsRange
        {
            get;
            set;
        }

        /// <summary>
        /// 支付区间
        /// </summary>
        public string AmountSort
        {
            get;
            set;
        }

        public bool IsMustBeConverted { get; set; }
        public string CurrencyCode { get; set; }
        /// <summary>
        /// 优先比重
        /// </summary>
        public byte Priority
        {
            get;
            set;
        }

        /// <summary>
        /// 是否启动
        /// </summary>
        public bool Status
        {
            get;
            set;
        }

        /// <summary>
        /// 最小金額
        /// </summary>
        public long MinValue
        {
            get;
            set;
        }

        /// <summary>
        /// 最大金額
        /// </summary>
        public long MaxValue
        {
            get;
            set;
        }

        /// <summary>
        /// 热门程度
        /// </summary>
        public int Popular
        {
            get;
            set;
        }

        /// <summary>
        /// 排序值
        /// </summary>
        public int Sort
        {
            get;
            set;
        }
    }
}
