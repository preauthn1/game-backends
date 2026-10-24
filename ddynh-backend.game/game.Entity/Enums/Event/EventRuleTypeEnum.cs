using FW.Core.Helper.Enum;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Event
{  

    public enum EventRuleTypeEnum
    {


        [EnumDescription("用户累计存款量")]
        [Description("需 >=")]
        [ToCash]
        TotalDepositAmount = 2001,


        [EnumDescription("用户累计流水")]
        [Description("需 >=")]
        [ToCash]
        TotalFlowtAmount = 2002,

        [EnumDescription("七日登入重复领取")]
        [Description("")]
        [ValueFormat(ValueFormatRestrictionEnum.Boolean)]
        Login_BonusReset = 2003,

        [EnumDescription("用户注册日期")]
        [Description("需 >= (天)")]
        Users_RegisterDays = 3001,


        [EnumDescription("用户需绑定银行卡")]
        [Description("")]
        [ValueFormat(ValueFormatRestrictionEnum.Boolean)]
        Users_IsBindBankAccount = 3002,


        [EnumDescription("用户需绑定支付宝")]
        [Description("")]
        [ValueFormat(ValueFormatRestrictionEnum.Boolean)]
        Users_IsBindAlipayAccount = 3003,


        [EnumDescription("用户首储金额")]
        [Description("需 >=")]
        [ToCash]
        DepositAmount_FirstDeposit = 6001,        


        [EnumDescription("用户期间内的存款金额")]
        [Description("需 >=")]
        [ToCash]
        Duration_DepositAmount = 7001,


        [EnumDescription("用户期间内的流水金额")]
        [Description("需 >=")]
        [ToCash]
        Duration_FlowAmount = 7002,




        [EnumDescription("时间区间_起始日")]
        [Description("")]
        Duration_TimeDuration_Begin = 7771,

        [EnumDescription("时间区间_结束日")]
        [Description("")]
        Duration_TimeDuration_End = 7772,


        [EnumDescription("累计数值当日结算")]
        [Description("")]
        [ValueFormat(ValueFormatRestrictionEnum.Boolean)]
        LuckyRoulette_CumulativeTopUpOfTheDay = 8001,


    }


}
