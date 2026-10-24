using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{
    public enum InvoiceAggregateType
    {
        OneMerchantOneExport = 1,

        /// <summary>
        /// 整個代理樹狀合併成一筆帳單
        /// </summary>
        GroupByAgentTree = 5,

        AllInOne = 7,
    }
}
