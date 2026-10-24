using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;


namespace target.Models.Enums
{
    /// <summary>
    /// 遊戲商類別
    /// 定義要顯示的遊戲商 AppHelper.GetStillAvailableGameVendors() 
    /// </summary>  
    public enum GameVendorTypeEnum
    {
       
        /// <summary>
        /// 預設:天馬
        /// </summary>
        target = 0,
           
        /// <summary>
        /// slot88
        /// </summary>
        Slot88 = 2001,

        

        /* 重要** 每個遊戲商編號 請至少間隔10000 (不要連號) 因為遊戲編號是以遊戲商來區別 */
        /* 重要** 每個遊戲商編號 請至少間隔10000 (不要連號) 因為遊戲編號是以遊戲商來區別 */
        /* 重要** 每個遊戲商編號 請至少間隔10000 (不要連號) 因為遊戲編號是以遊戲商來區別 */
        /* 重要** 每個遊戲商編號 請至少間隔10000 (不要連號) 因為遊戲編號是以遊戲商來區別 */
        /* 重要** 每個遊戲商編號 請至少間隔10000 (不要連號) 因為遊戲編號是以遊戲商來區別 */
        /* 重要** 每個遊戲商編號 請至少間隔10000 (不要連號) 因為遊戲編號是以遊戲商來區別 */
        /* 重要** 每個遊戲商編號 請至少間隔10000 (不要連號) 因為遊戲編號是以遊戲商來區別 */
        /* 重要** 每個遊戲商編號 請至少間隔10000 (不要連號) 因為遊戲編號是以遊戲商來區別 */

        Funky = 250000,

        AGG = 290000,

        CrazyFishing = 310000,

        AGGLive = 330000,

        AGGFish = 370000,

        Jili = 390000,

        GPI = 410000,

        Mario = 430000,

        YesGame = 450000,

        ATGames = 480000,

        AT = 490000,

        /// <summary>
        /// 新名稱Tiger9
        /// </summary>
        [Description("Tiger9")]
        Galaxy = 530000,

        AGin = 550000,

        ClotPlay = 570000,

        /// <summary>
        /// RichGaming
        /// </summary>
        RG = 590000,


        /// <summary>
        /// 後來改名成 VPlus
        /// </summary>
        [Description("VPlus")]
        SlotMania = 610000,


        Fuma = 630000,


        EFG = 650000

    }
}
