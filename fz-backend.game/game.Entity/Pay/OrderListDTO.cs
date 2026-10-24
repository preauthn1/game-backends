using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FW.Core.Helper.Enum;

namespace game.Entity.Pay
{
    public class OrderListDTO
    {
        /// <summary>
        /// 平台資訊
        /// </summary>
        public int PlatformID { get; set; }
        /// <summary>
        /// 平台名稱
        /// </summary>
        public string PlatformName { get; set; }
        /// <summary>
        /// 所屬渠道
        /// </summary>
        public int AreaID { get; set; }
        /// <summary>
        /// 支付类型
        /// </summary>
        public PayType PayTypeID
        {
            get;
            set;
        }
        /// <summary>
        /// 支付名稱
        /// </summary>
        public string TypeName
        {
            get;
            set;
        }

        //支付來源
        public int? SourceTypeID
        {
            get;
            set;
        }

        /// <summary>
        /// 支付厂商
        /// </summary>
        public int? GWID
        {
            get;
            set;
        }

        /// <summary>
        /// 充值账号
        /// </summary>
        public int UserID
        {
            get;
            set;
        }
        /// <summary>
        /// 登入帳號名稱
        /// </summary>
        public string LoginName
        {
            get;
            set;
        }
        /// <summary>
        /// 訂單編號
        /// </summary>
        public int OrderID { get; set; }

        /// <summary>
        /// 充值金额
        /// </summary>
        [MoneyInt]
        public Int64 Amount
        {
            get;
            set;
        }

        /// <summary>
        /// 客服補單金额
        /// </summary>
        [MoneyInt]
        public Int64 ClaimAmount
        {
            get;
            set;
        }

        /// <summary>
        /// 订单状态
        /// </summary>
        public CDOrderStatus Status
        {
            get;
            set;
        }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress
        {
            get;
            set;
        }

        /// <summary>
        /// 第三方交易标识
        /// </summary>
        public string TransID
        {
            get;
            set;
        }

        /// <summary>
        /// 第三方交易数据
        /// </summary>        
        public string TransData
        {
            get;
            set;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get;
            set;
        }
        /// <summary>
        /// 回調時間
        /// </summary>
        public DateTime? LastUpdateTime
        {
            get;
            set;
        }
        /// <summary>
        /// 使用者等級
        /// </summary>
        public int UserLevel
        {
            get;
            set;
        }
        /// <summary>
        /// 第三方平台商
        /// </summary>
        public string GatewayName { get; set; }

    }

}
