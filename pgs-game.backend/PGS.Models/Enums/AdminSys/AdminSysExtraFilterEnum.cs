using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{

    /// <summary>
    /// 部分頁面會提供特殊的篩選條件
    /// </summary>
    public enum AdminSysExtraFilterEnum
    {
        /// <summary>
        /// 錢包餘額不為0
        /// </summary>
        [DataCategoryAttribute(DataCategoryEnum.ExtraFilterType_Player)]
        Player_FilterAmountNotZero = 3500,



        [DataCategoryAttribute(DataCategoryEnum.ExtraFilterType_ThirdPartyApiReqeustLog)]
        ThirdPartyApiLog_ResponseTime_5s = 3601,

        [DataCategoryAttribute(DataCategoryEnum.ExtraFilterType_ThirdPartyApiReqeustLog)]
        ThirdPartyApiLog_ResponseTime_10s = 3602,

        //[DataCategoryAttribute(DataCategoryEnum.ExtraFilterType_ThirdPartyApiReqeustLog)]
        //Exclude_LaunchAPI = 3605,

    }


    public enum AdminSysSpecialFilterTypeEnum
    {
        // 請改用上方那個Eunum的
        // 請改用上方那個Eunum的
        // 請改用上方那個Eunum的
    }


}
