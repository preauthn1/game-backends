using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{
    public enum InvoiceExportTemplateFileTypeEnum
    {
        Standard = 0,

        /// <summary>
        /// SJ (W88捕魚)版型
        /// </summary>
        LandscapeLayoutWithExchangeRate = 5, 
    }
}
