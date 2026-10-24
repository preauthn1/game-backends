using target.Models.ViewModels.API.AdminSys;
using System;
using System.Collections.Generic;

namespace target.Models.Models
{
    public class AccountingAdjustmentRuleExtraSettingModel
    {

        public List<int> FilterEventTypes { get; set; }


        /// <summary>
        /// 參考 EventPrizeTypeEnum
        /// </summary>
        public List<int> FilterEventPrizeTypes { get; set; }


        /// <summary>
        /// 若type=1002 會嘗試搜尋1001的活動是否有重疊日期的數字 要額外扣除
        /// </summary>
        public bool IsExcludeOverlayFreeGameDiscount { get; set; }
    }

}
