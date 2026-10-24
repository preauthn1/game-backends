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

    public class GameServerTaxLogRowDataExcelModel : GameServerTaxLogGroupRoomInfoExcelModel
    {

        [Description("統計時間")]
        public string Date { get; set; }

    }


    public class GameServerTaxLogGroupRoomInfoExcelModel
    {      

        [Description("游戏房间名称")]
        public string GroupDisplayName { get; set; }

        [ToCash]
        [Description("用户金币_变化")]
        public long Player_Balance_Amount { get; set; }

        [ToCash]
        [Description("用户金币_输赢")]
        public long Player_Balance_WinLoss { get; set; }

        [ToCash]
        [Description("用户金币_税收")]
        public long Player_Balance_Tax { get; set; }

        [ToCash]
        [Description("用户金币_流水")]
        public long Player_Balance_Flow { get; set; }

        [ToCash]
        [Description("机器人金币_变化")]
        public long Robot_Balance_Amount { get; set; }

        [ToCash]
        [Description("机器人金币_输赢")]
        public long Robot_Balance_WinLoss { get; set; }

        [ToCash]
        [Description("机器人金币_税收")]
        public long Robot_Balance_Tax { get; set; }
        
    }
}