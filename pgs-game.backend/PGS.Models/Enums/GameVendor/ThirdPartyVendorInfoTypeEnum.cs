using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;


namespace target.Models.Enums
{
    
    public enum ThirdPartyVendorInfoTypeEnum
    {
        /// <summary>
        /// 外部連結
        /// </summary>
        ExternalLink = 1,

        /// <summary>
        /// 原始json資訊
        /// </summary>
        RawJson = 2,


        RawHtml = 5,


        /// <summary>
        /// 外部連結
        /// </summary>
        PgsCustomizedPage = 11,
    }
}
