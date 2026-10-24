using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Area
{
    /// <summary>
    /// 平台配置信息
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class Platform : BaseEntity
    {
        /// <summary>
        /// 平台名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 支付方式排序
        /// </summary>
        public string PaySort { get; set; }

        /// <summary>
        /// 界面控制标识
        /// </summary>
        public string UIFlag { get; set; }

        /// <summary>
        /// 升级提示内容
        /// </summary>
        public string UpGradeMsg { get; set; }

        /// <summary>
        /// 公告提示内容
        /// </summary>
        public string AnnMsg { get; set; }

        /// <summary>
        /// 充值提示文本
        /// </summary>
        public string PayTip { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FAQContent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PayAmountConfig { get; set; }

        /// <summary>
        /// 充值开关
        /// </summary>
        public byte PayStatus { get; set; }

        /// <summary>
        /// 充值关闭提示文本
        /// </summary>
        public string PayCloseTip { get; set; }

        /// <summary>
        /// 遊客升級綁定帳號, 贈送金幣
        /// </summary>
        [MoneyInt]
        public long GiftMoney { get; set; }

        /// <summary>
        /// 註冊預設金幣
        /// </summary>
        [MoneyInt]
        public long DefaultMoney { get; set; }

        /// <summary>
        /// 公告提示開關
        /// </summary>
        public byte AnnMsgShow { get; set; }

        /// <summary>
        /// 營運商
        /// </summary>
        public int Company { get; set; }

        /// <summary>
        /// 闪付商户号
        /// </summary>
        public string ShanFuAccountID { get; set; }

        /// <summary>
        /// 闪付密钥
        /// </summary>
        public string ShanFuSign { get; set; }
    }
}
