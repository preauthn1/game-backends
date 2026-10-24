using target.Models.ViewModels.API.AdminSys;
using System;
using System.Collections.Generic;

namespace target.Models.Models
{
    public class AccountingAdjustmentRuleViewModel
    {

        public string CopyText_AllParticipantMerchant { get; set; }

        public List<AccountingAdjustmentRuleItemViewModel> Items { get; set; }

        public List<AccountingAdjustmentRuleAggregativeGroupMappingItems> AggregativeGroupList { get; set; }

    }


    public class AccountingAdjustmentRuleAggregativeGroupMappingItems
    {

        public int AggregativeGroup { get; set; }

        public string Name { get; set; }

        public List<DropdownValueViewModel> DropdownItems { get; set; }

    }


    public class AccountingAdjustmentRuleAndAssinGroupInfoModel
    {
        public int AccountingAdjustmentRuleID { get; set; }

        public int AccountingGroupID { get; set; }

        public string GroupName { get; set; }
    }


    public class AccountingAdjustmentRuleItemViewModel
    {

        public int RuleID { get; set; }
        public string NameCN { get; set; }
        public string NameEN { get; set; }

        public int Type { get; set; }

        public string TypeText { get; set; }

        public decimal AdjustValue { get; set; }

        public decimal AdjustValuePercentage { get; set; }


        public string GameIDs { get; set; }

        public string MerchantIDs { get; set; }

        public string MerchantsDisplayInfo { get; set; }

        public int NumberOfMerchantID { get; set; }

        public DateTime? BeginTime { get; set; }

        public DateTime? EndTime { get; set; }

        public long BeginTimeTS { get; set; }

        public long EndTimeTS { get; set; }

        public string BeginTimeTwText { get; set; }

        public string EndTimeTwText { get; set; }

        public List<string> AssignGroups { get; set; }

        public bool IsExpired { get; set; }

        public bool IsHintEmptyAssign { get; set; }

        public int? SortSequence { get; set; }

        public int? AggregativeGroup { get; set; }        

    }





}
