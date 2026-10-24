using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Pay
{
    /// <summary>
    /// 订单
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class CDPayOrder : BaseEntity
    {

        /// <summary>
        /// 支付类型
        /// </summary>
        public int PayTypeID
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
        /// 充值金额
        /// </summary>
        [MoneyInt]
        public Int64 Amount 
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
        /// 支付厂商
        /// </summary>
        public int? GWID
        {
            get;
            set;
        }

        public virtual UserInfo UserInfo
        {
            get;
            set;
        }

        public virtual CDPayType PayType
        {
            get;
            set;
        }
    }

}
