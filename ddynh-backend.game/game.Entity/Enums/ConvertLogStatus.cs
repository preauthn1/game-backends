using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Users;
using System;

namespace game.Entity.Money
{
 

    /// <summary>
    /// new兑换记录状态
    /// </summary>
    public enum ConvertLogStatus : byte
    {

        /// <summary>
        /// 提出兑换
        /// </summary>
        [EnumDescription("提出兑换")]
        Exchange = 0,

        /// <summary>
        /// 审核中
        /// </summary>
        [EnumDescription("审核中")]
        [Disabled(true)]
        WaitAuditing = 1,

        /// <summary>
        /// 审核通过
        /// </summary>
        [EnumDescription("审核通过")]
        ConvertSuccessed = 2,

        /// <summary>
        /// 已打款
        /// </summary>
        [EnumDescription("已打款")]
        AlreadyPaid = 3,

        /// <summary>
        /// 订单异常
        /// </summary>
        [EnumDescription("订单异常")]
        Unusual = 4,

        /// <summary>
        /// 异常帐号
        /// </summary>
        [EnumDescription("异常帐号")]
        AccountUnusual = 5,

        /// <summary>
        /// 黑名单
        /// </summary>
        [EnumDescription("黑名单")]
        Blacklist = 6,

        /// <summary>
        /// 账户错误
        /// </summary>
        [EnumDescription("账户错误")]
        AccountError = 7,

        /// <summary>
        /// 拒绝兑换
        /// </summary>
        [EnumDescription("拒绝兑换")]
        ConvertFailed = 8,

        /// <summary>
        /// 疑似洗钱
        /// </summary>
        [EnumDescription("疑似洗钱")]
        MoneyLaundering = 9,

        /// <summary>
        /// 补单完成
        /// </summary>
        [EnumDescription("补单完成")]
        AlternateSuccessed = 10,

        /// <summary>
        /// 退还金币
        /// </summary>
        [EnumDescription("退还金币")]
        ReturnMoney = 11,

        /// <summary>
        /// 人工处理
        /// </summary>
        [EnumDescription("人工处理")]
        ManualProcess = 98,

        /// <summary>
        /// 自動打款程式排队中
        /// </summary>
        [EnumDescription("排程处理")]
        [Disabled(true)]
        Queueing = 99,

        /// <summary>
        /// 自動打款程式排队中
        /// </summary>
        [EnumDescription("自动打款失败")]
        [Disabled(true)]
        AutoPaymentFailed = 100,
    }
}