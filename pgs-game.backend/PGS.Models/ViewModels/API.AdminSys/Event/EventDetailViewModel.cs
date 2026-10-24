using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels.API.AdminSys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class EventQuickCreateRequestModel
    {
        public string DisplayName { get; set; }

        /// <summary>
        /// EventTemplateTypeEnum
        /// </summary>
        public int EventTemplateType { get; set; }


        public EventExtraParameterModel ExtraEventParameter { get; set; }

    }


    /// <summary>
    /// 根據不同活動類型 有各自獨立的參數 可能是必填的
    /// </summary>
    public class EventExtraParameterModel
    {

        public int VendorConfigID { get; set; }

        public int EventID { get; set; }

        public int ValueCompareType { get; set; }

        public bool IsAccountCreateTimeCheck { get; set; }

    }




    public class EventQuickCreateResultViewModel
    {
        public int EventID { get; set; }

    }


    public class EventDetailViewModel
    {
        public int EventID { get; set; }

        public int EventType { get; set; }

        public int EventTemplateType { get; set; }

        public string EventTypeText { get; set; }

        public string DisplayName { get; set; }

        public string DisplayNameExternal { get; set; }

        public int Status { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }


        /// <summary>
        /// 為了配合前端 額外*1000
        /// </summary>
        public long BeginTimeTS_1000 { get; set; }


        public long EndTimeTS_1000 { get; set; }


        /// <summary>
        /// 是否有需要檢查event finish的機制 (不一定每個遊戲都要檢查)
        /// </summary>
        public bool IsEventFinishTagProcessRequired { get; set; }


        public bool IsQuestEvent { get; set; }

        /// <summary>
        /// 活動進行中
        /// </summary>
        public bool IsOnProgressing { get; set; }

        /// <summary>
        /// 是否每日統計活動積分
        /// </summary>
        public bool IsDailyEventCounter { get; set; }


        public List<int> SupportGameIDs { get; set; }


        public int? MerchantSelectTemplateID { get; set; }


        public List<int> AvailableMerchantIDs { get; set; }


        /// <summary>
        /// 泛用規則
        /// </summary>
        public EventRuleBundleViewModel AlwaysRuleBundle { get; set; }

        /// <summary>
        /// 獎項組合 清單
        /// </summary>
        public List<EventPrizeBundleViewModel> PrizeBundles { get; set; }


        public List<SystemConfigSettingDisplayViewModel> CustomizedSettings { get; set; }


        /// <summary>
        /// 後台才額外需要的資訊 adminSys API自己查詢
        /// </summary>
        public AdminSysExtraInfoViewModel AdminSysExtraInfo { get; set; }


        public EventManagementUIControl UIControl { get; set; }


        public List<EventExtraInfoViewModel> ExtraInfo { get; set; }

    }


    public class EventExtraInfoViewModel
    {
        public string SettingName { get; set; }

        public string SettingValue { get; set; }
    }


    public class AdminSysExtraInfoViewModel
    {

    }




    public class EventManagementUIControl
    {
        /// <summary>
        /// 限制特定活動類型 只有特定商戶可供選擇
        /// </summary>
        public List<int> RestrictMerchantIDs { get; set; }

        public List<int> RestrictGameIDs { get; set; }

        public bool IsDisableEditMerchants { get; set; }

        public bool IsDisableEditGameIDs { get; set; }

    }



    public class EventRuleBundleViewModel
    {
        public int EventRuleBundleID { get; set; }


        public int Sequence { get; set; }


        /// <summary>
        /// 排序順序請參考 GetEventFullInformation
        /// </summary>
        public List<EventRuleItemInfoModel> RuleItems { get; set; }

    }


    public class EventPrizeBundleViewModel
    {

        public int EventPrizeBundleID { get; set; }

        public string Memo { get; set; }

        public string HintMessageWhenEdit { get; set; }

        /// <summary>
        /// 是否包含 多個可選達成條件 (擇一完成)
        /// </summary>
        public bool IsMultipleRuleBundle { get; set; }

        /// <summary>
        /// DB欄位
        /// </summary>
        public bool IsDisabled { get; set; }


        public bool IsAllowEditDisabledFlag { get; set; }


        public List<EventPrizeItemViewModel> PrizesItems { get; set; }


        /// <summary>
        /// 若有多個ruleBundle 代表其中一個bundle符合 即可領獎
        /// </summary>
        public List<EventRuleBundleViewModel> RuleBundles { get; set; }


    }


    public class EventRuleItemViewModel
    {
        public int RuleType { get; set; }

        public string RuleTypeText { get; set; }

        public string Value { get; set; }

        public DateTime? BeginTime { get; set; }

        public DateTime? EndTime { get; set; }
    }


    public class EventPrizeItemViewModel
    {
        public int EventPrizeItemID { get; set; }

        public int EventPrizeBundleID { get; set; }

        /// <summary>
        /// 參考 EventPrizeTypeEnum
        /// </summary>
        public int PrizeType { get; set; }

        public string PrizeTypeText { get; set; }

        public decimal AmountOrQuantity { get; set; }


        /// <summary>
        /// 用來判斷是否異動過
        /// </summary>
        public decimal OriginalAmountOrQuantity { get; set; }


        public bool IsValueRequired { get; set; }


        public bool IsDropdownSelect { get; set; }


        public List<DropdownValueViewModel> DropdwonItems { get; set; }


    }


    public class EventDeleteRequestModel
    {
        public int EventID { get; set; }
    }



    public class EventSimpleRequestModel
    {
        public int EventID { get; set; }

        public EventCounterOrderByEnum OrderBy { get; set; }
    }


    public class EventPrizeBundleCheckResult
    {

        public EventPrizeBundleCheckResult()
        {

        }

        public EventPrizeBundleCheckResult(string memo)
        {
            this.Memo = memo;
        }


        public string Memo { get; set; }


        public bool IsPassAll { get; set; }


        /// <summary>
        /// 部分活動會有積分判斷。返回當前積分/進度值
        /// </summary>
        public decimal CurrentQuestProgress { get; set; }



        public ValidateDetailInfo ValidateDetailInfo { get; set; }

    }


    public class ValidateDetailInfo
    {

        /// <summary>
        /// 通用規則驗證不通過
        /// </summary>
        public bool IsValidateFailByAlwaysRule { get; set; }


        /// <summary>
        /// 獎項已領過了
        /// </summary>
        public bool IsValidateFailByPrizeRecordExist { get; set; }



        public List<EventRuleCheckResultDebugInfoModel> ValidateResultsForDebug { get; set; }
    }






}
