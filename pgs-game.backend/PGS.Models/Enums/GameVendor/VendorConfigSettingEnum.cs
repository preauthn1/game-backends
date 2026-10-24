using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{
    
    public enum VendorConfigSettingEnum
    {

        /// <summary>
        /// 主要對接的API域名
        /// </summary>
        [ReferType(typeof(string))]
        [DisableEdit]
        [DataCategory(DataCategoryEnum.DoaminSetting)] 
        ApiUrl = 1001,


        /// <summary>
        /// (如果有分開) 投注紀錄/統計數據 的API域名
        /// </summary>
        [ReferType(typeof(string))]
        [DisableEdit]
        [DataCategory(DataCategoryEnum.DoaminSetting)] 
        BetReportApiUrl = 1002,



        /// <summary>
        /// 啟動遊戲 靜態網址
        /// </summary>
        [ReferType(typeof(string))]
        [DisableEdit]
        [DataCategory(DataCategoryEnum.DoaminSetting)]
        LaunchGameStaticLinkUrl = 1007,


        /// <summary>
        /// 遊戲商分配給我們的 對接廠商識別ID
        /// </summary>
        [ReferType(typeof(string))]
        [DisableEdit]
        [DataCategory(DataCategoryEnum.SecretKey)]
        MainAssignVendorID = 1011,


        /// <summary>
        /// 遊戲商分配給我們的 對接API Token
        /// </summary>
        [ReferType(typeof(string))]
        [DisableEdit]
        [DataCategory(DataCategoryEnum.SecretKey)]
        MainSecretKeyOrApiToken = [REDACTED],



        /// <summary>
        /// 其他配置1 (e.g. 渠道)
        /// </summary>
        [ReferType(typeof(string))]
        [DisableEdit]
        [DataCategory(DataCategoryEnum.SecretKey)]
        OtherKeySettingValue1 = 1015,


        /// <summary>
        /// 其他配置2 (e.g. 渠道)
        /// </summary>
        [ReferType(typeof(string))]
        [DisableEdit]
        [DataCategory(DataCategoryEnum.SecretKey)]
        OtherKeySettingValue2 = 1016,


        /// <summary>
        /// 其他配置3 (e.g. 渠道)
        /// </summary>
        [ReferType(typeof(string))]
        [DisableEdit]
        [DataCategory(DataCategoryEnum.SecretKey)]
        OtherKeySettingValue3 = 1017,



        /// <summary>
        /// 是否總是採用試玩模式
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategory(DataCategoryEnum.Common)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        GameVendor_IsAlwaysTrialMode = 3701,


        /// <summary>
        /// 強制指定幣別 (僅部分遊戲商有效)
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategory(DataCategoryEnum.Common)]
        [SettingDefaultValue("")]
        GameVendor_ForceCurrency = 3705,


        /// <summary>
        /// 三方帳務比對 隱藏顯示總帳務 (通常是因為混了多貨幣)
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategory(DataCategoryEnum.Common)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        GameVendor_HiddenTotalAmountFinanceCompare = 3731,


        /// <summary>
        /// json配置 (1000字元內)
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategory(DataCategoryEnum.Common)]
        [SettingDefaultValue("")]
        GameVendor_ExtraJsonSetting = 3761,



        /// <summary>
        /// 後台網址
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategory(DataCategoryEnum.ThirdBackOfficeSetting)]
        BackOffice_BackOfficeApiUrl = 5101,


        /// <summary>
        /// 後台網址-帳號
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategory(DataCategoryEnum.ThirdBackOfficeSetting)]
        BackOffice_BackOfficeUserName = 5103,


        /// <summary>
        /// 後台網址-密碼
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategory(DataCategoryEnum.ThirdBackOfficeSetting)]
        BackOffice_BackOfficePassword = [REDACTED],


        /// <summary>
        /// 後台網址-加密key
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategory(DataCategoryEnum.ThirdBackOfficeSetting)]
        BackOffice_BackOfficeHashEncryptKey = 5109,



        /// <summary>
        /// 額外配置1
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategory(DataCategoryEnum.ThirdBackOfficeSetting)]
        BackOffice_ExtraInfo1 = 5131,



        /// <summary>
        /// 額外配置2
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategory(DataCategoryEnum.ThirdBackOfficeSetting)]
        BackOffice_ExtraInfo2 = 5132,


        /// <summary>
        /// 預設注額 (AGG)
        /// </summary>
        [ReferType(typeof(decimal))]
        [DataCategory(DataCategoryEnum.GameVendor_AGGOnly)]
        AGG_DefaultBetAmount = 1003101,

    }
}
