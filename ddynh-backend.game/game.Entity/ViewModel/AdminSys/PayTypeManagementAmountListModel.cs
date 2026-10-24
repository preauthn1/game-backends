using FW.Core.Helper;

namespace game.Entity.ViewModels
{

    public class PayTypeManagementAmountListModel
    {
        /// <summary>
        /// 第三方厂商
        /// </summary>
        public int GWID { get; set; }

        /// <summary>
        /// 第三方支付方式 (PayType.cs enum)
        /// </summary>
        public int TypeID { get; set; }

        /// <summary>
        /// PayType.ID
        /// </summary>
        public int PayTypeID { get; set; }

        /// <summary>
        /// 顯示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 第三方支付名称
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 第三方交易方式
        /// </summary>
        public string TradeCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SourceTypeID { get; set; }

        /// <summary>
        /// 优先比重
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 是否有范围
        /// </summary>
        public bool IsRange { get; set; }

        /// <summary>
        /// 最小金額
        /// </summary>
        public long MinValue { get; set; }

        /// <summary>
        /// 最大金額
        /// </summary>
        public long MaxValue { get; set; }

        public string AmountSort { get; set; }

        public string GatewayName { get; set; }

        public int PlatformPayTypeSettingId { get; set; }

        /// <summary>
        /// 是否启动
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// 金币转换比例
        /// </summary>
        public long Scale { get; set; }
    }
}
