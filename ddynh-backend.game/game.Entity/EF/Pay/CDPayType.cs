using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Money;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.Pay
{
    /// <summary>
    /// 支付接口
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class CDPayType : BaseEntity
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
        /// 类型名称
        /// </summary>
        [Required, StringLength(32)]
        public string TypeName
        {
            get;
            set;
        }

        /// <summary>
        /// 流通类型
        /// </summary>
        public int SourceTypeID
        {
            get;
            set;
        }

        /// <summary>
        /// 充值比例
        /// 1 块钱充值金币数量
        /// </summary>
        public int Scale
        {
            get;
            set;
        }


        /// <summary>
        /// 支付渠道状态
        /// </summary>
        public CDPayTypeStatus Status
        {
            get;
            set;
        }

        public virtual MoneySourceType SourceType
        {
            get;
            set;
        }

    }

    
}
