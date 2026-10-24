using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Money
{

    public enum ThirdPaymentErrorEnum 
    {
        [EnumDescription("支付渠道目前忙碌中，请稍后再做尝试")]
        SystemError = 0,


        [EnumDescription("支付渠道回应失败代码，可能原因为渠道线路已设为维护或关闭")]
        ThirdPayment_Error_GatewayNotAvialable = 40001,


        [EnumDescription("支付渠道回应失败代码，请确认渠道配置是否正确设置")]
        ThirdPayment_Error_GatewaySettingIncorrect = 40002,


        [EnumDescription("支付渠道回应失败代码，金额非允许的数值或格式不正确")]
        ThirdPayment_Error_AmountFormatError = 40003,


        [EnumDescription("支付渠道回应失败代码，已超过本日可收限额、请稍后再做尝试")]
        ThirdPayment_Error_AmountLimit = 40004,


        [EnumDescription("支付渠道回应失败代码，系统可能忙碌中无法回应")]
        ThirdPayment_Error_ApiError = 40005,


        [EnumDescription("支付渠道回应失败代码，回应资讯无法正确解析，请稍后再做尝试")]
        ThirdPayment_Error_ParsingError = 40005,

    }
}
