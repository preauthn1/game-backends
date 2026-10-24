using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{

    public enum PromotionCostCalculateType
    {        
        /// <summary>
        /// 運算時會查出活動成本。但不會計算進入輸贏中
        /// 會分成編號0 & 編號7是因為改A壞B 太可怕了...還是分開吧
        /// </summary>
        QueryPromotionCost = 0,

        /// <summary>
        /// 每個商戶的GGR數字是包含活動成本的
        /// 若有需要 可透過adjustRule排除掉這些數字
        /// </summary>
        QueryAndAutoMergeToWinLoss = 7,

        None = 9,
    }
}
