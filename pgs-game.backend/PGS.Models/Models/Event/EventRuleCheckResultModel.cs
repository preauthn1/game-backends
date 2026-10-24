using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class EventRuleCheckResultDebugInfoModel
    {
        public EventRuleCheckResultDebugInfoModel() { }


        public EventRuleCheckResultDebugInfoModel(bool isOK , string msg)
        {
            this.IsValidPass = isOK;
            this.Message = msg;
        }
     

        public int EventID { get; set; }


        /// <summary>
        /// EventRuleTypeEnum
        /// </summary>
        public int RuleType { get; set; }


        public bool IsAlwaysRule { get; set; }


        /// <summary>
        /// 是否驗證通過
        /// </summary>
        public bool IsValidPass { get; set; }


        public decimal? ValueForCompare { get; set; }


        public string Message { get; set; }


        public string Memo { get; set; }
    }


}
