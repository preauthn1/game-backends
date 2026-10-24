using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Users;
using System;

namespace game.Entity.Money
{
    /// <summary>
    /// 兑换记录
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class ConvertLog : BaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 兑换金额
        /// </summary>
        [MoneyInt]
        public Int64 Amount { get; set; }

        /// <summary>
        /// 兑换手续费
        /// </summary>
        [MoneyInt]
        public Int64 TaxAmount { get; set; }

        /// <summary>
        /// 真实兑换手续费
        /// </summary>
        [MoneyInt]
        public Int64 RealTaxAmount { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public ConvertLogStatus Status { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 支付宝账号 or 银行卡账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 银行卡ID
        /// </summary>
        public int? BankCodeID { get; set; }

        /// <summary>
        /// 银行卡名稱
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// 交易种类
        /// </summary>
        public byte ConvertType { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string CreateIP { get; set; }

        /// <summary>
        /// 第三方交易记录编号
        /// </summary>
        public string TransID { get; set; }

        /// <summary>
        /// 第三方交易数据
        /// </summary>
        public string TransData { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime CheckTime { get; set; }

        /// <summary>
        /// 平台ID
        /// </summary>
        public int PlatformID { get; set; }

        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; }

        public virtual UserInfo User { get; set; }

        /// <summary>
        /// 用户银行金额
        /// </summary>
        [MoneyInt]
        public Int64 BankAmount { get; set; }

        /// <summary>
        /// 用户剩余金额
        /// </summary>
        [MoneyInt]
        public Int64 CashAmount { get; set; }

        /// <summary>
        /// 操作者
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// 累计兑换金额
        /// </summary>
        public Int64 TotalConvert { get; set; }

        /// <summary>
        /// 累计兑换金额IOS
        /// </summary>
        public Int64 TotalConvertIOS { get; set; }

        /// <summary>
        /// 累计兑换金额Android
        /// </summary>
        public Int64 TotalConvertAndroid { get; set; }

        /// <summary>
        /// 总充值金额
        /// </summary>
        public Int64 TotalCharge { get; set; }

        /// <summary>
        /// 总输赢
        /// </summary>
        public Int64 ChargeConvertRate { get; set; }

        /// <summary>
        /// 是否是代理帳號
        /// </summary>
        public int ClubAgent { get; set; }

        /// <summary>
        /// 是否是嫌疑帳號
        /// </summary>
        public int Suspicious { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// 待付商编号
        /// </summary>
        public int? GWID { get; set; }

        /// <summary>
        /// 代付商名称
        /// </summary>
        public string GatewayName { get; set; }

    }


    //自動兌換撈取紀錄
    public class PullConvertLog
    {
        public string id { get; set; } //自增id，也是订单id
        public string user_id { get; set; } //玩家id
        public string gold_pre { get; set; }  //提现前金额
        public string gold_next { get; set; }  //提现后金额
        public string gold_bank { get; set; }  //银行金额
        public Int64 gold_req { get; set; }  //当前订单金额
        public string gold_req_get { get; set; }  //已到账金额
        public Int64 fee { get; set; }  //手续费
        public string ip { get; set; }  //玩家ip
        public string alipay_account { get; set; }  //支付宝账户
        public string alipay_name { get; set; }  //支付宝名字
        public string union_pay_account { get; set; }  //银行卡账户
        public string union_pay_name { get; set; }  //银行卡名字
        public string total_req { get; set; }  //积累申请兑换金额
        public string total_get { get; set; }  //积累兑换金额
        public string recharge_total { get; set; }  //总充值金额
        public string recharge_delegate { get; set; }  //代理充值金额
        public string time_start { get; set; }  //提交时间
        public string time_end { get; set; }  //到账时间
        public string status { get; set; }  //订单状态 0.兑换完成 1.审核中 2.已审核 3.已打款 4.订单异常 5. 异常帐号无法提现 6.黑名单账户无法提现 7.提现账户或名字错误 8.拒绝兑换 9.审核中，可能是刷分账户
        public string link_id { get; set; }  //连接erlang表的id
        public int exchange_account_type { get; set; }  // 1-支付宝，2-银行卡
        public string total_win_lose { get; set; }  //总输赢
        public string union_pay_bank { get; set; }  //银行
        public string exchange_black { get; set; }  //是否兑换黑名单 0.不是 1.是
        public string Operator { get; set; }  //操作人员
        public int export { get; set; }  //是否导出1是0否
        public string file_name { get; set; }  //导出的文件名
    }
}