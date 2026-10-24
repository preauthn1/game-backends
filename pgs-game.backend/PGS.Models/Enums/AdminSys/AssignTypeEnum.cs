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
    public enum AssignTypeEnum
    {
        SpecificOrganizationCanView = 11,

        SpecificUserCanView = 15,

        /// <summary>
        /// 指定單獨商戶 (不含下線)
        /// </summary>
        SpecificReferenceMerchantCanView = 19,

        /// <summary>
        /// 指定特定代理商戶 (含下線)
        /// </summary>
        SpecificReferenceMerchantHierarchyCanView = 21,

        /// <summary>
        /// 全後台公告
        /// </summary>
        All = 999,
    }
}
