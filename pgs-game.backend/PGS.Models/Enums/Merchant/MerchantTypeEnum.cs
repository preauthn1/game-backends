using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{
    
    public enum MerchantTypeEnum
    {     
        None = 0,

        /// <summary>
        /// 平台對接 正式環境
        /// </summary>
        Plaform_ProductionEnvironment = 20,


        /// <summary>
        /// 自己人內部測試用
        /// </summary>
        InternalTest = 30,


        /// <summary>
        /// 拉JP專用
        /// </summary>
        JackpotMerchant = 35,


        /// <summary>
        /// 平台對接 測試環境
        /// </summary>
        Plaform_DevelopEnvironment = 40,


        /// <summary>
        /// 提供給遊戲商測試用
        /// </summary>
        GameProvider_QATest = 50,


        /// <summary>
        /// 幽靈網站 (疑似根本沒有數據)
        /// </summary>
        NoDataOrGhostPlatform = 80,


    }
}
