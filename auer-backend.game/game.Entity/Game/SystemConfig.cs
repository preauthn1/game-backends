using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;

namespace game.Entity.Game
{
    /// <summary>
    /// 系统配置
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class SystemConfig : BaseEntity
    {
        /// <summary>
        /// 状态
        /// </summary>
        public ConvertConfigStatus ConvertStatus
        {
            get;
            set;
        }

        /// <summary>
        /// 关闭兑换功能的提示消息
        /// </summary>
        public string PromptMsg
        {
            get;
            set;
        }

        /// <summary>
        /// 兑换自动审核金额限制
        /// </summary>
        [MoneyInt]
        public Int64 AutoCheckLimit
        {
            get;
            set;
        }

        /// <summary>
        /// 兑换时现金必须保留的数量
        /// </summary>
        [MoneyInt]
        public long ConvertRetainAmount { get; set; }

        /// <summary>
        /// 银联兑换金币最小数量(单次)
        /// </summary>
        [MoneyInt]
        public long ConvertMinAmount { get; set; }

        /// <summary>
        /// 银联兑换金币最大数量(单次)
        /// </summary>
        [MoneyInt]
        public long ConvertMaxAmount { get; set; }

        // <summary>
        /// 支付宝兑换金币最小数量(单次)
        /// </summary>
        [MoneyInt]
        public long ConvertAlipayMinAmount { get; set; }

        /// <summary>
        /// 支付宝兑换金币最大数量(单次)
        /// </summary>
        [MoneyInt]
        public long ConvertAlipayMaxAmount { get; set; }

        /// <summary>
        /// 兑换比例的提示文本
        /// </summary>
        public string ConvertRateTip { get; set; }

        /// <summary>
        /// 银行默认密码提示文本
        /// </summary>
        public string BankDefaultPwdTip { get; set; }

        /// <summary>
        /// 客服默认提示讯息文本
        /// </summary>
        public string QuestionContactTip { get; set; }

        /// <summary>
        /// 银行开关
        /// </summary>
        public byte ConvertBankShow { get; set; }

        /// <summary>
        /// 支付宝开关
        /// </summary>
        public byte ConvertAlipayShow { get; set; }

        /// <summary>
        /// 通知兑换金额上限
        /// </summary>
        [MoneyInt]
        public long? ConvertWinLostAlert { get; set; }

        /// <summary>
        /// telegram Token
        /// </summary>
        public string TGBotToken { get; set; }

        /// <summary>
        /// telegram ChatID
        /// </summary>
        public string TGChatID { get; set; }

    }

    /// <summary>
    /// 兑换配置状态
    /// </summary>
    public enum ConvertConfigStatus : byte
    {
        /// <summary>
        /// 关闭
        /// </summary>
        [EnumDescription("关闭")]
        Closed = 0,

        /// <summary>
        /// 开启
        /// </summary>
        [EnumDescription("开启")]
        Opened = 1
    }
}