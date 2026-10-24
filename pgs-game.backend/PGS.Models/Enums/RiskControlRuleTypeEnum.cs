using System.ComponentModel;

namespace target.Models.Enums
{
    public enum RiskControlRuleTypeEnum
    {

        /// <summary>
        /// 派彩金額大於
        /// </summary>
        PayoutAmountLargeThan = 1001,


        /// <summary>
        /// 派彩倍率 大於
        /// </summary>
        PayoutRateLargeThan = 1005,


        /// <summary>
        /// 同一個風控ID發生多少次
        /// </summary>
        CounterRiskControlTimes = 2101,


    }

}
