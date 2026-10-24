using target.Models.Attributes;
using target.Models.CultureText;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace target.Models.Enums
{

    public enum UserAssignMerchantModeEnum
    {    
        /// <summary>
        /// 勾選哪些商戶 就只能看那些商戶
        /// </summary>
        ViewSelectMerchant = 1,

        /// <summary>
        /// 單一代理樹狀模式瀏覽 (若有加值信用額度 限定此)
        /// </summary>
        CreditMerchant = 5,

        /// <summary>
        /// 可以勾選多個代理商戶、並且可以瀏覽多個代理樹狀底下的所有商戶
        /// *禁止混用 代理+非代理  (混用是下拉選單會壞掉嗎...? 忘記這個限制原因了...)
        /// </summary>
        MultipleHierarchyMerchant = 99,
    }
}
