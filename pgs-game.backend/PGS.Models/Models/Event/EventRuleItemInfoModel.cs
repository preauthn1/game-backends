using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryEventRuleItemInfoParameterModel
    {
        /// <summary>
        /// 必填
        /// </summary>
        public int EventID { get; set; }

        public bool IsAlwaysRule { get; set; }

        public List<int> RuleTypes { get; set; }

        public List<int> EventPrizeBundleIDs { get; set; }

    }

    public class EventRuleItemInfoModel
    {

        public int EventID { get; set; }

        public int EventPrizeBundleID { get; set; }

        public int EventRuleBundleID { get; set; }

        public int EventRuleItemID { get; set; }

        public int Sequence { get; set; }

        /// <summary>
        /// 請參考 EventRuleTypeEnum
        /// </summary>
        public int RuleType { get; set; }

        /// <summary>
        /// 透過其他程式補上
        /// </summary>
        public string RuleTypeText { get; set; }


        public string Value { get; set; }

        public DateTime? BeginTime { get; set; }

        public DateTime? EndTime { get; set; }



        /// -----------------------------------------
        /// -----------------------------------------
        /// -----------------------------------------
        /// -----------------------------------------
        /// 以下欄位都不是透過DB查詢的
        /// -----------------------------------------
        /// -----------------------------------------
        /// -----------------------------------------
        /// -----------------------------------------


        public bool IsValueRequired { get; set; }

        public bool IsDateTimeRangeValue { get; set; }

        public bool IsRulesOfWhen { get; set; }

        public long BeginTimeTS_1000 { get; set; }

        public long EndTimeTS_1000 { get; set; }

        public string OriginalValue { get; set; }

    }


}
