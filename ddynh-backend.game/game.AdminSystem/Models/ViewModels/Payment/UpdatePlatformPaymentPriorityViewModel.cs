using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FW.Core.Helper;
using game.Entity.AdminSys;

namespace game.AdminSystem
{
    public class UpdatePlatformPaymentPriorityViewModel
    {

        public int PlatformID { get; set; }

        public int PayTypeID { get; set; }

        public int GWID { get; set; }

        public int TypeID { get; set; }

        public int Priority { get; set; }

    }
}