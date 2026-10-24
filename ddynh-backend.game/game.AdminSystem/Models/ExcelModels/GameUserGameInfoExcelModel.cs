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

    public class GameUserGameInfoExcelModel 
    {        

        [Description("所属平台")]
        public string PlatformName { get; set; }

        [Description("用户ID")]
        public int UserID { get; set; }

        [Description("帐号")]
        public string LoginName { get; set; }

        [Description("用户等级")]
        public int GradeID { get; set; }

        [Description("游戏名称")]
        public string GameName { get; set; }

        [Description("赢")]
        public int WinCount { get; set; }

        [Description("输")]
        public int LossCount { get; set; }

        [Description("平")]
        public int DrawCount { get; set; }

        [Description("积分")]
        public long TotalScore { get; set; }

        [Description("游戏时间_单位秒")]
        public long GameTime { get; set; }

        
    }

}