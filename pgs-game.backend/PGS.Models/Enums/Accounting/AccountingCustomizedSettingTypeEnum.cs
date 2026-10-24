using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{

    public enum AccountingCustomizedSettingTypeEnum
    {

        //活動成本
        //每個商戶一張發票
        //視訊返水回扣
        //輸贏折扣.. (輸入項目&數字?)


        [IsUnique]
        [ReferType(typeof(PromotionCostCalculateType))]
        [SettingDefaultValue((int)PromotionCostCalculateType.QueryPromotionCost)]
        PromotionCostFee_CalculateType = 1001,

        /// <summary>
        /// 指定使用特殊的範本檔案
        /// </summary>
        [IsUnique]
        [ReferType(typeof(InvoiceExportTemplateFileTypeEnum))]
        [SettingDefaultValue((int)InvoiceExportTemplateFileTypeEnum.Standard)]
        InvoiceTemplateFileType = 1801,



        /// <summary>
        /// 排除特定的商戶不計算在內 (通常是代理樹狀型的特例)
        /// </summary>
        [IsUnique]
        [ReferType(typeof(string))]
        [SettingDefaultValue("")]
        ExcludeMerchantIDs = 2333,



        [IsUnique]
        [ReferType(typeof(decimal))]
        [SettingDefaultValue("0.05")]
        [DataCategory(DataCategoryEnum.MerchantFeeRate)]
        CommissionFee_PGS = 10000000,

        [IsUnique]
        [ReferType(typeof(decimal))]
        [SettingDefaultValue("0.05")]
        [DataCategory(DataCategoryEnum.MerchantFeeRate)]
        CommissionFee_Funky = 10250000,

        [IsUnique]
        [ReferType(typeof(decimal))]
        [DataCategory(DataCategoryEnum.MerchantFeeRate)]
        [SettingDefaultValue("0.05")]
        CommissionFee_AGG = 10290000,

        [IsUnique]
        [ReferType(typeof(decimal))]
        [SettingDefaultValue("0.05")]
        [DataCategory(DataCategoryEnum.MerchantFeeRate)]
        CommissionFee_AGGLive = 10330000,

        [IsUnique]
        [ReferType(typeof(decimal))]
        [SettingDefaultValue("0.05")]
        [DataCategory(DataCategoryEnum.MerchantFeeRate)]
        CommissionFee_Jili = 10390000,

        [IsUnique]
        [ReferType(typeof(decimal))]
        [SettingDefaultValue("0.05")]
        [DataCategory(DataCategoryEnum.MerchantFeeRate)]
        CommissionFee_Mario = 10430000,

        [IsUnique]
        [ReferType(typeof(decimal))]
        [SettingDefaultValue("0.05")]
        [DataCategory(DataCategoryEnum.MerchantFeeRate)]
        CommissionFee_AT = 10490000,

        [IsUnique]
        [ReferType(typeof(decimal))]
        [SettingDefaultValue("0.05")]
        [DataCategory(DataCategoryEnum.MerchantFeeRate)]
        CommissionFee_Galaxy = 10530000,

        [IsUnique]
        [ReferType(typeof(decimal))]
        [SettingDefaultValue("0.05")]
        [DataCategory(DataCategoryEnum.MerchantFeeRate)]
        CommissionFee_VPlus = 10610000,

        [IsUnique]
        [ReferType(typeof(decimal))]
        [SettingDefaultValue("0.05")]
        [DataCategory(DataCategoryEnum.MerchantFeeRate)]
        CommissionFee_Fuma = 10630000,

        [IsUnique]
        [ReferType(typeof(decimal))]
        [SettingDefaultValue("0.05")]
        [DataCategory(DataCategoryEnum.MerchantFeeRate)]
        CommissionFee_EFG = 10650000,

    }
}
