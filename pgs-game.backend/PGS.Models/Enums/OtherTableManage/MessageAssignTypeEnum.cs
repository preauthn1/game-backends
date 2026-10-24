using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;


namespace target.Models.Enums
{

    public enum MessageAssignTypeEnum
    {
       
        /// <summary>
        /// 僅發送給特定玩家
        /// </summary>
        SpecificPlayerOnly = 0,


        /// <summary>
        /// 特定商戶之下的玩家 (包含未來註冊的新玩家)
        /// </summary>
        MerchantAll = 71,


        /// <summary>
        /// 特定商戶之下的玩家 (不包含新玩家)
        /// </summary>
        MerchantAllExcludeNewPlayer = 73,


        /// <summary>
        /// 僅發送給特定玩家
        /// </summary>
        //AllPlayers = 999,

    }

}
