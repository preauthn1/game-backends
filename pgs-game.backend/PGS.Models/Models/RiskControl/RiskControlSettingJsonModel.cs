using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{


    public class RiskControlCheckParameterModel
    {
        public string BetRecordNoPGS { get; set; }

        public int MerchantID { get; set; }

        public int GameID { get; set; }

        public int PlayerID { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal BetAmount { get; set; }
    }


    public class RiskControlSettingJsonModel
    {

        public List<RiskControlRuleTargerBundleModel> RuleTargetDefines { get; set; }

    }


    public class RiskControlRuleTargerBundleModel
    {

        public List<RiskControlSettingRuleModel> Rules { get; set; }

        /// <summary>
        /// 若條件符號 要做哪些事情
        /// </summary>
        public RiskControlSettingActionModel Action { get; set; }

    }


    public class RiskControlSettingRuleModel
    {
        /// <summary>
        /// RiskControlRuleTypeEnum
        /// </summary>
        public int RuleType { get; set; }


        public decimal CompareNumericValueLargeThan { get; set; }

       
        /// <summary>
        /// 在多少時間內發生的總次數 (e.g. 60 = 查出60分鐘內發生了多少次)
        /// </summary>
        public int CompareDurationMinutes { get; set; }


    }



    public class RiskControlSettingActionModel
    {

        public bool IsNotify { get; set; }

        public bool IsForceException { get; set; }

        /// <summary>
        /// 假裝回應成功 但是讓注單卡在未結算
        /// </summary>
        public bool IsResponseFakeSuccessButPendingRecordStatus { get; set; }

        /// <summary>
        /// 強制關閉遊戲
        /// </summary>
        public bool IsCloseGame { get; set; }
    }


    public class RiskControlResultModel
    {
        public int RiskControlConfigID { get; set; }

        public bool IsResponseFakeSuccessButPendingRecordStatus { get; set; }

        public bool IsForceException { get; set; }

        public List<RiskControlResultModel> ListOfDebugInfo { get; set; }

    }



}
