using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;


namespace target.Models.Enums
{
    /// <summary>
    /// 啟動遊戲要帶的參數，會影響部分遊戲的顯示版型
    /// </summary>
    public enum UserDeviceTypeEnum
    {
        /// <summary>
        /// 預設無指定
        /// </summary>
        None = 0,


        /// <summary>
        /// 桌機版電腦
        /// </summary>
        Web = 1,

        H5 = 2,

        Mobile_Android = 11,

        Mobile_iOS = 12,

    }

    public enum EazyDeviceTypeEnum
    {
        /// <summary>
        /// 預設無指定
        /// </summary>
        None = 0,

        /// <summary>
        /// 桌機版電腦
        /// </summary>
        PC = 1,

        Mobile = 2,
    }
}
