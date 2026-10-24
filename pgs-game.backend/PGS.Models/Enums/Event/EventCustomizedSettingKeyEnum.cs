using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.Attributes.ColumnValue;
using target.Models.CultureText;


namespace target.Models.Enums
{

    public enum EventCustomizedSettingKeyEnum
    {

        /// <summary>
        /// 需紀錄 活動期間投注計數器 (全遊戲合併統計)
        /// ps. 觸發eventCounter的方式有兩種.... 
        /// 1. 是建立 EventCustomized
        /// 2. 是建立 WhenSettle後的 prize行為 (我認為這種方式較好)
        /// </summary>
        [ValueUnnecessary]
        BetCounterRequired_TotalSummary_AllInOne = 1051,


        /// <summary>
        /// 需紀錄 活動期間投注計數器 (每個遊戲分開記錄)
        /// ps. 觸發eventCounter的方式有兩種.... 
        /// 1. 是建立 EventCustomized
        /// 2. 是建立 WhenSettle後的 prize行為 (我認為這種方式較好)
        /// </summary>
        [ValueUnnecessary] 
        BetCounterRequired_TotalSummary_OneGameOneRecord = 1052,


        /// <summary>
        /// 需指定特定的vendorConfigID
        /// </summary>
        [DisableEdit]
        TargetVendorConfigID = 2021,


        /// <summary>
        /// 需關聯特定的轉盤活動
        /// </summary>
        [DisableEdit]
        TargetLuckyWheelEventID = 3301,


        [DisableEdit]
        HideBeginEndTime_PublicDescriptionPage = 3508,


        [DisableEdit]
        EventContentColorType = 3666,




        EventContentBrandingType = 3677,


        /// <summary>
        /// 排行榜活動。競爭類型 
        /// 可參考 EventCounterOrderByEnum
        /// </summary>
        RankEventCompetiteType = 3751,


        /// <summary>
        /// 關聯特定的活動說明頁
        /// </summary>
        [DisableEdit]
        ReferenceContentEventID = 4331,


        /// <summary>
        /// 最高抽獎次數
        /// </summary>
        [SettingDefaultValue("-1")]
        MaxDrawTimes = 5121,


        /// <summary>
        /// 獎品已發完
        /// </summary>
        [ValueUnnecessary]
        HidePointDescriptionInEventPage = 6501,


        /// <summary>
        /// 獎品已發完
        /// </summary>
        [ValueUnnecessary]
        [ExtraAttributeValue(SystemConstant.Attr_ShowTagInListPage)]
        Tag_PrizeEmpty = 7777,
    }
}
