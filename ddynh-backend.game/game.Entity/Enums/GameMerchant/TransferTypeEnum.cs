using System;
using System.ComponentModel;
using FW.Core.Helper.Enum;

namespace game.Entity.Game
{
 

    public enum TransferTypeEnum
    {
        [EnumDescription("转入游戏钱包")]
        Transfer_In = 1,

        [EnumDescription("自游戏钱包转出")]
        Transfer_Out = 2,
    }


}
