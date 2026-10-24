using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;

namespace target.Models.Enums
{
    /// <summary>
    /// 支援的語系
    /// </summary>
    [Description("資料庫是存字串_編號本身沒意義")]
    public enum LanguageEnum
    {
        zh_cn,

        zh_tw,

        en_us,

        vi_vn,

        ms_my,

        th_th,

        /// <summary>
        /// 緬甸文
        /// </summary>
        my_mm,

        /// <summary>
        /// 印尼文
        /// </summary>
        id_id,
    }
}
