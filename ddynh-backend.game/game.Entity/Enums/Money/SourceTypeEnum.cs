using FW.Core.Helper.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Enums
{


    /// <summary>
    /// 对应 [Stat].[MoneySourceType]
    /// </summary>
    public enum SourceTypeEnum 
    {

        [EnumDescription("兑换")]
        Convert_LockAmount = 14000,

        [EnumDescription("推广代理转帐")]
        Agent_Transfer = 14001,

        [EnumDescription("兑换失败退还用户金币")]
        Convert_ReleaseLockAmount = 14002,
        	
	


        [EnumDescription("转到游戏钱包")]
        TransferWallet_ToGameWallet = 15001,

        [EnumDescription("从游戏转回中心钱包")]
        TransferWallet_ToCenterWallet = 15002,

        [EnumDescription("游戏转帐失败，金币退还")]
        TransferWallet_ReleaseBack = 15009,


        [EnumDescription("登入奖励送金币")]
        Event_LoginBonus = 21001,

        [EnumDescription("活动收费扣金币")]
        Event_MinusCashMoney = 21002,        

        [EnumDescription("活动赠送奖励金币")]
        Event_BonusAmount = 21003,


	
	
    }

    
}
