using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using FW.Core.Helper;
using game.Entity.AdminSys;
using game.Entity.Money;

namespace game.AdminSystem
{

    public class GameServerTaxLogGroupRoomInfoViewModel
    {
        public string GroupDisplayName { get; set; }
       
        public long Player_Balance_Amount { get; set; }

        public long Player_Balance_WinLoss { get; set; }

        public long Player_Balance_Tax { get; set; }

        public long Player_Balance_Flow { get; set; }

        public long Robot_Balance_Amount { get; set; }

        public long Robot_Balance_WinLoss { get; set; }

        public long Robot_Balance_Tax { get; set; }

    }
}