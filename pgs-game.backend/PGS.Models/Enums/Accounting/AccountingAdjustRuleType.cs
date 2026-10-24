using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{
    public enum AccountingAdjustRuleType
    {
        /// <summary>
        /// 輸贏調整值 特定遊戲免盤費 或 折扣
        /// 請限定給《遊戲免盤費》使用 (其他類型的折扣不會混用type=1001)
        /// </summary>
        AdjustValue_FeeAmount_GameFeeDiscount  = 1001,

        /// <summary>
        /// 輸贏調整值 特定商戶一個月FC免盤費/折扣
        /// </summary>
        AdjustValue_FeeAmount_GameFeeDiscount_IncludePromotion = 1002,


        /// <summary>
        /// 輸贏調整值 特定遊戲用有效流水扣除額外比例的輸贏
        /// </summary>
        AdjustValue_FeeAmount_DiscountByLiveCasinoRebate = 1201,

        /// <summary>
        /// 盤費調整值 特定遊戲 額外減免一部份 (e.g. 補回1%)
        /// 如果指定全遊戲、默認會直接把活動成本也算進去折扣
        /// </summary>

        AdjustValue_FeeAmount_RecoverFee_SpecificGame = 1301,

        /// <summary>
        /// 活動數字校正
        /// </summary>
        AdjustValue_AdjustPromotionValueByRate = 3333,


        /// <summary>
        /// 活動數字校正 (僅針對vworld帳單)
        /// </summary>
        AdjustValue_AdjustPromotionValueByRate_VworldInvoiceOnly = 3343,


        /// <summary>
        /// 兩種用途
        /// 1. 固定金額調整額 (100%) (要填單一商戶號 & 金額可能是負數)
        /// 2. 備註信息-提醒娟娟用 (AdjustValue 請填寫 0.0001)
        /// </summary>
        AdjustValue_MemoHint_ForFinanceTeam = 8888,



        AdjustValue_ExcludeTagWager = 9931,


        /// <summary>
        /// 特殊型 (DB不要塞這種type) 額外計算產生的
        /// 會排除免盤費FC + 遊戲免盤費重疊的數字
        /// </summary>
        Special_ExcludeOverlayGameFeeDiscount = 999001,

    }
}
