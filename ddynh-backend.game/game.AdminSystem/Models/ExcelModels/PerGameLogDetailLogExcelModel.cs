using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using FW.Core.Helper;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Money;
using game.Infrastructure;

namespace game.AdminSystem
{

    public class PerGameLogDetailLogExcelModel 
    {

        [Description("时间")]
        public string CreateTime { get; set; }

        [Description("牌局标识")]
        public string DealNo { get; set; }

        [Description("服务器编号")]
        public long ServerID { get; set; }

        [Description("服务器名称")]
        public string ServerName { get; set; }

        [Description("游戏名称")]
        public string GameName { get; set; }

        [Description("所属平台")]
        public string PlatformName { get; set; }

        [Description("用户ID")]
        public int UserID { get; set; }
	
        [Description("帐号")]
        public string UserLoginName { get; set; }

        [Description("用户等级")]
        public int GradeID { get; set; }

        [Description("当前金币")]
        [ToCash]
        public long CurrentAmount { get; set; }

        [Description("更新金币")]
        [ToCash]
        public long UpdateAmount { get; set; }
        
    }
    
}