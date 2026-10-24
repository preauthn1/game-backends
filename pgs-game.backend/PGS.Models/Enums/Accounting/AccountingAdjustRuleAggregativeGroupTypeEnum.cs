using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{
    /// <summary> 
    /// 編號規則需要符合下方 GroupTypeEnum
    /// </summary>
    public enum AccountingAdjustmentRuleAggregativeGroupEnum
    {
       
        [Description("免盤費一個月(100%)")]
        Promotion_OneMonthFreeCredit_100p = 50,

        [Description("新遊戲免盤費")]
        Promotion_NewGameRelease_100p = 150,

        [Description("1月 30%折扣")]
        Promotion_202401_30p = 11000,

        [Description("2月 70%折扣")]
        Promotion_202402_70p = 12000,

        [Description("3月 50%折扣")]
        Promotion_202403_50p = 13000,


        /// <summary>
        /// 如果過時的項目要隱藏 可以標記invisible
        /// </summary>
        [Invisible]
        XXXXXXX = 9999,
    }




    public enum AccountingAdjustRuleAggregativeGroupTypeEnum
    {      

        OneMerchantOneGroup = 0,

        /// <summary>
        /// 編號 10000~19999 視為這種類型
        /// </summary>
        AggregativeByParentAgent = 10000,
    }








}
