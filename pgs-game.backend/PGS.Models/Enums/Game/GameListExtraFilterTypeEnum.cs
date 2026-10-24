using target.Models.Attributes;
using target.Models.CultureText;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace target.Models.Enums
{
    /// <summary>
    /// 遊戲名稱有用table管理，但是管理上比較像列舉
    /// </summary>
    public enum GameListExtraFilterTypeEnum
    {        
        SingleWalletSupportGame_Support = 1001,
        SingleWalletSupportGame_NotSupport = 1002,       
        
        RaiseProcessGame_Support = 1031,
        RaiseProcessGame_NotSupport = 1032,

        LaunchProcessAvailableGame_Support = 1071,
        LaunchProcessAvailableGame_NotSupport = 1072,

        AGGFreespin_Support = 1081,
        AGGFreespin_NotSupport = 1082,
    }
}
